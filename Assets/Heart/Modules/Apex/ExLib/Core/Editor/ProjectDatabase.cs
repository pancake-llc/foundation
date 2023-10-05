using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.ExLibEditor
{
    public static class ProjectDatabase
    {
        public const string DEFAULT_PATH_SCRIPT_GENERATED = "Assets/_Root/Scripts/Generated";
        public const string DEFAULT_PATH_SCRIPTABLE_ASSET_GENERATED = "Assets/_Root/Storages/Generated/Scriptable";
        public static string ProjectDir => Directory.GetParent(Application.dataPath)?.FullName;

        public static void EnsureDirectoryExists(string path)
        {
            if (Directory.Exists(path)) return;
            if (!EditorUtility.DisplayDialog("Create Directory", $"Do you want to create '{path}'?", "Yes", "No")) throw new Exception("Directory not created");
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Find all asset with type <typeparamref name="T"/>
        /// </summary>
        /// <param name="path">path use for find all asset</param>
        /// <typeparam name="T">type asset to find</typeparam>
        /// <returns></returns>
        public static List<T> FindAll<T>(string path) where T : Object
        {
            var results = new List<T>();
            var filter = $"t:{typeof(T).Name}";
            var assetNames = AssetDatabase.FindAssets(filter, new[] {path});

            foreach (string assetName in assetNames)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetName);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset == null) continue;

                results.Add(asset);
            }

            return results;
        }

        /// <summary>
        /// Find all asset with type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> FindAll<T>() where T : Object
        {
            var results = new List<T>();
            var filter = $"t:{typeof(T).Name}";
            var assetNames = AssetDatabase.FindAssets(filter);

            foreach (string assetName in assetNames)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetName);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset == null) continue;

                results.Add(asset);
            }

            return results;
        }

        /// <summary>
        /// Search for assets with type <typeparamref name="T"/> by specified <paramref name="fullPath"/>
        /// </summary>
        /// <param name="fullPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindAssetWithPath<T>(string fullPath) where T : Object
        {
            string path = GetPathInCurrentEnvironent(fullPath);
            var t = AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if (t == null) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t as T;
        }
        
        /// <summary>
        /// Search for assets with type <typeparamref name="T"/> by specified <paramref name="nameAsset"/> and relative path <paramref name="relativePath"/>
        /// </summary>
        public static T FindAssetWithPath<T>(string nameAsset, string relativePath) where T : Object
        {
            string path = AssetInPackagePath(relativePath, nameAsset);
            var t = AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if (t == null) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t as T;
        }

        /// <summary>
        /// Search for all assets with type <typeparamref name="T"/> by specified <paramref name="nameAsset"/> and relative path <paramref name="relativePath"/>
        /// typical case is loading sprite sheet, it will return sprite array
        /// </summary>
        public static T[] FindAssetsWithPath<T>(string nameAsset, string relativePath) where T : Object
        {
            string path = AssetInPackagePath(relativePath, nameAsset);
            var t = AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>().ToArray();
            if (t.Length == 0) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t;
        }

        /// <summary>
        /// Returns the exact path used for the current environment
        /// In use under upm package => return upm path
        /// In use directly in folder Assets => return normal path under folder, (only should use for development, recomment use upm instead)
        /// <param name="relativePath">path to convert</param>
        /// <param name="nameAsset"> it will include file name and file extension</param>
        /// </summary>
        public static string AssetInPackagePath(string relativePath, string nameAsset) { return GetPathInCurrentEnvironent($"{relativePath}/{nameAsset}"); }

        /// <summary>
        /// Returns the exact path used for the current environment
        /// In use under upm package => return upm path
        /// In use directly in folder Assets => return normal path under folder, (only should use for development, recomment use upm instead)
        /// <param name="fullRelativePath">path to convert, it will include file name and file extension</param>
        /// </summary>
        public static string GetPathInCurrentEnvironent(string fullRelativePath)
        {
            var upmPath = $"Packages/com.pancake.heart/{fullRelativePath}";
            var normalPath = $"Assets/Heart/{fullRelativePath}";
            return !File.Exists(Path.GetFullPath(upmPath)) ? normalPath : upmPath;
        }

        private static bool GetEmptyDirectories(DirectoryInfo dir, List<DirectoryInfo> results)
        {
            var isEmpty = true;
            try
            {
                isEmpty = dir.GetDirectories().Count(x => !GetEmptyDirectories(x, results)) == 0 // Are sub directories empty?
                          && dir.GetFiles("*.*").All(x => x.Extension == ".meta"); // No file exist?
            }
            catch
            {
                //
            }

            // Store empty directory to results.
            if (isEmpty) results.Add(dir);

            return isEmpty;
        }

        public static void RemoveAllEmptyFolder(DirectoryInfo dir)
        {
            var result = new List<DirectoryInfo>();
            GetEmptyDirectories(dir, result);

            if (result.Count > 0)
            {
                foreach (var d in result)
                {
                    FileUtil.DeleteFileOrDirectory(d.FullName);
                    FileUtil.DeleteFileOrDirectory(d.Parent + "\\" + d.Name + ".meta"); // unity 2020.2 need to delete the meta too
                }

                AssetDatabase.Refresh();
            }
        }

        public static void SelectAndPing(this Object @object)
        {
            UnityEditor.Selection.activeObject = @object;
            UnityEditor.EditorApplication.delayCall += () => UnityEditor.EditorGUIUtility.PingObject(@object);
        }
    }
}