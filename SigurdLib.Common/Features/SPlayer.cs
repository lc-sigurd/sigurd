using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

namespace Sigurd.Common.Features;

public class SPlayer : MonoBehaviour
{
    internal static GameObject PlayerPrefab { get; set; }
    /// <summary>
    /// Gets a dictionary containing all <see cref="SPlayer"/>s. Even ones that may no longer be active.
    /// </summary>
    public static Dictionary<PlayerControllerB, SPlayer> Dictionary { get; } = new Dictionary<PlayerControllerB, SPlayer>();

    /// <summary>
    /// Gets a list containing all <see cref="SPlayer"/>s. Even ones that may no longer be active.
    /// </summary>
    public static IReadOnlyCollection<SPlayer> List => Dictionary.Values;

    /// <summary>
    /// Gets a list containing only the currently active <see cref="SPlayer"/>s, dead or alive.
    /// </summary>
    /// TODO: `.Where` is bad. Potentially add and remove from this list as needed with a patch.
    public static IReadOnlyCollection<SPlayer> ActiveList => List.Where(p => p.IsActive).ToList();

    /// <summary>
    /// Gets the local <see cref="SPlayer"/>.
    /// </summary>
    public static SPlayer LocalPlayer { get; set; }

    /// <summary>
    /// Gets the host <see cref="SPlayer"/>.
    /// </summary>
    public static SPlayer HostPlayer { get; set; }

    /// <summary>
    /// Gets the encapsulated <see cref="PlayerControllerB"/>.
    /// </summary>
    public PlayerControllerB PlayerController { get; set; }

    /// <summary>
    /// Gets a <see cref="List{T}"/> of <see cref="Tip"/>s that will show to the player.
    /// </summary>
    public List<Tip> TipQueue { get; internal set; } = new List<Tip>();

    internal Tip? CurrentTip { get; set; }

    internal int NextTipId = int.MinValue;

    public ulong _clientId = ulong.MaxValue;

    /// <summary>
    /// Gets the <see cref="SPlayer"/>'s client id.
    /// </summary>
    public ulong ClientId => PlayerController.actualClientId;

    /// <summary>
    /// Gets the <see cref="SPlayer"/>'s player object id. This should be used when accessing allPlayerScripts, or any other array that's index correlates to a player.
    /// </summary>
    public int PlayerObjectId => StartOfRound.Instance.ClientPlayerList[ClientId];

    /// <summary>
    /// Gets the <see cref="SPlayer"/>'s steam id.
    /// </summary>
    public ulong SteamId => PlayerController.playerSteamId;

    /// <summary>
    /// Gets whether or not the <see cref="SPlayer"/> is the host.
    /// </summary>
    public bool IsHost => PlayerController.gameObject == PlayerController.playersManager.allPlayerObjects[0];

    /// <summary>
    /// Gets whether or not the <see cref="SPlayer"/> is the current local player.
    /// </summary>
    public bool IsLocalPlayer => PlayerController == StartOfRound.Instance.localPlayerController;

    /// <summary>
    /// Gets whether or not the <see cref="SPlayer"/> has a connected user.
    /// </summary>
    public bool IsActive => IsControlled || IsDead;

    /// <summary>
    /// Gets whether or not the <see cref="SPlayer"/> is currently being controlled.
    /// Lethal Company creates PlayerControllers ahead of time, so all of them always exist.
    /// </summary>
    public bool IsControlled => PlayerController.isPlayerControlled;

    /// <summary>
    /// Gets whether or not the <see cref="SPlayer"/> is currently dead.
    /// Due to the way the PlayerController works, this is false if there is not an active user connected to the controller.
    /// </summary>
    public bool IsDead => PlayerController.isPlayerDead;

    /// <summary>
    /// Gets or sets the <see cref="SPlayer"/>'s username locally.
    /// </summary>
    public string Username
    {
        get
        {
            return PlayerController.playerUsername;
        }
        set
        {
            PlayerController.playerUsername = value;
            PlayerController.usernameBillboardText.text = value;
            int index = StartOfRound.Instance.mapScreen.radarTargets.FindIndex(t => t.transform == PlayerController.transform);

            if (index != -1)
            {
                StartOfRound.Instance.mapScreen.radarTargets[index].name = value;

                if (StartOfRound.Instance.mapScreen.targetTransformIndex == index)
                    StartOfRound.Instance.mapScreenPlayerName.text = "MONITORING: " + value;
            }

            PlayerController.quickMenuManager.playerListSlots[PlayerObjectId].usernameHeader.text = value;
        }
    }

    /// <summary>
    /// Gets the <see cref="SPlayer"/>'s currently held item.
    /// </summary>
    public SItem HeldItem
    {
        get
        {
            if (PlayerController.currentlyHeldObjectServer == null) return null;

            return SItem.Dictionary[PlayerController.currentlyHeldObjectServer];
        }
    }

    /// <summary>
    /// Gets whether or not the <see cref="SPlayer"/> is holding an item.
    /// </summary>
    public bool IsHoldingItem => HeldItem != null;

    /// <summary>
    /// Gets whether or not the <see cref="SPlayer"/> has free hands, meaning their currently held item is not two handed.
    /// </summary>
    public bool HasFreeHands => !IsHoldingItem || !HeldItem.IsTwoHanded;

    /// <summary>
    /// Gets the <see cref="SPlayer"/>'s <see cref="PlayerInventory"/>.
    /// </summary>
    public PlayerInventory Inventory { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="SPlayer"/>'s carry weight.
    /// </summary>
    public float CarryWeight
    {
        get
        {
            return PlayerController.carryWeight;
        }
        set
        {
            PlayerController.carryWeight = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="SPlayer"/>'s sprint meter.
    /// </summary>
    public float SprintMeter
    {
        get
        {
            return PlayerController.sprintMeter;
        }
        set
        {
            PlayerController.sprintMeter = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="SPlayer"/>'s position.
    /// If you set a <see cref="SPlayer"/>'s position out of bounds, they will be teleported back to a safe location next to the ship or entrance/exit to a dungeon.
    /// </summary>
    public Vector3 Position
    {
        get
        {
            return PlayerController.transform.position;
        }
        set
        {
            PlayerController.transform.position = value;
            PlayerController.serverPlayerPosition = value;
            PlayerController.TeleportPlayer(value);

            bool inShip = PlayerController.playersManager.shipBounds.bounds.Contains(value);

            if (IsLocalPlayer) PlayerController.UpdatePlayerPositionServerRpc(value, inShip, inShip, PlayerController.isExhausted, PlayerController.thisController.isGrounded);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="SPlayer"/>'s euler angles.
    /// </summary>
    public Vector3 EulerAngles
    {
        get
        {
            return PlayerController.transform.eulerAngles;
        }
        set
        {
            PlayerController.transform.eulerAngles = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="SPlayer"/>'s rotation. Quaternions can't gimbal lock, but they are harder to understand.
    /// Use <see cref="SPlayer.EulerAngles"/> if you don't know what you're doing.
    /// </summary>
    public Quaternion Rotation
    {
        get
        {
            return PlayerController.transform.rotation;
        }
        set
        {
            PlayerController.transform.rotation = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="SPlayer"/>'s health.
    /// </summary>
    public int Health
    {
        get
        {
            return PlayerController.health;
        }
        set
        {
            int oldHealth = PlayerController.health;

            PlayerController.health = value;

            if (PlayerController.IsOwner) HUDManager.Instance.UpdateHealthUI(value, value < oldHealth);

            if (value <= 0 && !PlayerController.isPlayerDead && PlayerController.AllowPlayerDeath())
            {
                PlayerController.KillPlayer(default, true, CauseOfDeath.Unknown, 0);
            }
        }
    }

    /// <summary>
    /// Gets whether or not the <see cref="SPlayer"/> is in the factory.
    /// </summary>
    public bool IsInFactory => PlayerController.isInsideFactory;

    /// <summary>
    /// Hurts the <see cref="SPlayer"/>.
    /// </summary>
    /// <param name="damage">The amount of health to take from the <see cref="SPlayer"/>.</param>
    /// <param name="causeOfDeath">The cause of death to show on the end screen.</param>
    /// <param name="bodyVelocity">he velocity to launch the ragdoll at, if killed.</param>
    /// <param name="overrideOneShotProtection">Whether or not to override one shot protection.</param>
    /// <param name="deathAnimation">Which death animation to use.</param>
    /// <param name="fallDamage">Whether or not this should be considered fall damage.</param>
    /// <param name="hasSFX">Whether or not this damage has sfx.</param>
    public void Hurt(int damage, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, Vector3 bodyVelocity = default, bool overrideOneShotProtection = false, int deathAnimation = 0, bool fallDamage = false, bool hasSFX = true)
    {
        if (overrideOneShotProtection && Health - damage <= 0)
        {
            Kill(bodyVelocity, true, causeOfDeath, deathAnimation);
            return;
        }

        PlayerController.DamagePlayer(damage, hasSFX, true, causeOfDeath, deathAnimation, fallDamage, bodyVelocity);
    }

    /// <summary>
    /// Kills the <see cref="SPlayer"/>.
    /// </summary>
    /// <param name="bodyVelocity">The velocity to launch the ragdoll at, if spawned.</param>
    /// <param name="spawnBody">Whether or not to spawn a ragdoll.</param>
    /// <param name="causeOfDeath">The cause of death to show on the end screen.</param>
    /// <param name="deathAnimation">Which death animation to use.</param>
    public void Kill(Vector3 bodyVelocity = default, bool spawnBody = true, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, int deathAnimation = 0)
    {
        PlayerController.KillPlayer(bodyVelocity, spawnBody, causeOfDeath, deathAnimation);
    }

    /// <summary>
    /// Queues a <see cref="Tip"/> to show to the <see cref="SPlayer"/>.
    /// </summary>
    /// <param name="header">The <see cref="Tip"/>'s header</param>
    /// <param name="message">The <see cref="Tip"/>'s message.</param>
    /// <param name="duration">The <see cref="Tip"/>'s duration.</param>
    /// <param name="priority">The priority of the <see cref="Tip"/>. Higher means will show sooner. Goes to the end of the priority list.</param>
    /// <param name="isWarning">Whether or not this <see cref="Tip"/> is a warning.</param>
    /// <param name="useSave">Whether or not to save <see langword="true"/> to the <paramref name="prefsKey"/>. Useful for showing one time only tips.</param>
    /// <param name="prefsKey">The key to save as when <paramref name="useSave"/> is set to <see langword="true" /></param>
    public void QueueTip(string header, string message, float duration = 5f, int priority = 0, bool isWarning = false, bool useSave = false, string prefsKey = "LC_Tip1")
    {
        if (!IsLocalPlayer) return;

        QueueTipInternal(header, message, duration, priority, isWarning, useSave, prefsKey);
    }

    internal void QueueTipInternal(string header, string message, float duration, int priority, bool isWarning, bool useSave, string prefsKey)
    {
        Tip tip = new Tip(header, message, duration, priority, isWarning, useSave, prefsKey, NextTipId++);

        if (!HUDManager.Instance.CanTipDisplay(tip.IsWarning, tip.UseSave, tip.PreferenceKey))
        {
            return;
        }

        if (TipQueue.Count == 0)
        {
            TipQueue.Add(tip);
            return;
        }

        if (TipQueue[TipQueue.Count - 1].CompareTo(tip) <= 0)
        {
            TipQueue.Add(tip);
            return;
        }

        if (TipQueue[0].CompareTo(tip) >= 0)
        {
            TipQueue.Insert(0, tip);
            return;
        }

        int index = TipQueue.BinarySearch(tip);

        if (index < 0) index = ~index;

        TipQueue.Insert(index, tip);
    }

    /// <summary>
    /// Shows a <see cref="Tip"/> to the <see cref="SPlayer"/>, bypassing the queue.
    /// </summary>
    /// <param name="header">The <see cref="Tip"/>'s header</param>
    /// <param name="message">The <see cref="Tip"/>'s message.</param>
    /// <param name="duration">The <see cref="Tip"/>'s duration.</param>
    /// <param name="isWarning">Whether or not this <see cref="Tip"/> is a warning.</param>
    /// <param name="useSave">Whether or not to save <see langword="true"/> to the <paramref name="prefsKey"/>. Useful for showing one time only tips.</param>
    /// <param name="prefsKey">The key to save as when <paramref name="useSave"/> is set to <see langword="true" /></param>
    public void ShowTip(string header, string message, float duration = 5f, bool isWarning = false, bool useSave = false, string prefsKey = "LC_Tip1")
    {
        if (!IsLocalPlayer) return;

        ShowTipInternal(header, message, duration, isWarning, useSave, prefsKey);
    }
    internal void ShowTipInternal(string header, string message, float duration, bool isWarning, bool useSave, string prefsKey)
    {
        Tip tip = new Tip(header, message, duration, int.MaxValue, isWarning, useSave, prefsKey, NextTipId++);

        if (!HUDManager.Instance.CanTipDisplay(tip.IsWarning, tip.UseSave, tip.PreferenceKey))
        {
            return;
        }

        // if there is a tip with >= 1.5 seconds left, queue it back up
        if (CurrentTip != null && CurrentTip.TimeLeft >= 1.5f)
        {
            // Ensures the current tip will continue afterwards
            CurrentTip.TipId = int.MinValue;
            TipQueue.Insert(0, CurrentTip);
        }

        CurrentTip = tip;

        HUDManager.Instance.tipsPanelAnimator.speed = 1;
        HUDManager.Instance.tipsPanelAnimator.ResetTrigger("TriggerHint");

        DisplayTip();
    }

    internal void DisplayTip()
    {
        if (CurrentTip == null) return;

        if (!HUDManager.Instance.CanTipDisplay(CurrentTip.IsWarning, CurrentTip.UseSave, CurrentTip.PreferenceKey))
        {
            CurrentTip = null;
            return;
        }

        if (CurrentTip.UseSave)
        {
            ES3.Save(CurrentTip.PreferenceKey, true, "LCGeneralSaveData");
        }

        HUDManager.Instance.tipsPanelHeader.text = CurrentTip.Header;
        HUDManager.Instance.tipsPanelBody.text = CurrentTip.Message;

        if (CurrentTip.IsWarning)
        {
            HUDManager.Instance.tipsPanelAnimator.SetTrigger("TriggerWarning");
            RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, HUDManager.Instance.warningSFX, false, 1f, 0);
            return;
        }

        HUDManager.Instance.tipsPanelAnimator.SetTrigger("TriggerHint");
        RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, HUDManager.Instance.tipsSFX, false, 1f, 0);
    }


    #region Unity related things
    private void Start()
    {
        PlayerController = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(c => c.actualClientId == _clientId);

        if (PlayerController != null)
        {
            if (IsLocalPlayer) LocalPlayer = this;
            if (IsHost) HostPlayer = this;

            if (!Dictionary.ContainsKey(PlayerController)) Dictionary.Add(PlayerController, this);
        }

        Inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if (!IsLocalPlayer) return;

        if (CurrentTip != null)
        {
            // Prevent the panel from automatically disappearing after 5 seconds
            if (HUDManager.Instance.tipsPanelAnimator.speed > 0 &&
                HUDManager.Instance.tipsPanelAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                HUDManager.Instance.tipsPanelAnimator.speed = 0;
            }

            CurrentTip.TimeLeft -= Time.deltaTime;

            if (CurrentTip.TimeLeft <= 0)
            {
                HUDManager.Instance.tipsPanelAnimator.speed = 1;
                HUDManager.Instance.tipsPanelAnimator.ResetTrigger("TriggerHint");
                CurrentTip = null;
            }
        }

        if (CurrentTip == null && TipQueue.Count > 0)
        {
            CurrentTip = TipQueue[0];
            TipQueue.RemoveAt(0);

            DisplayTip();
        }
    }
    #endregion

    #region Player getters
    /// <summary>
    /// Gets or adds a <see cref="Features.SPlayer"/> from a <see cref="PlayerControllerB"/>.
    /// </summary>
    /// <param name="playerController">The player's <see cref="PlayerControllerB"/>.</param>
    /// <returns>A <see cref="SPlayer"/>.</returns>
    public static SPlayer GetOrAdd(PlayerControllerB playerController)
    {
        if (playerController == null) return null;

        if (Dictionary.TryGetValue(playerController, out SPlayer player)) return player;

        foreach (SPlayer p in FindObjectsOfType<SPlayer>())
        {
            if (p._clientId == playerController.actualClientId)
            {
                p.PlayerController = playerController;
                if (!Dictionary.ContainsKey(playerController)) Dictionary.Add(playerController, p);
                return p;
            }
        }

        SPlayer newPlayer = playerController.gameObject.AddComponent<SPlayer>();
        newPlayer._clientId = playerController.actualClientId;

        playerController.gameObject.AddComponent<PlayerInventory>();

        return newPlayer;
    }

    /// <summary>
    /// Gets a <see cref="Features.SPlayer"/> from a <see cref="PlayerControllerB"/>.
    /// </summary>
    /// <param name="playerController">The player's <see cref="PlayerControllerB"/>.</param>
    /// <returns>A <see cref="SPlayer"/> or <see langword="null"/> if not found.</returns>
    public static SPlayer Get(PlayerControllerB playerController)
    {
        if (playerController == null) return null;

        if (Dictionary.TryGetValue(playerController, out SPlayer player)) return player;

        return null;
    }

    /// <summary>
    /// Tries to get a <see cref="Features.SPlayer"/> from a <see cref="PlayerControllerB"/>.
    /// </summary>
    /// <param name="playerController">The player's <see cref="PlayerControllerB"/>.</param>
    /// <param name="player">Outputs a <see cref="SPlayer"/>, or <see langword="null"/> if not found.</param>
    /// <returns><see langword="true"/> if a <see cref="Features.SPlayer"/> is found, <see langword="false"/> otherwise.</returns>
    public static bool TryGet(PlayerControllerB playerController, out SPlayer player)
    {
        player = null;
        if (playerController == null) return false;

        return Dictionary.TryGetValue(playerController, out player);
    }

    /// <summary>
    /// Gets a <see cref="Features.SPlayer"/> from a <see cref="Features.SPlayer"/>'s client id.
    /// </summary>
    /// <param name="clientId">The player's client id.</param>
    /// <returns>A <see cref="SPlayer"/> or <see langword="null"/> if not found.</returns>
    public static SPlayer Get(ulong clientId)
    {
        return List.FirstOrDefault(p => p.ClientId == clientId);
    }

    /// <summary>
    /// Tries to get a <see cref="Features.SPlayer"/> from a <see cref="Features.SPlayer"/>'s client id.
    /// </summary>
    /// <param name="clientId">The player's client id.</param>
    /// <param name="player">Outputs a <see cref="SPlayer"/>, or <see langword="null"/> if not found.</param>
    /// <returns><see langword="true"/> if a <see cref="Features.SPlayer"/> is found, <see langword="false"/> otherwise.</returns>
    public static bool TryGet(ulong clientId, out SPlayer player)
    {
        return (player = Get(clientId)) != null;
    }
    #endregion

    /// <summary>
    /// Encapsulates a <see cref="Player"/>'s inventory to provide useful tools to it.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        /// <summary>
        /// Gets the <see cref="Player"/> that this <see cref="PlayerInventory"/> belongs to.
        /// </summary>
        public SPlayer Player { get; private set; }

        /// <summary>
        /// Gets the <see cref="Player"/>'s items in order.
        /// </summary>
        /// TODO: I'm not sure how feasible it is to get this to work in any other way, but I hate this.
        public SItem?[] Items => Player.PlayerController.ItemSlots.Select(i => i != null ? SItem.Dictionary[i] : null).ToArray();

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
                Player.PlayerController.SwitchToItemSlot(value);
            }
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

        private void Awake()
        {
            Player = GetComponent<SPlayer>();
        }
    }
}
