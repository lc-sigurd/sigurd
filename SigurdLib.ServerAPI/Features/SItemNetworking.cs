using Sigurd.ServerAPI.Exceptions;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.ServerAPI.Features;

/// <summary>
/// Encapsulates a <see cref="global::GrabbableObject"/> for easier interacting.
/// </summary>
public class SItemNetworking : NetworkBehaviour
{
    /// <summary>
    /// Gets a dictionary mapping <see cref="Common.Features.SItem"/>'s to their respective <see cref="SItemNetworking"/>.
    /// </summary>
    public static Dictionary<Common.Features.SItem, SItemNetworking> Dictionary { get; } = new Dictionary<Common.Features.SItem, SItemNetworking>();

    public Common.Features.SItem Item { get; private set; }

    public SPlayerNetworking HolderNetworking => SPlayerNetworking.Get(Item.Holder);

    /// <summary>
    /// Gets or sets the <see cref="Item"/>'s name.
    /// </summary>
    /// <exception cref="NoAuthorityException">Thrown when attempting to set item name from the client.</exception>
    public string Name
    {
        get
        {
            return Item.Name;
        }
        set
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                throw new NoAuthorityException("Tried to set item name on client.");
            }

            SetGrabbableNameClientRpc(value);
        }
    }

    [ClientRpc]
    private void SetGrabbableNameClientRpc(string name)
    {
        Item.Name = name;
    }

    /// <summary>
    /// Gets or sets the position of this <see cref="Item"/>.
    /// </summary>
    /// <exception cref="NoAuthorityException">Thrown when attempting to set the item's position from the client.</exception>
    public Vector3 Position
    {
        get
        {
            return Item.Position;
        }
        set
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                throw new NoAuthorityException("Tried to set item position on client.");
            }

            SetItemPositionClientRpc(value);
        }
    }

    [ClientRpc]
    private void SetItemPositionClientRpc(Vector3 pos)
    {
        Item.Position = pos;
    }

    /// <summary>
    /// Gets or sets the rotation of this <see cref="Item"/>.
    /// </summary>
    public Quaternion Rotation
    {
        get
        {
            return Item.Rotation;
        }
        set
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                throw new NoAuthorityException("Tried to set item rotation on client.");
            }

            SetItemRotationClientRpc(value);
        }
    }

    [ClientRpc]
    private void SetItemRotationClientRpc(Quaternion rotation)
    {
        Item.Rotation = rotation;
    }

    /// <summary>
    /// Sets the scale of the <see cref="Item"/>.
    /// </summary>
    public Vector3 Scale
    {
        get
        {
            return Item.Scale;
        }
        set
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                throw new NoAuthorityException("Tried to set item scale on client.");
            }

            SetItemScaleClientRpc(value);
        }
    }

    [ClientRpc]
    private void SetItemScaleClientRpc(Vector3 scale)
    {
        Scale = scale;
    }

    /// <summary>
    /// Gets or sets whether this <see cref="Item"/> should be considered scrap.
    /// </summary>
    /// <exception cref="NoAuthorityException">Thrown when attempting to set isScrap from the client.</exception>
    public bool IsScrap
    {
        get
        {
            return Item.IsScrap;
        }
        set
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                throw new NoAuthorityException("Tried to set item name on client.");
            }

            SetIsScrapClientRpc(value);
        }
    }

    [ClientRpc]
    private void SetIsScrapClientRpc(bool isScrap)
    {
        Item.IsScrap = isScrap;
    }

    /// <summary>
    /// Gets or sets this <see cref="Item"/>'s scrap value.
    /// </summary>
    /// <exception cref="NoAuthorityException">Thrown when attempting to set scrap value from the client.</exception>
    public int ScrapValue
    {
        get
        {
            return Item.ScrapValue;
        }
        set
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                throw new NoAuthorityException("Tried to set scrap value on client.");
            }

            SetScrapValueClientRpc(value);
        }
    }

    [ClientRpc]
    private void SetScrapValueClientRpc(int scrapValue)
    {
        Item.ScrapValue = scrapValue;
    }

    /// <summary>
    /// Removes the <see cref="Item"/> from its current holder.
    /// </summary>
    /// <param name="position">The position to place the object after removing.</param>
    /// <param name="rotation">The rotation the object should have after removing.</param>
    public void RemoveFromHolder(Vector3 position = default, Quaternion rotation = default)
    {
        if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            throw new NoAuthorityException("Tried to remove item from player on client.");
        }

        if (!Item.IsHeld) return;

        NetworkObject.RemoveOwnership();

        SPlayerNetworking.Get(Item.Holder).InventoryNetworking.RemoveItem(this);

        RemoveFromHolderClientRpc();

        Position = position;
        Rotation = rotation;
    }

    [ClientRpc]
    private void RemoveFromHolderClientRpc()
    {
        if (!Item.IsHeld) return;

        SPlayerNetworking.Get(Item.Holder).InventoryNetworking.RemoveItem(this);
    }

    /// <summary>
    /// Initializes the <see cref="Item"/> with base game scrap values.
    /// </summary>
    public void InitializeScrap()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            throw new NoAuthorityException("Tried to initialize scrap from client.");
        }

        if (RoundManager.Instance.AnomalyRandom != null) InitializeScrapClientRpc((int)(RoundManager.Instance.AnomalyRandom.Next(Item.ItemProperties.minValue, Item.ItemProperties.maxValue) * RoundManager.Instance.scrapValueMultiplier));
        else InitializeScrapClientRpc((int)(UnityEngine.Random.Range(Item.ItemProperties.minValue, Item.ItemProperties.maxValue) * RoundManager.Instance.scrapValueMultiplier));
    }

    /// <summary>
    /// Initializes the <see cref="Item"/> with a specific scrap value.
    /// </summary>
    /// <param name="scrapValue">The desired scrap value.</param>
    [ClientRpc]
    public void InitializeScrapClientRpc(int scrapValue)
    {
        Item.InitializeScrap(scrapValue);
    }

    /// <summary>
    /// Gives this <see cref="Item"/> to the specific player. Deleting it from another <see cref="SPlayerNetworking"/>'s inventory, if necessary.
    /// </summary>
    /// <param name="player">The player to give the item to.</param>
    /// <param name="switchTo">Whether or not to switch to the item. Forced for 2 handed items.</param>
    /// <returns><see langword="true"/> if the player had an open slot to add the item to, <see langword="flase"/> otherwise.</returns>
    public bool GiveTo(SPlayerNetworking player, bool switchTo = true)
    {
        if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            throw new NoAuthorityException("Tried to give item to player on client.");
        }

        return player.InventoryNetworking.TryAddItem(this, switchTo);
    }

    /// <summary>
    /// Creates and spawns an <see cref="Item"/> in the world.
    /// </summary>
    /// <param name="itemName">The item's name. Uses a simple Contains check to see if the provided item name is contained in the actual item's name. Case insensitive.</param>
    /// <param name="position">The position to spawn at.</param>
    /// <param name="rotation">The rotation to spawn at.</param>
    /// <param name="andInitialize">Whether or not to initialize the item after spawning.</param>
    /// <returns>A new <see cref="Item"/>, or <see langword="null"/> if the provided item name is not found.</returns>
    /// <exception cref="NoAuthorityException">Thrown when trying to spawn an <see cref="Item"/> on the client.</exception>
    public static Common.Features.SItem CreateAndSpawnItem(string itemName, Vector3 position = default, Quaternion rotation = default, bool andInitialize = true)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            throw new NoAuthorityException("Tried to create and spawn item on client.");
        }

        string name = itemName.ToLower();

        GameObject go = StartOfRound.Instance.allItemsList.itemsList.FirstOrDefault(i => i.itemName.ToLower().Contains(name))?.spawnPrefab;
        if (go != null)
        {
            GameObject instantiated = Instantiate(go, position, rotation);

            instantiated.GetComponent<NetworkObject>().Spawn();

            Common.Features.SItem item = instantiated.GetComponent<Common.Features.SItem>();

            if (item.IsScrap && andInitialize) item.InitializeScrap();

            return item;
        }

        return null;
    }

    /// <summary>
    /// Creates an <see cref="Item"/> and gives it to a specific <see cref="SPlayerNetworking"/>.
    /// </summary>
    /// <param name="itemName">The item's name. Uses a simple Contains check to see if the provided item name is contained in the actual item's name. Case insensitive.</param>
    /// <param name="player">The <see cref="SPlayerNetworking"/> to give the <see cref="Common.Features.SItem"/> to.</param>
    /// <param name="switchTo">Whether or not to switch to the item. Forced for 2 handed items.</param>
    /// <param name="andInitialize">Whether or not to initialize this item after spawning.</param>
    /// <returns>A new <see cref="Item"/>, or <see langword="null"/> if the provided item name is not found.</returns>
    /// <exception cref="NoAuthorityException">Thrown when trying to spawn an <see cref="Item"/> on the client.</exception>
    public static SItemNetworking CreateAndGiveItem(string itemName, SPlayerNetworking player, bool switchTo = true, bool andInitialize = true)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            throw new NoAuthorityException("Tried to create and give item on client.");
        }

        string name = itemName.ToLower();

        GameObject go = StartOfRound.Instance.allItemsList.itemsList.FirstOrDefault(i => i.itemName.ToLower().Contains(name))?.spawnPrefab;
        if (go != null)
        {
            GameObject instantiated = Instantiate(go, Vector3.zero, default);

            instantiated.GetComponent<NetworkObject>().Spawn();

            SItemNetworking item = instantiated.GetComponent<SItemNetworking>();

            if (item.IsScrap && andInitialize) item.InitializeScrap();

            item.GiveTo(player, switchTo);

            return item;
        }

        return null;
    }

    #region Unity related things
    private void Awake()
    {
        Item = GetComponent<Common.Features.SItem>();
        Dictionary.Add(Item, this);
    }
    #endregion

    public static implicit operator Common.Features.SItem(SItemNetworking itemNetworking) => itemNetworking.Item;
    public static implicit operator SItemNetworking(Common.Features.SItem item) => Get(item);

    #region Item getters
    /// <summary>
    /// Gets an <see cref="SItemNetworking"/> from an <see cref="Common.Features.SItem"/>.
    /// </summary>
    /// <param name="item">The <see cref="Common.Features.SItem"/>.</param>
    /// <returns>An <see cref="SItemNetworking"/>.</returns>
    public static SItemNetworking? Get(Common.Features.SItem item)
    {
        if (item == null) return null;

        if (Dictionary.TryGetValue(item, out SItemNetworking itemNetworking))
            return itemNetworking;

        return null;
    }

    /// <summary>
    /// Attempts to get an <see cref="SItemNetworking"/> from an <see cref="Common.Features.SItem"/>.
    /// </summary>
    /// <param name="item">The <see cref="Common.Features.SItem"/>.</param>
    /// <param name="itemNetworking">The <see cref="SItemNetworking"/>, or <see langword="null"/> if not found.</param>
    /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise.</returns>
    public static bool TryGet(Common.Features.SItem item, out SItemNetworking? itemNetworking)
    {
        itemNetworking = null;
        if (item == null) return false;

        return Dictionary.TryGetValue(item, out itemNetworking);
    }

    /// <summary>
    /// Gets an <see cref="Item"/> from its network object id.
    /// </summary>
    /// <param name="netId">The <see cref="Item"/>'s network object id.</param>
    /// <returns>An <see cref="Item"/>.</returns>
    public static SItemNetworking? Get(ulong netId)
    {
        return Dictionary.Values.FirstOrDefault(i => i.NetworkObjectId == netId);
    }

    /// <summary>
    /// Attempts to get an <see cref="Item"/> from its network object id.
    /// </summary>
    /// <param name="netId">The <see cref="Item"/>'s network object id.</param>
    /// <param name="item">The <see cref="Item"/>, or <see langword="null"/> if not found.</param>
    /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise.</returns>
    public static bool TryGet(ulong netId, out SItemNetworking? item)
    {
        item = Get(netId);

        return item != null;
    }
    #endregion
}