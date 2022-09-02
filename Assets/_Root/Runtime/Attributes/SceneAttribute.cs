using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
using System.Text.RegularExpressions;
using Pancake.Linq;
#endif

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SceneAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(SceneAttribute))]
        class SceneDrawer : BasePropertyDrawer<SceneAttribute>
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                bool validPropertyType = property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.Integer;
                bool anySceneInBuildSettings = GetScenes().Length > 0;

                return (validPropertyType && anySceneInBuildSettings)
                    ? EditorGUI.GetPropertyHeight(property)
                    : EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);
                string[] scenes = GetScenes();
                bool anySceneInBuildSettings = scenes.Length > 0;
                if (!anySceneInBuildSettings)
                {
                    EditorGUI.HelpBox(position, "No scenes in the build settings", MessageType.Warning);
                    return;
                }

                string[] sceneOptions = GetSceneOptions(scenes);
                switch (property.propertyType)
                {
                    case SerializedPropertyType.String:
                        DrawPropertyForString(position,
                            property,
                            label,
                            scenes,
                            sceneOptions);
                        break;
                    case SerializedPropertyType.Integer:
                        DrawPropertyForInt(position, property, label, sceneOptions);
                        break;
                    default:
                        EditorGUI.HelpBox(position, string.Format("{0} must be an int or a string", property.name), MessageType.Warning);
                        break;
                }

                EditorGUI.EndProperty();
            }

            private string[] GetScenes()
            {
                return EditorBuildSettings.scenes.Filter(scene => scene.enabled).Map(scene => Regex.Match(scene.path, @".+\/(.+)\.unity").Groups[1].Value);
            }

            private string[] GetSceneOptions(string[] scenes) { return scenes.Map((sceneName, index) => string.Format("{0}: {1}", index, sceneName)); }

            private static int IndexOf(string[] scenes, string scene)
            {
                var index = Array.IndexOf(scenes, scene);
                return Mathf.Clamp(index, 0, scenes.Length - 1);
            }

            private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label, string[] scenes, string[] sceneOptions)
            {
                int index = IndexOf(scenes, property.stringValue);
                int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);
                string newScene = scenes[newIndex];

                if (!property.stringValue.Equals(newScene, StringComparison.Ordinal))
                {
                    property.stringValue = scenes[newIndex];
                }
            }

            private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label, string[] sceneOptions)
            {
                int index = property.intValue;
                int newIndex = EditorGUI.Popup(rect, label.text, index, sceneOptions);

                if (property.intValue != newIndex)
                {
                    property.intValue = newIndex;
                }
            }
        }
#endif
    }
}