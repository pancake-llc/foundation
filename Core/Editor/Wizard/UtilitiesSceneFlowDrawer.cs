using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace PancakeEditor
{
    public static class UtilitiesSceneFlowDrawer
    {
        public static void OnInspectorGUI()
        {
            var launcherScene = AssetDatabase.LoadAssetAtPath("Assets/_Root/Scenes/[C]launcher", typeof(Scene));
            var menuScene = AssetDatabase.LoadAssetAtPath("Assets/_Root/Scenes/[C]launcher", typeof(Scene));
            var persistentScene = AssetDatabase.LoadAssetAtPath("Assets/_Root/Scenes/[C]launcher", typeof(Scene));
            //var launcherScene = AssetDatabase.LoadAssetAtPath("Assets/_Root/Scenes/[C]launcher", typeof(Scene));
        }
    }
}