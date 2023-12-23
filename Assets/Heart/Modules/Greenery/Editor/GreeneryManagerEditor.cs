using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryManager), true)]
    public class GreeneryManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GreeneryManager manager = target as GreeneryManager;
            base.OnInspectorGUI();
            // foreach (var kvp in manager.proceduralRendererDictionary) {
            //     DisplayRenderer(kvp.Key, kvp.Value);
            // }

            // foreach (var kvp in manager.instanceRendererDictionary) {
            //     DisplayRenderer(kvp.Key, kvp.Value);
            // }

            foreach (var kvp in manager.rendererDictionary)
            {
                DisplayRenderer(kvp.Key, kvp.Value);
            }
        }

        private static void DisplayRenderer(GreeneryItem item, GreeneryRenderer renderer)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            renderer.active = EditorGUILayout.Toggle(renderer.active, GUILayout.Width(20));
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("Renderer: " + item.name);
            EditorGUILayout.LabelField("Spawn points: " + renderer.spawnDataList.Count);
            GUILayout.EndVertical();

            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_ViewToolOrbit")), GUILayout.Width(30), GUILayout.Height(30)))
            {
                EditorGUIUtility.PingObject(item);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}