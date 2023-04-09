// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using SquidEyes.Fundamentals;

namespace SquidEyes.SharedModels;

public class Metadata
{
    public Metadata(ClientId clientId, DocKind docKind, DateOnly signDate)
    {
        ClientId = clientId.MayNot().BeDefault();
        DocKind = docKind.Must().BeEnumValue();
        SignDate = signDate.MayNot().BeDefault();

        DocCode = ToCode(DocKind, SignDate);
    }

    public ClientId ClientId { get; }
    public DocKind DocKind { get; }
    public DateOnly SignDate { get; }
    public string DocCode { get; }

    public override string ToString() => $"CID={ClientId}|DOC={DocCode}";

    public static Metadata Parse(string value)
    {
        value.MayNot().BeNullOrWhitespace();

        var fields = value.Split('|');

        if (fields.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(value));

        var clientId = ClientId.From(GetValue(fields[0], "CID"));

        var (docKind, signDate) =
            ToDocKindAndSignDate(GetValue(fields[1], "DOC"));

        return new Metadata(clientId, docKind, signDate);
    }

    private static string GetValue(string value, string token)
    {
        var fields = value.Split('=');

        fields.Length.Must().Be(2);
        fields[0].Must().Be(token);

        return fields[1];
    }

    private static (DocKind, DateOnly) ToDocKindAndSignDate(string value)
    {
        value.MayNot().BeNullOrWhitespace();

        var parenIndex = value.IndexOf('(');

        if (parenIndex == -1)
            throw new ArgumentOutOfRangeException(nameof(value));

        var docKind = value[..parenIndex] switch
        {
            "AFFILIATE" => DocKind.AffiliateContract,
            "PARTNER" => DocKind.PartnerContract,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        var signDate = DateOnly.ParseExact(
            value[(parenIndex + 1)..^2], "yyyyMMdd");

        return (docKind, signDate);
    }

    private static string ToCode(DocKind docKind, DateOnly signDate)
    {
        var prefix = docKind switch
        {
            DocKind.AffiliateContract => "AFFILIATE",
            DocKind.PartnerContract => "PARTNER",
            _ => throw new ArgumentOutOfRangeException(nameof(docKind))
        };

        return $"{prefix}({signDate:yyyyMMdd})";
    }
}