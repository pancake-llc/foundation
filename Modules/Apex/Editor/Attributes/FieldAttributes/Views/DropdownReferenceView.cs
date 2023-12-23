using Pancake.Apex;
using Pancake.ExLibEditor.Windows;
using System;
using System.Reflection;
using Pancake.ExLib.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(DropdownReferenceAttribute))]
    public sealed class DropdownReferenceView : FieldView, ITypeValidationCallback
    {
        private static GUIContent DropdownIcon;
        static DropdownReferenceView()
        {
            DropdownIcon = EditorGUIUtility.IconContent("icon dropdown");
        }

        private float headerHeight = 22;
        private GUIContent instanceContent;
        private DropdownReferenceAttribute attribute;

        /// <summary>
        /// Called once when initializing FieldView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as DropdownReferenceAttribute;
            instanceContent = GetInstanceContent(serializedField);

            if(!attribute.PopupStyle && serializedField.GetManagedReference() == null)
            {
                serializedField.IsExpanded(false);
            }
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (attribute.PopupStyle)
            {
                DrawPopup(position, serializedField, label);
            }
            else
            {
                DrawGroup(position, serializedField, label);
            }
        }

        /// <summary>
        /// Draw popup control field of reference.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        private void DrawPopup(Rect position, SerializedField serializedField, GUIContent label)
        {
            Rect totalPosition = position;

            position.height = EditorGUIUtility.singleLineHeight;
            object value = serializedField.GetManagedReference();
            if (value == null || !serializedField.GetSerializedProperty().hasVisibleChildren)
            {
                position.x -= 1;
                GUI.Label(EditorGUI.IndentedRect(position), label);
                position.x += 1;
            }
            else
            {
                using (new EditorGUI.IndentLevelScope(1))
                {
                    serializedField.IsExpanded(EditorGUI.Foldout(position, serializedField.IsExpanded(), label, false));
                }
            }

            position.x += EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;
            position.width = Mathf.Max(30, position.width - EditorGUIUtility.labelWidth) - EditorGUIUtility.standardVerticalSpacing;
            if (GUI.Button(position, instanceContent, EditorStyles.popup))
            {
                DropdownReferences(position, serializedField);
            }

            if (serializedField.IsExpanded())
            {
                position.x = totalPosition.x;
                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                position.width = totalPosition.width;
                position.height = totalPosition.height - (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                if(position.height > 0)
                {
                    EditorGUI.indentLevel++;
                    serializedField.DrawChildren(position);
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// Draw box group control field of reference.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        private void DrawGroup(Rect position, SerializedField serializedField, GUIContent label)
        {
            position.width += 1;

            Rect totalPosition = position;

            position.height = headerHeight;
            position.width -= headerHeight;
            if (GUI.Button(position, GUIContent.none, ApexStyles.BoxButton))
            {
                if (serializedField.GetManagedReference() != null && serializedField.GetSerializedProperty().hasVisibleChildren)
                {
                    serializedField.IsExpanded(!serializedField.IsExpanded());
                }
            }

            position.x = position.xMax - 1;
            position.width = headerHeight;
            if (GUI.Button(position, DropdownIcon, ApexStyles.BoxCenteredButton))
            {
                DropdownReferences(totalPosition, serializedField);
            }

            Vector2 suffixSize = ApexStyles.SuffixMessage.CalcSize(instanceContent);
            position.x -= suffixSize.x + 4;
            position.width = suffixSize.x;
            GUI.Label(position, instanceContent, ApexStyles.SuffixMessage);

            position.x = totalPosition.x;
            position.width = totalPosition.width - headerHeight;

            object value = serializedField.GetManagedReference();
            if (value == null || !serializedField.GetSerializedProperty().hasVisibleChildren)
            {
                position.x += 8;
                GUI.Label(position, label, ApexStyles.Label);
            }
            else
            {
                Event current = Event.current;
                bool isHover = position.Contains(current.mousePosition);
                if (current.type == EventType.Repaint)
                {
                    position.x += 4;
                    ApexStyles.BoldFoldout.Draw(position, label, isHover, false, serializedField.IsExpanded(), false);
                }
            }

            if (serializedField.IsExpanded())
            {
                position.x = totalPosition.x;
                position.y = position.yMax - 1;
                position.width = totalPosition.width - 1;
                position.height = totalPosition.height - headerHeight;
                GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);

                position.y += 3;

                using (new BoxScope(ref position, false))
                {
                    EditorGUI.indentLevel++;
                    serializedField.DrawChildren(position);
                    EditorGUI.indentLevel--;
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
            float height = 0;
            if (attribute.PopupStyle)
            {
                height = EditorGUIUtility.singleLineHeight;
                if (serializedField.IsExpanded())
                {
                    height += EditorGUIUtility.standardVerticalSpacing + serializedField.GetChildrenHeight();
                }
            }
            else
            {
                height = headerHeight;
                if (serializedField.IsExpanded())
                {
                    height += serializedField.GetChildrenHeight() + (EditorGUIUtility.standardVerticalSpacing * 3);
                }
            }
            return height;
        }

        /// <summary>
        /// Dropdown window with available references for this serialized field.
        /// </summary>
        /// <param name="position">Rectangle position of dropdown window.</param>
        /// <param name="serializedField">Serialized field of reference.</param>
        private void DropdownReferences(Rect position, SerializedField serializedField)
        {
            ExSearchWindow window = ExSearchWindow.Create("References");
            window.SetSortType(ExSearchWindow.SortType.None);

            window.AddEntry("None", () => SetReference(null, null));

            Type type = serializedField.GetMemberType();
            TypeCache.TypeCollection derivedTypes = TypeCache.GetTypesDerivedFrom(type);
            for (int i = 0; i < derivedTypes.Count; i++)
            {
                Type derivedType = derivedTypes[i];
                if (derivedType.IsAbstract || !derivedType.HasDefaultConstructor())
                {
                    continue;
                }

                GUIContent content = new GUIContent(derivedType.Name);
                SearchContent searchContent = derivedType.GetCustomAttribute<SearchContent>();
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

                window.AddEntry(content, ()=> SetReference(derivedType, content));
            }

            window.Open(position);

            void SetReference(Type type, GUIContent content)
            {
                if (type == null)
                {
                    serializedField.SetManagedReference((object)null);
                    serializedField.IsExpanded(false);
                    instanceContent = new GUIContent("None");
                }
                else
                {
                    serializedField.SetManagedReference(type);
                    instanceContent = content;
                }
                serializedField.GetSerializedObject().ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Get GUIContent of reference.
        /// </summary>
        /// <param name="serializedField">Serialized field of reference.</param>
        private GUIContent GetInstanceContent(SerializedField serializedField)
        {
            GUIContent content = new GUIContent("None");

            object value = serializedField.GetManagedReference();
            if (value != null)
            {
                Type type = serializedField.GetMemberType();
                Type valueType = value.GetType();
                TypeCache.TypeCollection derivedTypes = TypeCache.GetTypesDerivedFrom(type);
                for (int i = 0; i < derivedTypes.Count; i++)
                {
                    Type derivedType = derivedTypes[i];
                    if (derivedType.IsAbstract || !derivedType.HasDefaultConstructor() || valueType != derivedType)
                    {
                        continue;
                    }

                    content = new GUIContent(ObjectNames.NicifyVariableName(derivedType.Name));

                    SearchContent searchContent = derivedType.GetCustomAttribute<SearchContent>();
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
                }
            }
            return content;
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ManagedReference;
        }

        #region [Getter / Setter]
        public float GetHeaderHeight()
        {
            return headerHeight;
        }

        public void SetHeaderHeight(float value)
        {
            headerHeight = value;
        }
        #endregion
    }
}