namespace SquidEyes.ESignatures;

public interface IMetadata<T>
    where T : IMetadata<T>, new()
{
    T Parse(string value);
}
