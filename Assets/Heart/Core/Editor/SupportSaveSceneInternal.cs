using UnityEditor;
using UnityEditor.SceneManagement;

namespace PancakeEditor
{
    [InitializeOnLoad]
    internal class SupportSaveSceneInternal
    {
        static SupportSaveSceneInternal() { EditorApplication.playModeStateChanged += OnPlayModeStateChanged; }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (Internal.InternalData.Settings.requireSceneSave)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                    }
                }
            }
        }
    }
}