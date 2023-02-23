using UnityEditor;
using UnityEngine;

namespace Pancake.Console
{
    internal static class Textures
    {
        private static Texture2D eyeClosed;
        private static Texture2D eyeOpen;
        private static Texture2D solo;
        private static Texture2D remove;


        public static Texture2D EyeClosed
        {
            get
            {
                if (!eyeClosed) eyeClosed = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff");
                return eyeClosed;
            }
        }

        public static Texture2D EyeOpen
        {
            get
            {
                if (!eyeOpen) eyeOpen = EditorGUIUtility.FindTexture("animationvisibilitytoggleon");
                return eyeOpen;
            }
        }

        public static Texture2D Solo
        {
            get
            {
                if (!solo) solo = FindAssetWithPath<Texture2D>("Solo.png", "Plugins/NeedleConsole/CustomConsole/GUI/Textures");
                return solo;
            }
        }

        public static Texture2D Remove
        {
            get
            {
                if (!remove) remove = FindAssetWithPath<Texture2D>("Remove.png", "Plugins/NeedleConsole/CustomConsole/GUI/Textures");
                return remove;
            }
        }

        private static T FindAssetWithPath<T>(string nameAsset, string relativePath) where T : Object
        {
            var upmPath = $"Packages/com.pancake.heart/{relativePath}/{nameAsset}";
            string path = !System.IO.File.Exists(System.IO.Path.GetFullPath(upmPath)) ? $"Assets/heart/{relativePath}/{nameAsset}" : upmPath;
            var t = AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if (t == null) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t as T;
        }
    }
}