using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(ArrayAttribute))]
    sealed class ArrayView : FieldView, ITypeValidationCallback
    {
        private object target;
        private ArrayAttribute attribute;
        private GUIContent removeButtonContent;
        private FoldoutContainer foldoutContainer;
        private MethodInfo onElementGUI;
        private MethodInfo getElementHeight;
        private MethodInfo getElementLabel;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as ArrayAttribute;
            removeButtonContent = EditorGUIUtility.IconContent("Toolbar Minus");
            foldoutContainer = new FoldoutContainer(label.text, "Group", null)
            {
                onChildrenGUI = (position) => OnArrayElements(position, serializedField),
                getChildrenHeight = () => GetArrayElementsHeight(serializedField),
                onMenuButtonClick = (position) => { serializedField.ResizeArray(serializedField.GetArrayLength() + 1); },
                menuIconContent = EditorGUIUtility.IconContent("Toolbar Plus")
            };

            target = serializedField.GetMemberTarget();
            Type type = target.GetType();

            int done = 0;
            foreach (MethodInfo methodInfo in type.AllMethods())
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();

                if (onElementGUI == null && !string.IsNullOrEmpty(attribute.OnElementGUI) && methodInfo.Name == attribute.OnElementGUI &&
                    methodInfo.ReturnType == typeof(void) && parameters.Length == 3 && parameters[0].ParameterType == typeof(Rect) &&
                    parameters[1].ParameterType == typeof(SerializedProperty) && parameters[2].ParameterType == typeof(GUIContent))
                {
                    onElementGUI = methodInfo;
                    done++;
                }

                if (getElementHeight == null && !string.IsNullOrEmpty(attribute.GetElementHeight) && methodInfo.Name == attribute.GetElementHeight &&
                    methodInfo.ReturnType == typeof(float) && parameters.Length == 2 && parameters[0].ParameterType == typeof(SerializedProperty) &&
                    parameters[1].ParameterType == typeof(GUIContent))
                {
                    getElementHeight = methodInfo;
                    done++;
                }

                if (getElementLabel == null && !string.IsNullOrEmpty(attribute.GetElementLabel) && methodInfo.Name == attribute.GetElementLabel &&
                    methodInfo.ReturnType == typeof(string) && parameters.Length == 2 && parameters[0].ParameterType == typeof(SerializedProperty) &&
                    parameters[1].ParameterType == typeof(int))
                {
                    getElementLabel = methodInfo;
                    done++;
                }

                if (done == 3)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Called for drawing serializedField view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized serializedField.</param>
        /// <param name="serializedField">Serialized serializedField with ViewAttribute.</param>
        /// <param name="label">Label of serialized serializedField.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label) { foldoutContainer.OnGUI(position); }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="property">Serialized serializedField with ViewAttribute.</param>
        /// <param name="label">Label of serialized serializedField.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label) { return foldoutContainer.GetHeight(); }

        /// <summary>
        /// Called for drawing array elements view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized serializedField.</param>
        /// <param name="arrayField">Serialized serializedField with ViewAttribute.</param>
        public void OnArrayElements(Rect position, SerializedField arrayField)
        {
            for (int i = 0; i < arrayField.GetArrayLength(); i++)
            {
                SerializedField field = arrayField.GetArrayElement(i);
                if (field.IsVisible())
                {
                    float arrayElementHeight = 0;
                    if (getElementHeight != null)
                        arrayElementHeight = ((float) getElementHeight?.Invoke(target, new object[2] {field.GetSerializedProperty(), field.GetLabel()}));
                    else
                        arrayElementHeight = field.GetHeight();

                    if (getElementLabel != null)
                    {
                        field.GetLabel().text = (string) getElementLabel.Invoke(target, new object[2] {arrayField.GetSerializedProperty(), i});
                    }

                    Rect backgroundPosition = new Rect(position.x, position.y - 3, position.width, arrayElementHeight + 6);
                    backgroundPosition = EditorGUI.IndentedRect(backgroundPosition);
                    backgroundPosition.xMin -= (EditorStyles.foldout.padding.left - EditorStyles.label.padding.left) + 2;
                    GUI.Box(backgroundPosition, GUIContent.none, Uniform.ContentBackground);

                    Rect arrayElementPosition = new Rect(position.x, position.y, position.width - 26, arrayElementHeight);
                    if (onElementGUI != null)
                        onElementGUI.Invoke(target, new object[3] {arrayElementPosition, field.GetSerializedProperty(), field.GetLabel()});
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
        /// <param name="arrayField">Serialized serializedField with ViewAttribute.</param>
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
                            height += ((float) getElementHeight?.Invoke(target, new object[2] {field.GetSerializedProperty(), field.GetLabel()}));
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