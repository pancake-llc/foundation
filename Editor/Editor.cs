using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class Editor
    {
        public static T FindAssetWithPath<T>(string nameAsset, string relativePath) where T : Object
        {
            string path = AssetInPackagePath(relativePath, nameAsset);
            var t = AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if (t == null) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t as T;
        }

        public static T[] FindAssetsWithPath<T>(string nameAsset, string relativePath) where T : Object
        {
            string path = AssetInPackagePath(relativePath, nameAsset);
            var t = AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>().ToArray();
            if (t.Length == 0) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t;
        }

        private static string AssetInPackagePath(string relativePath, string nameAsset, string namePackage = "com.pancake.heart")
        {
            var upmPath = $"Packages/{namePackage}/{relativePath}/{nameAsset}";
            return !File.Exists(Path.GetFullPath(upmPath)) ? $"Assets/heart/{relativePath}/{nameAsset}" : upmPath;
        }
    }
}