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
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as ReorderableListAttribute;
            SearchCallbacks(element);
            CreateList(element, label);
        }

        /// <summary>
        /// Searching custom method callbacks.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        private void SearchCallbacks(SerializedField element)
        {
            Type type = element.serializedObject.targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            if (!string.IsNullOrEmpty(attribute.OnHeaderGUI))
            {
                onHeaderGUI = type.GetAllMembers(attribute.OnHeaderGUI, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 1 &&
                                method.GetParameters()[0].ParameterType == typeof(Rect))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.OnElementGUI))
            {
                onElementGUI = type.GetAllMembers(attribute.OnElementGUI, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 3 &&
                                method.GetParameters()[0].ParameterType == typeof(Rect) && method.GetParameters()[1].ParameterType == typeof(SerializedProperty) &&
                                method.GetParameters()[2].ParameterType == typeof(GUIContent))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.OnNoneElementGUI))
            {
                onNoneElementGUI = type.GetAllMembers(attribute.OnNoneElementGUI, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 1 &&
                                method.GetParameters()[0].ParameterType == typeof(Rect))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.GetElementHeight))
            {
                getElementHeight = type.GetAllMembers(attribute.GetElementHeight, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(float) && method.GetParameters().Length == 1 &&
                                method.GetParameters()[0].ParameterType == typeof(SerializedProperty))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.OnAddElement))
            {
                onAddElement = type.GetAllMembers(attribute.OnAddElement, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 0)
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.OnAddDropdownElement))
            {
                onAddDropdownElement = type.GetAllMembers(attribute.OnAddDropdownElement, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 2 &&
                                method.GetParameters()[0].ParameterType == typeof(Rect) && method.GetParameters()[1].ParameterType == typeof(SerializedProperty))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.OnRemoveElement))
            {
                onRemoveElement = type.GetAllMembers(attribute.OnRemoveElement, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 0)
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.GetElementLabel))
            {
                getElementLabel = type.GetAllMembers(attribute.GetElementLabel, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(GUIContent) && method.GetParameters().Length == 2 &&
                                method.GetParameters()[0].ParameterType == typeof(SerializedProperty) && method.GetParameters()[1].ParameterType == typeof(int))
                    .FirstOrDefault() as MethodInfo;
            }
        }

        /// <summary>
        /// Create new reorderable list.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        private void CreateList(SerializedField element, GUIContent label)
        {
            reorderableList = new ReorderableList(element.serializedObject,
                element.serializedProperty,
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
                        onHeaderGUI.Invoke(element.serializedObject.targetObject, new object[1] {position});
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

                    SerializedField field = element.GetArrayElement(index);
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
                            field.SetLabel((GUIContent) getElementLabel.Invoke(element.serializedObject.targetObject, new object[2] {element.serializedProperty, index}));
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
                            onElementGUI.Invoke(element.serializedObject.targetObject, new object[3] {position, field.serializedProperty, field.GetLabel()});
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
                        onNoneElementGUI.Invoke(element.serializedObject.targetObject, new object[1] {rect});
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
                    SerializedField field = element.GetArrayElement(index);
                    if (getElementHeight == null)
                    {
                        if (field.IsVisible())
                        {
                            return element.GetArrayElement(index).GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return (float) getElementHeight.Invoke(element.serializedObject.targetObject, new object[1] {field.serializedProperty});
                    }
                },
                onAddCallback = (list) =>
                {
                    element.IncreaseArraySize();
                    if (onAddElement != null)
                    {
                        onAddElement.Invoke(element.serializedObject.targetObject, null);
                    }
                },
                onRemoveCallback = (list) =>
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    element.serializedObject.ApplyModifiedProperties();
                    element.ApplyNestedProperties();
                    if (onRemoveElement != null)
                    {
                        onRemoveElement.Invoke(element.serializedObject.targetObject, null);
                    }
                }
            };

            if (onAddDropdownElement != null)
            {
                reorderableList.onAddDropdownCallback = (rect, list) =>
                {
                    onAddDropdownElement.Invoke(element.serializedObject.targetObject, new object[2] {rect, list.serializedProperty});
                };
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