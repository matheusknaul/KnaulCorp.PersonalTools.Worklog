using Microsoft.EntityFrameworkCore;
using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence;
/// <summary>Contexto de escrita restrito ao schema worklog.</summary>
public sealed class WorklogDbContext(DbContextOptions<WorklogDbContext> options) : DbContext(options)
{
    #region Properties
    /// <summary>Intervalos de trabalho.</summary>
    public DbSet<WorkEntry> Entries => Set<WorkEntry>();
    #endregion
    #region Methods
    /// <summary>Configura a persistência dos intervalos.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("worklog");
        var entry = modelBuilder.Entity<WorkEntry>();
        entry.ToTable("entries", table => table.HasCheckConstraint("ck_entries_time", "\"EndedAt\" IS NULL OR \"EndedAt\" > \"StartedAt\""));
        entry.HasKey(value => value.Id);
        entry.Property(value => value.Version).IsConcurrencyToken();
        entry.HasIndex(value => value.StartedAt);
        entry.Property(value => value.Description).HasMaxLength(4000).IsRequired();
        entry.Property(value => value.Project).HasMaxLength(200);
        entry.Property(value => value.Epic).HasMaxLength(200);
        entry.Property(value => value.Activity).HasMaxLength(100);
        entry.Property(value => value.Type).HasMaxLength(200);
        entry.Property(value => value.Interruption).HasMaxLength(4000);
        entry.Property(value => value.Origin).HasMaxLength(200);
        entry.Property(value => value.Result).HasMaxLength(4000);
    }
    #endregion
}
