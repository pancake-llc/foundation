using JetBrains.Annotations;
using UnityEditor;

namespace Pancake.Editor
{
    public static class MenuToolsCreator
    {
        [MenuItem("Tools/Project Settings", priority = 100000), UsedImplicitly]
        private static void OpenSettings()
        {
#if UNITY_2019_1_OR_NEWER
            SettingsService.OpenProjectSettings("Project/Console");
#else
			EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
#endif
        }

        [MenuItem("Tools/User Preferences", priority = 100000), UsedImplicitly]
        private static void OpenPreferences()
        {
#if UNITY_2019_1_OR_NEWER
            SettingsService.OpenUserPreferences("Preferences/Console");
#else
			EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
#endif
        }
    }
}