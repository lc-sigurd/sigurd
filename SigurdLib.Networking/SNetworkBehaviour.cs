using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using Sigurd.Networking.Messages;
using Sigurd.Networking.NetworkVariables;
using Unity.Collections;

namespace Sigurd.Networking;


/// <summary>
///
/// </summary>
public struct SNetworkBehaviourId
{
    /// <summary>
    ///
    /// </summary>
    public string ModGuid { get; internal set; }

    /// <summary>
    ///
    /// </summary>
    public ushort BehaviourId { get; internal set; }
}

/// <summary>
///
/// </summary>
public abstract class SNetworkBehaviour : MonoBehaviour
{
    /*
     * This section of code is taken and modified from https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/36368846c5bfe6cfb93adc36282507614955955c/com.unity.netcode.gameobjects/Runtime/Core/NetworkBehaviour.cs
     * in com.unity.netcode.gameobjects, which is released under the MIT License.
     * See file libs/unity-ngo/LICENSE.md or go to https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/LICENSE.md for full license details.
     * Copyright: © 2024 Unity Technologies
     */

    /// <summary>
    /// Gets the NetworkManager that owns this SNetworkBehaviour instance
    /// </summary>
    public NetworkManager NetworkManager => NetworkManager.Singleton;

    /// <summary>
    /// Gets the SNetworkManager that owns this SNetworkBehaviour instance
    /// </summary>
    internal SNetworkManager SNetworkManager => SNetworkManager.Singleton;

    /// <summary>
    /// If a NetworkObject is assigned, it will return whether or not this NetworkObject
    /// is the local player object.  If no NetworkObject is assigned it will always return false.
    /// </summary>
    public bool IsLocalPlayer => NetworkObject.IsLocalPlayer;

    /// <summary>
    /// Gets if the object is owned by the local player or if the object is the local player object
    /// </summary>
    public bool IsOwner => NetworkObject.IsOwner;

    /// <summary>
    /// Gets if we are executing as server
    /// </summary>
    public bool IsServer => NetworkManager.IsServer;

    /// <summary>
    /// Gets if we are executing as client
    /// </summary>
    public bool IsClient => NetworkManager.IsClient;

    /// <summary>
    /// Gets if we are executing as Host, I.E Server and Client
    /// </summary>
    public bool IsHost => NetworkManager.IsHost;

    /// <summary>
    /// Gets Whether or not the object has a owner
    /// </summary>
    public bool IsOwnedByServer => NetworkObject.IsOwnedByServer;

    /// <summary>
    /// Used to determine if it is safe to access NetworkObject and NetworkManager from within a SNetworkBehaviour component
    /// Primarily useful when checking NetworkObject/NetworkManager properties within FixedUpdate
    /// </summary>
    public bool IsSpawned => NetworkObject.IsSpawned;

    /// <summary>
    /// Gets the NetworkObject that owns this SNetworkBehaviour instance
    /// </summary>
    public NetworkObject NetworkObject
    {
        get
        {
            try
            {
                if (_networkObject == null)
                {
                    _networkObject = GetComponentInParent<NetworkObject>();
                }
            }
            catch (Exception)
            {
                return null!;
            }

            // ShutdownInProgress check:
            // This prevents an edge case scenario where the NetworkManager is shutting down but user code
            // in Update and/or in FixedUpdate could still be checking SNetworkBehaviour.NetworkObject directly (i.e. does it exist?)
            // or SNetworkBehaviour.IsSpawned (i.e. to early exit if not spawned) which, in turn, could generate several Warning messages
            // per spawned NetworkObject.  Checking for ShutdownInProgress prevents these unnecessary LogWarning messages.
            // We must check IsSpawned, otherwise a warning will be logged under certain valid conditions (see OnDestroy)
            if (IsSpawned && _networkObject == null && (NetworkManager.Singleton == null || !NetworkManager.Singleton.ShutdownInProgress))
            {
                if (NetworkLog.CurrentLogLevel <= LogLevel.Normal)
                {
                    Plugin.Log.LogWarning($"Could not get {nameof(NetworkObject)} for the {nameof(SNetworkBehaviour)}. Are you missing a {nameof(NetworkObject)} component?");
                }
            }

            return _networkObject!;
        }
    }

    private SNetworkObject _sNetworkObject = null!;

    internal SNetworkObject SNetworkObject
    {
        get
        {
            try
            {
                if (_sNetworkObject == null)
                {
                    _sNetworkObject = _networkObject.GetComponent<SNetworkObject>();
                }
            }
            catch (Exception)
            {
                return null!;
            }

            return _sNetworkObject;
        }
    }

    /// <summary>
    /// Gets whether or not this SNetworkBehaviour instance has a NetworkObject owner.
    /// </summary>
    public bool HasNetworkObject => NetworkObject != null;

    private NetworkObject _networkObject = null!;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    protected internal virtual string __getTypeName() => nameof(SNetworkBehaviour);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Gets the NetworkId of the NetworkObject that owns this SNetworkBehaviour
    /// </summary>
    public ulong NetworkObjectId => NetworkObject.NetworkObjectId;

    /// <summary>
    /// Gets the SNetworkId for this SNetworkBehaviour from the owner NetworkObject
    /// </summary>
    public SNetworkBehaviourId SNetworkBehaviourId { get; internal set; }

    /// <summary>
    /// Returns a the SNetworkBehaviour with a given SNetworkBehaviourId for the current NetworkObject
    /// </summary>
    /// <param name="sNetworkBehaviourId">The <see cref="SNetworkBehaviourId"/> of the target <see cref="SNetworkBehaviour"/></param>
    /// <returns>Returns <see cref="SNetworkBehaviour"/> with given sNetworkBehaviourId</returns>
    protected SNetworkBehaviour GetSNetworkBehaviour(SNetworkBehaviourId sNetworkBehaviourId)
    {
        return SNetworkObject.GetSNetworkBehaviourAtOrderIndex(sNetworkBehaviourId);
    }

    /// <summary>
    /// Returns a the NetworkBehaviour with a given BehaviourId for the current NetworkObject
    /// </summary>
    /// <param name="behaviourId">The behaviourId to return</param>
    /// <returns>Returns SNetworkBehaviour with given behaviourId</returns>
    protected NetworkBehaviour GetNetworkBehaviour(ushort behaviourId)
    {
        return NetworkObject.GetNetworkBehaviourAtOrderIndex(behaviourId);
    }

    /// <summary>
    /// Gets the ClientId that owns the NetworkObject
    /// </summary>
    public ulong OwnerClientId => NetworkObject.OwnerClientId;

    internal void UpdateNetworkProperties()
    {
        // Set NetworkObject dependent properties
        if (NetworkObject != null)
        {
            // This is "OK" because GetNetworkBehaviourOrderIndex uses the order of
            // NetworkObject.ChildNetworkBehaviours which is set once when first
            // accessed.
            SNetworkBehaviourId = (SNetworkBehaviourId)SNetworkObject.GetSNetworkBehaviourOrderIndex(this)!;
        }
        else // Shouldn't happen, but if so then set the properties to their default value;
        {
            SNetworkBehaviourId = default;
        }
    }

    /// <summary>
    /// Gets called when the <see cref="NetworkObject"/> gets spawned, message handlers are ready to be registered and the network is setup.
    /// </summary>
    public virtual void OnNetworkSpawn() { }

    internal void InternalOnNetworkSpawn()
    {
        InitializeVariables();
        UpdateNetworkProperties();
    }

    internal void VisibleOnNetworkSpawn()
    {
        try
        {
            OnNetworkSpawn();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        InitializeVariables();
        if (IsServer)
        {
            // Since we just spawned the object and since user code might have modified their NetworkVariable, esp.
            // NetworkList, we need to mark the object as free of updates.
            // This should happen for all objects on the machine triggering the spawn.
            PostNetworkVariableWrite(true);
        }
    }

    internal void InternalOnNetworkDespawn()
    {
        try
        {
            OnNetworkDespawn();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Gets called when the <see cref="NetworkObject"/> gets de-spawned. Is called both on the server and clients.
    /// </summary>
    public virtual void OnNetworkDespawn() { }

    /// <summary>
    /// Gets called when the local client gains ownership of this object
    /// </summary>
    public virtual void OnGainedOwnership() { }

    /// <summary>
    /// Invoked on all clients, override this method to be notified of any
    /// ownership changes (even if the instance was neither the previous or
    /// newly assigned current owner).
    /// </summary>
    /// <param name="previous">the previous owner</param>
    /// <param name="current">the current owner</param>
    protected virtual void OnOwnershipChanged(ulong previous, ulong current)
    {

    }

    internal void InternalOnOwnershipChanged(ulong previous, ulong current)
    {
        OnOwnershipChanged(previous, current);
    }

    /// <summary>
    /// Gets called when we loose ownership of this object
    /// </summary>
    public virtual void OnLostOwnership() { }

    /// <summary>
    /// Gets called when the parent NetworkObject of this SNetworkBehaviour's NetworkObject has changed
    /// </summary>
    /// <param name="parentNetworkObject">the new <see cref="NetworkObject"/> parent</param>
    public virtual void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject) { }

    private bool _varInit = false;

    private readonly List<HashSet<int>> _deliveryMappedNetworkVariableIndices = [];
    private readonly List<NetworkDelivery> _deliveryTypesForNetworkVariableGroups = [];

    // RuntimeAccessModifiersILPP will make this `protected`
    internal readonly List<SNetworkVariableBase> NetworkVariableFields = [];

#pragma warning disable IDE1006 // disable naming rule violation check
    // RuntimeAccessModifiersILPP will make this `protected`
    internal virtual void __initializeVariables()
#pragma warning restore IDE1006 // restore naming rule violation check
    {
        List<FieldInfo> fields = SNetworkManager.InitializeVariables(this);

        foreach (var field in fields)
        {
            var variable = (SNetworkVariableBase)field.GetValue(this);

            if (variable == null)
                throw new Exception($"{field.DeclaringType}.{field.Name} cannot be null. All {nameof(SNetworkVariableBase)} instances must be initialized.");

            variable.Initialize(this);
            __nameNetworkVariable(variable, field.Name);
            NetworkVariableFields.Add(variable);
        }
    }

#pragma warning disable IDE1006 // disable naming rule violation check
    // RuntimeAccessModifiersILPP will make this `protected`
    // Using this method here because ILPP doesn't seem to let us do visibility modification on properties.
    internal void __nameNetworkVariable(SNetworkVariableBase variable, string varName)
#pragma warning restore IDE1006 // restore naming rule violation check
    {
        variable.Name = varName;
    }

    internal void InitializeVariables()
    {
        if (_varInit)
        {
            return;
        }

        _varInit = true;

        __initializeVariables();

        {
            // Create index map for delivery types
            var firstLevelIndex = new Dictionary<NetworkDelivery, int>();
            var secondLevelCounter = 0;

            for (var i = 0; i < NetworkVariableFields.Count; i++)
            {
                var networkDelivery = SNetworkVariableBase.Delivery;
                if (!firstLevelIndex.ContainsKey(networkDelivery))
                {
                    firstLevelIndex.Add(networkDelivery, secondLevelCounter);
                    _deliveryTypesForNetworkVariableGroups.Add(networkDelivery);
                    secondLevelCounter++;
                }

                if (firstLevelIndex[networkDelivery] >= _deliveryMappedNetworkVariableIndices.Count)
                {
                    _deliveryMappedNetworkVariableIndices.Add(new HashSet<int>());
                }

                _deliveryMappedNetworkVariableIndices[firstLevelIndex[networkDelivery]].Add(i);
            }
        }
    }

    internal void PreNetworkVariableWrite()
    {
        // reset our "which variables got written" data
        NetworkVariableIndexesToReset.Clear();
        NetworkVariableIndexesToResetSet.Clear();
    }

    internal void PostNetworkVariableWrite(bool forced = false)
    {
        if (forced)
        {
            // Mark every variable as no longer dirty. We just spawned the object and whatever the game code did
            // during OnNetworkSpawn has been sent and needs to be cleared
            foreach (var t in NetworkVariableFields)
            {
                t.ResetDirty();
            }
        }
        else
        {
            // mark any variables we wrote as no longer dirty
            foreach (var t in NetworkVariableIndexesToReset)
            {
                NetworkVariableFields[t].ResetDirty();
            }
        }

        MarkVariablesDirty(false);
    }

    internal void PreVariableUpdate()
    {
        if (!_varInit)
        {
            InitializeVariables();
        }

        PreNetworkVariableWrite();
    }

    internal void VariableUpdate(ulong targetClientId)
    {
        NetworkVariableUpdate(targetClientId, SNetworkBehaviourId);
    }

    internal readonly List<int> NetworkVariableIndexesToReset = [];
    internal readonly HashSet<int> NetworkVariableIndexesToResetSet = [];

    private void NetworkVariableUpdate(ulong targetClientId, SNetworkBehaviourId behaviourIndex)
    {
        if (!CouldHaveDirtyNetworkVariables())
        {
            return;
        }

        for (var j = 0; j < _deliveryMappedNetworkVariableIndices.Count; j++)
        {
            var shouldSend = NetworkVariableFields.Any(networkVariable => networkVariable.IsDirty() && networkVariable.CanClientRead(targetClientId));

            if (!shouldSend) continue;

            var message = new SNetworkVariableDeltaMessage
            {
                NetworkObjectId = NetworkObjectId,
                SNetworkBehaviourIndex = behaviourIndex,
                SNetworkBehaviour = this,
                TargetClientId = targetClientId,
                DeliveryMappedNetworkVariableIndex = _deliveryMappedNetworkVariableIndices[j]
            };
            // TODO: Serialization is where the IsDirty flag gets changed.
            // Messages don't get sent from the server to itself, so if we're host and sending to ourselves,
            // we still have to actually serialize the message even though we're not sending it, otherwise
            // the dirty flag doesn't change properly. These two pieces should be decoupled at some point
            // so we don't have to do this serialization work if we're not going to use the result.
            if (IsServer && targetClientId == NetworkManager.ServerClientId)
            {
                var tmpWriter = new FastBufferWriter(NetworkManager.MessageManager.NonFragmentedMessageMaxSize, Allocator.Temp, NetworkManager.MessageManager.FragmentedMessageMaxSize);
                using (tmpWriter)
                {
                    message.Serialize(tmpWriter, message.Version);
                }
            }
            else
            {
                NetworkManager.ConnectionManager.SendMessage(ref message, _deliveryTypesForNetworkVariableGroups[j], targetClientId);
            }
        }
    }

    private bool CouldHaveDirtyNetworkVariables()
    {
        return NetworkVariableFields.Any(i => i.IsDirty());
    }

    internal void MarkVariablesDirty(bool dirty)
    {
        foreach (var t in NetworkVariableFields)
        {
            t.SetDirty(dirty);
        }
    }

    /// <summary>
    /// Synchronizes by setting only the NetworkVariable field values that the client has permission to read.
    /// Note: This is only invoked when first synchronizing a SNetworkBehaviour (i.e. late join or spawned NetworkObject)
    /// </summary>
    /// <remarks>
    /// When NetworkConfig.EnsureNetworkVariableLengthSafety is enabled each NetworkVariable field will be preceded
    /// by the number of bytes written for that specific field.
    /// </remarks>
    internal void WriteNetworkVariableData(FastBufferWriter writer, ulong targetClientId)
    {
        if (NetworkVariableFields.Count == 0)
        {
            return;
        }

        foreach (var t in NetworkVariableFields)
        {
            if (t.CanClientRead(targetClientId))
            {
                if (NetworkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
                {
                    var writePos = writer.Position;
                    // Note: This value can't be packed because we don't know how large it will be in advance
                    // we reserve space for it, then write the data, then come back and fill in the space
                    // to pack here, we'd have to write data to a temporary buffer and copy it in - which
                    // isn't worth possibly saving one byte if and only if the data is less than 63 bytes long...
                    // The way we do packing, any value > 63 in a ushort will use the full 2 bytes to represent.
                    writer.WriteValueSafe((ushort)0);
                    var startPos = writer.Position;
                    t.WriteField(writer);
                    var size = writer.Position - startPos;
                    writer.Seek(writePos);
                    writer.WriteValueSafe((ushort)size);
                    writer.Seek(startPos + size);
                }
                else
                {
                    t.WriteField(writer);
                }
            }
            else // Only if EnsureNetworkVariableLengthSafety, otherwise just skip
            if (NetworkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
            {
                writer.WriteValueSafe((ushort)0);
            }
        }
    }

    /// <summary>
    /// Synchronizes by setting only the NetworkVariable field values that the client has permission to read.
    /// Note: This is only invoked when first synchronizing a SNetworkBehaviour (i.e. late join or spawned NetworkObject)
    /// </summary>
    /// <remarks>
    /// When NetworkConfig.EnsureNetworkVariableLengthSafety is enabled each NetworkVariable field will be preceded
    /// by the number of bytes written for that specific field.
    /// </remarks>
    internal void SetNetworkVariableData(FastBufferReader reader, ulong clientId)
    {
        if (NetworkVariableFields.Count == 0)
        {
            return;
        }

        for (int j = NetworkVariableFields.Count - 1; j >= 0; j--)
        {
            var varSize = (ushort)0;
            var readStartPos = 0;
            if (NetworkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
            {
                reader.ReadValueSafe(out varSize);
                if (varSize == 0)
                {
                    continue;
                }
                readStartPos = reader.Position;
            }
            else // If the client cannot read this field, then skip it
            if (!NetworkVariableFields[j].CanClientRead(clientId))
            {
                continue;
            }

            NetworkVariableFields[j].ReadField(reader);

            if (!NetworkManager.NetworkConfig.EnsureNetworkVariableLengthSafety) continue;

            if (reader.Position > (readStartPos + varSize))
            {
                if (NetworkLog.CurrentLogLevel <= LogLevel.Normal)
                {
                    Plugin.Log.LogWarning($"Var data read too far. {reader.Position - (readStartPos + varSize)} bytes.");
                }

                reader.Seek(readStartPos + varSize);
            }
            else if (reader.Position < (readStartPos + varSize))
            {
                if (NetworkLog.CurrentLogLevel <= LogLevel.Normal)
                {
                    Plugin.Log.LogWarning($"Var data read too little. {(readStartPos + varSize) - reader.Position} bytes.");
                }

                reader.Seek(readStartPos + varSize);
            }
        }
    }

    /// <summary>
    /// Gets the local instance of a object with a given NetworkId
    /// </summary>
    /// <param name="networkId"></param>
    /// <returns></returns>
    protected NetworkObject GetNetworkObject(ulong networkId)
    {
        return (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkId, out NetworkObject networkObject) ? networkObject : null)!;
    }

    /// <summary>
    /// Override this method if your derived SNetworkBehaviour requires custom synchronization data.
    /// Note: Use of this method is only for the initial client synchronization of SNetworkBehaviours
    /// and will increase the payload size for client synchronization and dynamically spawned
    /// <see cref="NetworkObject"/>s.
    /// </summary>
    /// <remarks>
    /// When serializing (writing) this will be invoked during the client synchronization period and
    /// when spawning new NetworkObjects.
    /// When deserializing (reading), this will be invoked prior to the SNetworkBehaviour's associated
    /// NetworkObject being spawned.
    /// </remarks>
    /// <param name="serializer">The serializer to use to read and write the data.</param>
    /// <typeparam name="T">
    /// Either BufferSerializerReader or BufferSerializerWriter, depending whether the serializer
    /// is in read mode or write mode.
    /// </typeparam>
    protected virtual void OnSynchronize<T>(ref BufferSerializer<T> serializer) where T : IReaderWriter
    {

    }

    /// <summary>
    /// The relative client identifier targeted for the serialization of this <see cref="SNetworkBehaviour"/> instance.
    /// </summary>
    /// <remarks>
    /// This value will be set prior to <see cref="OnSynchronize{T}(ref BufferSerializer{T})"/> being invoked.
    /// For writing (server-side), this is useful to know which client will receive the serialized data.
    /// For reading (client-side), this will be the <see cref="NetworkManager.LocalClientId"/>.
    /// When synchronization of this instance is complete, this value will be reset to 0
    /// </remarks>
    protected ulong TargetIdBeingSynchronized { get; private set; }

    /// <summary>
    /// Internal method that determines if a SNetworkBehaviour has additional synchronization data to
    /// be synchronized when first instantiated prior to its associated NetworkObject being spawned.
    /// </summary>
    /// <remarks>
    /// This includes try-catch blocks to recover from exceptions that might occur and continue to
    /// synchronize any remaining SNetworkBehaviours.
    /// </remarks>
    /// <returns>true if it wrote synchronization data and false if it did not</returns>
    internal bool Synchronize<T>(ref BufferSerializer<T> serializer, ulong targetClientId = 0) where T : IReaderWriter
    {
        TargetIdBeingSynchronized = targetClientId;
        if (serializer.IsWriter)
        {
            // Get the writer to handle seeking and determining how many bytes were written
            var writer = serializer.GetFastBufferWriter();
            // Save our position before we attempt to write anything so we can seek back to it (i.e. error occurs)
            var positionBeforeWrite = writer.Position;
            writer.WriteValueSafe(SNetworkBehaviourId.ToBytes());

            // Save our position where we will write the final size being written so we can skip over it in the
            // event an exception occurs when deserializing.
            var sizePosition = writer.Position;
            writer.WriteValueSafe((ushort)0);

            // Save our position before synchronizing to determine how much was written
            var positionBeforeSynchronize = writer.Position;
            var threwException = false;
            try
            {
                OnSynchronize(ref serializer);
            }
            catch (Exception ex)
            {
                threwException = true;
                if (NetworkManager.LogLevel <= LogLevel.Normal)
                {
                    Plugin.Log.LogWarning($"{name} threw an exception during synchronization serialization, this {nameof(SNetworkBehaviour)} is being skipped and will not be synchronized!");
                    if (NetworkManager.LogLevel == LogLevel.Developer)
                    {
                        Plugin.Log.LogError($"{ex.Message}\n {ex.StackTrace}");
                    }
                }
            }
            var finalPosition = writer.Position;

            // Reset before exiting
            TargetIdBeingSynchronized = default;
            // If we wrote nothing then skip writing anything for this SNetworkBehaviour
            if (finalPosition == positionBeforeSynchronize || threwException)
            {
                writer.Seek(positionBeforeWrite);
                return false;
            }

            // Write the number of bytes serialized to handle exceptions on the deserialization side
            var bytesWritten = finalPosition - positionBeforeSynchronize;
            writer.Seek(sizePosition);
            writer.WriteValueSafe((ushort)bytesWritten);
            writer.Seek(finalPosition);

            return true;
        }
        else
        {
            var reader = serializer.GetFastBufferReader();
            // We will always read the expected byte count
            reader.ReadValueSafe(out ushort expectedBytesToRead);

            // Save our position before we begin synchronization deserialization
            var positionBeforeSynchronize = reader.Position;
            var synchronizationError = false;
            try
            {
                // Invoke synchronization
                OnSynchronize(ref serializer);
            }
            catch (Exception ex)
            {
                if (NetworkManager.LogLevel <= LogLevel.Normal)
                {
                    Plugin.Log.LogWarning($"{name} threw an exception during synchronization deserialization, this {nameof(SNetworkBehaviour)} is being skipped and will not be synchronized!");
                    if (NetworkManager.LogLevel == LogLevel.Developer)
                    {
                        Plugin.Log.LogError($"{ex.Message}\n {ex.StackTrace}");
                    }
                }
                synchronizationError = true;
            }

            var totalBytesRead = reader.Position - positionBeforeSynchronize;
            if (totalBytesRead != expectedBytesToRead)
            {
                if (NetworkManager.LogLevel <= LogLevel.Normal)
                {
                    Plugin.Log.LogWarning($"{name} read {totalBytesRead} bytes but was expected to read {expectedBytesToRead} bytes during synchronization deserialization! This {nameof(SNetworkBehaviour)} is being skipped and will not be synchronized!");
                }
                synchronizationError = true;
            }

            // Reset before exiting
            TargetIdBeingSynchronized = default;

            // Skip over the entry if deserialization fails
            if (!synchronizationError) return true;

            var skipToPosition = positionBeforeSynchronize + expectedBytesToRead;
            reader.Seek(skipToPosition);

            return false;
        }
    }

    /// <summary>
    /// Invoked when the <see cref="GameObject"/> the <see cref="SNetworkBehaviour"/> is attached to.
    /// NOTE:  If you override this, you will want to always invoke this base class version of this
    /// <see cref="OnDestroy"/> method!!
    /// </summary>
    public virtual void OnDestroy()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned && IsSpawned)
        {
            // If the associated NetworkObject is still spawned then this
            // SNetworkBehaviour will be removed from the NetworkObject's
            // ChildSNetworkBehaviours list.
            SNetworkObject.OnSNetworkBehaviourDestroyed(this);
        }

        // this seems odd to do here, but in fact especially in tests we can find ourselves
        //  here without having called InitializedVariables, which causes problems if any
        //  of those variables use native containers (e.g. NetworkList) as they won't be
        //  registered here and therefore won't be cleaned up.
        //
        // we should study to understand the initialization patterns
        if (!_varInit)
        {
            InitializeVariables();
        }

        foreach (var t in NetworkVariableFields)
        {
            t.Dispose();
        }
    }

}
