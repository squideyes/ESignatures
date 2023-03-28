namespace ESignatures;

public class SignerInfo
{
    public SignerInfo(Signer signer, 
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