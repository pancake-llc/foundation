using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(ArrayAttribute))]
    public sealed class ArrayView : FieldView, ITypeValidationCallback
    {
        private SerializedField serializedField;
        private ReorderableArray reorderableArray;

        private Action<SerializedProperty> onReorder;
        private Action<SerializedProperty, int> onAdd;
        private Action<SerializedProperty, int> onRemove;
        private Action<Rect, SerializedProperty, GUIContent> onGUI;
        private Func<SerializedProperty, int, float> getHeight;
        private Func<SerializedProperty, int, GUIContent> getLabel;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            FindCallbacks(serializedField, viewAttribute as ArrayAttribute);
            OverrideElementsLabel(serializedField);

            this.serializedField = serializedField;
            reorderableArray = new ReorderableArray(serializedField, true)
            {
                onElementGUI = OnElementGUI,
                getElementHeight = GetElementHeight,
                onAddClick = OnAddElement,
                onRemoveClick = OnRemoveElement,
                onReorder = OnReorderList
            };
        }

        /// <summary>
        /// Called for drawing serializedField view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized serializedField.</param>
        /// <param name="serializedField">Serialized serializedField with ViewAttribute.</param>
        /// <param name="label">Label of serialized serializedField.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            reorderableArray.Draw(EditorGUI.IndentedRect(position));
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="property">Serialized serializedField with ViewAttribute.</param>
        /// <param name="label">Label of serialized serializedField.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label)
        {
            return reorderableArray.GetHeight();
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.isArray
                && property.propertyType == SerializedPropertyType.Generic;
        }

        /// <summary>
        /// Called to draw element of array.
        /// </summary>
        private void OnElementGUI(Rect position, int index, bool isFocused, bool isActive)
        {
            SerializedField field = serializedField.GetArrayElement(index);
            if (onGUI != null)
            {
                onGUI.Invoke(position, field.GetSerializedProperty(), field.GetLabel());
            }
            else
            {
                field.OnGUI(position);
            }
        }

        /// <summary>
        /// Called to calculate height of array element.
        /// </summary>
        private float GetElementHeight(int index)
        {
            if(getHeight != null)
            {
                return getHeight.Invoke(serializedField.GetSerializedProperty(), index);
            }
            else
            {
                return serializedField.GetArrayElement(index).GetHeight();
            }
        }

        /// <summary>
        /// Called to add new element to array.
        /// </summary>
        private void OnAddElement(Rect position)
        {
            int index = serializedField.GetArrayLength();
            serializedField.IncreaseArraySize();
            OverrideElementsLabel(serializedField);
            onAdd?.Invoke(serializedField.GetSerializedProperty(), index);
        }

        /// <summary>
        /// Called to remove selected element from array.
        /// </summary>
        private void OnRemoveElement(Rect position, int index)
        {
            onRemove?.Invoke(serializedField.GetSerializedProperty(), index);
            serializedField.RemoveArrayElement(index);
            OverrideElementsLabel(serializedField);
        }

        /// <summary>
        /// Called when list reordered.
        /// </summary>
        private void OnReorderList()
        {
            OverrideElementsLabel(serializedField);
            onReorder?.Invoke(serializedField.GetSerializedProperty());
        }

        /// <summary>
        /// Override elements label by label callback.
        /// </summary>
        /// <param name="serializedField">Serialzied field of array.</param>
        private void OverrideElementsLabel(SerializedField serializedField)
        {
            if (getLabel != null)
            {
                SerializedProperty array = serializedField.GetSerializedProperty();
                for (int i = 0; i < serializedField.GetArrayLength(); i++)
                {
                    SerializedField field = serializedField.GetArrayElement(i);
                    field.SetLabel(getLabel.Invoke(array, i));
                }
            }
        }

        /// <summary>
        /// Find callbacks of array view.
        /// </summary>
        /// <param name="serializedField">Serialized field of array.</param>
        /// <param name="attribute">Array view attribute.</param>
        private void FindCallbacks(SerializedField serializedField, ArrayAttribute attribute)
        {
            object target = serializedField.GetDeclaringObject();
            Type type = target.GetType();
            Type limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
            {
                if(onGUI == null && methodInfo.IsValidCallback(attribute.OnGUI, typeof(void), typeof(Rect), typeof(SerializedProperty), typeof(GUIContent)))
                {
                    onGUI = (Action<Rect, SerializedProperty, GUIContent>)methodInfo.CreateDelegate(typeof(Action<Rect, SerializedProperty, GUIContent>), target);
                    continue;
                }

                if (getHeight == null && methodInfo.IsValidCallback(attribute.GetHeight, typeof(float), typeof(SerializedProperty), typeof(int)))
                {
                    getHeight = (Func<SerializedProperty, int, float>)methodInfo.CreateDelegate(typeof(Func<SerializedProperty, int, float>), target);
                    continue;
                }

                if (getLabel == null && methodInfo.IsValidCallback(attribute.GetLabel, typeof(GUIContent), typeof(SerializedProperty), typeof(int)))
                {
                    getLabel = (Func<SerializedProperty, int, GUIContent>)methodInfo.CreateDelegate(typeof(Func<SerializedProperty, int, GUIContent>), target);
                    continue;
                }

                if (onAdd == null && methodInfo.IsValidCallback(attribute.OnAdd, typeof(void), typeof(SerializedProperty), typeof(int)))
                {
                    onAdd = (Action<SerializedProperty, int>)methodInfo.CreateDelegate(typeof(Action<SerializedProperty, int>), target);
                    continue;
                }

                if (onRemove == null && methodInfo.IsValidCallback(attribute.OnRemove, typeof(void), typeof(SerializedProperty), typeof(int)))
                {
                    onRemove = (Action<SerializedProperty, int>)methodInfo.CreateDelegate(typeof(Action<SerializedProperty, int>), target);
                    continue;
                }

                if (onReorder == null && methodInfo.IsValidCallback(attribute.OnReorder, typeof(void), typeof(SerializedProperty)))
                {
                    onReorder = (Action<SerializedProperty>)methodInfo.CreateDelegate(typeof(Action<SerializedProperty>), target);
                    continue;
                }
            }
        }
    }
}