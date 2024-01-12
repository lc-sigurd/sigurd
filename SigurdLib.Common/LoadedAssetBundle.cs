using System.Collections.Generic;

namespace Sigurd.Common
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
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="InvalidCastException"/>
        public TAsset GetAsset<TAsset>(string path) where TAsset : UnityEngine.Object
        {
            var lowerPath = path.ToLower();
            var asset = _loadedAssets[lowerPath];
            if (asset is not TAsset typedAsset) throw new InvalidCastException($"Found asset is not of type {typeof(TAsset)}");
            return typedAsset;
        }

        /// <summary>
        /// Tries to get an asset from the bundle.
        /// </summary>
        /// <typeparam name="TAsset">The type of the asset. Usually <see cref="UnityEngine.GameObject"/>.</typeparam>
        /// <param name="path">The path to the asset in the bundle.</param>
        /// <param name="asset">Outputs the asset.</param>
        /// <returns><see langword="true"/> if the asset is found. <see langword="false"/> if the asset isn't found, or couldn't be casted to TAsset</returns>
        public bool TryGetAsset<TAsset>(string path, out TAsset? asset) where TAsset : UnityEngine.Object
        {
            var lowerPath = path.ToLower();

            asset = null;

            if (!_loadedAssets.TryGetValue(lowerPath, out var obj)) return false;

            if (obj is TAsset tAsset) asset = tAsset;
            return asset is not null;
        }
    }
}
