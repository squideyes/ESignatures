namespace SharedModels;

public class SignerEvent
{
    public required SignerEventKind Kind { get; init; }
    public required DateTime TimeStamp { get; init; }
}
