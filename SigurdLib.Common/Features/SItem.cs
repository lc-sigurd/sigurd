using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.Common.Features
{
    /// <summary>
    /// Encapsulates a <see cref="global::GrabbableObject"/> for easier interacting.
    /// </summary>
    public class SItem : MonoBehaviour
    {
        /// <summary>
        /// Gets a dictionary containing all <see cref="SItem"/>s that are currently spawned in the world or in <see cref="Common.Features.SPlayer"/>s' inventories.
        /// </summary>
        public static Dictionary<GrabbableObject, SItem> Dictionary { get; } = new Dictionary<GrabbableObject, SItem>();

        /// <summary>
        /// Gets a list containing all <see cref="SItem"/>s.
        /// </summary>
        public static IReadOnlyCollection<SItem> List => Dictionary.Values;

        /// <summary>
        /// Gets the encapsulated <see cref="global::GrabbableObject"/>
        /// </summary>
        public GrabbableObject GrabbableObject { get; private set; }

        /// <summary>
        /// Gets the <see cref="SItem"/>'s <see cref="SItem">item properties</see>.
        /// These do not network, it is recommended to use the getters/setters on the <see cref="SItem"/> itself.
        /// </summary>
        public global::Item ItemProperties => GrabbableObject.itemProperties;

        /// <summary>
        /// Gets the <see cref="SItem"/>'s <see cref="ScanNodeProperties"/>.
        /// These do not network, it is recommended to use the getters/setters on the <see cref="SItem"/> itself.
        /// </summary>
        public ScanNodeProperties ScanNodeProperties { get; set; }

        /// <summary>
        /// Gets whether or not this <see cref="SItem"/> is currently being held.
        /// </summary>
        public bool IsHeld => GrabbableObject.isHeld;

        /// <summary>
        /// Gets whether or not this <see cref="SItem"/> is two handed.
        /// </summary>
        public bool IsTwoHanded => ItemProperties.twoHanded;

        /// <summary>
        /// Gets the <see cref="SPlayer"/> that is currently holding this <see cref="SItem"/>. <see langword="null"/> if not held.
        /// </summary>
        public SPlayer Holder => IsHeld ? SPlayer.Dictionary.TryGetValue(GrabbableObject.playerHeldBy, out SPlayer p) ? p : null : null;

        /// <summary>
        /// Gets or sets the <see cref="SItem"/>'s name.
        /// </summary>
        public string Name
        {
            get
            {
                return ItemProperties.itemName;
            }
            set
            {
                string current = ItemProperties.itemName.ToLower();

                CloneProperties();

                ItemProperties.itemName = value;
                OverrideTooltips(current, value.ToLower());

                ScanNodeProperties.headerText = value;
            }
        }

        private void OverrideTooltips(string oldName, string newName)
        {
            for (int i = 0; i < ItemProperties.toolTips.Length; i++)
            {
                ItemProperties.toolTips[i] = ItemProperties.toolTips[i].ReplaceWithCase(oldName, newName);
            }

            if (IsHeld && Holder == SPlayer.LocalPlayer) GrabbableObject.SetControlTipsForItem();
        }

        /// <summary>
        /// Gets or sets the position of this <see cref="SItem"/>.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return GrabbableObject.transform.position;
            }
            set
            {
                GrabbableObject.startFallingPosition = value;
                GrabbableObject.targetFloorPosition = value;
                GrabbableObject.transform.position = value;
            }
        }

        /// <summary>
        /// Gets or sets the rotation of this <see cref="SItem"/>.
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return GrabbableObject.transform.rotation;
            }
            set
            {
                GrabbableObject.transform.rotation = value;
            }
        }

        /// <summary>
        /// Sets the scale of the <see cref="SItem"/>.
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return GrabbableObject.transform.localScale;
            }
            set
            {
                GrabbableObject.transform.localScale = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this <see cref="SItem"/> should be considered scrap.
        /// </summary>
        public bool IsScrap
        {
            get
            {
                return ItemProperties.isScrap;
            }
            set
            {
                CloneProperties();

                ItemProperties.isScrap = value;
            }
        }

        /// <summary>
        /// Gets or sets this <see cref="SItem"/>'s scrap value.
        /// </summary>
        public int ScrapValue
        {
            get
            {
                return GrabbableObject.scrapValue;
            }
            set
            {
                GrabbableObject.SetScrapValue(value);
            }
        }

        /// <summary>
        /// Enables/disables the <see cref="SItem"/>'s physics.
        /// </summary>
        /// <param name="enable"><see langword="true"/> to enable physics, <see langword="false" /> otherwise.</param>
        public void EnablePhysics(bool enable) => GrabbableObject.EnablePhysics(enable);

        /// <summary>
        /// Enables/disables the <see cref="SItem"/>'s meshes.
        /// </summary>
        /// <param name="enable"><see langword="true"/> to enable meshes, <see langword="false" /> otherwise.</param>
        public void EnableMeshes(bool enable) => GrabbableObject.EnableItemMeshes(enable);

        /// <summary>
        /// Start the <see cref="SItem"/> falling to the ground.
        /// </summary>
        /// <param name="randomizePosition">Whether or not to add some randomness to the position.</param>
        public void FallToGround(bool randomizePosition = false)
        {
            GrabbableObject.FallToGround(randomizePosition);
        }

        /// <summary>
        /// Pockets the <see cref="SItem"/> by disabling its meshes. Plays pocket sound effects.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="SItem"/> was able to be pocketed, <see langword="false" /> otherwise.</returns>
        public bool PocketItem()
        {
            // We can only pocket items that are currently being held in a player's hand. Two handed
            // objects cannot be pocketed.
            if (!IsHeld || Holder.HeldItem != this || IsTwoHanded) return false;

            GrabbableObject.PocketItem();

            return true;
        }

        /// <summary>
        /// Initializes the <see cref="SItem"/> with base game scrap values.
        /// </summary>
        public void InitializeScrap()
        {
            if (RoundManager.Instance.AnomalyRandom != null) InitializeScrap((int)(RoundManager.Instance.AnomalyRandom.Next(ItemProperties.minValue, ItemProperties.maxValue) * RoundManager.Instance.scrapValueMultiplier));
            else InitializeScrap((int)(UnityEngine.Random.Range(ItemProperties.minValue, ItemProperties.maxValue) * RoundManager.Instance.scrapValueMultiplier));
        }

        /// <summary>
        /// Initializes the <see cref="SItem"/> with a specific scrap value.
        /// </summary>
        /// <param name="scrapValue">The desired scrap value.</param>
        public void InitializeScrap(int scrapValue)
        {
            ScrapValue = scrapValue;

            if (GrabbableObject.gameObject.TryGetComponent(out MeshFilter filter)
                && ItemProperties.meshVariants != null && ItemProperties.meshVariants.Length != 0)
            {
                if (RoundManager.Instance.ScrapValuesRandom != null)
                    filter.mesh = ItemProperties.meshVariants[RoundManager.Instance.ScrapValuesRandom.Next(ItemProperties.meshVariants.Length)];
                else
                    filter.mesh = ItemProperties.meshVariants[0];
            }

            if (GrabbableObject.gameObject.TryGetComponent(out MeshRenderer renderer)
                && ItemProperties.materialVariants != null && ItemProperties.materialVariants.Length != 0)
            {
                if (RoundManager.Instance.ScrapValuesRandom != null)
                    renderer.sharedMaterial = ItemProperties.materialVariants[RoundManager.Instance.ScrapValuesRandom.Next(ItemProperties.materialVariants.Length)];
                else
                    renderer.sharedMaterial = ItemProperties.materialVariants[0];
            }
        }

        #region Unity related things
        private void Awake()
        {
            GrabbableObject = GetComponent<GrabbableObject>();
            ScanNodeProperties = GrabbableObject.gameObject.GetComponentInChildren<ScanNodeProperties>();

            Dictionary.Add(GrabbableObject, this);
        }

        // All items have the same properties, so if we change it, we need to clone it.
        private bool hasNewProps = false;
        private void CloneProperties()
        {
            global::Item newProps = Instantiate(ItemProperties);

            // Don't want to destroy any that aren't custom
            if (hasNewProps) Destroy(ItemProperties);

            GrabbableObject.itemProperties = newProps;

            hasNewProps = true;
        }

        /// <summary>
        /// For internal use. Do not use.
        /// </summary>
        public void OnDestroy()
        {
            Dictionary.Remove(GrabbableObject);
        }
        #endregion

        #region Item getters
        /// <summary>
        /// Gets an <see cref="SItem"/> from a <see cref="GrabbableObject"/>.
        /// </summary>
        /// <param name="grabbableObject">The encapsulated <see cref="GrabbableObject"/>.</param>
        /// <returns>An <see cref="SItem"/>.</returns>
        public static SItem? Get(GrabbableObject grabbableObject)
        {
            if (Dictionary.TryGetValue(grabbableObject, out SItem item))
                return item;

            return null;
        }

        /// <summary>
        /// Attempts to get an <see cref="SItem"/> from a <see cref="GrabbableObject"/>.
        /// </summary>
        /// <param name="grabbableObject">The encapsulated <see cref="GrabbableObject"/>.</param>
        /// <param name="item">The <see cref="SItem"/>, or <see langword="null"/> if not found.</param>
        /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise.</returns>
        public static bool TryGet(GrabbableObject grabbableObject, out SItem? item)
        {
            return Dictionary.TryGetValue(grabbableObject, out item);
        }

        /// <summary>
        /// Gets an <see cref="SItem"/> from its network object id.
        /// </summary>
        /// <param name="netId">The <see cref="SItem"/>'s network object id.</param>
        /// <returns>An <see cref="SItem"/>.</returns>
        public static SItem? Get(ulong netId)
        {
            return List.FirstOrDefault(i => i.GrabbableObject.NetworkObjectId == netId);
        }

        /// <summary>
        /// Attempts to get an <see cref="SItem"/> from its network object id.
        /// </summary>
        /// <param name="netId">The <see cref="SItem"/>'s network object id.</param>
        /// <param name="item">The <see cref="SItem"/>, or <see langword="null"/> if not found.</param>
        /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise.</returns>
        public static bool TryGet(ulong netId, out SItem? item)
        {
            item = Get(netId);

            return item != null;
        }
        #endregion
    }
}
