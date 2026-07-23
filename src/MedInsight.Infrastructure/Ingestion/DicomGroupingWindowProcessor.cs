using MedInsight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedInsight.Infrastructure.Ingestion;

/// <summary>
/// Bekleme penceresi (ingestion-pipeline.md): son DICOM dosyasından sonra
/// pencere süresi kadar yeni dosya gelmezse grup tamamlanmış sayılır ve
/// Case üzerinden DICOMStudyGrouped yayınlanır.
/// </summary>
public sealed class DicomGroupingWindowProcessor(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<DicomGroupingWindowProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var window = TimeSpan.FromSeconds(configuration.GetValue("Ingestion:DicomGroupingWindowSeconds", 120));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CloseExpiredWindowsAsync(window, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DICOM gruplama penceresi islenirken hata");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task CloseExpiredWindowsAsync(TimeSpan window, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedInsightDbContext>();

        var cutoff = DateTime.UtcNow - window;
        var expired = await db.DicomStudies.AsNoTracking()
            .Where(s => !s.IsGrouped && s.LastFileReceivedAtUtc < cutoff && s.Series.Count > 0)
            .Select(s => new { s.Id, s.CaseId })
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var study in expired)
        {
            var medicalCase = await db.Cases
                .Include(c => c.DicomStudies).ThenInclude(s => s.Series)
                .FirstOrDefaultAsync(c => c.Id == study.CaseId, cancellationToken);

            medicalCase?.CompleteDicomGrouping(study.Id);
        }

        if (expired.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
