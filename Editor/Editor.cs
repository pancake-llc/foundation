using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public static partial class Editor
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
        /// thanks @JoshuaMcKenzie and @Edvard-D
        /// remove all empty object reference elements
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int RemoveEmptyArrayElements(this SerializedProperty list)
        {
            var elementsRemoved = 0;
            if (list == null) return elementsRemoved;

            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                var element = list.GetArrayElementAtIndex(i);
                if (element.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    list.RemoveElement(i);
                    elementsRemoved++;
                }
            }

            return elementsRemoved;
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        public static void RemoveElement(this SerializedProperty list, int index)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (!list.isArray) throw new ArgumentException("Property is not an array");

            if (index < 0 || index >= list.arraySize) throw new ArgumentOutOfRangeException(nameof(list));

            list.GetArrayElementAtIndex(index).SetPropertyValue(null);
            list.DeleteArrayElementAtIndex(index);

            list.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(this SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = value as AnimationCurve;
                    break;

                case SerializedPropertyType.ArraySize:
                    property.intValue = System.Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = System.Convert.ToBoolean(value);
                    break;

                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds?) value ?? new Bounds();
                    break;

                case SerializedPropertyType.Character:
                    property.intValue = System.Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.Color:
                    property.colorValue = (Color?) value ?? new Color();
                    break;

                case SerializedPropertyType.Float:
                    property.floatValue = System.Convert.ToSingle(value);
                    break;

                case SerializedPropertyType.Integer:
                    property.intValue = System.Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.LayerMask:
                    property.intValue = (value as LayerMask?)?.value ?? System.Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as UnityEngine.Object;
                    break;

                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion?) value ?? Quaternion.identity;
                    break;

                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect?) value ?? new Rect();
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = value as string;
                    break;

                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2?) value ?? Vector2.zero;
                    break;

                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3?) value ?? Vector3.zero;
                    break;

                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4?) value ?? Vector4.zero;
                    break;
            }
        }

        /// <summary>
        /// get inspector type to display window
        /// </summary>
        public static Type InspectorWindow => typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");

        public static string GetSizeInMemory(this long byteSize)
        {
            string[] sizes = {"B", "KB", "MB", "GB", "TB"};
            double len = Convert.ToDouble(byteSize);
            int order = 0;
            while (len >= 1024D && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:0.##} {1}", len, sizes[order]);
        }

        /// <summary>
        /// Create Texture2D from color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Texture2D CreateTexture(Color color)
        {
            var result = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            result.SetPixel(0, 0, color);
            result.Apply();
            return result;
        }

        internal static TextAsset CreateTextFile(string content, string fileName, string relativePath)
        {
            string filePath = relativePath + "/" + fileName;
            string folderPath = Directory.GetParent(Application.dataPath)?.FullName + "/" + relativePath;
            string fullPath = folderPath + "/" + fileName;

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            if (File.Exists(fullPath)) throw new IOException($"A file with the name {filePath} already exists.");

            File.WriteAllText(fullPath, content);
            AssetDatabase.Refresh();
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            return textAsset;
        }

        /// <summary>
        /// Rename asset
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="newName"></param>
        internal static void RenameAsset(Object obj, string newName)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            AssetDatabase.RenameAsset(path, newName);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Creates a copy of an object. Also adds a number to the copy name.
        /// </summary>
        /// <param name="obj"></param>
        public static void CreateCopyAsset(Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            string copyPath = GenerateCopyPath(obj.name, path);
            AssetDatabase.CopyAsset(path, copyPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var newAsset = AssetDatabase.LoadMainAssetAtPath(copyPath);
            EditorGUIUtility.PingObject(newAsset);
        }

        private static string GenerateCopyPath(string fileName, string filePath)
        {
            var count = 0;
            string copyPath;
            do
            {
                count++;
                var newName = $"{fileName}_{count}";
                copyPath = filePath.Replace($"{fileName}.asset", $"{newName}.asset");
            } while (File.Exists(copyPath));

            return copyPath;
        }

        /// <summary>
        /// Deletes an object after showing a confirmation dialog.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool DeleteObjectWithConfirmation(Object obj)
        {
            bool confirmDelete = EditorUtility.DisplayDialog("Delete " + obj.name + "?", "Are you sure you want to delete '" + obj.name + "'?", "Yes", "No");
            if (confirmDelete)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                AssetDatabase.DeleteAsset(path);
                return true;
            }

            return false;
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
        private static string GetPathInCurrentEnvironent(string fullRelativePath)
        {
            var upmPath = $"Packages/com.pancake.heart/{fullRelativePath}";
            var normalPath = $"Assets/heart/{fullRelativePath}";
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
    }
}