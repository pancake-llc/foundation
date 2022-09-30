namespace Pancake
{
    [System.Serializable]
    public sealed class AssetEntry
    {
        [UnityEngine.SerializeField] private string guid;
        [UnityEngine.SerializeField] private UnityEngine.Object asset;
        public string Guid { get => guid; private set => guid = value; }

        public UnityEngine.Object Asset { get => asset; private set => asset = value; }

        public AssetEntry(string guid, UnityEngine.Object asset)
        {
            Guid = guid;
            Asset = asset;
        }
    }
}