#if PANCAKE_GAM
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Pancake.LevelBase
{
    [InitializeOnLoad]
    public static class LevelDebug
    {
        public static bool IsTest
        {
            get => EditorPrefs.GetBool($"{Application.identifier}_leveldebug_istest");
            set => EditorPrefs.SetBool($"{Application.identifier}_leveldebug_istest", value);
        }

        public static string PathLevelAsset
        {
            get => EditorPrefs.GetString($"{Application.identifier}_leveldebug_pathlevel");
            set => EditorPrefs.SetString($"{Application.identifier}_leveldebug_pathlevel", value);
        }

        private static BaseLevel levelPrefab;

        public static BaseLevel LevelAsset
        {
            get
            {
                if (levelPrefab != null) return levelPrefab;

                if (string.IsNullOrEmpty(PathLevelAsset)) return null;

                levelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathLevelAsset)?.GetComponent<BaseLevel>();
                return levelPrefab;
            }
        }

        static LevelDebug() { EditorApplication.playModeStateChanged += OnModeChange; }

        private static void OnModeChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                IsTest = false;
                PathLevelAsset = string.Empty;
                levelPrefab = null;
            }
        }
    }
}
#endif
#endif