using SquidEyes.Fundamentals;

namespace SharedModels;

public class Metadata
{
    public Metadata(ClientId clientId, DocKind docKind, DateOnly docDate)
    {
        ClientId = clientId.MayNot().BeDefault();
        DocKind = docKind.Must().BeEnumValue();
        DocDate = docDate.MayNot().BeDefault();

        DocCode = ToCode(DocKind, DocDate);
    }

    public ClientId ClientId { get; }
    public DocKind DocKind { get; }
    public DateOnly DocDate { get; }
    public string DocCode { get; }

    public override string ToString() => $"CID={ClientId}|DOC={DocCode}";

    public static Metadata Parse(string value)
    {
        value.MayNot().BeNullOrWhitespace();

        var fields = value.Split('|');

        if (fields.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(value));

        var clientId = ClientId.From(GetValue(fields[0], "CID"));

        var (docKind, docDate) = ToDocKindAndDate(GetValue(fields[1], "DOC"));

        return new Metadata(clientId, docKind, docDate);
    }

    private static string GetValue(string value, string token)
    {
        var fields = value.Split('=');

        if (fields.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(value));

        if (fields[0] != token)
            throw new ArgumentOutOfRangeException(nameof(value));

        return fields[1];
    }

    private static (DocKind, DateOnly) ToDocKindAndDate(string value)
    {
        if (value.Length != 12)
            throw new ArgumentOutOfRangeException(nameof(value));

        var docKind = value[1..2] switch
        {
            "AC" => DocKind.AffiliateContract,
            "PC" => DocKind.PartnerContract,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        var docDate = DateOnly.ParseExact(value[3..11], "yyyyMMdd");

        return (docKind, docDate);
    }

    private static string ToCode(DocKind kind, DateOnly date)
    {
        var prefix = kind switch
        {
            DocKind.AffiliateContract => "AC",
            DocKind.PartnerContract => "PC",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        return $"{prefix}({date:yyyyMMdd})";
    }
}
