using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PancakeEditor.Finder
{
    internal class EnumDrawer
    {
        internal class EnumInfo
        {
            public static readonly Dictionary<Type, EnumInfo> Cache = new();
            public readonly GUIContent[] contents;
            public readonly Array values;

            public static EnumInfo Get(Type type)
            {
                if (Cache.TryGetValue(type, out var result))
                {
                    return result;
                }

                result = new EnumInfo(type);
                Cache.Add(type, result);
                return result;
            }

            public EnumInfo(Type enumType)
            {
                string[] names = Enum.GetNames(enumType);

                values = Enum.GetValues(enumType);
                contents = new GUIContent[names.Length];
                for (var i = 0; i < names.Length; i++)
                {
                    contents[i] = MyGUIContent.FromString(names[i]);
                }
            }

            public EnumInfo(params object[] enumValues)
            {
                values = enumValues;
                contents = new GUIContent[values.Length];
                for (var i = 0; i < values.Length; i++)
                {
                    contents[i] = MyGUIContent.FromString(enumValues[i].ToString());
                }
            }

            public int IndexOf(object enumValue) { return Array.IndexOf(values, enumValue); }

            public object ValueAt(int index) { return values.GetValue(index); }
        }

        [NonSerialized] internal EnumInfo enumInfo;
        public int index;
        public string tooltip;

        public bool DrawLayout<T>(ref T enumValue, params GUILayoutOption[] options)
        {
            if (enumInfo == null)
            {
                var enumType = enumValue.GetType();
                enumInfo = EnumInfo.Get(enumType);
                index = enumInfo.IndexOf(enumValue);
            }

            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
            {
                GUILayout.Label(enumInfo.contents[index], EditorStyles.toolbarPopup, options);
                return false;
            }

            int nIndex = EditorGUILayout.Popup(index, enumInfo.contents, EditorStyles.toolbarPopup, options);
            if (nIndex == index)
            {
                // Debug.LogWarning($"Same index: {nIndex} | {index}");
                return false;
            }

            index = nIndex;
            enumValue = (T) enumInfo.ValueAt(index);
            return true;
        }

        public bool Draw<T>(Rect rect, ref T enumValue)
        {
            if (enumInfo == null)
            {
                var enumType = enumValue.GetType();
                enumInfo = EnumInfo.Get(enumType);
                index = enumInfo.IndexOf(enumValue);
            }

            if (Event.current.type == EventType.Layout) return false;
            if (Event.current.type == EventType.Repaint)
            {
                var content = enumInfo.contents[index];
                if (!string.IsNullOrEmpty(tooltip)) content.tooltip = tooltip;
                GUI.Label(rect, content, EditorStyles.toolbarPopup);
                return false;
            }

            var nIndex = EditorGUI.Popup(rect, index, enumInfo.contents, EditorStyles.toolbarPopup); //, options
            if (nIndex != index)
            {
                index = nIndex;
                enumValue = (T) enumInfo.ValueAt(index);
                return true;
            }

            return false;
        }
    }
}