using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.ContextMenu
{
    [InitializeOnLoad]
    internal static class ProjectWindowMenuCustomizer
    {
        private static ProjectWindowMenuSettings settings;

        static ProjectWindowMenuCustomizer() { EditorApplication.projectWindowItemOnGUI += OnGUI; }

        private static ProjectWindowMenuSettings GetSettings()
        {
            if (settings == null)
            {
                settings = AssetDatabase.FindAssets("t:ProjectWindowMenuSettings")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<ProjectWindowMenuSettings>)
                    .FirstOrDefault();
            }

            return settings;
        }

        private static void OnGUI(string guid, Rect selectionRect)
        {
            if (Event.current.type != EventType.ContextClick) return;

            var s = GetSettings();

            if (s == null) return;

            Event.current.Use();

            var list = s.List;
            var genericMenu = new GenericMenu();

            for (var i = 0; i < list.Count; i++)
            {
                var data = list[i];
                string name = data.Name;

                if (data.IsSeparator)
                {
                    genericMenu.AddSeparator(name);
                }
                else
                {
                    string menuItemPath = data.MenuItemPath;
                    var content = new GUIContent(name);

                    genericMenu.AddItem(content: content, @on: false, func: () => EditorApplication.ExecuteMenuItem(menuItemPath));
                }
            }

            genericMenu.ShowAsContext();
        }
    }
}