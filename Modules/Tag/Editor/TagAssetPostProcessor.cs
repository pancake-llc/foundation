using UnityEditor;

namespace Pancake.TagEditor
{
    public class TagAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            TagSettingsProvider.CheckForCollisions();
        }
    }
}