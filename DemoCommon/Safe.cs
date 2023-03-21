namespace DemoCommon;

public static class Safe
{
    public static bool TryGetValue<T>(Func<T> func, out T value)
    {
        try
        {
            value = func();

            return true;
        }
        catch
        {
            value = default!;

            return false;
        }
    }
}