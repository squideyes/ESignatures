using System.Text.Json.Nodes;

namespace WebHookDemo;

public class MobileUpdate : IWebHookData<MobileUpdate>
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }
    public string? NewMobile { get; set; }

    public WebHookDataKind Kind => WebHookDataKind.MobileUpdate;

    public static MobileUpdate Create(JsonNode? node)
    {
        var data = node.GetData("signer-mobile-update-request");

        var contract = data!["contract"]!;

        var signer = data!["signer"]!;

        return new MobileUpdate()
        {
            ContractId = Guid.Parse(contract.GetString("id")),
            Metadata = Metadata.Parse(contract.GetString("metadata")),
            Signer = new Signer()
            {
                SignerId = Guid.Parse(signer.GetString("id")),
                Name = signer.GetString("name"),
                Email = signer.GetString("email"),
                Mobile = signer.GetString("mobile")
            },
            NewMobile = signer.GetString("mobile_new")
        };
    }
}