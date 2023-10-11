using UnityEngine;

namespace Pancake.AssetLoader
{
    [CreateAssetMenu(fileName = "ResourcesAssetLoader", menuName = "Pancake/Asset Loader/Resources Asset Loader")]
    [EditorIcon("scriptable_interface")]
    public sealed class ResourcesAssetLoaderObject : AssetLoaderObject, IAssetLoader
    {
        private readonly ResourcesAssetLoader _loader = new ResourcesAssetLoader();

        public override AssetLoadHandle<T> Load<T>(string key) { return _loader.Load<T>(key); }

        public override AssetLoadHandle<T> LoadAsync<T>(string key) { return _loader.LoadAsync<T>(key); }

        public override void Release(AssetLoadHandle handle) { _loader.Release(handle); }
    }
}