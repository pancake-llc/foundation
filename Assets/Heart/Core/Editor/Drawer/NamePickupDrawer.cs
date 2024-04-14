using Alchemy.Editor.Drawers;
using Pancake.Linq;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PancakeEditor
{
    public abstract class NamePickupDrawer<T> : TrackSerializedObjectAttributeDrawer
    {
        protected abstract string NameClass { get; }
        protected abstract string NameOfT { get; }

        protected override void OnInspectorChanged()
        {
            var label = "Select type...";
            if (!string.IsNullOrEmpty(SerializedProperty.stringValue))
            {
                var type = GetTypeByFullName();
                if (type != null)
                {
                    var result = type.GetAllSubClass<T>();
                    for (var i = 0; i < result.Count; i++)
                    {
                        var t = result[i];
                        if (t.Name == SerializedProperty.stringValue)
                        {
                            label = SerializedProperty.stringValue;
                            break;
                        }

                        label = "Load Failed...";
                    }
                }
            }

            var picker = new IMGUIContainer(() =>
            {
                EditorGUILayout.PrefixLabel(new GUIContent($" {SerializedProperty.displayName}"));
                var lastRect = GUILayoutUtility.GetLastRect();
                var rect = TargetElement.contentRect;
                const float percent = 0.39f;
                rect.x = lastRect.x + rect.width * percent + 4f;
                rect.width = TargetElement.contentRect.width * (1f - percent);
                
                if (GUI.Button(rect, new GUIContent(label), EditorStyles.popup))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None (-1)"), false, () => SetAndApplyProperty(SerializedProperty, string.Empty));
                    var type = GetTypeByFullName();
                    if (type != null)
                    {
                        var result = type.GetAllSubClass<T>().Filter(t => !t.Name.Equals(NameOfT));
                        for (var i = 0; i < result.Count; i++)
                        {
                            if (i == 0) menu.AddSeparator("");
                            int cachei = i;
                            menu.AddItem(new GUIContent($"{result[i].Name} ({i})"), false, () => SetAndApplyProperty(SerializedProperty, result[cachei].Name));
                        }
                    }

                    menu.DropDown(rect);
                }
            });

            TargetElement.RemoveAt(TargetElement.childCount - 1);
            TargetElement.Insert(TargetElement.childCount, picker);
        }

        private void SetAndApplyProperty(SerializedProperty element, string value)
        {
            element.stringValue = value;
            element.serializedObject.ApplyModifiedProperties();
        }

        private System.Type GetTypeByFullName()
        {
            TypeExtensions.TryFindTypeByFullName(NameClass, out var type);
            return type;
        }
    }
}