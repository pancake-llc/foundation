using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pancake.ExLibEditor
{
    public static class EditorCreator
    {
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

        public static TextAsset CreateTextFile(string content, string fileName, string relativePath)
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
        /// Creates a copy of an object. Also adds a number to the copy name.
        /// </summary>
        /// <param name="obj"></param>
        public static void CreateCopyAsset(Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            string copyPath = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CopyAsset(path, copyPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var newAsset = AssetDatabase.LoadMainAssetAtPath(copyPath);
            EditorGUIUtility.PingObject(newAsset);
        }

        /// <summary>
        /// Rename asset
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="newName"></param>
        public static void RenameAsset(Object obj, string newName)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            AssetDatabase.RenameAsset(path, newName);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Creates a Scriptable object at a certain path
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ScriptableObject CreateScriptableAt(System.Type type, string name, string path)
        {
            ProjectDatabase.EnsureDirectoryExists($"{ProjectDatabase.ProjectDir}/{path}");
            var instance = ScriptableObject.CreateInstance(type);
            instance.name = string.IsNullOrEmpty(name) ? type.ToString().Replace("Pancake.Scriptalbe.", "") : name;
            var creationPath = $"{path}/{instance.name}.asset";
            string uniqueFilePath = AssetDatabase.GenerateUniqueAssetPath(creationPath);
            AssetDatabase.CreateAsset(instance, uniqueFilePath);
            EditorGUIUtility.PingObject(instance);
            return instance;
        }
    }
}