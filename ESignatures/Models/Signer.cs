using FluentValidation;
using System.Text;
using System.Text.Json.Serialization;

namespace SquidEyes.ESignatures;

public class Signer
{
    public class Validator : AbstractValidator<Signer>
    {
        public Validator()
        {
            var emailValidator = new EmailValidator();

            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Email)
                .Must(emailValidator.IsValid)
                .WithMessage("'Email' must be a valid email adddress.");

            RuleFor(x => x.Mobile)
                .Must(v => v.IsPhoneNumber())
                .WithMessage("'Mobile' must be a valid phone number.");

            RuleFor(x => x.Company)
                .NotEmpty();

            RuleFor(x => x.Ordinal)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.SigReqBy)
                .IsInEnum();

            RuleFor(x => x.GetDocBy)
                .IsInEnum();
        }
    }

    // UPDATED AFTER CONTRACT-SENT
    [JsonIgnore]
    public Guid? SignerId { get; set; } = null!;

    // REQUIRED
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Mobile { get; init; }

    // OPTIONAL
    public string? Company { get; init; }
    public string? KnownAs { get; init; }
    public string? Country { get; init; }
    public string? Address1 { get; init; }
    public string? Address2 { get; init; }
    public string? Locality { get; init; }
    public string? Region { get; init; }
    public string? PostalCode { get; init; }

    // DEFAULTED
    public int Ordinal { get; set; } = 0;
    public Mode SigReqBy { get; set; } = Mode.Email;
    public Mode GetDocBy { get; set; } = Mode.Email;
    public bool IdBySms { get; set; } = true;
    public bool IdByEmail { get; set; } = true;

    public string GetSha256Hash() => CryptoHelper.GetHash(
        Name, Email, Mobile.ToPlusAndDigits());

    public string GetAddress()
    {
        var sb = new StringBuilder();

        sb.Append(Address1);

        if (Address2 != null)
            sb.AppendDelimited(Address2, ", ");

        sb.AppendDelimited(Locality!, ", ");
        sb.AppendDelimited(Region!, ", ");
        sb.AppendDelimited(PostalCode!, ", ");
        sb.AppendDelimited(Country!, ", ");

        return sb.ToString();
    }
}
