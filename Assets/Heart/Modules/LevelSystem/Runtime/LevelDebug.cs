#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


namespace Pancake.LevelSystem
{
    [InitializeOnLoad]
    public static class LevelDebug
    {
        private static LevelComponent levelPrefab;

        public static bool IsTest
        {
            get => SessionState.GetBool($"{Application.identifier}_leveldebug_istest", false);
            set => SessionState.SetBool($"{Application.identifier}_leveldebug_istest", value);
        }

        internal static string PathLevelPrefab
        {
            get => SessionState.GetString($"{Application.identifier}_leveldebug_pathlevelprefab", "");
            set => SessionState.SetString($"{Application.identifier}_leveldebug_pathlevelprefab", value);
        }

        public static LevelComponent LevelPrefab
        {
            get
            {
                if (levelPrefab != null) return levelPrefab;
                if (string.IsNullOrEmpty(PathLevelPrefab)) return null;
                levelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathLevelPrefab)?.GetComponent<LevelComponent>();
                return levelPrefab;
            }
        }

        static LevelDebug() { EditorApplication.playModeStateChanged += OnPlayModeStateChanged; }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                IsTest = false;
                PathLevelPrefab = string.Empty;
                levelPrefab = null;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }
    }
}
#endif