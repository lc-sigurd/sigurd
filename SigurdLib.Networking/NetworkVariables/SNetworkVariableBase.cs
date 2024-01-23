using System.Collections.Generic;
using Unity.Netcode;

namespace Sigurd.Networking.NetworkVariables;

using System;
using UnityEngine;

/// <summary>
/// Interface for network value containers
/// </summary>
public abstract class SNetworkVariableBase : IDisposable
{
    /*
     * This section of code is taken and modified from https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/36368846c5bfe6cfb93adc36282507614955955c/com.unity.netcode.gameobjects/Runtime/NetworkVariable/NetworkVariableBase.cs
     * in com.unity.netcode.gameobjects, which is released under the MIT License.
     * See file libs/unity-ngo/LICENSE.md or go to https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/LICENSE.md for full license details.
     * Copyright: © 2024 Unity Technologies
     */

    /// <summary>
    /// The delivery type (QoS) to send data with
    /// </summary>
    internal const NetworkDelivery Delivery = NetworkDelivery.ReliableFragmentedSequenced;

    /// <summary>
    /// Maintains a link to the associated SNetworkBehaviour
    /// </summary>
    private protected SNetworkBehaviour SNetworkBehaviour;

    public SNetworkBehaviour GetBehaviour()
    {
        return SNetworkBehaviour;
    }

    /// <summary>
    /// Initializes the SNetworkVariable
    /// </summary>
    /// <param name="sNetworkBehaviour">The SNetworkBehaviour the SNetworkVariable belongs to</param>
    public void Initialize(SNetworkBehaviour sNetworkBehaviour)
    {
        SNetworkBehaviour = sNetworkBehaviour;
    }

    /// <summary>
    /// The default read permissions
    /// </summary>
    public const NetworkVariableReadPermission DefaultReadPerm = NetworkVariableReadPermission.Everyone;

    /// <summary>
    /// The default write permissions
    /// </summary>
    public const NetworkVariableWritePermission DefaultWritePerm = NetworkVariableWritePermission.Server;

    /// <summary>
    /// The default constructor for <see cref="SNetworkVariableBase"/> that can be used to create a
    /// custom NetworkVariable.
    /// </summary>
    /// <param name="readPerm">the <see cref="NetworkVariableReadPermission"/> access settings</param>
    /// <param name="writePerm">the <see cref="NetworkVariableWritePermission"/> access settings</param>
    protected SNetworkVariableBase(
        NetworkVariableReadPermission readPerm = DefaultReadPerm,
        NetworkVariableWritePermission writePerm = DefaultWritePerm)
    {
        ReadPerm = readPerm;
        WritePerm = writePerm;
    }

    /// <summary>
    /// The <see cref="_isDirty"/> property is used to determine if the
    /// value of the `NetworkVariable` has changed.
    /// </summary>
    private bool _isDirty;

    /// <summary>
    /// Gets or sets the name of the network variable's instance
    /// (MemberInfo) where it was declared.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The read permission for this var
    /// </summary>
    public readonly NetworkVariableReadPermission ReadPerm;

    /// <summary>
    /// The write permission for this var
    /// </summary>
    public readonly NetworkVariableWritePermission WritePerm;

    /// <summary>
    /// Sets whether or not the variable needs to be delta synced
    /// </summary>
    /// <param name="isDirty">Whether or not the var is dirty</param>
    public virtual void SetDirty(bool isDirty)
    {
        _isDirty = isDirty;

        if (!_isDirty) return;

        if (SNetworkBehaviour == null)
        {
            Plugin.Log.LogWarning($"NetworkVariable is written to, but doesn't know its NetworkBehaviour yet. " +
                                  "Are you modifying a NetworkVariable before the NetworkObject is spawned?");
            return;
        }

        SNetworkBehaviour.SNetworkManager.SBehaviourUpdater.AddForUpdate(SNetworkBehaviour.SNetworkObject);
    }

    /// <summary>
    /// Resets the dirty state and marks the variable as synced / clean
    /// </summary>
    public virtual void ResetDirty()
    {
        _isDirty = false;
    }

    /// <summary>
    /// Gets Whether or not the container is dirty
    /// </summary>
    /// <returns>Whether or not the container is dirty</returns>
    public virtual bool IsDirty()
    {
        return _isDirty;
    }

    /// <summary>
    /// Gets if a specific client has permission to read the var or not
    /// </summary>
    /// <param name="clientId">The client id</param>
    /// <returns>Whether or not the client has permission to read</returns>
    public bool CanClientRead(ulong clientId)
    {
        switch (ReadPerm)
        {
            default:
            case NetworkVariableReadPermission.Everyone:
                return true;
            case NetworkVariableReadPermission.Owner:
                return clientId == SNetworkBehaviour.NetworkObject.OwnerClientId || NetworkManager.ServerClientId == clientId;
        }
    }

    /// <summary>
    /// Gets if a specific client has permission to write the var or not
    /// </summary>
    /// <param name="clientId">The client id</param>
    /// <returns>Whether or not the client has permission to write</returns>
    public bool CanClientWrite(ulong clientId)
    {
        switch (WritePerm)
        {
            default:
            case NetworkVariableWritePermission.Server:
                return clientId == NetworkManager.ServerClientId;
            case NetworkVariableWritePermission.Owner:
                return clientId == SNetworkBehaviour.NetworkObject.OwnerClientId;
        }
    }

    /// <summary>
    /// Returns the ClientId of the owning client
    /// </summary>
    internal ulong OwnerClientId()
    {
        return SNetworkBehaviour.NetworkObject.OwnerClientId;
    }

    /// <summary>
    /// Writes the dirty changes, that is, the changes since the variable was last dirty, to the writer
    /// </summary>
    /// <param name="writer">The stream to write the dirty changes to</param>
    public abstract void WriteDelta(FastBufferWriter writer);

    /// <summary>
    /// Writes the complete state of the variable to the writer
    /// </summary>
    /// <param name="writer">The stream to write the state to</param>
    public abstract void WriteField(FastBufferWriter writer);

    /// <summary>
    /// Reads the complete state from the reader and applies it
    /// </summary>
    /// <param name="reader">The stream to read the state from</param>
    public abstract void ReadField(FastBufferReader reader);

    /// <summary>
    /// Reads delta from the reader and applies them to the internal value
    /// </summary>
    /// <param name="reader">The stream to read the delta from</param>
    /// <param name="keepDirtyDelta">Whether or not the delta should be kept as dirty or consumed</param>
    public abstract void ReadDelta(FastBufferReader reader, bool keepDirtyDelta);

    /// <summary>
    /// Virtual <see cref="IDisposable"/> implementation
    /// </summary>
    public virtual void Dispose()
    {
    }

    /// <summary>
    /// Sees if two values are equal.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Whether the values are equal</returns>
    public static bool Equals<T>(T x, T y)
    {
        return EqualityComparer<T>.Default.Equals(x, y);
    }
}
