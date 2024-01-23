using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using OdinSerializer;
using UnityEngine;
using Unity.Netcode;

namespace Sigurd.Networking;

internal class SNetworkObject : MonoBehaviour
{
    /*
     * This section of code is taken and modified from https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/36368846c5bfe6cfb93adc36282507614955955c/com.unity.netcode.gameobjects/Runtime/Core/NetworkObject.cs
     * in com.unity.netcode.gameobjects, which is released under the MIT License.
     * See file libs/unity-ngo/LICENSE.md or go to https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/LICENSE.md for full license details.
     * Copyright: © 2024 Unity Technologies
     */

    internal readonly Dictionary<SNetworkBehaviourId, SNetworkBehaviour> ChildSNetworkBehaviours = [];

    internal NetworkObject NetworkObject = null!;

    private void Awake()
    {
        NetworkObject = GetComponent<NetworkObject>() ?? throw new Exception(message: $"SNetworkObject was added to Game Object \"{gameObject.name}\" without a NetworkObject component.");
    }

    internal void InvokeBehaviourNetworkSpawn()
    {
        foreach (var networkBehaviour in ChildSNetworkBehaviours)
        {
            if (networkBehaviour.Value.gameObject.activeInHierarchy)
                networkBehaviour.Value.InternalOnNetworkSpawn();
            else
                Debug.LogWarning($"{networkBehaviour.Value.gameObject.name} is disabled! Netcode for GameObjects does not support spawning disabled NetworkBehaviours! The {networkBehaviour.Value.GetType().Name} component was skipped during spawn!");
        }

        foreach (var networkBehaviour in ChildSNetworkBehaviours)
        {
            if (networkBehaviour.Value.gameObject.activeInHierarchy)
                networkBehaviour.Value.VisibleOnNetworkSpawn();
        }
    }

    internal void InvokeBehaviourNetworkDespawn()
    {
        foreach (var networkBehaviour in ChildSNetworkBehaviours)
        {
            networkBehaviour.Value.InternalOnNetworkDespawn();
        }
    }

    internal void WriteNetworkVariableData(FastBufferWriter writer, ulong targetClientId)
    {
        foreach (var networkBehaviour in ChildSNetworkBehaviours)
        {
            var behavior = networkBehaviour.Value;
            behavior.InitializeVariables();
            behavior.WriteNetworkVariableData(writer, targetClientId);
        }
    }

    /// <summary>
    /// Only invoked during first synchronization of a NetworkObject (late join or newly spawned)
    /// </summary>
    internal void SetNetworkVariableData(FastBufferReader reader, ulong clientId)
    {
        foreach (var networkBehaviour in ChildSNetworkBehaviours)
        {
            var behaviour = networkBehaviour.Value;
            behaviour.InitializeVariables();
            behaviour.SetNetworkVariableData(reader, clientId);
        }
    }

    internal SNetworkBehaviourId? GetSNetworkBehaviourOrderIndex(SNetworkBehaviour sNetworkBehaviour)
    {
        var availableSNetworkBehaviourId = ChildSNetworkBehaviours
            .Where(behaviourInfo => behaviourInfo.Value == sNetworkBehaviour)
            .Select(behaviourInfo => behaviourInfo.Key).ToArray();

        return (availableSNetworkBehaviourId.Any()) ? availableSNetworkBehaviourId.First() : null;
    }

    internal SNetworkBehaviour GetSNetworkBehaviourAtOrderIndex(SNetworkBehaviourId sNetworkBehaviourId)
    {
        var availableSNetworkBehaviours = ChildSNetworkBehaviours
            .Where(behaviourInfo => behaviourInfo.Key.ModGuid == sNetworkBehaviourId.ModGuid)
            .Select(behaviourInfo => behaviourInfo.Key).ToArray();

        return (sNetworkBehaviourId.BehaviourId < availableSNetworkBehaviours.Count()) ? ChildSNetworkBehaviours[sNetworkBehaviourId] : null!;
    }

    internal void PostNetworkVariableWrite()
    {
        foreach (var networkBehaviour in ChildSNetworkBehaviours)
        {
            networkBehaviour.Value.PostNetworkVariableWrite();
        }
    }

    /// <summary>
    /// Handles synchronizing NetworkVariables and custom synchronization data for NetworkBehaviours.
    /// </summary>
    /// <remarks>
    /// This is where we determine how much data is written after the associated NetworkObject in order to recover
    /// from a failed instantiated NetworkObject without completely disrupting client synchronization.
    /// </remarks>
    internal void SynchronizeNetworkBehaviours<T>(ref BufferSerializer<T> serializer, ulong targetClientId = 0) where T : IReaderWriter
    {
        if (serializer.IsWriter)
        {
            var writer = serializer.GetFastBufferWriter();
            var positionBeforeSynchronizing = writer.Position;
            writer.WriteValueSafe((ushort)0);
            var sizeToSkipCalculationPosition = writer.Position;

            // Synchronize NetworkVariables
            WriteNetworkVariableData(writer, targetClientId);
            // Reserve the NetworkBehaviour synchronization count position
            var networkBehaviourCountPosition = writer.Position;
            writer.WriteValueSafe((byte)0);

            // Parse through all NetworkBehaviours and any that return true
            // had additional synchronization data written.
            // (See notes for reading/deserialization below)
            var synchronizationCount = (byte)0;
            foreach (var childBehaviour in ChildSNetworkBehaviours)
            {
                if (childBehaviour.Value.Synchronize(ref serializer, targetClientId))
                {
                    synchronizationCount++;
                }
            }

            var currentPosition = writer.Position;
            // Write the total number of bytes written for NetworkVariable and NetworkBehaviour
            // synchronization.
            writer.Seek(positionBeforeSynchronizing);
            // We want the size of everything after our size to skip calculation position
            var size = (ushort)(currentPosition - sizeToSkipCalculationPosition);
            writer.WriteValueSafe(size);
            // Write the number of NetworkBehaviours synchronized
            writer.Seek(networkBehaviourCountPosition);
            writer.WriteValueSafe(synchronizationCount);
            // seek back to the position after writing NetworkVariable and NetworkBehaviour
            // synchronization data.
            writer.Seek(currentPosition);
        }
        else
        {
            var reader = serializer.GetFastBufferReader();

            reader.ReadValueSafe(out ushort sizeOfSynchronizationData);
            var seekToEndOfSynchData = reader.Position + sizeOfSynchronizationData;
            // Apply the network variable synchronization data
            SetNetworkVariableData(reader, targetClientId);
            // Read the number of NetworkBehaviours to synchronize
            reader.ReadValueSafe(out byte numberSynchronized);
            byte[] networkBehaviourId = [];

            // If a SNetworkBehaviour writes synchronization data, it will first
            // write its SNetworkBehaviourId so when deserializing the client-side
            // can find the right SNetworkBehaviour to deserialize the synchronization data.
            for (int i = 0; i < numberSynchronized; i++)
            {
                serializer.SerializeValue(ref networkBehaviourId);
                var networkBehaviour = GetSNetworkBehaviourAtOrderIndex(SerializationUtility.DeserializeValue<SNetworkBehaviourId>(networkBehaviourId, DataFormat.Binary));
                networkBehaviour.Synchronize(ref serializer, targetClientId);
            }
        }
    }

    internal void OnSNetworkBehaviourDestroyed(SNetworkBehaviour sNetworkBehaviour)
    {
        if (!sNetworkBehaviour.IsSpawned || !NetworkObject.IsSpawned)
            return;

        if (sNetworkBehaviour.NetworkManager.LogLevel == LogLevel.Developer)
            Plugin.Log.LogWarning("SNetworkBehaviour-" + sNetworkBehaviour.name + " is being destroyed while NetworkObject-" + NetworkObject.name + " is still spawned! (could break state synchronization)");

        ChildSNetworkBehaviours.Remove(sNetworkBehaviour.SNetworkBehaviourId);
    }
}

[HarmonyPatch(typeof(NetworkObject))]
[HarmonyPriority(Priority.VeryHigh)]
[HarmonyWrapSafe]
internal class NetworkObjectPatches
{
    [HarmonyPatch(nameof(NetworkObject.InvokeBehaviourNetworkSpawn))]
    private static void InvokeBehaviourNetworkSpawn(NetworkObject __instance)
    {
        __instance.GetComponent<SNetworkObject>().InvokeBehaviourNetworkSpawn();
    }

    [HarmonyPatch(nameof(NetworkObject.InvokeBehaviourNetworkDespawn))]
    private static void InvokeBehaviourNetworkDespawn(NetworkObject __instance)
    {
        __instance.GetComponent<SNetworkObject>().InvokeBehaviourNetworkDespawn();
    }
}
