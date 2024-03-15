namespace Sigurd.EventBus.Api;

public record BusConfiguration
{
    public bool StartImmediately { get; init; } = true;

    public IEventBus Build() => new EventBus(this);
}
