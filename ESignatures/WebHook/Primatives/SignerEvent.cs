// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace SquidEyes.ESignatures.WebHook;

public class SignerEvent
{
    public required SignerEventKind Kind { get; init; }
    public required DateTime TimeStamp { get; init; }
}