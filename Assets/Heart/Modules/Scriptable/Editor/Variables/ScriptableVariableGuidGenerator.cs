using System.Collections.Generic;
using PancakeEditor.Common;

using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    internal class ScriptableVariableGuidGenerator : AssetPostprocessor
    {
        //this gets cleared every time the domain reloads
        private static readonly HashSet<string> GuidsCache = new HashSet<string>();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool isInitialized = SessionState.GetBool("initialized", false);
            if (!isInitialized)
            {
                RegenerateAllGuids();
                SessionState.SetBool("initialized", true);
            }
            else
            {
                OnAssetCreatedOrSaved(importedAssets);
                OnAssetDeleted(deletedAssets);
                OnAssetMoved(movedFromAssetPaths, movedAssets);
            }
        }

        private static void RegenerateAllGuids()
        {
            var scriptableVariables = ProjectDatabase.FindAll<ScriptableBase>();
            foreach (var scriptableVariable in scriptableVariables)
            {
                if (scriptableVariable is IGuid iGuid)
                {
                    if (iGuid.GuidCreateMode != ECreationMode.Auto) continue;
                    iGuid.Guid = GenerateGuid(scriptableVariable);
                    GuidsCache.Add(iGuid.Guid);
                }
            }
        }

        private static void OnAssetCreatedOrSaved(string[] importedAssets)
        {
            foreach (string assetPath in importedAssets)
            {
                if (GuidsCache.Contains(assetPath)) continue;

                var asset = AssetDatabase.LoadAssetAtPath<ScriptableBase>(assetPath);
                if (asset == null || asset is not IGuid iGuid) continue;
                iGuid.Guid = GenerateGuid(asset);
                GuidsCache.Add(iGuid.Guid);
            }
        }

        private static void OnAssetDeleted(string[] deletedAssets)
        {
            foreach (string assetPath in deletedAssets)
            {
                if (!GuidsCache.Contains(assetPath)) continue;

                GuidsCache.Remove(assetPath);
            }
        }

        private static void OnAssetMoved(string[] movedFromAssetPaths, string[] movedAssets)
        {
            OnAssetDeleted(movedFromAssetPaths);
            OnAssetCreatedOrSaved(movedAssets);
        }

        private static string GenerateGuid(ScriptableObject scriptableObject)
        {
            string path = AssetDatabase.GetAssetPath(scriptableObject);
            string guid = AssetDatabase.AssetPathToGUID(path);
            return guid;
        }
    }
}