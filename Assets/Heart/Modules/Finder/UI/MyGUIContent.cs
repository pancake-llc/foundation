using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    internal static class MyGUIContent
    {
        // Cache to improve performance
        private static readonly Dictionary<string, GUIContent> StringMap = new();
        private static readonly Dictionary<string, GUIContent> TooltipMap = new();
        private static readonly Dictionary<int, GUIContent> IntMap = new();
        private static readonly Dictionary<Texture, GUIContent> TexMap = new();
        private static readonly Dictionary<string, GUIContent> StringTexMap = new();
        private static readonly Dictionary<Type, GUIContent> TypeMap = new();

        public static void Release()
        {
            StringMap.Clear();
            TexMap.Clear();
        }

        public static GUIContent FromString(string title, string tooltip = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                Debug.LogWarning("Title is null or empty!");
                return GUIContent.none;
            }

            if (StringMap.TryGetValue(title, out var result)) return result;
            result = new GUIContent(title, tooltip);
            StringMap.Add(title, result);
            return result;
        }

        public static GUIContent FromType(Type t, string tooltip = null)
        {
            if (TypeMap.TryGetValue(t, out var result)) return result;
            result = new GUIContent(EditorGUIUtility.ObjectContent(null, t).image, tooltip);
            TypeMap.Add(t, result);
            return result;
        }

        public static GUIContent Tooltip(string tooltip)
        {
            if (TooltipMap.TryGetValue(tooltip, out var result)) return result;
            result = new GUIContent(string.Empty, tooltip);
            TooltipMap.Add(tooltip, result);
            return result;
        }

        public static GUIContent From(object data)
        {
            if (data is GUIContent content) return content;
            if (data is Texture texture) return FromTexture(texture);
            if (data is string s) return FromString(s);
            if (data is Type t) return FromType(t);
            return data is int i ? FromInt(i) : GUIContent.none;
        }

        public static GUIContent FromInt(int val)
        {
            if (IntMap.TryGetValue(val, out var result)) return result;

            var str = val.ToString();
            result = FromString(str);
            IntMap.Add(val, result);
            return result;
        }

        public static GUIContent FromTexture(Texture tex, string tooltip = null)
        {
            if (TexMap.TryGetValue(tex, out var result)) return result;
            result = new GUIContent(tex, tooltip);
            TexMap.Add(tex, result);
            return result;
        }

        public static GUIContent From(string title, Texture tex, string tooltip = null)
        {
            if (StringTexMap.TryGetValue(title, out var result)) return result;
            result = new GUIContent(title, tex, tooltip);
            StringTexMap.Add(title, result);
            return result;
        }

        public static GUIContent[] FromArrayLabelIcon(params object[] data)
        {
            var result = new List<GUIContent>();
            for (var i = 0; i < data.Length; i++)
            {
                result.Add(From(data[0].ToString(), (Texture) data[1]));
            }

            return result.ToArray();
        }

        public static GUIContent[] FromArray(params object[] data)
        {
            var result = new List<GUIContent>();
            foreach (object item in data)
            {
                if (item is string s)
                {
                    result.Add(FromString(s));
                    continue;
                }

                if (item is Texture texture)
                {
                    result.Add(FromTexture(texture));
                    continue;
                }

                if (item is GUIContent content)
                {
                    result.Add(content);
                    continue;
                }

                Debug.LogWarning("Unsupported type: " + item);
            }

            return result.ToArray();
        }

        public static GUIContent[] FromEnum(Type enumType)
        {
            var values = Enum.GetValues(enumType);
            var result = new List<GUIContent>();
            foreach (object item in values)
            {
                result.Add(FromString(item.ToString()));
            }

            return result.ToArray();
        }
    }
}