using System.Collections.Generic;
using HarmonyLib;
using Unity.Netcode;

namespace Sigurd.Networking;

/// <summary>
/// An helper class that helps NetworkManager update NetworkBehaviours and replicate them down to connected clients.
/// </summary>
public class SNetworkBehaviourUpdater
{
    /*
     * This section of code is taken and modified from https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/36368846c5bfe6cfb93adc36282507614955955c/com.unity.netcode.gameobjects/Runtime/Core/NetworkBehaviourUpdater.cs
     * in com.unity.netcode.gameobjects, which is released under the MIT License.
     * See file libs/unity-ngo/LICENSE.md or go to https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/LICENSE.md for full license details.
     * Copyright: © 2024 Unity Technologies
     */

    private NetworkManager _networkManager = null!;
    private NetworkConnectionManager _connectionManager = null!;
    private readonly HashSet<SNetworkObject> _dirtyNetworkObjects = [];

    internal void AddForUpdate(SNetworkObject networkObject)
    {
        _dirtyNetworkObjects.Add(networkObject);
    }

    internal void NetworkBehaviourUpdate()
    {
        // NetworkObject references can become null, when hidden or despawned. Once NUll, there is no point
        // trying to process them, even if they were previously marked as dirty.
        _dirtyNetworkObjects.RemoveWhere((sobj) => sobj == null);

        if (_connectionManager.LocalClient.IsServer)
        {
            foreach (var dirtyObj in _dirtyNetworkObjects)
            {
                foreach (var networkBehaviour in dirtyObj.ChildSNetworkBehaviours)
                {
                    networkBehaviour.Value.PreVariableUpdate();
                }

                for (int i = 0; i < _connectionManager.ConnectedClientsList.Count; i++)
                {
                    var client = _connectionManager.ConnectedClientsList[i];

                    if (dirtyObj.NetworkObject.IsNetworkVisibleTo(client.ClientId))
                    {
                        // Sync just the variables for just the objects this client sees
                        foreach (var networkBehaviour in dirtyObj.ChildSNetworkBehaviours)
                        {
                            networkBehaviour.Value.VariableUpdate(client.ClientId);
                        }
                    }
                }
            }
        }
        else
        {
            // when client updates the server, it tells it about all its objects
            foreach (var sobj in _dirtyNetworkObjects)
            {
                if (sobj.NetworkObject.IsOwner)
                {
                    foreach (var networkBehaviour in sobj.ChildSNetworkBehaviours)
                    {
                        networkBehaviour.Value.PreVariableUpdate();
                    }
                    foreach (var networkBehaviour in sobj.ChildSNetworkBehaviours)
                    {
                        networkBehaviour.Value.VariableUpdate(NetworkManager.ServerClientId);
                    }
                }
            }
        }

        foreach (var dirtyObj in _dirtyNetworkObjects)
        {
            foreach (var networkBehaviour in dirtyObj.ChildSNetworkBehaviours)
            {
                var behaviour = networkBehaviour.Value;
                for (int i = 0; i < behaviour.NetworkVariableFields.Count; i++)
                {
                    if (behaviour.NetworkVariableFields[i].IsDirty() &&
                        !behaviour.NetworkVariableIndexesToResetSet.Contains(i))
                    {
                        behaviour.NetworkVariableIndexesToResetSet.Add(i);
                        behaviour.NetworkVariableIndexesToReset.Add(i);
                    }
                }
            }
        }
        // Now, reset all the no-longer-dirty variables
        foreach (var dirtyobj in _dirtyNetworkObjects)
        {
            dirtyobj.PostNetworkVariableWrite();
        }
        _dirtyNetworkObjects.Clear();
    }

    internal void Initialize(NetworkManager networkManager)
    {
        _networkManager = networkManager;
        _connectionManager = networkManager.ConnectionManager;
        _networkManager.NetworkTickSystem.Tick += NetworkBehaviourUpdater_Tick;
    }

    internal void Shutdown()
    {
        _networkManager.NetworkTickSystem.Tick -= NetworkBehaviourUpdater_Tick;
    }

    // Order of operations requires NetworkVariable updates first then showing NetworkObjects
    private void NetworkBehaviourUpdater_Tick()
    {
        // First update NetworkVariables
        NetworkBehaviourUpdate();
    }
}
