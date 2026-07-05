using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Persistence;

public sealed class MedInsightDbContext(DbContextOptions<MedInsightDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MedInsightDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
