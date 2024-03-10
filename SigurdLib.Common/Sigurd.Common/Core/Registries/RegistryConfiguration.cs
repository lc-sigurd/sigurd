using System;

namespace Sigurd.Common.Core.Registries;

public record RegistryConfiguration<TValue> where TValue : class
{
    private const int DefaultMaxInt = Int32.MaxValue - 1;

    public int MinId { get; set; } = 0;
    public int MaxId { get; set; } = DefaultMaxInt;

    public bool AllowModifications { get; set; } = false;

    public bool Taggable { get; set; } = false;

    /// <summary>
    /// Event invoked when contents are added to the registry. This will be invoked when the registry
    /// is rebuilt on the client side due to a server-side synchronization.
    /// </summary>
    public EventHandler<IRegistry.AddEventArgs<TValue>>? AddCallback;

    /// <summary>
    /// Event invoked when the registry's contents are cleared. This will be invoked before a registry
    /// is rebuilt.
    /// </summary>
    public EventHandler<ISigurdRegistryModifiable.ClearEventArgs<TValue>>? ClearCallback;

    /// <summary>
    /// Event invoked when a registry instance is initially created.
    /// </summary>
    public EventHandler<IRegistry.CreateEventArgs<TValue>>? CreateCallback;

    /// <summary>
    /// Event invoked when the registry's contents are validated.
    /// </summary>
    public EventHandler<IRegistry.ValidateEventArgs<TValue>>? ValidateCallback;

    /// <summary>
    /// Event invoked when the registry has finished processing.
    /// </summary>
    public EventHandler<IRegistry.BakeEventArgs<TValue>>? BakeCallback;
}
