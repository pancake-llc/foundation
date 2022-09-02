using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
using System.Collections.Generic;
#endif

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class TagAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(TagAttribute))]
        class TagDrawer : BasePropertyDrawer<TagAttribute>
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return (property.propertyType == SerializedPropertyType.String)
                    ? EditorGUI.GetPropertyHeight(property)
                    : EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                if (property.propertyType == SerializedPropertyType.String)
                {
                    // generate the taglist + custom tags
                    List<string> tagList = new List<string>();
                    tagList.Add("(None)");
                    tagList.Add("Untagged");
                    tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

                    string propertyString = property.stringValue;
                    int index = 0;
                    // check if there is an entry that matches the entry and get the index
                    // we skip index 0 as that is a special custom case
                    for (int i = 1; i < tagList.Count; i++)
                    {
                        if (tagList[i].Equals(propertyString, System.StringComparison.Ordinal))
                        {
                            index = i;
                            break;
                        }
                    }

                    // Draw the popup box with the current selected index
                    int newIndex = EditorGUI.Popup(position, label.text, index, tagList.ToArray());

                    // Adjust the actual string value of the property based on the selection
                    string newValue = newIndex > 0 ? tagList[newIndex] : string.Empty;

                    if (!property.stringValue.Equals(newValue, System.StringComparison.Ordinal))
                    {
                        property.stringValue = newValue;
                    }
                }
                else
                {
                    EditorGUI.HelpBox(position, string.Format("{0} supports only string fields", nameof(TagAttribute)), MessageType.Warning);
                }

                EditorGUI.EndProperty();
            }
        }
#endif
    }
}