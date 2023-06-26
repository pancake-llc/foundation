using UnityEngine;
using UnityEditor;

namespace Pancake.SensorEditor
{
    public class EditorUtils
    {
        public static void InlinePropertyField(SerializedProperty root)
        {
            if (!root.hasChildren)
            {
                return;
            }

            SerializedProperty child = root.Copy();
            SerializedProperty nextSiblingProperty = root.Copy();
            nextSiblingProperty.NextVisible(false);

            child.NextVisible(true);

            while (!SerializedProperty.EqualContents(child, nextSiblingProperty))
            {
                EditorGUILayout.PropertyField(child, true);
                child.NextVisible(false);
            }
        }

        public static void HorizontalLine(Color color)
        {
            GUIStyle horizontalLine;
            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }
    }
}