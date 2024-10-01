using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using Editor = PancakeEditor.Common.Editor;

namespace PancakeEditor
{
    public static class Inspector
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            var globalEventHandler = typeof(EditorApplication).GetFieldValue<EditorApplication.CallbackFunction>("globalEventHandler");
            typeof(EditorApplication).SetFieldValue("globalEventHandler", ComponentShortcuts + (globalEventHandler - ComponentShortcuts));
        }

        private static void ComponentShortcuts()
        {
            if (EditorWindow.mouseOverWindow is not EditorWindow hoveredWindow) return;
            if (!hoveredWindow) return;
            if (hoveredWindow.GetType() != TypeExtensions.InspectorWindow && hoveredWindow.GetType() != TypeExtensions.PropertyEditor) return;
            if (!Editor.CurrentEvent.IsKeyDown) return;
            if (Editor.CurrentEvent.KeyCode == KeyCode.None) return;

            CheckDeleteComponent();
        }

        private static void CheckDeleteComponent()
        {
            if (Editor.CurrentEvent.HoldingAnyModifierKey) return;
            if (!Editor.CurrentEvent.IsKeyDown || Editor.CurrentEvent.KeyCode != KeyCode.X) return;
            
        }
    }
}