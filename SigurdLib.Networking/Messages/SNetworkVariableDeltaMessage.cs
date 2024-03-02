using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace Sigurd.Networking.Messages;

internal struct SNetworkVariableDeltaMessage : INetworkMessage
{
    /*
     * This section of code is taken and modified from https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/36368846c5bfe6cfb93adc36282507614955955c/com.unity.netcode.gameobjects/Runtime/Messaging/Messages/NetworkVariableDeltaMessage.cs
     * in com.unity.netcode.gameobjects, which is released under the MIT License.
     * See file libs/unity-ngo/LICENSE.md or go to https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/LICENSE.md for full license details.
     * Copyright: © 2024 Unity Technologies
     */

    public int Version => 0;

    public ulong NetworkObjectId;
    public SNetworkBehaviourId SNetworkBehaviourIndex;

    public HashSet<int> DeliveryMappedNetworkVariableIndex;
    public ulong TargetClientId;
    public SNetworkBehaviour SNetworkBehaviour;

    private FastBufferReader _receivedNetworkVariableData;

    public void Serialize(FastBufferWriter writer, int targetVersion)
    {
        if (!writer.TryBeginWrite(FastBufferWriter.GetWriteSize(NetworkObjectId) + FastBufferWriter.GetWriteSize(SNetworkBehaviourIndex.ModGuid) + FastBufferWriter.GetWriteSize(SNetworkBehaviourIndex.BehaviourId)))
        {
            throw new OverflowException($"Not enough space in the buffer to write {nameof(NetworkVariableDeltaMessage)}");
        }

        BytePacker.WriteValueBitPacked(writer, NetworkObjectId);
        BytePacker.WriteValuePacked(writer, SNetworkBehaviourIndex.ModGuid);
        BytePacker.WriteValueBitPacked(writer, SNetworkBehaviourIndex.BehaviourId);

        for (int i = 0; i < SNetworkBehaviour.NetworkVariableFields.Count; i++)
        {
            if (!DeliveryMappedNetworkVariableIndex.Contains(i))
            {
                // This var does not belong to the currently iterating delivery group.
                if (SNetworkBehaviour.NetworkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
                {
                    BytePacker.WriteValueBitPacked(writer, (ushort)0);
                }
                else
                {
                    writer.WriteValueSafe(false);
                }

                continue;
            }

            var startingSize = writer.Length;
            var networkVariable = SNetworkBehaviour.NetworkVariableFields[i];
            var shouldWrite = networkVariable.IsDirty() &&
                              networkVariable.CanClientRead(TargetClientId) &&
                              (SNetworkBehaviour.NetworkManager.IsServer || networkVariable.CanClientWrite(SNetworkBehaviour.NetworkManager.LocalClientId));

            // Prevent the server from writing to the client that owns a given NetworkVariable
            // Allowing the write would send an old value to the client and cause jitter
            if (networkVariable.WritePerm == NetworkVariableWritePermission.Owner &&
                networkVariable.OwnerClientId() == TargetClientId)
            {
                shouldWrite = false;
            }

            // The object containing the behaviour we're about to process is about to be shown to this client
            // As a result, the client will get the fully serialized NetworkVariable and would be confused by
            // an extraneous delta
            if (SNetworkBehaviour.NetworkManager.SpawnManager.ObjectsToShowToClient.ContainsKey(TargetClientId) &&
                SNetworkBehaviour.NetworkManager.SpawnManager.ObjectsToShowToClient[TargetClientId]
                    .Contains(SNetworkBehaviour.NetworkObject))
            {
                shouldWrite = false;
            }

            if (SNetworkBehaviour.NetworkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
            {
                if (!shouldWrite)
                {
                    BytePacker.WriteValueBitPacked(writer, (ushort)0);
                }
            }
            else
            {
                writer.WriteValueSafe(shouldWrite);
            }

            if (shouldWrite)
            {
                if (SNetworkBehaviour.NetworkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
                {
                    var tempWriter = new FastBufferWriter(SNetworkBehaviour.NetworkManager.MessageManager.NonFragmentedMessageMaxSize, Allocator.Temp, SNetworkBehaviour.NetworkManager.MessageManager.FragmentedMessageMaxSize);
                    SNetworkBehaviour.NetworkVariableFields[i].WriteDelta(tempWriter);
                    BytePacker.WriteValueBitPacked(writer, tempWriter.Length);

                    if (!writer.TryBeginWrite(tempWriter.Length))
                    {
                        throw new OverflowException($"Not enough space in the buffer to write {nameof(NetworkVariableDeltaMessage)}");
                    }

                    tempWriter.CopyTo(writer);
                }
                else
                {
                    networkVariable.WriteDelta(writer);
                }
                SNetworkBehaviour.NetworkManager.NetworkMetrics.TrackNetworkVariableDeltaSent(
                    TargetClientId,
                    SNetworkBehaviour.NetworkObject,
                    networkVariable.Name,
                    SNetworkBehaviour.__getTypeName(),
                    writer.Length - startingSize);
            }
        }
    }

    public bool Deserialize(FastBufferReader reader, ref NetworkContext context, int receivedMessageVersion)
    {
        ByteUnpacker.ReadValueBitPacked(reader, out NetworkObjectId);
        ByteUnpacker.ReadValuePacked(reader, out string modGuid);
        ByteUnpacker.ReadValueBitPacked(reader, out ushort behaviourId);

        SNetworkBehaviourIndex = new SNetworkBehaviourId { ModGuid = modGuid, BehaviourId = behaviourId };

        _receivedNetworkVariableData = reader;

        return true;
    }

    public void Handle(ref NetworkContext context)
    {
        var networkManager = (NetworkManager)context.SystemOwner;

        if (networkManager.SpawnManager.SpawnedObjects.TryGetValue(NetworkObjectId, out NetworkObject networkObject))
        {
            var networkBehaviour = networkObject.GetComponent<SNetworkObject>().GetSNetworkBehaviourAtOrderIndex(SNetworkBehaviourIndex);

            if (networkBehaviour == null)
            {
                if (NetworkLog.CurrentLogLevel <= LogLevel.Normal)
                {
                    NetworkLog.LogWarning($"Network variable delta message received for a non-existent behaviour. {nameof(NetworkObjectId)}: {NetworkObjectId}, {nameof(SNetworkBehaviourIndex)}: {SNetworkBehaviourIndex}");
                }
            }
            else
            {
                for (int i = 0; i < networkBehaviour.NetworkVariableFields.Count; i++)
                {
                    int varSize = 0;
                    if (networkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
                    {
                        ByteUnpacker.ReadValueBitPacked(_receivedNetworkVariableData, out varSize);

                        if (varSize == 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        _receivedNetworkVariableData.ReadValueSafe(out bool deltaExists);
                        if (!deltaExists)
                        {
                            continue;
                        }
                    }

                    var networkVariable = networkBehaviour.NetworkVariableFields[i];

                    if (networkManager.IsServer && !networkVariable.CanClientWrite(context.SenderId))
                    {
                        // we are choosing not to fire an exception here, because otherwise a malicious client could use this to crash the server
                        if (networkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
                        {
                            if (NetworkLog.CurrentLogLevel <= LogLevel.Developer)
                            {
                                NetworkLog.LogWarning($"Client wrote to {typeof(NetworkVariable<>).Name} without permission. => {nameof(NetworkObjectId)}: {NetworkObjectId} - {nameof(SNetworkObject.GetSNetworkBehaviourOrderIndex)}(): {networkObject.GetComponent<SNetworkObject>().GetSNetworkBehaviourOrderIndex(networkBehaviour)} - VariableIndex: {i}");
                                NetworkLog.LogError($"[{networkVariable.GetType().Name}]");
                            }

                            _receivedNetworkVariableData.Seek(_receivedNetworkVariableData.Position + varSize);
                            continue;
                        }

                        //This client wrote somewhere they are not allowed. This is critical
                        //We can't just skip this field. Because we don't actually know how to dummy read
                        //That is, we don't know how many bytes to skip. Because the interface doesn't have a
                        //Read that gives us the value. Only a Read that applies the value straight away
                        //A dummy read COULD be added to the interface for this situation, but it's just being too nice.
                        //This is after all a developer fault. A critical error should be fine.
                        // - TwoTen
                        if (NetworkLog.CurrentLogLevel <= LogLevel.Developer)
                        {
                            NetworkLog.LogError($"Client wrote to {typeof(NetworkVariable<>).Name} without permission. No more variables can be read. This is critical. => {nameof(NetworkObjectId)}: {NetworkObjectId} - {nameof(SNetworkObject.GetSNetworkBehaviourOrderIndex)}(): {networkObject.GetComponent<SNetworkObject>().GetSNetworkBehaviourOrderIndex(networkBehaviour)} - VariableIndex: {i}");
                            NetworkLog.LogError($"[{networkVariable.GetType().Name}]");
                        }

                        return;
                    }
                    int readStartPos = _receivedNetworkVariableData.Position;

                    networkVariable.ReadDelta(_receivedNetworkVariableData, networkManager.IsServer);

                    networkManager.NetworkMetrics.TrackNetworkVariableDeltaReceived(
                        context.SenderId,
                        networkObject,
                        networkVariable.Name,
                        networkBehaviour.__getTypeName(),
                        context.MessageSize);

                    if (networkManager.NetworkConfig.EnsureNetworkVariableLengthSafety)
                    {
                        if (_receivedNetworkVariableData.Position > (readStartPos + varSize))
                        {
                            if (NetworkLog.CurrentLogLevel <= LogLevel.Normal)
                            {
                                NetworkLog.LogWarning($"Var delta read too far. {_receivedNetworkVariableData.Position - (readStartPos + varSize)} bytes. => {nameof(NetworkObjectId)}: {NetworkObjectId} - {nameof(SNetworkObject.GetSNetworkBehaviourOrderIndex)}(): {networkObject.GetComponent<SNetworkObject>().GetSNetworkBehaviourOrderIndex(networkBehaviour)} - VariableIndex: {i}");
                            }

                            _receivedNetworkVariableData.Seek(readStartPos + varSize);
                        }
                        else if (_receivedNetworkVariableData.Position < (readStartPos + varSize))
                        {
                            if (NetworkLog.CurrentLogLevel <= LogLevel.Normal)
                            {
                                NetworkLog.LogWarning($"Var delta read too little. {readStartPos + varSize - _receivedNetworkVariableData.Position} bytes. => {nameof(NetworkObjectId)}: {NetworkObjectId} - {nameof(SNetworkObject.GetSNetworkBehaviourOrderIndex)}(): {networkObject.GetComponent<SNetworkObject>().GetSNetworkBehaviourOrderIndex(networkBehaviour)} - VariableIndex: {i}");
                            }

                            _receivedNetworkVariableData.Seek(readStartPos + varSize);
                        }
                    }
                }
            }
        }
        else
        {
            networkManager.DeferredMessageManager.DeferMessage(IDeferredNetworkMessageManager.TriggerType.OnSpawn, NetworkObjectId, _receivedNetworkVariableData, ref context);
        }
    }
}
