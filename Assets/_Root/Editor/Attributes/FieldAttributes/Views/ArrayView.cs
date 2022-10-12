using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(ArrayAttribute))]
    sealed class ArrayView : FieldView, ITypeValidationCallback
    {
        private ArrayAttribute attribute;
        private GUIContent removeButtonContent;
        private FoldoutContainer foldoutContainer;
        private MethodInfo onElementGUI;
        private MethodInfo getElementHeight;
        private MethodInfo getElementLabel;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as ArrayAttribute;
            removeButtonContent = EditorGUIUtility.IconContent("Toolbar Minus");
            foldoutContainer = new FoldoutContainer(label.text, "Group", null)
            {
                onChildrenGUI = (position) => OnArrayElements(position, element),
                getChildrenHeight = () => GetArrayElementsHeight(element),
                onMenuButtonClick = (position) => { element.ResizeArray(element.GetArrayLength() + 1); },
                menuIconContent = EditorGUIUtility.IconContent("Toolbar Plus")
            };

            Type type = element.serializedObject.targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            if (!string.IsNullOrEmpty(attribute.OnElementGUI))
            {
                onElementGUI = type.GetAllMembers(attribute.OnElementGUI, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 3 &&
                                method.GetParameters()[0].ParameterType == typeof(Rect) && method.GetParameters()[1].ParameterType == typeof(SerializedProperty) &&
                                method.GetParameters()[2].ParameterType == typeof(GUIContent))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.GetElementHeight))
            {
                getElementHeight = type.GetAllMembers(attribute.GetElementHeight, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(float) && method.GetParameters().Length == 2 &&
                                method.GetParameters()[0].ParameterType == typeof(SerializedProperty) && method.GetParameters()[1].ParameterType == typeof(GUIContent))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.GetElementLabel))
            {
                getElementLabel = type.GetAllMembers(attribute.GetElementLabel, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(string) && method.GetParameters().Length == 2 &&
                                method.GetParameters()[0].ParameterType == typeof(SerializedProperty) && method.GetParameters()[1].ParameterType == typeof(int))
                    .FirstOrDefault() as MethodInfo;
            }
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label) { foldoutContainer.OnGUI(position); }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="property">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override float GetHeight(SerializedField element, GUIContent label) { return foldoutContainer.GetHeight(); }

        /// <summary>
        /// Called for drawing array elements view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="arrayField">Serialized element with ViewAttribute.</param>
        public void OnArrayElements(Rect position, SerializedField arrayField)
        {
            for (int i = 0; i < arrayField.GetArrayLength(); i++)
            {
                SerializedField field = arrayField.GetArrayElement(i);
                if (field.IsVisible())
                {
                    float arrayElementHeight = 0;
                    if (getElementHeight != null)
                        arrayElementHeight = ((float) getElementHeight?.Invoke(arrayField.serializedObject.targetObject,
                            new object[2] {field.serializedProperty, field.GetLabel()}));
                    else
                        arrayElementHeight = field.GetHeight();

                    if (getElementLabel != null)
                    {
                        field.GetLabel().text =
                            (string) getElementLabel.Invoke(arrayField.serializedObject.targetObject, new object[2] {arrayField.serializedProperty, i});
                    }

                    Rect backgroundPosition = new Rect(position.x, position.y - 3, position.width, arrayElementHeight + 6);
                    backgroundPosition = EditorGUI.IndentedRect(backgroundPosition);
                    backgroundPosition.xMin -= (EditorStyles.foldout.padding.left - EditorStyles.label.padding.left) + 2;
                    GUI.Box(backgroundPosition, GUIContent.none, Uniform.ContentBackground);

                    Rect arrayElementPosition = new Rect(position.x, position.y, position.width - 26, arrayElementHeight);
                    if (onElementGUI != null)
                        onElementGUI.Invoke(arrayField.serializedObject.targetObject, new object[3] {arrayElementPosition, field.serializedProperty, field.GetLabel()});
                    else
                        field.OnGUI(arrayElementPosition);

                    Rect removeButtonPosition = new Rect(arrayElementPosition.xMax + 4, backgroundPosition.y, 26, backgroundPosition.height);
                    if (GUI.Button(removeButtonPosition, removeButtonContent, Uniform.ActionButton))
                    {
                        arrayField.RemoveArrayElement(i);
                    }

                    position.y += backgroundPosition.height - 1;
                }
            }
        }

        /// <summary>
        /// Get height which needed to all array properties.
        /// </summary>
        /// <param name="arrayField">Serialized element with ViewAttribute.</param>
        public float GetArrayElementsHeight(SerializedField arrayField)
        {
            float height = 0;
            if (arrayField.GetArrayLength() > 0)
            {
                for (int i = 0; i < arrayField.GetArrayLength(); i++)
                {
                    SerializedField field = arrayField.GetArrayElement(i);
                    if (field.IsVisible())
                    {
                        if (getElementHeight != null)
                            height += ((float) getElementHeight?.Invoke(arrayField.serializedObject.targetObject,
                                new object[2] {field.serializedProperty, field.GetLabel()}));
                        else
                            height += field.GetHeight();
                        height += 5;
                    }
                }

                height -= 5;
            }

            return height;
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        /// <param name="label">Display label of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.isArray && property.propertyType == SerializedPropertyType.Generic; }
    }
}