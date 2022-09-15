

namespace Pancake
{
    [System.Serializable]
    internal sealed class AssetEntry
    {
        [field: UnityEngine.SerializeField] public string Guid { get; private set; }
#if UNITY_EDITOR
        [field: UnityEngine.SerializeField] public UnityEngine.Object Asset { get; private set; }
#endif

#if UNITY_EDITOR
        public AssetEntry(string guid, UnityEngine.Object asset)
        {
            Guid = guid;
            Asset = asset;
        }
#endif

        public AssetEntry(string guid) { Guid = guid; }
    }
}