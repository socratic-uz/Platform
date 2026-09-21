namespace Domain.Abstractions;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

public abstract record EventBase(EventCallback callback = default)
{
    public static implicit operator EventCallback(EventBase @event) => @event.callback;
}

public abstract record EventBase<T>(EventCallback<T> callback = default)
{
    public static implicit operator EventCallback<T>(EventBase<T> @event) => @event.callback;
}
