using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Unity.Netcode;
using OdinSerializer;

namespace Sigurd.Networking;

/// <summary>
/// Base class for Network Variables.
/// </summary>
/// <remarks>For internal purposes only.</remarks>
public abstract class SNetworkVariableBase
{
    internal readonly string UniqueName;

    private protected static bool CurrentlyConnected;
    private protected bool MadeDuringConnection { get; }

    private static Dictionary<string, SNetworkVariableBase> StoredMenuVariables { get; } = [];
    internal static Dictionary<string, SNetworkVariableBase> StoredLobbyVariables { get; } = [];

    internal SNetworkVariableBase(string uniqueName)
    {
        if (StoredMenuVariables.ContainsKey(uniqueName) || StoredLobbyVariables.ContainsKey(uniqueName))
            throw new UniqueNameException($"{uniqueName} already registered");

        // Add ".var" to unique name to help prevent conflicting names
        UniqueName = $"{uniqueName}.var";

        if (CurrentlyConnected)
        {
            MadeDuringConnection = true;
            StoredLobbyVariables.Add(uniqueName, this);
        }
        else
            StoredMenuVariables.Add(uniqueName, this);
    }

    internal static void RegisterVariables()
    {
        StoredMenuVariables.Do(variable => variable.Value.RegisterVariable());
        CurrentlyConnected = true;
    }

    internal static void UnregisterVariables()
    {
        CurrentlyConnected = false;
        StoredMenuVariables.Do(variable => variable.Value.UnregisterVariable());
        StoredLobbyVariables.Do(variable => variable.Value.UnregisterVariable());

        StoredLobbyVariables.Clear();
    }

    protected abstract void RegisterVariable();
    protected abstract void UnregisterVariable();
}

/// <typeparam name="T">The serializable data type of the message.</typeparam>
// ReSharper disable once ClassNeverInstantiated.Global
public class SNetworkVariable<T> : SNetworkVariableBase
{
    private bool _isDirty;
    private T _value = default!;
    private readonly NetworkObject? _ownerObject;

    /// <summary>
    /// Get or set the value of the variable.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            // Check to see if the value is the same (including if it was null before and null now for support for nullable types)
            if (value != null && value.Equals(_value) || value == null && _value == null)
                return;

            if (CurrentlyConnected && !CheckIfOwned()) return;

            // Make dirty and update value
            _isDirty = true;
            _value = value;

            OnValueChanged?.Invoke(_value);
        }
    }

    /// <summary>
    /// The callback to invoke when the variable's value changes.
    /// </summary>
    /// <remarks>Invoked when changed locally and/or over the network.</remarks>
    public event Action<T>? OnValueChanged;

    /// <summary>
    /// Create a new server-owned network variable.
    /// </summary>
    /// <param name="uniqueName">The name of the network message.</param>
    /// <exception cref="UniqueNameException">Thrown when the name is not unique.</exception>
    public SNetworkVariable(string uniqueName) : this(uniqueName, null!) {}

    internal SNetworkVariable(string uniqueName, NetworkObject networkObject) : base(uniqueName)
    {
        _ownerObject = networkObject;

        if (MadeDuringConnection)
            RegisterVariable();
    }

    #region Network Transfer

    // Check if Variable is dirty and send the new value if so.
    private void OnNetworkTick()
    {
        if (!_isDirty) return;

        // Clean the variable now that the new value is being sent over the network (or to prevent constant passing for non-owned vars).
        _isDirty = false;

        Send();
    }

    // Utilize NetworkMessageWrapper for networking purposes
    private void Read(ulong fakeSender, FastBufferReader reader)
    {
        reader.ReadValueSafe(out byte[] data);

        NetworkMessageWrapper wrapped = data.ToObject<NetworkMessageWrapper>()!;

        if (StartOfRound.Instance.localPlayerController.actualClientId == wrapped.Sender) return;

        // Ensure that the variable is changed by the right client
        if (!CheckIfOwned(wrapped.Sender)) return;

        var newValue = SerializationUtility.DeserializeValue<T>(wrapped.Message, DataFormat.Binary);

        // Check to see if the value is the same (including if it was null before and null now for support for nullable types)
        if (newValue != null && newValue.Equals(_value) || newValue == null && _value == null)
            return;

        _value = newValue;

        OnValueChanged?.Invoke(_value);
    }

    // Utilize NetworkMessageWrapper for networking purposes
    private void Send()
    {
        if (!CheckIfOwned()) return;

        NetworkMessageWrapper wrapped = new NetworkMessageWrapper(UniqueName, NetworkManager.Singleton.LocalClientId);
        byte[] serialized = wrapped.ToBytes();

        using FastBufferWriter writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(serialized), Unity.Collections.Allocator.Temp);

        writer.WriteValueSafe(serialized);

        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(UniqueName, writer, NetworkDelivery.ReliableFragmentedSequenced);
        }
        else
        {
            //TODO: Add Client-Owned Network Variables
        }
    }

    #endregion

    protected sealed override void RegisterVariable()
    {
        var networkManager = NetworkManager.Singleton;

        networkManager.CustomMessagingManager.RegisterNamedMessageHandler(UniqueName, Read);
        networkManager.NetworkTickSystem.Tick += OnNetworkTick;

        if (networkManager.IsServer || networkManager.IsHost)
            networkManager.OnClientConnectedCallback += _ => { OnNetworkTick(); };
    }

    protected override void UnregisterVariable()
    {
        var networkManager = NetworkManager.Singleton;

        networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(UniqueName);
        networkManager.NetworkTickSystem.Tick -= OnNetworkTick;

        if (MadeDuringConnection)
        {
            // TODO: Write destruction code
        }
    }

    /// <summary>
    /// Check to see if the variable is owned by the local client, or by a specific client if specified.
    /// </summary>
    /// <param name="sender">The client the message is received from</param>
    /// <returns>(<see cref="bool"/>) If the variable is owned.</returns>
    private bool CheckIfOwned(ulong sender = ulong.MaxValue)
    {
        // Check if current or sent client is Server if unowned
        if (_ownerObject == null && !(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost || sender == NetworkManager.ServerClientId)) return false;

        // Check if current or sent client is the owner if owned
        if (_ownerObject != null && (_ownerObject.OwnerClientId == NetworkManager.Singleton.LocalClientId || _ownerObject.OwnerClientId == sender)) return false;

        return true;
    }
}
