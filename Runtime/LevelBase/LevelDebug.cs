#if UNITY_EDITOR
using UnityEditor;

namespace Pancake.LevelBase
{
    [InitializeOnLoad]
    public static class LevelDebug
    {
        public static bool isTest;
        public static BaseLevel levelTest;

        static LevelDebug() { EditorApplication.playModeStateChanged += OnModeChange; }

        private static void OnModeChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                LevelDebug.isTest = false;
                LevelDebug.levelTest = null;
            }
        }
    }
}
#endif