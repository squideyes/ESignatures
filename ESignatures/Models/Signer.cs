using FluentValidation;
using System.Text;
using System.Text.Json.Serialization;
using static SquidEyes.ESignatures.EmailValidator;
using static SquidEyes.ESignatures.PostalCodeValidator;

namespace SquidEyes.ESignatures;

public class Signer
{
    public class Validator : AbstractValidator<Signer>
    {
        public Validator()
        {
            RuleFor(x => x.Kind)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v.IsToken())
                .WithMessage("'Kind' must be a valid alpha-numeric token.");

            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Name' must be non-empty and trimmed.");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(IsEmailAddress)
                .WithMessage("'Email' must be a valid email adddress.");

            RuleFor(x => x.Mobile)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v.IsPhoneNumber())
                .WithMessage("'Mobile' must be a valid mobile phone number.");

            RuleFor(x => x.Company)
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Company' must be non-empty and trimmed.")
                .When(v => v is not null);

            RuleFor(x => x.KnownAs)
                .Must(v => v!.IsToken())
                .WithMessage("'Kind' must be an alpha-numeric token.")
                .When(v => v is not null);

            RuleFor(x => x.Country)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(CountryValidator.IsCountryCode)
                .WithMessage("'Country' must be a valid ISO 3166 country code.");

            RuleFor(x => x.Address1)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Address1' must be non-empty and trimmed.");

            RuleFor(x => x.Address2)
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Address2' must be non-empty and trimmed.")
                .When(v => v is not null);

            RuleFor(x => x.Locality)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Locality' must be non-empty and trimmed.");

            RuleFor(x => x.Region)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Region' must be non-empty and trimmed.");

            RuleFor(x => x.PostalCode)
                .NotEmpty();

            RuleFor(x => x)
                .Must(v => IsPostalCode(v.Country, v.PostalCode))
                .WithMessage(v => $"'PostalCode' must be valid for {v.Country}.")
                .When(v => v.PostalCode is not null);

            RuleFor(x => x.Ordinal)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.SigReqBy)
                .IsInEnum();

            RuleFor(x => x.GetDocBy)
                .IsInEnum();
        }
    }

    public required string Kind { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Mobile { get; init; }
    public required string Country { get; init; }
    public required string Address1 { get; init; }
    public string? Address2 { get; init; }
    public required string Locality { get; init; }
    public required string Region { get; init; }
    public required string PostalCode { get; init; }

    public string? Company { get; init; }
    public string? KnownAs { get; init; }

    public int Ordinal { get; set; } = 0;
    public Mode SigReqBy { get; set; } = Mode.Email;
    public Mode GetDocBy { get; set; } = Mode.Email;
    public bool IdBySms { get; set; } = true;
    public bool IdByEmail { get; set; } = true;


    // UPDATED AFTER CONTRACT-SENT
    [JsonIgnore]
    public Guid? SignerId { get; set; } = null!;

    public string GetSha256Hash() => CryptoHelper.GetHash(
        Name, Email, Mobile.ToPlusAndDigits());

    public string GetOneLineAddress()
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

    public Dictionary<string, string> AsPlaceholders()
    {
        var prefix = Name.ToLower() + "-";

        var dict = new Dictionary<string, string>
        {
            { prefix + "signer-kind", Kind! },
            { prefix + "name", Name! },
            { prefix + "email", Email! },
            { prefix + "mobile", Mobile! },
            { prefix + "company", Company! },
            { prefix + "known-as", KnownAs! },
            { prefix + "one-line-address", GetOneLineAddress() },
            { prefix + "country", Country! },
            { prefix + "address1", Address1! },
            { prefix + "address2", Address2! },
            { prefix + "locality", Locality! },
            { prefix + "region", Region! },
            { prefix + "postal-code", PostalCode! }
        };

        return dict;
    }
}
