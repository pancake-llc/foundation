using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.AssetLoader
{
    [CreateAssetMenu(fileName = "PreloadedAssetLoader", menuName = "Pancake/Asset Loader/Preloaded Asset Loader")]
    [EditorIcon("scriptable_interface")]
    public sealed class PreloadedAssetLoaderObject : AssetLoaderObject, IAssetLoader
    {
        [SerializeField] private List<KeyAssetPair> preloadedObjects = new List<KeyAssetPair>();

        private readonly PreloadedAssetLoader _loader = new PreloadedAssetLoader();

        public List<KeyAssetPair> PreloadedObjects => preloadedObjects;

        private void OnEnable()
        {
            foreach (var preloadedObject in preloadedObjects)
            {
                if (string.IsNullOrEmpty(preloadedObject.Key)) continue;

                if (_loader.PreloadedObjects.ContainsKey(preloadedObject.Key)) continue;

                _loader.PreloadedObjects.Add(preloadedObject.Key, preloadedObject.Asset);
            }
        }

        private void OnDisable() { _loader?.PreloadedObjects.Clear(); }

        public override AssetLoadHandle<T> Load<T>(string key) { return _loader.Load<T>(key); }

        public override AssetLoadHandle<T> LoadAsync<T>(string key) { return _loader.LoadAsync<T>(key); }

        public override void Release(AssetLoadHandle handle) { _loader.Release(handle); }

        [Serializable]
        public sealed class KeyAssetPair
        {
            public enum KeySourceType
            {
                InputField,
                AssetName
            }

            [SerializeField] private KeySourceType keySource;
            [SerializeField] private string key;
            [SerializeField] private Object asset;

            public string Key { get => GetKey(); set => key = value; }
            public KeySourceType KeySource { get => keySource; set => keySource = value; }
            public Object Asset { get => asset; set => asset = value; }

            private string GetKey()
            {
                if (keySource == KeySourceType.AssetName) return asset == null ? "" : asset.name;
                return key;
            }
        }
    }
}