using HarmonyLib;
using Sigurd.Common.Features;
using System;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.ServerAPI.Features.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
    class GameNetworkManagerStartPatch
    {
        private static readonly string BUNDLE_PATH = Path.Combine(Plugin.Instance.Info.Location.Substring(0, Plugin.Instance.Info.Location.LastIndexOf(Path.DirectorySeparatorChar)), "Bundles", "networking");

        private const string PLAYER_NETWORKING_ASSET_LOCATION = "assets/sigurd/playernetworkingprefab.prefab";

        private static void Postfix(GameNetworkManager __instance)
        {
            if (!File.Exists(BUNDLE_PATH))
            {
                throw new Exception("Networking bundle not found at expected path.");
            }

            NetworkManager networkManager = __instance.GetComponent<NetworkManager>();

            LoadedAssetBundle assets = BundleHelper.LoadAssetBundle(BUNDLE_PATH, false);

            GameObject playerObj = assets.GetAsset<GameObject>(PLAYER_NETWORKING_ASSET_LOCATION);
            playerObj.AddComponent<PlayerNetworking>();
            playerObj.AddComponent<PlayerNetworking.PlayerInventoryNetworking>();
            networkManager.AddNetworkPrefab(playerObj);
            PlayerNetworking.PlayerNetworkPrefab = playerObj;

            foreach (NetworkPrefab prefab in networkManager.NetworkConfig.Prefabs.Prefabs)
            {
                if (prefab.Prefab != null && prefab.Prefab.GetComponent<GrabbableObject>() != null)
                {
                    prefab.Prefab.AddComponent<ItemNetworking>();
                }
            }
        }
    }
}
