#if UNITY_EDITOR
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PancakeEditor
{
    public static class MenuSceneItemCreator
    {
        [MenuItem("Tools/Open Scene/Launcher &2", priority = 500), UsedImplicitly]
        private static void OpenLauncherScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene("Assets/_Root/Scenes/launcher.unity");
        }

        [MenuItem("Tools/Open Scene/Persistent &6", priority = 503), UsedImplicitly]
        private static void OpenPersistentScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene("Assets/_Root/Scenes/persistent.unity");
        }

        [MenuItem("Tools/Open Scene/Menu &3", priority = 501), UsedImplicitly]
        private static void OpenMenuScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene("Assets/_Root/Scenes/menu.unity");
        }

        [MenuItem("Tools/Open Scene/Gameplay &4", priority = 502), UsedImplicitly]
        private static void OpenGameplayScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene("Assets/_Root/Scenes/gameplay.unity");
        }
    }
}
#endif