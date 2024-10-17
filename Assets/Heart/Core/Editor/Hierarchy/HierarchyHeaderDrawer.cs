using UnityEditor;
using UnityEngine;
using Pancake;
using PancakeEditor.Common;

namespace PancakeEditor
{
    // ReSharper disable once UnusedType.Global
    public sealed class HierarchyHeaderDrawer : HierarchyDrawer
    {
        private static Color HeaderColor => EditorGUIUtility.isProSkin ? new Color(0.45f, 0.45f, 0.45f, 0.5f) : new Color(0.55f, 0.55f, 0.55f, 0.5f);
        private static GUIStyle labelStyle;

        public override void OnGUI(int instanceID, Rect selectionRect)
        {
            labelStyle ??= new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter, fontSize = 11};
          
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;
            if (!gameObject.TryGetComponent<HierarchyHeader>(out _)) return;

            DrawBackground(instanceID, selectionRect);

            var headerRect = selectionRect.AddXMax(14f).AddYMax(-1f);
            EditorGUI.DrawRect(headerRect, HeaderColor);
            EditorGUI.LabelField(headerRect, gameObject.name, labelStyle);
        }
    }
}