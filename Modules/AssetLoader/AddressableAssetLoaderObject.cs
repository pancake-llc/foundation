#if PANCAKE_ADDRESSABLE
using UnityEngine;

namespace Pancake.AssetLoader
{
    [CreateAssetMenu(fileName = "AddressableAssetLoader", menuName = "Pancake/Asset Loader/Addressable Asset Loader")]
    [EditorIcon("scriptable_interface")]
    public sealed class AddressableAssetLoaderObject : AssetLoaderObject, IAssetLoader
    {
        private readonly AddressableAssetLoader _loader = new AddressableAssetLoader();

        public override AssetLoadHandle<T> Load<T>(string key) { return _loader.Load<T>(key); }

        public override AssetLoadHandle<T> LoadAsync<T>(string key) { return _loader.LoadAsync<T>(key); }

        public override void Release(AssetLoadHandle handle) { _loader.Release(handle); }
    }
}
#endif