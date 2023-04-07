using FluentValidation;
using SquidEyes.Fundamentals;

namespace ESignatures;

public class ContractInfo<M>
    where M : class
{
    public class Validator : AbstractValidator<ContractInfo<M>>
    {
        public Validator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty();

            RuleFor(x => x.Metadata)
                .NotEmpty();

            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v.IsNonEmptyAndTrimmed())
                .WithMessage("'%PropertyName%' must be a non-empty trimmed string.");

            RuleFor(x => x.WebHookUri)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v.IsAbsoluteUri)
                .WithMessage("'%PropertyName%' must be a valid \"absolute\" URI.");

            RuleFor(x => x.ExpiryHours)
                .ExclusiveBetween(1, 168);

            RuleFor(x => x.Locale)
                .IsInEnum();

            RuleFor(x => x.LogoUri)
                .Must(v => v!.IsAbsoluteUri)
                .WithMessage("'%PropertyName%' must be a valid \"absolute\" URI.")
                .When(v => v is not null);
        }
    }

    public required Guid TemplateId { get; init; }
    public required M Metadata { get; init; }
    public required string Title { get; init; }
    public required Uri WebHookUri { get; init; }

    public EmailSpec? RequestSpec { get; init; }
    public EmailSpec? ContractSpec { get; init; }
    public Email? ReplyTo { get; init; }
    public Uri? LogoUri { get; init; }
    public Locale Locale { get; init; } = Locale.EN;
    public int ExpiryHours { get; init; } = 6;
    public DateOnly SignDate { get; init; } = default;
}
