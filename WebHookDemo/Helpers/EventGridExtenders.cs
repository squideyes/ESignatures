namespace WebHookDemo;

public static class EventGridExtenders
{
    public static string ToSubject<T>(this T data)
        where T : IWebHookData<T>, new()
    {
        return $"{typeof(T).Name} ({data.ContractId})";
    }
}
