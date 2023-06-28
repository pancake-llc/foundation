using System.Collections.Generic;
using UnityEditor;

namespace Pancake.BTagEditor
{
    public class BTagAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            BTagSettingsProvider.CheckForCollisions();
        }
    }
}