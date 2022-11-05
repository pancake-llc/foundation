using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(ReorderableListAttribute))]
    sealed class ReorderableListView : FieldView, ITypeValidationCallback
    {
        private ReorderableList reorderableList;
        private ReorderableListAttribute attribute;

        private object target;
        private MethodInfo onHeaderGUI;
        private MethodInfo onElementGUI;
        private MethodInfo onNoneElementGUI;
        private MethodInfo getElementHeight;
        private MethodInfo onAddElement;
        private MethodInfo onAddDropdownElement;
        private MethodInfo onRemoveElement;
        private MethodInfo getElementLabel;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as ReorderableListAttribute;
            target = serializedField.GetMemberTarget();
            SearchCallbacks(serializedField);
            CreateList(serializedField, label);
        }

        /// <summary>
        /// Searching custom method callbacks.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        private void SearchCallbacks(object target)
        {
            Type type = target.GetType();

            int done = 0;
            foreach (MethodInfo methodInfo in type.AllMethods())
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();

                if (onHeaderGUI == null && !string.IsNullOrEmpty(attribute.OnHeaderGUI) && methodInfo.Name == attribute.OnHeaderGUI &&
                    methodInfo.ReturnType == typeof(void) && parameters.Length == 1 && parameters[0].ParameterType == typeof(Rect))
                {
                    onHeaderGUI = methodInfo;
                    done++;
                }

                if (onElementGUI == null && !string.IsNullOrEmpty(attribute.OnElementGUI) && methodInfo.Name == attribute.OnElementGUI &&
                    methodInfo.ReturnType == typeof(void) && parameters.Length == 3 && parameters[0].ParameterType == typeof(Rect) &&
                    parameters[1].ParameterType == typeof(SerializedProperty) && parameters[2].ParameterType == typeof(GUIContent))
                {
                    onElementGUI = methodInfo;
                    done++;
                }

                if (onNoneElementGUI == null && !string.IsNullOrEmpty(attribute.OnNoneElementGUI) && methodInfo.Name == attribute.OnNoneElementGUI &&
                    methodInfo.ReturnType == typeof(void) && parameters.Length == 1 && parameters[0].ParameterType == typeof(Rect))
                {
                    onNoneElementGUI = methodInfo;
                    done++;
                }

                if (getElementHeight == null && !string.IsNullOrEmpty(attribute.GetElementHeight) && methodInfo.Name == attribute.GetElementHeight &&
                    methodInfo.ReturnType == typeof(float) && parameters.Length == 1 && parameters[0].ParameterType == typeof(SerializedProperty))
                {
                    getElementHeight = methodInfo;
                    done++;
                }

                if (onAddElement == null && !string.IsNullOrEmpty(attribute.OnAddElement) && methodInfo.Name == attribute.OnAddElement &&
                    methodInfo.ReturnType == typeof(void) && parameters.Length == 0)
                {
                    onAddElement = methodInfo;
                    done++;
                }

                if (onAddDropdownElement == null && !string.IsNullOrEmpty(attribute.OnAddDropdownElement) && methodInfo.Name == attribute.OnAddDropdownElement &&
                    methodInfo.ReturnType == typeof(void) && parameters.Length == 2 && parameters[0].ParameterType == typeof(Rect) &&
                    parameters[1].ParameterType == typeof(SerializedProperty))
                {
                    onAddDropdownElement = methodInfo;
                    done++;
                }

                if (onRemoveElement == null && !string.IsNullOrEmpty(attribute.OnRemoveElement) && methodInfo.Name == attribute.OnRemoveElement &&
                    methodInfo.ReturnType == typeof(void) && parameters.Length == 0)
                {
                    onRemoveElement = methodInfo;
                    done++;
                }

                if (getElementLabel == null && !string.IsNullOrEmpty(attribute.GetElementLabel) && methodInfo.Name == attribute.GetElementLabel &&
                    methodInfo.ReturnType == typeof(GUIContent) && parameters.Length == 2 && parameters[0].ParameterType == typeof(SerializedProperty) &&
                    parameters[1].ParameterType == typeof(int))
                {
                    getElementLabel = methodInfo;
                    done++;
                }

                if (done == 8)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Create new reorderable list.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        private void CreateList(SerializedField serializedField, GUIContent label)
        {
            reorderableList = new ReorderableList(serializedField.GetSerializedObject(),
                serializedField.GetSerializedProperty(),
                true,
                true,
                true,
                true)
            {
                headerHeight = attribute.HeaderHeight,
                draggable = attribute.Draggable,
                displayAdd = attribute.ShowAddButton,
                displayRemove = attribute.ShowRemoveButton,
                drawHeaderCallback = (position) =>
                {
                    if (onHeaderGUI == null)
                    {
                        GUI.Label(position, label);
                    }
                    else
                    {
                        onHeaderGUI.Invoke(target, new object[1] {position});
                    }
                },
                drawElementCallback = (position, index, isActive, isFocused) =>
                {
                    int level = EditorGUI.indentLevel;
                    int indent = EditorStyles.foldout.padding.left - EditorStyles.label.padding.left;
                    indent += 2;
                    if (EditorGUIUtility.hierarchyMode)
                    {
                        EditorGUI.indentLevel = 0;
                        EditorGUIUtility.labelWidth -= indent * level;
                    }
                    else
                    {
                        EditorGUI.indentLevel = -1;
                    }

                    SerializedField field = serializedField.GetArrayElement(index);
                    if (field.IsVisible())
                    {
                        if (field.GetChildrenCount() > 0)
                        {
                            position.x -= 5;
                            position.width += 5;
                            EditorGUI.indentLevel++;
                        }

                        if (getElementLabel != null)
                        {
                            field.SetLabel((GUIContent) getElementLabel.Invoke(target, new object[2] {serializedField.GetSerializedProperty(), index}));
                        }

                        if (onElementGUI == null)
                        {
                            float width = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth -= 21;
                            position.y += 1;
                            field.ToggleOnLabelClick(false);
                            field.OnGUI(position);
                            EditorGUIUtility.labelWidth = width;
                        }
                        else
                        {
                            onElementGUI.Invoke(target, new object[3] {position, field.GetSerializedProperty(), field.GetLabel()});
                        }

                        if (field.GetChildrenCount() > 0)
                        {
                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel = level;
                    if (EditorGUIUtility.hierarchyMode)
                    {
                        EditorGUIUtility.labelWidth += indent * level;
                    }
                },
                drawNoneElementCallback = (rect) =>
                {
                    int level = EditorGUI.indentLevel;
                    int indent = EditorStyles.foldout.padding.left - EditorStyles.label.padding.left;
                    indent += 2;
                    if (EditorGUIUtility.hierarchyMode)
                    {
                        EditorGUI.indentLevel = 0;
                        EditorGUIUtility.labelWidth -= indent * level;
                    }

                    if (onNoneElementGUI != null)
                    {
                        onNoneElementGUI.Invoke(target, new object[1] {rect});
                    }
                    else
                    {
                        ReorderableList.defaultBehaviours.DrawNoneElement(rect, attribute.Draggable);
                    }

                    EditorGUI.indentLevel = level;
                    if (EditorGUIUtility.hierarchyMode)
                    {
                        EditorGUIUtility.labelWidth += indent * level;
                    }
                },
                elementHeightCallback = (index) =>
                {
                    SerializedField field = serializedField.GetArrayElement(index);
                    if (getElementHeight == null)
                    {
                        if (field.IsVisible())
                        {
                            return serializedField.GetArrayElement(index).GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return (float) getElementHeight.Invoke(target, new object[1] {field.GetSerializedProperty()});
                    }
                },
                onAddCallback = (list) =>
                {
                    serializedField.IncreaseArraySize();
                    if (onAddElement != null)
                    {
                        onAddElement.Invoke(target, null);
                    }
                },
                onRemoveCallback = (list) =>
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    serializedField.GetSerializedObject().ApplyModifiedProperties();
                    serializedField.ApplyNestedProperties();
                    if (onRemoveElement != null)
                    {
                        onRemoveElement.Invoke(target, null);
                    }
                }
            };

            if (onAddDropdownElement != null)
            {
                reorderableList.onAddDropdownCallback = (rect, list) => { onAddDropdownElement.Invoke(target, new object[2] {rect, list.serializedProperty}); };
            }
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.IndentedRect(position);
            reorderableList.DoList(position);
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override float GetHeight(SerializedField element, GUIContent label) { return reorderableList.GetHeight(); }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.Generic && property.isArray; }
    }
}