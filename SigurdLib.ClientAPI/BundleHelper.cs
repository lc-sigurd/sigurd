using System.Collections.Generic;
using UnityEngine;

namespace Sigurd.ClientAPI
{
    /// <summary>
    /// Provides a helper method to load asset bundles.
    /// </summary>
    public class BundleHelper
    {
        /// <summary>
        /// A loaded asset bundle.
        /// </summary>
        public class LoadedAssetBundle
        {
            private Dictionary<string, UnityEngine.Object> _loadedAssets;

            internal LoadedAssetBundle(Dictionary<string, UnityEngine.Object> loadedAssets)
            {
                _loadedAssets = loadedAssets;
            }

            /// <summary>
            /// Gets an asset from the bundle.
            /// </summary>
            /// <typeparam name="TAsset">The type of the asset. Usually <see cref="UnityEngine.GameObject"/>.</typeparam>
            /// <param name="path">The path to the asset in the bundle.</param>
            /// <returns>The asset.</returns>
            public TAsset GetAsset<TAsset>(string path) where TAsset : UnityEngine.Object
            {
                string lowerPath = path.ToLower();

                if (_loadedAssets.TryGetValue(lowerPath, out UnityEngine.Object obj))
                {
                    return obj as TAsset;
                }

                return null;
            }

            /// <summary>
            /// Tries to get an asset from the bundle.
            /// </summary>
            /// <typeparam name="TAsset">The type of the asset. Usually <see cref="UnityEngine.GameObject"/>.</typeparam>
            /// <param name="path">The path to the asset in the bundle.</param>
            /// <param name="asset">Outputs the asset.</param>
            /// <returns><see langword="true"/> if the asset is found. <see langword="false"/> if the asset isn't found, or couldn't be casted to TAsset</returns>
            public bool TryGetAsset<TAsset>(string path, out TAsset asset) where TAsset : UnityEngine.Object
            {
                string lowerPath = path.ToLower();

                asset = null;

                if (_loadedAssets.TryGetValue(lowerPath, out UnityEngine.Object obj))
                {
                    if (obj is TAsset tasset) asset = tasset;
                }

                return asset != null;
            }
        }

        private static Dictionary<string, LoadedAssetBundle> loadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

        /// <summary>
        /// Loads an entire asset bundle. It is recommended to use this to load asset bundles.
        /// </summary>
        /// <param name="filePath">The file system path of the asset bundle.</param>
        /// <param name="cache">Whether or not to cache the loaded bundle. Set to <see langword="false" /> if you cache it yourself, or don't need it more than once.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> containing all assets from the bundle mapped to their path in lowercase.</returns>
        public static LoadedAssetBundle LoadAssetBundle(string filePath, bool cache = true)
        {
            if (loadedAssetBundles.TryGetValue(filePath, out LoadedAssetBundle _assets))
                return _assets;

            Dictionary<string, UnityEngine.Object> assetPairs = new Dictionary<string, UnityEngine.Object>();

            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            try
            {
                string[] assetPaths = bundle.GetAllAssetNames();

                foreach (string assetPath in assetPaths)
                {
                    UnityEngine.Object loadedAsset = bundle.LoadAsset(assetPath);
                    if (loadedAsset == null)
                    {
                        Plugin.Log.LogWarning($"Failed to load an asset from bundle '{bundle.name}' - Asset path: {loadedAsset}");
                        continue;
                    }

                    assetPairs.Add(assetPath.ToLower(), loadedAsset);
                }
            }
            finally
            {
                bundle?.Unload(false);
            }

            if (cache) loadedAssetBundles.Add(filePath, new LoadedAssetBundle(assetPairs));

            return new LoadedAssetBundle(assetPairs);
        }
    }
}
