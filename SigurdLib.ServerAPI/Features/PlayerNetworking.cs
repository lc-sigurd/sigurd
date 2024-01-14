using GameNetcodeStuff;
using Sigurd.Common.Features;
using Sigurd.ServerAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sigurd.ServerAPI.Features
{
    /// <summary>
    /// Encapsulates a <see cref="PlayerControllerB"/> for earier interacting.
    /// </summary>
    public class PlayerNetworking : NetworkBehaviour
    {
        internal static GameObject PlayerNetworkPrefab { get; set; }

        /// <summary>
        /// Gets a dictionary mapping <see cref="Common.Features.SPlayer"/>'s to their respective <see cref="PlayerNetworking"/>. Even inactive ones. When on a client, this may not contain all inactive players as they will not yet have been linked to a player controller.
        /// </summary>
        public static Dictionary<SPlayer, PlayerNetworking> Dictionary { get; } = new Dictionary<SPlayer, PlayerNetworking>();

        /// <summary>
        /// The related <see cref="Common.Features.SPlayer"/>.
        /// </summary>
        public SPlayer Player { get; private set; }

        /// <summary>
        /// Gets whether or not this <see cref="PlayerNetworking"/> is related to the local player, or if execution is happening on the server.
        /// </summary>
        public bool IsLocalPlayerOrServer => IsLocalPlayer || NetworkManager.Singleton.IsServer;

        internal ClientRpcParams SendToMeParams { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Player"/>'s username.
        /// </summary>
        public string Username
        {
            get
            {
                return Player.Username;
            }
            set
            {
                Player.Username = value;

                if (NetworkManager.Singleton.IsServer)
                {
                    SetPlayerUsernameClientRpc(value);
                }
                else
                {
                    SetPlayerUsernameServerRpc(value);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerUsernameServerRpc(string name, ServerRpcParams @params = default)
        {
            if (@params.Receive.SenderClientId == Player.ClientId)
            {
                SetPlayerUsernameClientRpc(name);
            }
        }

        [ClientRpc]
        private void SetPlayerUsernameClientRpc(string name)
        {
            Player.Username = name;
        }

        /// <summary>
        /// Gets or sets the <see cref="Player"/>'s sprint meter.
        /// </summary>
        /// <exception cref="NoAuthorityException">Thrown when attempting to set position from another client.</exception>
        public float SprintMeter
        {
            get
            {
                return Player.SprintMeter;
            }
            set
            {
                if (!IsLocalPlayerOrServer)
                {
                    throw new NoAuthorityException("Tried to set sprint meter of other client from client.");
                }

                if (NetworkManager.Singleton.IsServer)
                {
                    SetSprintMeterClientRpc(value);
                }
                else
                {
                    Player.SprintMeter = value;
                }
            }
        }

        [ClientRpc]
        private void SetSprintMeterClientRpc(float value)
        {
            if (!NetworkManager.Singleton.IsClient) return;

            Player.SprintMeter = value;
        }

        /// <summary>
        /// Gets the <see cref="PlayerNetworking"/>'s <see cref="PlayerInventoryNetworking"/>.
        /// </summary>
        public PlayerInventoryNetworking InventoryNetworking { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Player"/>'s position.
        /// If you set a <see cref="Common.Features.SPlayer"/>'s position out of bounds, they will be teleported back to a safe location next to the ship or entrance/exit to a dungeon.
        /// </summary>
        /// <exception cref="NoAuthorityException">Thrown when attempting to set position from another client.</exception>
        public Vector3 Position
        {
            get
            {
                return Player.Position;
            }
            set
            {
                if (!IsLocalPlayerOrServer)
                {
                    throw new NoAuthorityException("Tried to set position of client from another client.");
                }

                if (NetworkManager.Singleton.IsServer)
                {
                    TeleportPlayerClientRpc(value);
                }
                else
                {
                    Player.Position = value;
                }
            }
        }

        // UpdatePlayerPositionClientRpc doesn't actually set the player's position, so we need a custom rpc to do so.
        [ClientRpc]
        private void TeleportPlayerClientRpc(Vector3 position)
        {
            if (!NetworkManager.Singleton.IsClient) return;

            Player.Position = position;
        }

        /// <summary>
        /// Gets or sets the <see cref="Player"/>'s euler angles.
        /// </summary>
        /// <exception cref="NoAuthorityException">Thrown when attempting to update euler angles from another client.</exception>
        public Vector3 EulerAngles
        {
            get
            {
                return Player.EulerAngles;
            }
            set
            {
                if (!IsLocalPlayerOrServer)
                {
                    throw new NoAuthorityException("Tried to update euler angles from another client.");
                }

                if (NetworkManager.Singleton.IsServer)
                {
                    Player.PlayerController.UpdatePlayerRotationFullClientRpc(value);
                }
                else
                {
                    Player.EulerAngles = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Player"/>'s rotation. Quaternions can't gimbal lock, but they are harder to understand.
        /// Use <see cref="SPlayer.EulerAngles"/> if you don't know what you're doing.
        /// </summary>
        /// <exception cref="NoAuthorityException">Thrown when attempting to update rotation from a client that isn't the local client, or the host.</exception>
        public Quaternion Rotation
        {
            get
            {
                return Player.Rotation;
            }
            set
            {
                if (!IsLocalPlayerOrServer)
                {
                    throw new NoAuthorityException("Tried to update rotation from other client.");
                }

                if (NetworkManager.Singleton.IsServer)
                {
                    Player.PlayerController.UpdatePlayerRotationFullClientRpc(value.eulerAngles);
                }
                else
                {
                    Player.Rotation = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Player"/>'s health.
        /// </summary>
        /// <exception cref="NoAuthorityException">Thrown when attempting to set health from the client.</exception>
        public int Health
        {
            get
            {
                return Player.Health;
            }
            set
            {
                if (!IsLocalPlayerOrServer)
                {
                    throw new NoAuthorityException("Tried to set health on client.");
                }

                if (NetworkManager.Singleton.IsServer)
                {
                    SetHealthClientRpc(value);
                }
                else
                {
                    Player.Health = value;
                }
            }
        }

        [ClientRpc]
        private void SetHealthClientRpc(int health)
        {
            if (!NetworkManager.Singleton.IsClient) return;

            Player.Health = health;
        }

        /// <summary>
        /// Hurts the <see cref="Player"/>.
        /// </summary>
        /// <param name="damage">The amount of health to take from the <see cref="Player"/>.</param>
        /// <param name="causeOfDeath">The cause of death to show on the end screen.</param>
        /// <param name="bodyVelocity">he velocity to launch the ragdoll at, if killed.</param>
        /// <param name="overrideOneShotProtection">Whether or not to override one shot protection.</param>
        /// <param name="deathAnimation">Which death animation to use.</param>
        /// <param name="fallDamage">Whether or not this should be considered fall damage.</param>
        /// <param name="hasSFX">Whether or not this damage has sfx.</param>
        /// <exception cref="NoAuthorityException">Thrown when attempting to hurt a <see cref="Player"/> that isn't the local <see cref="Player"/>'s, if not the host.</exception>
        public void Hurt(int damage, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, Vector3 bodyVelocity = default, bool overrideOneShotProtection = false, int deathAnimation = 0, bool fallDamage = false, bool hasSFX = true)
        {
            if (!IsLocalPlayerOrServer)
            {
                throw new NoAuthorityException("Tried to set health from another client.");
            }

            if (overrideOneShotProtection && Health - damage <= 0)
            {
                Kill(bodyVelocity, true, causeOfDeath, deathAnimation);
                return;
            }

            if (NetworkManager.Singleton.IsServer)
            {
                HurtPlayerClientRpc(damage, (int)causeOfDeath, bodyVelocity, overrideOneShotProtection, deathAnimation, fallDamage, hasSFX);
            }
            else
            {
                Player.Hurt(damage, causeOfDeath, bodyVelocity, overrideOneShotProtection, deathAnimation, fallDamage, hasSFX);
            }
        }

        private void HurtPlayerClientRpc(int damage, int causeOfDeath, Vector3 bodyVelocity, bool overrideOneShotProtection, int deathAnimation, bool fallDamage, bool hasSFX)
        {
            Player.Hurt(damage, (CauseOfDeath)causeOfDeath, bodyVelocity, overrideOneShotProtection, deathAnimation, fallDamage, hasSFX);
        }

        /// <summary>
        /// Kills the <see cref="Player"/>.
        /// </summary>
        /// <param name="bodyVelocity">The velocity to launch the ragdoll at, if spawned.</param>
        /// <param name="spawnBody">Whether or not to spawn a ragdoll.</param>
        /// <param name="causeOfDeath">The cause of death to show on the end screen.</param>
        /// <param name="deathAnimation">Which death animation to use.</param>
        /// <exception cref="NoAuthorityException">Thrown when attempting to kill a <see cref="Player"/> that isn't the local <see cref="Player"/>'s, if not the host.</exception>
        public void Kill(Vector3 bodyVelocity = default, bool spawnBody = true, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, int deathAnimation = 0)
        {
            if (!IsLocalPlayerOrServer)
            {
                throw new NoAuthorityException("Tried to kill player from another client.");
            }

            if (NetworkManager.Singleton.IsServer)
            {
                Player.PlayerController.KillPlayerClientRpc((int)Player.ClientId, spawnBody, bodyVelocity, (int)causeOfDeath, deathAnimation);
            }
            else
            {
                Player.Kill(bodyVelocity, spawnBody, causeOfDeath, deathAnimation);
            }
        }

        [ClientRpc]
        private void QueueTipClientRpc(string header, string message, float duration, int priority, bool isWarning, bool useSave, string prefsKey, ClientRpcParams clientRpcParams = default)
        {
            Player.QueueTip(header, message, duration, priority, isWarning, useSave, prefsKey);
        }

        [ClientRpc]
        private void ShowTipClientRpc(string header, string message, float duration, bool isWarning, bool useSave, string prefsKey, ClientRpcParams clientRpcParams = default)
        {
            Player.ShowTip(header, message, duration, isWarning, useSave, prefsKey);
        }

        #region Unity related things
        private PlayerControllerB playerController;
        private void Start()
        {
            playerController = GetComponent<PlayerControllerB>();

            Player = SPlayer.Get(playerController);

            if (Player != null)
            {
                if (!Dictionary.ContainsKey(Player)) Dictionary.Add(Player, this);

                SendToMeParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { Player.ClientId }
                    }
                };
            }
        }

        private void Update()
        {
            if (Player == null)
            {
                if (playerController == null && !TryGetComponent(out playerController)) return;

                if (SPlayer.TryGet(playerController, out SPlayer player))
                {
                    Player = player;

                    if (!Dictionary.ContainsKey(Player)) Dictionary.Add(Player, this);

                    SendToMeParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { Player.ClientId }
                        }
                    };
                }
            }
        }
        #endregion

        #region Event replication
        internal void CallHurtingOnOtherClients(int damage, bool hasSFX, CauseOfDeath causeOfDeath,
            int deathAnimation, bool fallDamage, Vector3 force)
        {
            CallHurtingOnOtherClientsServerRpc(damage, hasSFX, (int)causeOfDeath, deathAnimation, fallDamage, force);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CallHurtingOnOtherClientsServerRpc(int damage, bool hasSFX, int causeOfDeath,
            int deathAnimation, bool fallDamage, Vector3 force, ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != Player.ClientId) return;

            CallHurtingOnOtherClientsClientRpc(damage, hasSFX, causeOfDeath, deathAnimation, fallDamage, force);
        }

        [ClientRpc]
        private void CallHurtingOnOtherClientsClientRpc(int damage, bool hasSFX, int causeOfDeath,
            int deathAnimation, bool fallDamage, Vector3 force)
        {
            if (IsLocalPlayer) return;

            Events.Handlers.Player.OnHurting(new Events.EventArgs.Player.HurtingEventArgs(Player, damage, hasSFX,
                (CauseOfDeath)causeOfDeath, deathAnimation, fallDamage, force));
        }

        internal void CallHurtOnOtherClients(int damage, bool hasSFX, CauseOfDeath causeOfDeath,
            int deathAnimation, bool fallDamage, Vector3 force)
        {
            CallHurtOnOtherClientsServerRpc(damage, hasSFX, (int)causeOfDeath, deathAnimation, fallDamage, force);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CallHurtOnOtherClientsServerRpc(int damage, bool hasSFX, int causeOfDeath,
            int deathAnimation, bool fallDamage, Vector3 force, ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != Player.ClientId) return;

            CallHurtOnOtherClientsClientRpc(damage, hasSFX, causeOfDeath, deathAnimation, fallDamage, force);
        }

        [ClientRpc]
        private void CallHurtOnOtherClientsClientRpc(int damage, bool hasSFX, int causeOfDeath,
            int deathAnimation, bool fallDamage, Vector3 force)
        {
            if (IsLocalPlayer) return;

            Events.Handlers.Player.OnHurt(new Events.EventArgs.Player.HurtEventArgs(Player, damage, hasSFX,
                (CauseOfDeath)causeOfDeath, deathAnimation, fallDamage, force));
        }

        internal void CallDroppingItemOnOtherClients(Common.Features.SItem item, bool placeObject, Vector3 targetPosition,
            int floorYRotation, NetworkObject parentObjectTo, bool matchRotationOfParent, bool droppedInShip)
        {
            CallDroppingItemOnOtherClientsServerRpc(item.GrabbableObject.NetworkObjectId, placeObject, targetPosition, floorYRotation, parentObjectTo != null, parentObjectTo != null ? parentObjectTo.NetworkObjectId : 0, matchRotationOfParent, droppedInShip);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CallDroppingItemOnOtherClientsServerRpc(ulong itemNetworkId, bool placeObject, Vector3 targetPosition,
            int floorYRotation, bool hasParent, ulong parentObjectToId, bool matchRotationOfParent, bool droppedInShip,
            ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != Player.ClientId) return;

            CallDroppingItemOnOtherClientsClientRpc(itemNetworkId, placeObject, targetPosition, floorYRotation, hasParent, parentObjectToId, matchRotationOfParent, droppedInShip);
        }

        [ClientRpc]
        private void CallDroppingItemOnOtherClientsClientRpc(ulong itemNetworkId, bool placeObject, Vector3 targetPosition,
            int floorYRotation, bool hasParent, ulong parentObjectToId, bool matchRotationOfParent, bool droppedInShip)
        {
            if (IsLocalPlayer) return;

            Events.Handlers.Player.OnDroppingItem(new Events.EventArgs.Player.DroppingItemEventArgs(Player, Common.Features.SItem.Get(itemNetworkId)!, placeObject, targetPosition, floorYRotation, hasParent ? NetworkManager.Singleton.SpawnManager.SpawnedObjects[parentObjectToId] : null, matchRotationOfParent, droppedInShip));
        }

        internal void CallDroppedItemOnOtherClients(Common.Features.SItem item, bool placeObject, Vector3 targetPosition,
            int floorYRotation, NetworkObject parentObjectTo, bool matchRotationOfParent, bool droppedInShip)
        {
            CallDroppedItemOnOtherClientsServerRpc(item.GrabbableObject.NetworkObjectId, placeObject, targetPosition, floorYRotation, parentObjectTo != null, parentObjectTo != null ? parentObjectTo.NetworkObjectId : 0, matchRotationOfParent, droppedInShip);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CallDroppedItemOnOtherClientsServerRpc(ulong itemNetworkId, bool placeObject, Vector3 targetPosition,
            int floorYRotation, bool hasParent, ulong parentObjectToId, bool matchRotationOfParent, bool droppedInShip,
            ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != Player.ClientId) return;

            CallDroppedItemOnOtherClientsClientRpc(itemNetworkId, placeObject, targetPosition, floorYRotation, hasParent, parentObjectToId, matchRotationOfParent, droppedInShip);
        }

        [ClientRpc]
        private void CallDroppedItemOnOtherClientsClientRpc(ulong itemNetworkId, bool placeObject, Vector3 targetPosition,
            int floorYRotation, bool hasParent, ulong parentObjectToId, bool matchRotationOfParent, bool droppedInShip)
        {
            if (IsLocalPlayer) return;

            Events.Handlers.Player.OnDroppedItem(new Events.EventArgs.Player.DroppedItemEventArgs(Player, Common.Features.SItem.Get(itemNetworkId)!, placeObject, targetPosition, floorYRotation, hasParent ? NetworkManager.Singleton.SpawnManager.SpawnedObjects[parentObjectToId] : null, matchRotationOfParent, droppedInShip));
        }
        #endregion

        public static implicit operator SPlayer(PlayerNetworking playerNetworking) => playerNetworking.Player;

        public static implicit operator PlayerNetworking(SPlayer player) => Get(player);

        #region Player getters
        /// <summary>
        /// Gets or adds a <see cref="PlayerNetworking"/> from a <see cref="Common.Features.SPlayer"/>.
        /// </summary>
        /// <param name="player">The <see cref="Common.Features.SPlayer"/>.</param>
        /// <returns>A <see cref="PlayerNetworking"/>.</returns>
        public static PlayerNetworking GetOrAdd(SPlayer player)
        {
            if (player == null) return null;

            if (TryGet(player, out PlayerNetworking playerNetworking)) return playerNetworking;

            foreach (PlayerNetworking p in FindObjectsOfType<PlayerNetworking>())
            {
                if (p.Player?._clientId == player._clientId)
                {
                    if (!Dictionary.ContainsKey(player)) Dictionary.Add(player, p);

                    return p;
                }
            }

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                GameObject go = Instantiate(PlayerNetworkPrefab, Vector3.zero, default);
                go.SetActive(true);
                PlayerNetworking p = go.GetComponent<PlayerNetworking>();
                p.Player = player;
                go.GetComponent<NetworkObject>().Spawn(false);

                if (!Dictionary.ContainsKey(player)) Dictionary.Add(player, p);

                return p;
            }

            return null;
        }

        /// <summary>
        /// Gets a <see cref="PlayerNetworking"/> from a <see cref="Common.Features.SPlayer"/>.
        /// </summary>
        /// <param name="playerController">The <see cref="Common.Features.SPlayer"/>.</param>
        /// <returns>A <see cref="PlayerNetworking"/> or <see langword="null"/> if not found.</returns>
        public static PlayerNetworking Get(SPlayer playerController)
        {
            if (playerController == null) return null;

            if (Dictionary.TryGetValue(playerController, out PlayerNetworking player)) return player;

            return null;
        }

        /// <summary>
        /// Tries to get a <see cref="PlayerNetworking"/> from a <see cref="Common.Features.SPlayer"/>.
        /// </summary>
        /// <param name="playerController">The <see cref="Common.Features.SPlayer"/>.</param>
        /// <param name="player">Outputs a <see cref="PlayerNetworking"/>, or <see langword="null"/> if not found.</param>
        /// <returns><see langword="true"/> if a <see cref="Common.Features.SPlayer"/> is found, <see langword="false"/> otherwise.</returns>
        public static bool TryGet(SPlayer playerController, out PlayerNetworking player)
        {
            return Dictionary.TryGetValue(playerController, out player);
        }

        /// <summary>
        /// Gets a <see cref="PlayerNetworking"/> from a <see cref="PlayerControllerB"/>.
        /// </summary>
        /// <param name="playerController">The <see cref="Common.Features.SPlayer"/>.</param>
        /// <returns>A <see cref="PlayerNetworking"/> or <see langword="null"/> if not found.</returns>
        public static PlayerNetworking Get(PlayerControllerB playerController)
        {
            if (playerController == null) return null;

            if (SPlayer.TryGet(playerController, out SPlayer p)) return p;

            return null;
        }

        /// <summary>
        /// Tries to get a <see cref="PlayerNetworking"/> from a <see cref="Common.Features.SPlayer"/>.
        /// </summary>
        /// <param name="playerController">The <see cref="Common.Features.SPlayer"/>.</param>
        /// <param name="player">Outputs a <see cref="PlayerNetworking"/>, or <see langword="null"/> if not found.</param>
        /// <returns><see langword="true"/> if a <see cref="Common.Features.SPlayer"/> is found, <see langword="false"/> otherwise.</returns>
        public static bool TryGet(PlayerControllerB playerController, out PlayerNetworking player)
        {
            return (player = Get(playerController)) != null;
        }

        /// <summary>
        /// Gets a <see cref="PlayerNetworking"/> from a <see cref="Common.Features.SPlayer"/>'s client id.
        /// </summary>
        /// <param name="clientId">The player's client id.</param>
        /// <returns>A <see cref="Player"/> or <see langword="null"/> if not found.</returns>
        public static PlayerNetworking Get(ulong clientId)
        {
            return Get(SPlayer.List.FirstOrDefault(p => p.ClientId == clientId));
        }

        /// <summary>
        /// Tries to get a <see cref="PlayerNetworking"/> from a <see cref="Common.Features.SPlayer"/>'s client id.
        /// </summary>
        /// <param name="clientId">The player's client id.</param>
        /// <param name="player">Outputs a <see cref="Common.Features.SPlayer"/>, or <see langword="null"/> if not found.</param>
        /// <returns><see langword="true"/> if a <see cref="PlayerNetworking"/> is found, <see langword="false"/> otherwise.</returns>
        public static bool TryGet(ulong clientId, out PlayerNetworking player)
        {
            return (player = Get(clientId)) != null;
        }
        #endregion

        /// <summary>
        /// Encapsulates a <see cref="Player"/>'s inventory to provide useful tools to it.
        /// </summary>
        public class PlayerInventoryNetworking : NetworkBehaviour
        {
            /// <summary>
            /// Gets the <see cref="Common.Features.SPlayer"/> that this <see cref="PlayerInventoryNetworking"/> belongs to.
            /// </summary>
            public SPlayer Player { get; private set; }

            /// <summary>
            /// Gets the <see cref="Player"/>'s items in order.
            /// </summary>
            /// TODO: I'm not sure how feasible it is to get this to work in any other way, but I hate this.
            public Common.Features.SItem?[] Items => Player.PlayerController.ItemSlots.Select(i => i != null ? Common.Features.SItem.Dictionary[i] : null).ToArray();

            /// <summary>
            /// Gets the <see cref="Player"/>'s current item slot.
            /// </summary>
            public int CurrentSlot
            {
                get
                {
                    return Player.PlayerController.currentItemSlot;
                }
                set
                {
                    if (Player.IsLocalPlayer) SetSlotServerRpc(value);
                    else if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) SetSlotClientRpc(value);
                }
            }

            [ServerRpc(RequireOwnership = false)]
            private void SetSlotServerRpc(int slot, ServerRpcParams serverRpcParams = default)
            {
                if (serverRpcParams.Receive.SenderClientId != Player.ClientId) return;

                SetSlotClientRpc(slot);
            }

            [ClientRpc]
            private void SetSlotClientRpc(int slot)
            {
                Player.PlayerController.SwitchToItemSlot(slot);
            }

            /// <summary>
            /// Gets the first empty item slot, or -1 if there are none available.
            /// </summary>
            /// <returns></returns>
            public int GetFirstEmptySlot()
            {
                return Player.PlayerController.FirstEmptyItemSlot();
            }

            /// <summary>
            /// Tries to get the first empty item slot.
            /// </summary>
            /// <param name="slot">Outputs the empty item slot.</param>
            /// <returns><see langword="true"/> if there's an available slot, <see langword="false"/> otherwise.</returns>
            public bool TryGetFirstEmptySlot(out int slot)
            {
                slot = Player.PlayerController.FirstEmptyItemSlot();
                return slot != -1;
            }

            /// <summary>
            /// Tries to add an <see cref="Common.Features.SItem"/> to the inventory in the first available slot.
            /// </summary>
            /// <param name="item">The item to try to add.</param>
            /// <param name="switchTo">Whether or not to switch to this item after adding.</param>
            /// <returns><see langword="true"/> if added <see langword="false"/> otherwise.</returns>
            /// <exception cref="NoAuthorityException">Thrown when trying to add item from the client.</exception>
            public bool TryAddItem(Common.Features.SItem item, bool switchTo = true)
            {
                if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
                {
                    throw new NoAuthorityException("Tried to add item from client.");
                }

                if (TryGetFirstEmptySlot(out int slot))
                {
                    ItemNetworking itemNetworking = ItemNetworking.Get(item)!;

                    if (item.IsTwoHanded && !Player.HasFreeHands)
                    {
                        return false;
                    }

                    if (item.IsHeld)
                    {
                        itemNetworking.RemoveFromHolder();
                    }

                    itemNetworking.NetworkObject.ChangeOwnership(Player.ClientId);

                    if (item.IsTwoHanded)
                    {
                        SetSlotAndItemClientRpc(slot, itemNetworking.NetworkObjectId);
                    }
                    else
                    {
                        if (switchTo && Player.HasFreeHands)
                        {
                            SetSlotAndItemClientRpc(slot, itemNetworking.NetworkObjectId);
                        }
                        else
                        {
                            if (Player.PlayerController.currentItemSlot == slot)
                                SetSlotAndItemClientRpc(slot, itemNetworking.NetworkObjectId);
                            else
                                SetItemInSlotClientRpc(slot, itemNetworking.NetworkObjectId);
                        }
                    }

                    return true;
                }

                return false;
            }

            /// <summary>
            /// Tries to add an <see cref="Common.Features.SItem"/> to the inventory in a specific slot.
            /// </summary>
            /// <param name="item">The item to try to add.</param>
            /// <param name="slot">The slot to try to add the item to.</param>
            /// <param name="switchTo">Whether or not to switch to this item after adding.</param>
            /// <returns><see langword="true"/> if added <see langword="false"/> otherwise.</returns>
            /// <exception cref="NoAuthorityException">Thrown when trying to add item from the client.</exception>
            public bool TryAddItemToSlot(Common.Features.SItem item, int slot, bool switchTo = true)
            {
                if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
                {
                    throw new NoAuthorityException("Tried to add item from client.");
                }

                if (slot < Player.PlayerController.ItemSlots.Length && Player.PlayerController.ItemSlots[slot] == null)
                {
                    ItemNetworking itemNetworking = ItemNetworking.Get(item)!;

                    if (item.IsTwoHanded && !Player.HasFreeHands)
                    {
                        return false;
                    }

                    if (item.IsHeld)
                    {
                        itemNetworking.RemoveFromHolder();
                    }

                    itemNetworking.NetworkObject.ChangeOwnership(Player.ClientId);

                    if (item.IsTwoHanded)
                    {
                        SetSlotAndItemClientRpc(slot, itemNetworking.NetworkObjectId);
                    }
                    else
                    {
                        if (switchTo && Player.HasFreeHands)
                        {
                            SetSlotAndItemClientRpc(slot, itemNetworking.NetworkObjectId);
                        }
                        else
                        {
                            if (Player.PlayerController.currentItemSlot == slot)
                                SetSlotAndItemClientRpc(slot, itemNetworking.NetworkObjectId);
                            else
                                SetItemInSlotClientRpc(slot, itemNetworking.NetworkObjectId);
                        }
                    }

                    return true;
                }

                return false;
            }

            [ClientRpc]
            private void SetItemInSlotClientRpc(int slot, ulong itemId)
            {
                Common.Features.SItem item = Common.Features.SItem.List.FirstOrDefault(i => i.GrabbableObject.NetworkObjectId == itemId);

                if (item != null)
                {
                    if (Player.IsLocalPlayer)
                    {
                        HUDManager.Instance.itemSlotIcons[slot].sprite = item.ItemProperties.itemIcon;
                        HUDManager.Instance.itemSlotIcons[slot].enabled = true;
                    }

                    item.GrabbableObject.EnablePhysics(false);
                    item.GrabbableObject.EnableItemMeshes(false);
                    item.GrabbableObject.playerHeldBy = Player.PlayerController;
                    item.GrabbableObject.hasHitGround = false;
                    item.GrabbableObject.isInFactory = Player.IsInFactory;

                    Player.CarryWeight += Mathf.Clamp(item.ItemProperties.weight - 1f, 0f, 10f);

                    if (!Player.IsLocalPlayer)
                    {
                        item.GrabbableObject.parentObject = Player.PlayerController.serverItemHolder;
                    }
                    else
                    {
                        item.GrabbableObject.parentObject = Player.PlayerController.localItemHolder;
                    }

                    Player.PlayerController.ItemSlots[slot] = item.GrabbableObject;
                }
            }

            [ClientRpc]
            private void SetSlotAndItemClientRpc(int slot, ulong itemId)
            {
                Common.Features.SItem item = Common.Features.SItem.List.FirstOrDefault(i => i.GrabbableObject.NetworkObjectId == itemId);

                if (item != null)
                {
                    Player.PlayerController.SwitchToItemSlot(slot, item.GrabbableObject);

                    item.GrabbableObject.EnablePhysics(false);
                    item.GrabbableObject.isHeld = true;
                    item.GrabbableObject.hasHitGround = false;
                    item.GrabbableObject.isInFactory = Player.IsInFactory;

                    Player.PlayerController.twoHanded = item.ItemProperties.twoHanded;
                    Player.PlayerController.twoHandedAnimation = item.ItemProperties.twoHandedAnimation;
                    Player.PlayerController.isHoldingObject = true;
                    Player.CarryWeight += Mathf.Clamp(item.ItemProperties.weight - 1f, 0f, 10f);

                    if (!Player.IsLocalPlayer)
                    {
                        item.GrabbableObject.parentObject = Player.PlayerController.serverItemHolder;
                    }
                    else
                    {
                        item.GrabbableObject.parentObject = Player.PlayerController.localItemHolder;
                    }
                }
            }

            /// <summary>
            /// Removes an <see cref="Item"/> from the <see cref="Player"/>'s inventory at the current slot.
            /// </summary>
            /// <param name="slot"></param>
            public void RemoveItem(int slot)
            {
                if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) RemoveItemClientRpc(slot);
                else RemoveItemServerRpc(slot);
            }

            [ServerRpc(RequireOwnership = false)]
            private void RemoveItemServerRpc(int slot, ServerRpcParams serverRpcParams = default)
            {
                if (serverRpcParams.Receive.SenderClientId != Player.ClientId) return;

                RemoveItemClientRpc(slot);
            }

            [ClientRpc]
            private void RemoveItemClientRpc(int slot)
            {
                if (slot != -1)
                {
                    bool currentlyHeldOut = slot == Player.Inventory.CurrentSlot;
                    Common.Features.SItem item = Items[slot];

                    if (item == null) return;

                    GrabbableObject grabbable = item.GrabbableObject;

                    if (Player.IsLocalPlayer)
                    {
                        HUDManager.Instance.itemSlotIcons[slot].enabled = false;

                        if (item.IsTwoHanded) HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                    }

                    if (currentlyHeldOut)
                    {
                        if (Player.IsLocalPlayer)
                        {
                            grabbable.DiscardItemOnClient();
                        }
                        else
                        {
                            grabbable.DiscardItem();
                        }

                        Player.PlayerController.currentlyHeldObject = null;
                        Player.PlayerController.currentlyHeldObjectServer = null;
                        Player.PlayerController.isHoldingObject = false;

                        if (item.IsTwoHanded)
                        {
                            Player.PlayerController.twoHanded = false;
                            Player.PlayerController.twoHandedAnimation = false;
                        }
                    }

                    grabbable.heldByPlayerOnServer = false;
                    grabbable.parentObject = null;
                    item.EnablePhysics(false);
                    item.EnableMeshes(true);
                    item.Scale = item.GrabbableObject.originalScale;

                    if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                    {
                        item.Position = Vector3.zero;
                    }

                    grabbable.isHeld = false;
                    grabbable.isPocketed = false;

                    Player.CarryWeight -= Mathf.Clamp(item.ItemProperties.weight - 1f, 0f, 10f);

                    Player.PlayerController.ItemSlots[slot] = null;
                }
            }

            /// <summary>
            /// Removes an <see cref="Item"/> from the inventory. This should be called on all clients from a client rpc.
            /// </summary>
            /// <param name="item">The <see cref="Item"/> to remove.</param>
            public void RemoveItem(Common.Features.SItem item)
            {
                RemoveItem(Array.IndexOf(Player.PlayerController.ItemSlots, item.GrabbableObject));
            }

            /// <summary>
            /// Removes all <see cref="Item"/>s from the <see cref="Player"/>'s inventory.
            /// </summary>
            public void RemoveAllItems()
            {
                for (int i = 0; i < Player.PlayerController.ItemSlots.Length; i++)
                {
                    RemoveItem(i);
                }
            }

            private void Awake()
            {
                Player = GetComponent<SPlayer>();
            }
        }
    }
}
