using FluentValidation;
using Portfolio.Application.Contracts;

namespace Portfolio.Api.Endpoints;

public sealed class ProfileValidator : AbstractValidator<ProfileRequest>
{
    public ProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Headline).MaximumLength(200);
    }
}

public sealed class EducationValidator : AbstractValidator<EducationRequest>
{
    public EducationValidator()
    {
        RuleFor(x => x.Institution).NotEmpty();
        RuleFor(x => x.Course).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue);
    }
}

public sealed class ExperienceValidator : AbstractValidator<ExperienceRequest>
{
    public ExperienceValidator()
    {
        RuleFor(x => x.Company).NotEmpty();
        RuleFor(x => x.Role).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue);
    }
}

public sealed class CertificationValidator : AbstractValidator<CertificationRequest>
{
    public CertificationValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Issuer).NotEmpty();
        RuleFor(x => x.CredentialUrl).Must(u => Uri.IsWellFormedUriString(u, UriKind.Absolute))
            .When(x => !string.IsNullOrEmpty(x.CredentialUrl));
    }
}

public sealed class CategoryValidator : AbstractValidator<CategoryRequest>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$");
    }
}

public sealed class ProjectValidator : AbstractValidator<ProjectRequest>
{
    public ProjectValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$");
        RuleFor(x => x.Status).Must(s => s is "draft" or "published");
        RuleForEach(x => x.Repos).ChildRules(r => r.RuleFor(x => x.Url).Must(u => Uri.IsWellFormedUriString(u, UriKind.Absolute)));
    }
}

public sealed class CreateMediaValidator : AbstractValidator<CreateMediaRequest>
{
    public CreateMediaValidator()
    {
        RuleFor(x => x.Sha256).NotEmpty().Length(64);
        RuleFor(x => x.Alt).NotEmpty().WithMessage("Texto alternativo é obrigatório.");
        RuleFor(x => x.Variants).NotEmpty();
        RuleForEach(x => x.Variants).ChildRules(v => v.RuleFor(x => x.Width).InclusiveBetween(1, 4096));
    }
}
