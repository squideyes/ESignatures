using FluentValidation;
using SquidEyes.Fundamentals;

namespace ESignatures;

public class Signer
{
    public class Validator : AbstractValidator<Signer>
    {
        public Validator()
        {
            RuleFor(x => x.FullName)
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'FullName' must be non-empty and trimmed.");

            RuleFor(x => x.Nickname)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty();

            RuleFor(x => x.Mobile)
                .NotEmpty();

            RuleFor(x => x.Company)
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Company' must be non-empty and trimmed.")
                .When(v => v is not null);
        }
    }

    public required string FullName { get; init; }
    public required Token Nickname { get; init; }
    public required Email Email { get; init; }
    public required Phone Mobile { get; init; }
    public string? Company { get; init; }

    public Guid? SignerId { get; set; }

    public string GetSha256Hash() => CryptoHelper.GetHash(
        FullName, Email.ToString(), Mobile.Formatted(PhoneFormat.E164));
}
