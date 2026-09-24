using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> b)
    {
        b.ToTable("profile");
        b.Property(x => x.Name).HasMaxLength(120);
        b.Property(x => x.Headline).HasMaxLength(200);
        b.Property(x => x.Links).HasColumnType("hstore");
        b.HasOne(x => x.AvatarMedia).WithMany().HasForeignKey(x => x.AvatarMediaId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> b) => b.ToTable("education");
}

internal sealed class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> b) => b.ToTable("experience");
}

internal sealed class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> b)
    {
        b.ToTable("certification");
        b.HasOne(x => x.BadgeMedia).WithMany().HasForeignKey(x => x.BadgeMediaId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("category");
        b.Property(x => x.Name).HasMaxLength(80);
        b.Property(x => x.Slug).HasMaxLength(80);
        b.HasIndex(x => new { x.ParentId, x.Slug }).IsUnique();
        // DELETE bloqueado se houver filhos
        b.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("project");
        b.Property(x => x.Title).HasMaxLength(160);
        b.Property(x => x.Slug).HasMaxLength(160);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Content).HasColumnType("jsonb");
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        b.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CoverMedia).WithMany().HasForeignKey(x => x.CoverMediaId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Repos).WithOne().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Gallery).WithOne().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Tags).WithMany(x => x.Projects).UsingEntity(j => j.ToTable("project_tag"));
    }
}

internal sealed class ProjectRepoConfiguration : IEntityTypeConfiguration<ProjectRepo>
{
    public void Configure(EntityTypeBuilder<ProjectRepo> b) => b.ToTable("project_repo");
}

internal sealed class ProjectMediaConfiguration : IEntityTypeConfiguration<ProjectMedia>
{
    public void Configure(EntityTypeBuilder<ProjectMedia> b)
    {
        b.ToTable("project_media");
        b.HasKey(x => new { x.ProjectId, x.MediaId });
        b.HasOne(x => x.Media).WithMany().HasForeignKey(x => x.MediaId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> b)
    {
        b.ToTable("tag");
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

internal sealed class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> b)
    {
        b.ToTable("media");
        b.HasIndex(x => x.Sha256).IsUnique();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.Alt).HasMaxLength(300);
    }
}
