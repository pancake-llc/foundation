using System.IO;
using System.Linq;
using Pancake;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class Editor
    {
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
        private static string AssetInPackagePath(string relativePath, string nameAsset) { return GetPathInCurrentEnvironent($"{relativePath}/{nameAsset}"); }

        /// <summary>
        /// Returns the exact path used for the current environment
        /// In use under upm package => return upm path
        /// In use directly in folder Assets => return normal path under folder, (only should use for development, recomment use upm instead)
        /// <param name="fullRelativePath">path to convert, it will include file name and file extension</param>
        /// </summary>
        internal static string GetPathInCurrentEnvironent(string fullRelativePath)
        {
            var upmPath = $"Packages/com.pancake.heart/{fullRelativePath}";
            var normalPath = $"Assets/heart/{fullRelativePath}";
            return !File.Exists(Path.GetFullPath(upmPath)) ? normalPath : upmPath;
        }
        
        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        public static void RemoveElement(this SerializedProperty list, int index)
        {
            if (list == null) throw Error.ArgumentNullException("list");

            if (!list.isArray) throw Error.arg
                throw new ArgumentException("Property is not an array");

            if (index < 0 || index >= list.arraySize)
                throw new IndexOutOfRangeException();

            list.GetArrayElementAtIndex(index).SetPropertyValue(null);
            list.DeleteArrayElementAtIndex(index);

            list.serializedObject.ApplyModifiedProperties();
        }
    }
}