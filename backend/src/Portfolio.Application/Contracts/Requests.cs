namespace Portfolio.Application.Contracts;

public sealed record ProfileRequest(string Name, string Headline, string Bio, Dictionary<string, string>? Links, Guid? AvatarMediaId);

public sealed record EducationRequest(string Institution, string Course, DateOnly StartDate, DateOnly? EndDate, string? Description, int Order);

public sealed record ExperienceRequest(string Company, string Role, DateOnly StartDate, DateOnly? EndDate, string? Description, int Order);

public sealed record CertificationRequest(string Name, string Issuer, DateOnly IssuedAt, string? CredentialUrl, Guid? BadgeMediaId, int Order);

public sealed record CategoryRequest(Guid? ParentId, string Name, string Slug, int Order, string? Icon, string? Color);

public sealed record ReorderItem(Guid Id, Guid? ParentId, int Order);

public sealed record ProjectRepoRequest(string Url, string? Language, int Order);

public sealed record ProjectMediaRequest(Guid MediaId, int Order, string? Caption);

public sealed record ProjectRequest(
    Guid CategoryId,
    string Title,
    string Slug,
    string Summary,
    string Content,
    bool Featured,
    string Status,
    Guid? CoverMediaId,
    IReadOnlyList<ProjectRepoRequest> Repos,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ProjectMediaRequest> Gallery);

public sealed record MediaVariantRequest(int Width, long Bytes);

public sealed record CreateMediaRequest(string Sha256, int Width, int Height, string Alt, IReadOnlyList<MediaVariantRequest> Variants);

public sealed record CreateMediaResponse(Guid Id, IReadOnlyList<PresignedUploadDto> UploadUrls, bool Duplicate);

public sealed record PresignedUploadDto(int Width, string Url);

public sealed record UpdateMediaRequest(string Alt);
