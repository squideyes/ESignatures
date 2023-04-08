// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using SquidEyes.Fundamentals;

namespace ESignatures.Client;

public class SignerPlan
{
    public SignerPlan(Signer signer, 
        Handling handling, Address? address)
    {
        Signer = signer;
        Handling = handling;
        Address = address;
    }

    public Signer Signer { get; }
    public Handling Handling { get; }
    public Address? Address { get; }
}