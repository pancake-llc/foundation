using System.Collections.Generic;
using PancakeEditor.Sound;
using UnityEditor;

namespace PancakeEditor
{
    public class AssetPostprocessorEditor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            OnDeleteAssets(deletedAssets);
            OnReimportAsset(importedAssets);
        }

        private static void OnReimportAsset(string[] importedAssets)
        {
            if (importedAssets.Length > 0 && EditorWindow.HasOpenInstances<Wizard>())
            {
                AudioWindow.OnPostprocessAllAssets();
            }
        }

        private static void OnDeleteAssets(string[] deletedAssets)
        {
            if (deletedAssets == null || deletedAssets.Length == 0) return;

            EditorAudioEx.RemoveEmptyDatas();

            if (EditorWindow.HasOpenInstances<Wizard>())
            {
                foreach (string path in deletedAssets)
                {
                    if (path.Contains(".asset")) AudioWindow.RemoveAssetEditor(AssetDatabase.AssetPathToGUID(path));
                }
            }
        }
    }
}