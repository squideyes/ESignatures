//namespace SquidEyes.ESignatures;

//internal class SignerId : IEquatable<SignerId>
//{
//    public SignerId(string name, string email, string mobile)
//    {
//        Name = name;
//        Email = email;
//        Mobile = mobile;
//    }

//    public string Name { get; }
//    public string Email { get; }
//    public string Mobile { get; }

//    public (string, string, string) AsTuple() =>
//        (Name, Email, Mobile);

//    public bool Equals(SignerId? other) =>
//        other is not null && other!.AsTuple().Equals(AsTuple());

//    public override bool Equals(object? other) =>
//        other is SignerId key && Equals(key);

//    public override int GetHashCode() =>
//        HashCode.Combine(Name, Email, Mobile);

//    public static bool operator ==(SignerId lhs, SignerId rhs)
//    {
//        if (lhs is null)
//            return rhs is null;

//        return lhs.Equals(rhs);
//    }

//    public static bool operator !=(SignerId lhs, SignerId rhs) =>
//        !(lhs == rhs);
//}
