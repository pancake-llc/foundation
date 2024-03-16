#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    internal class VariableListenerWindow : EditorWindow
    {
        public static List<Object> objects;
        private static GUIStyle titleStyle;

        internal new static void Show()
        {
            var window = GetWindow<VariableListenerWindow>("Variable Listener Window");
            window.minSize = new Vector2(420, 320);
        }

        private void OnGUI()
        {
            titleStyle ??= new GUIStyle(GUIStyle.none)
            {
                margin = new RectOffset(4, 4, 4, 4),
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                richText = true,
                imagePosition = ImagePosition.ImageLeft,
                fixedHeight = 50,
                stretchHeight = true,
                stretchWidth = true,
                normal = {textColor = new Color(0.92f, 0.76f, 0.2f)}
            };
            if (objects.IsNullOrEmpty())
            {
                GUILayout.Label("No Objects reacting to OnValueChanged Event!", titleStyle);

                return;
            }

            DisplayAll(objects);
        }

        private void DisplayAll(List<Object> objects)
        {
            GUILayout.Space(15);
            GUILayout.BeginVertical($"Objects reacting to OnValueChanged Event : {objects.Count}", "window");
            foreach (var obj in objects)
            {
                try
                {
                    if (obj == null) continue;

                    var text = $"{obj.name}  ({obj.GetType().Name})";
                    DrawSelectableObject(obj, new[] {text, "Select"});
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draw a selectable object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="labels"></param>
        private static void DrawSelectableObject(Object obj, string[] labels)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(labels[0], GUILayout.MaxWidth(300))) EditorGUIUtility.PingObject(obj);

            if (GUILayout.Button(labels[1], GUILayout.MaxWidth(75)))
            {
                EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
                Selection.activeObject = obj;
                SceneView.FrameLastActiveSceneView();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }
    }
}
#endif