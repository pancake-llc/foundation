#if UNITY_EDITOR

using UnityEditor;

namespace Pancake.Editor
{
    /// <summary>
    /// Utilities for editor.
    /// </summary>
    public struct EditorUtilities
    {
        private static float unscaledDeltaTime;
        private static double lastTimeSinceStartup = -0.01;


        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update += () =>
            {
                unscaledDeltaTime = (float) (EditorApplication.timeSinceStartup - lastTimeSinceStartup);
                lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            };
        }


        public static float UnscaledDeltaTime => unscaledDeltaTime;


        public static PlayModeStateChange PlayMode
        {
            get
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (EditorApplication.isPlaying) return PlayModeStateChange.EnteredPlayMode;
                    return PlayModeStateChange.ExitingEditMode;
                }

                if (EditorApplication.isPlaying) return PlayModeStateChange.ExitingPlayMode;
                return PlayModeStateChange.EnteredEditMode;
            }
        }
    }
}

#endif