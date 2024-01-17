using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.Networking;

/// <summary>
/// Extensions to help with your networking journey.
/// </summary>
public static class NetworkExtensions
{
    /// <summary>
    /// Get a SNetworkVariable with the identifier specific to the NetworkObject. If one doesn't exist, it creates a new one on all clients.
    /// </summary>
    /// <param name="networkBehaviour">The <see cref="NetworkBehaviour"/> to attach the variable to.</param>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable. Specific to the network object.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="Networking.SNetworkVariable{T}"/>) The network variable.</returns>
    /// <remarks>The variable is set to only allow writing by the object's owner client.</remarks>
    public static SNetworkVariable<TData>? CreateSNetworkVariable<TData>(this NetworkBehaviour networkBehaviour, string identifier) => CreateSNetworkVariable<TData>(networkBehaviour.gameObject, identifier, networkBehaviour.NetworkBehaviourId);

    /// <summary>
    /// Get a SNetworkVariable with the identifier specific to the NetworkObject. If one doesn't exist, it creates a new one on all clients.
    /// </summary>
    /// <param name="networkObject">The <see cref="NetworkObject"/> to attach the variable to.</param>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable. Specific to the network object.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="Networking.SNetworkVariable{T}"/>) The network variable.</returns>
    /// <remarks>The variable is set to only allow writing by the object's owner client.</remarks>
    public static SNetworkVariable<TData>? CreateSNetworkVariable<TData>(this NetworkObject networkObject, string identifier) => CreateSNetworkVariable<TData>(networkObject.gameObject, identifier, ushort.MaxValue);

    /// <summary>
    /// Get a SNetworkVariable with the identifier specific to the NetworkObject. If one doesn't exist, it creates a new one on all clients.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to attach the variable to. Only networked objects are permitted.</param>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable. Specific to the network object.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="Networking.SNetworkVariable{T}"/>) The network variable.</returns>
    /// <remarks>The variable is set to only allow writing by the object's owner client.</remarks>
    public static SNetworkVariable<TData>? CreateSNetworkVariable<TData>(this GameObject gameObject, string identifier) => CreateSNetworkVariable<TData>(gameObject, identifier, ushort.MaxValue);

    private static SNetworkVariable<TData>? CreateSNetworkVariable<TData>(GameObject gameObject, string identifier, ushort index)
    {
        if (gameObject.TryGetComponent(out NetworkObject networkObjectComp) == false)
        {
            Plugin.Log.LogError("");
            return null;
        }

        string behaviourId = (index != ushort.MaxValue) ? $".{index}" : "";
        string uniqueName = $"{identifier}.{networkObjectComp.GlobalObjectIdHash}{behaviourId}";

        if (SNetworkVariableBase.StoredLobbyVariables.First(i =>
                i.Value.UniqueName == uniqueName).Value is SNetworkVariable<TData> networkVariable)
            return networkVariable;

        networkVariable = new SNetworkVariable<TData>(uniqueName, networkObjectComp);
        SNetworkVariableBase.StoredLobbyVariables.Add(uniqueName, networkVariable);

        return networkVariable;
    }
}
