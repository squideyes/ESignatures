namespace WebHookDemo;

public class Signer
{
    public required Guid SignerId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Mobile { get; init; }
    public List<SignerEvent>? Events { get; init; }
}
