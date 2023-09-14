#if UNITY_EDITOR
using System.IO;
using Pancake.SafeArea;
using UnityEditor;
using UnityEngine;

namespace Pancake.SafeAreEditor
{
    internal class SafeAreaPrefabPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (Path.GetExtension(path) != ".prefab") continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var isDirty = false;

                foreach (var safeArea in prefab.GetComponentsInChildren<ISafeAreaUpdatable>(true))
                {
                    safeArea.ResetRect();
                    isDirty = true;
                }

                if (isDirty)
                {
                    EditorUtility.SetDirty(prefab);
#if UNITY_2021_2_OR_NEWER
                    AssetDatabase.SaveAssetIfDirty(prefab);
#else
                    AssetDatabase.SaveAssets();
#endif
                }
            }
        }
    }
}
#endif