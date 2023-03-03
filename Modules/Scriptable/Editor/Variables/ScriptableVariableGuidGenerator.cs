using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal class ScriptableVariableGuidGenerator : AssetPostprocessor
    {
        //this gets cleared every time the domain reloads
        private static readonly HashSet<string> IdsCached = new HashSet<string>();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var isInitialized = SessionState.GetBool("initialized", false);
            if (!isInitialized)
            {
                RegenerateAllGuids();
                SessionState.SetBool("initialized", true);
            }
            else
            {
                OnAssetCreated(importedAssets);
                OnAssetDeleted(deletedAssets);
                OnAssetMoved(movedFromAssetPaths, movedAssets);
            }
        }

        private static void RegenerateAllGuids()
        {
            var scriptableVariableBases = Editor.FindAll<ScriptableVariableBase>();
            foreach (var scriptableVariable in scriptableVariableBases)
            {
                scriptableVariable.Id = GenerateGuid(scriptableVariable);
                IdsCached.Add(scriptableVariable.Id);
            }
        }

        private static void OnAssetCreated(string[] importedAssets)
        {
            foreach (string assetPath in importedAssets)
            {
                if (IdsCached.Contains(assetPath)) continue;

                var asset = AssetDatabase.LoadAssetAtPath<ScriptableVariableBase>(assetPath);
                if (asset == null) continue;

                asset.Id = GenerateGuid(asset);
                IdsCached.Add(asset.Id);
            }
        }

        private static void OnAssetDeleted(string[] deletedAssets)
        {
            foreach (string assetPath in deletedAssets)
            {
                if (!IdsCached.Contains(assetPath)) continue;

                IdsCached.Remove(assetPath);
            }
        }

        private static void OnAssetMoved(string[] movedFromAssetPaths, string[] movedAssets)
        {
            OnAssetDeleted(movedFromAssetPaths);
            OnAssetCreated(movedAssets);
        }

        private static string GenerateGuid(ScriptableObject scriptableObject)
        {
            string path = AssetDatabase.GetAssetPath(scriptableObject);
            string guid = AssetDatabase.AssetPathToGUID(path);
            return guid;
        }
    }
}