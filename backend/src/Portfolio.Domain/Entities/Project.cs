using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities;

public enum PublishStatus { Draft, Published }

public class Project : Entity
{
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    /// <summary>Documento do editor TipTap (JSON).</summary>
    public string Content { get; set; } = "{}";
    public bool Featured { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public Guid? CoverMediaId { get; set; }
    public Media? CoverMedia { get; set; }
    public List<ProjectRepo> Repos { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
    public List<ProjectMedia> Gallery { get; set; } = [];
}

public class ProjectRepo : Entity, IOrdered
{
    public Guid ProjectId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Language { get; set; }
    public int Order { get; set; }
}

public class Tag : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<Project> Projects { get; set; } = [];
}

public class ProjectMedia : IOrdered
{
    public Guid ProjectId { get; set; }
    public Guid MediaId { get; set; }
    public Media? Media { get; set; }
    public int Order { get; set; }
    public string? Caption { get; set; }
}
