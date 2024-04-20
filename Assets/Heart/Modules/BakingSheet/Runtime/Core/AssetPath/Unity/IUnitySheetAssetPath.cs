namespace Pancake.BakingSheet.Unity
{
    public interface IUnitySheetAssetPath : ISheetAssetPath
    {
        string MetaType { get; }
        string SubAssetName { get; }
    }
}