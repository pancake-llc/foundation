using Pancake.ExLib.Reflection;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.ExLibEditor
{
    public static class ScreenUtility
    {
        public static Rect GetCenter()
        {
            Type containerWindowType = typeof(ScriptableObject).Subclasses().Where(t => t.Name == "ContainerWindow").FirstOrDefault();
            if (containerWindowType == null)
            {
                return Rect.zero;
            }

            FieldInfo showModeField = containerWindowType.GetField("m_ShowMode", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            PropertyInfo positionProperty = containerWindowType.GetProperty("position", BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
            {
                return Rect.zero;
            }

            Object[] windows = Resources.FindObjectsOfTypeAll(containerWindowType);
            for (int i = 0; i < windows.Length; i++)
            {
                Object window = windows[i];
                var showmode = (int) showModeField.GetValue(window);
                if (showmode == 4)
                {
                    Rect position = (Rect) positionProperty.GetValue(window, null);
                    return position;
                }
            }

            return Rect.zero;
        }
    }
}