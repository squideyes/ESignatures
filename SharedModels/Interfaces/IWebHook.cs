﻿namespace SharedModels;

public interface IWebHook<T>
    where T : new()
{
    Guid ContractId { get; }
    WebHookKind WebHookKind { get; }
}
