﻿using FluentValidation;
using System.Text;
using static ESignatures.PostalCodeValidator;

namespace ESignatures;

public class Address 
{
    public class Validator : AbstractValidator<Address>
    {
        public Validator()
        {
            RuleFor(x => x.Country)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(CountryValidator.IsCountryCode)
                .WithMessage("'Country' must be an ISO 3166 country code.");

            RuleFor(x => x.Address1)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => v!.IsNonEmptyAndTrimmed())
                .WithMessage("'Address1' must be non-empty and trimmed.");

            RuleFor(x => x.Address2)
                .Must(v => v!.IsEmptyOrTrimmed())
                .WithMessage("'Address2' must be empty or trimmed.");

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

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(v => IsPostalCode(v.Country, v.PostalCode))
                .WithMessage(v => $"'PostalCode' must be valid for {v.Country}.");
        }
    }

    public required string Country { get; init; }
    public required string Address1 { get; init; }
    public string Address2 { get; init; } = "";
    public required string Locality { get; init; }
    public required string Region { get; init; }
    public required string PostalCode { get; init; }

    public string GetOneLineAddress()
    {
        var sb = new StringBuilder();

        sb.Append(Address1);

        if (!string.IsNullOrEmpty(Address2))
            sb.AppendDelimited(Address2, ", ");

        sb.AppendDelimited(Locality!, ", ");
        sb.AppendDelimited(Region!, ", ");
        sb.AppendDelimited(PostalCode!, ", ");
        sb.AppendDelimited(Country!, ", ");

        return sb.ToString();
    }
}