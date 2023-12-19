using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesGreeneryDrawer
    {
        public static void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Import Greenery Essential Resources", GUILayout.MaxHeight(30f)))
            {
                AssetDatabase.ImportPackage(ProjectDatabase.GetPathInCurrentEnvironent("Modules/Greenery/UnityPackage/greenery_asset.unitypackage"), true);
            }

            GUI.enabled = true;
        }
    }
}