using System.Collections.Generic;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    class ScriptableVariableGuidGenerator : AssetPostprocessor
    {
        //this gets cleared every time the domain reloads
        private static readonly HashSet<string> GuidsCache = new HashSet<string>();

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
            var scriptableVariableBases = ProjectDatabase.FindAll<ScriptableVariableBase>();
            foreach (var scriptableVariable in scriptableVariableBases)
            {
                scriptableVariable.Guid = GenerateGuid(scriptableVariable);
                GuidsCache.Add(scriptableVariable.Guid);
            }
        }

        private static void OnAssetCreated(string[] importedAssets)
        {
            foreach (var assetPath in importedAssets)
            {
                if (GuidsCache.Contains(assetPath))
                    continue;

                var asset = AssetDatabase.LoadAssetAtPath<ScriptableVariableBase>(assetPath);
                if (asset == null)
                    continue;

                asset.Guid = GenerateGuid(asset);
                GuidsCache.Add(asset.Guid);
            }
        }

        private static void OnAssetDeleted(string[] deletedAssets)
        {
            foreach (var assetPath in deletedAssets)
            {
                if (!GuidsCache.Contains(assetPath))
                    continue;

                GuidsCache.Remove(assetPath);
            }
        }

        private static void OnAssetMoved(string[] movedFromAssetPaths, string[] movedAssets)
        {
            OnAssetDeleted(movedFromAssetPaths);
            OnAssetCreated(movedAssets);
        }

        private static string GenerateGuid(ScriptableObject scriptableObject)
        {
            var path = AssetDatabase.GetAssetPath(scriptableObject);
            var guid = AssetDatabase.AssetPathToGUID(path);
            return guid;
        }
    }
}