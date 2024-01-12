using System.Collections.Generic;
using UnityEngine;

namespace Sigurd.Common.Features
{
    /// <summary>
    /// Provides a helper method to load asset bundles.
    /// </summary>
    public class BundleHelper
    {
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

            Dictionary<string, Object> assetPairs = new Dictionary<string, Object>();

            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            try
            {
                string[] assetPaths = bundle.GetAllAssetNames();

                foreach (string assetPath in assetPaths)
                {
                    Object loadedAsset = bundle.LoadAsset(assetPath);
                    if (loadedAsset == null)
                    {
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
