using FluentValidation;
using SquidEyes.Basics;
using static ESignatures.EmailValidator;

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
                .Must(v => v!.IsToken())
                .WithMessage("'Nickname' must be an alpha-numeric token.");

            RuleFor(x => x.Email)
                .Must(IsEmailAddress)
                .WithMessage("'Email' must be a valid email adddress.");

            RuleFor(x => x.Mobile)
                .Must(v => v.IsPhoneNumber())
                .WithMessage("'Mobile' must be a valid mobile phone number.");

            RuleFor(x => x.Company)
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Company' must be non-empty and trimmed.")
                .When(v => v is not null);
        }
    }

    public required string FullName { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string Mobile { get; init; }
    public string? Company { get; init; }

    public Guid? SignerId { get; set; }

    public string GetSha256Hash() => CryptoHelper.GetHash(
        FullName, Email, Mobile.ToPlusAndDigits());
}
