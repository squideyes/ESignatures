// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using SquidEyes.Fundamentals;

namespace SquidEyes.ESignatures.WebHook;

public class BasicSigner
{
    public required Guid? SignerId { get; init; }
    public required string FullName { get; init; }
    public required Email Email { get; init; }
    public required Phone Mobile { get; init; }
    public string? Company { get; init; }

    public string GetSha256Hash() => CryptoHelper.GetHash(
        FullName, Email.ToString(), Mobile.Formatted(PhoneFormat.E164));
}