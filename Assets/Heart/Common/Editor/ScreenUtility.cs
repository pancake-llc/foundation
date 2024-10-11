using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PancakeEditor.Common
{
    public static class ScreenUtility
    {
        public static Rect GetCenter()
        {
            var containerWindowType = typeof(ScriptableObject).Subclasses().FirstOrDefault(t => t.Name == "ContainerWindow");
            if (containerWindowType == null)
            {
                return Rect.zero;
            }

            var showModeField = containerWindowType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
            var positionProperty = containerWindowType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
            {
                return Rect.zero;
            }

            var windows = Resources.FindObjectsOfTypeAll(containerWindowType);
            for (var i = 0; i < windows.Length; i++)
            {
                var window = windows[i];
                var showmode = (int) showModeField.GetValue(window);
                if (showmode == 4)
                {
                    var position = (Rect) positionProperty.GetValue(window, null);
                    return position;
                }
            }

            return Rect.zero;
        }

        public static bool IsLeftClicking(Event currentEvent) { return (currentEvent.type == EventType.MouseDown && currentEvent.button == 0); }
    }
}