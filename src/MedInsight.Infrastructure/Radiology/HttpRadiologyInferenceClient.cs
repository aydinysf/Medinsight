using System.Net.Http.Json;
using MedInsight.Application.Abstractions.Radiology;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedInsight.Infrastructure.Radiology;

/// <summary>
/// Python Radiology Inference Service HTTP istemcisi (ADR-010).
/// Radiology:BaseUrl tanımlı değilse devre dışıdır — pipeline etkilenmez.
/// </summary>
public sealed class HttpRadiologyInferenceClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<HttpRadiologyInferenceClient> logger) : IRadiologyInferenceClient
{
    private sealed record InferenceRequest(string StudyId, IReadOnlyList<string> DicomSeriesUrls);

    private sealed record InferenceFinding(string FindingId, string ModelName, string ModelSource, string OutputType, string Description, System.Text.Json.JsonElement RawOutput, string Disclaimer);

    private sealed record InferenceResponse(string StudyId, List<InferenceFinding> Findings);

    public bool IsEnabled => !string.IsNullOrWhiteSpace(configuration["Radiology:BaseUrl"]);

    public async Task<IReadOnlyList<RadiologyFinding>> AnalyzeStudyAsync(Guid studyId, IReadOnlyList<string> dicomUrls, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return [];
        }

        try
        {
            var client = httpClientFactory.CreateClient("radiology");
            client.BaseAddress = new Uri(configuration["Radiology:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue("Radiology:TimeoutSeconds", 120));

            var response = await client.PostAsJsonAsync(
                "/inference",
                new InferenceRequest(studyId.ToString(), dicomUrls),
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<InferenceResponse>(cancellationToken: cancellationToken);
            return payload?.Findings
                    .Select(f => new RadiologyFinding(f.FindingId, f.ModelName, f.ModelSource, f.OutputType, f.Description, f.RawOutput.GetRawText(), f.Disclaimer))
                    .ToList()
                ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Görüntü analizi bilgilendirici katmandır — hatası çekirdek akışı durduramaz.
            logger.LogWarning(ex, "Radiology Inference Service cagrisi basarisiz (study: {StudyId})", studyId);
            return [];
        }
    }
}
