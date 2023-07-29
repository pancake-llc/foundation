using Pancake.Apex;
using Pancake.ExLibEditor.Windows;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(DropdownReferenceAttribute))]
    public sealed class DropdownReferenceView : FieldView, ITypeValidationCallback
    {
        private const float HEADER_HEIGHT = 22;
        private static GUIContent ButtonContent;
        static DropdownReferenceView() { ButtonContent = EditorGUIUtility.IconContent("icon dropdown"); }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            position.width += 1;

            float contentHeight = Mathf.Max(0, position.height - HEADER_HEIGHT);
            float totalWidth = position.width;

            position.height = HEADER_HEIGHT;
            position.width -= HEADER_HEIGHT;
            if (GUI.Button(position, GUIContent.none, ApexStyles.BoxButton))
            {
                serializedField.IsExpanded(!serializedField.IsExpanded());
            }

            object objValue = serializedField.GetManagedReference();
            string instanceName = objValue != null ? objValue.GetType().Name : "Empty";
            position.width -= 5;
            GUI.Label(position, instanceName, ApexStyles.SuffixMessage);
            position.width += 5;

            Rect buttonPosition = new Rect(position.xMax - 1, position.y, HEADER_HEIGHT, HEADER_HEIGHT);
            position.width += HEADER_HEIGHT;
            if (GUI.Button(buttonPosition, ButtonContent, ApexStyles.BoxCenteredButton))
            {
                ShowWindow(position, serializedField);
            }

            Event current = Event.current;
            bool isHover = position.Contains(current.mousePosition);
            if (current.type == EventType.Repaint)
            {
                position.x += 4;
                ApexStyles.BoldFoldout.Draw(position,
                    label,
                    isHover,
                    false,
                    serializedField.IsExpanded(),
                    false);
                position.x -= 4;
            }

            if (serializedField.IsExpanded() && contentHeight > 0)
            {
                position.y = position.yMax - 1;
                position.width -= 1;
                position.height = contentHeight;
                GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);
                position.y += 3;
                
                ApexGUI.RemoveIndentFromRect(ref position);
                using (new BoxScope(ref position, false))
                {
                    ApexGUI.IndentLevel++;
                    serializedField.DrawChildren(position);
                    ApexGUI.IndentLevel--;
                }
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label)
        {
            float height = HEADER_HEIGHT;
            if (serializedField.IsExpanded())
            {
                height += serializedField.GetChildrenHeight() + (ApexGUIUtility.VerticalSpacing * 3);
            }

            return height;
        }

        private void ShowWindow(Rect position, SerializedField serializedField)
        {
            ExSearchWindow searchWindow = ScriptableObject.CreateInstance<ExSearchWindow>();
            Type memberType = serializedField.GetMemberType();
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(memberType);
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];

                if (type.IsAbstract)
                {
                    continue;
                }

                GUIContent content = new GUIContent(type.Name);
                SearchContent searchContent = type.GetCustomAttribute<SearchContent>();
                if (searchContent != null)
                {
                    if (searchContent.Hidden)
                    {
                        continue;
                    }

                    content.text = searchContent.name;
                    if (SearchContentUtility.TryLoadContentImage(searchContent.Image, out Texture2D image))
                    {
                        content.image = image;
                    }
                }

                searchWindow.AddEntry(content,
                    () =>
                    {
                        serializedField.SetManagedReference(type);
                        serializedField.GetSerializedObject().ApplyModifiedProperties();
                    });
            }

            GUIContent resetContent = new GUIContent("Reset");
            resetContent.image = EditorGUIUtility.IconContent("P4_DeletedLocal@2x").image;
            searchWindow.AddEntry(resetContent,
                () =>
                {
                    serializedField.SetManagedReference((object) null);
                    serializedField.GetSerializedObject().ApplyModifiedProperties();
                });

            searchWindow.Open(position);
        }

        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.ManagedReference; }
    }
}