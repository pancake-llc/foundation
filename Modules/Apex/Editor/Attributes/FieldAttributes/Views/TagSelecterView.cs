using Pancake.Apex;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(TagSelecterAttribute))]
    public sealed class TagSelecterView : FieldView
    {
        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            if (string.IsNullOrEmpty(element.GetString()))
            {
                element.SetString("Untagged");
            }

            if (GUI.Button(position, element.GetString(), EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < InternalEditorUtility.tags.Length; i++)
                {
                    string tag = InternalEditorUtility.tags[i];
                    menu.AddItem(new GUIContent(tag),
                        false,
                        () =>
                        {
                            element.SetString(tag);
                            element.GetSerializedObject().ApplyModifiedProperties();
                        });
                }

                menu.DropDown(position);
            }
        }
    }
}