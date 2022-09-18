namespace Pancake
{
    [System.Serializable]
    public sealed class AssetEntry
    {
        [field: UnityEngine.SerializeField] public string Guid { get; private set; }

        [field: UnityEngine.SerializeField] public UnityEngine.Object Asset { get; private set; }

        public AssetEntry(string guid, UnityEngine.Object asset)
        {
            Guid = guid;
            Asset = asset;
        }
    }
}