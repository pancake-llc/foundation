using System;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [Serializable]
    public struct EditorThemeColor
    {
        public Color dark;
        public Color light;

        public Color Get() { return EditorGUIUtility.isProSkin ? dark : light; }
    }
}