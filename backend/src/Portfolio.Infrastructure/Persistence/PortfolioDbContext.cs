using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence;

public partial class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Education> Education => Set<Education>();
    public DbSet<Experience> Experience => Set<Experience>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectRepo> ProjectRepos => Set<ProjectRepo>();
    public DbSet<ProjectMedia> ProjectMedia => Set<ProjectMedia>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Media> Media => Set<Media>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("hstore");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
        ApplySnakeCaseNames(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Entity>().Where(e => e.State == EntityState.Modified))
            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Tabelas e colunas em snake_case, padrão do Postgres.</summary>
    private static void ApplySnakeCaseNames(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (!entity.IsOwned()) entity.SetTableName(ToSnake(entity.GetTableName()!));
            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnake(property.GetColumnName()));
            foreach (var key in entity.GetKeys())
                key.SetName(ToSnake(key.GetName()!));
            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnake(fk.GetConstraintName()!));
            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnake(index.GetDatabaseName()!));
        }
    }

    private static string ToSnake(string name) => SnakeRegex().Replace(name, "$1_$2").ToLowerInvariant();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex SnakeRegex();
}
