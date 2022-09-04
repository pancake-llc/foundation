using UnityEditor;
using System.Collections.Generic;
using System;

namespace Pancake.Editor.Finder
{
    /// <summary>
    /// The purpose of this class is to try detecting asset changes to automatically update the ProjectFinder database.
    /// </summary>
    public class AssetProcessor : UnityEditor.AssetModificationProcessor
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            EditorApplication.update += OnUpdate;
        }

        /// <summary>
        /// Some callbacks must be delayed on next frame
        /// </summary>
        private static void OnUpdate()
        {
            if (Actions.Count > 0) {
                while (Actions.Count > 0) {
                    Actions.Dequeue()?.Invoke();
                }
                ProjectFinder.SaveDatabase();
            }
        }

        private static Queue<Action> Actions = new Queue<Action>();

        static string[] OnWillSaveAssets(string[] paths)
        {
            if (ProjectFinderData.IsUpToDate) {
                Actions.Enqueue(() => {
                    foreach (string path in paths) {
                        var removedAsset = ProjectFinder.RemoveAssetFromDatabase(path);
                        ProjectFinder.AddAssetToDatabase(path, removedAsset?.referencers);
                    }
                });
            }
            return paths;
        }

        static void OnWillCreateAsset(string assetName)
        {
            if (ProjectFinderData.IsUpToDate) {
                Actions.Enqueue(() => {
                    ProjectFinder.AddAssetToDatabase(assetName);
                });
            }
        }

        static AssetDeleteResult OnWillDeleteAsset(string assetName, RemoveAssetOptions removeAssetOptions)
        {
            if (ProjectFinderData.IsUpToDate) {
                Actions.Enqueue(() => {
                    ProjectFinder.RemoveAssetFromDatabase(assetName);
                });
            }
            return AssetDeleteResult.DidNotDelete;
        }

        static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (ProjectFinderData.IsUpToDate) {
                Actions.Enqueue(() => {
                    ProjectFinder.RemoveAssetFromDatabase(sourcePath);
                    ProjectFinder.AddAssetToDatabase(destinationPath);
                });
            }
            return AssetMoveResult.DidNotMove;
        }
    }
}