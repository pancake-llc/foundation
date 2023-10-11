using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(ReorderableListAttribute))]
    public sealed class ReorderableListView : FieldView, ITypeValidationCallback
    {
        private ReorderableList reorderableList;
        private ReorderableListAttribute attribute;

        // Stored callback properties.
        private object target;
        private MethodCaller<object, object> onHeaderGUI;
        private MethodCaller<object, object> onElementGUI;
        private MethodCaller<object, object> onNoneElementGUI;
        private MethodCaller<object, object> getElementHeight;
        private MethodCaller<object, object> onAddElement;
        private MethodCaller<object, object> onAddDropdownElement;
        private MethodCaller<object, object> onRemoveElement;
        private MethodCaller<object, object> getElementLabel;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as ReorderableListAttribute;
            target = serializedField.GetDeclaringObject();
            serializedField.IsExpanded(true);
            FindCallbacks(target, attribute);
            CreateList(serializedField, label);
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label) { reorderableList.DoList(EditorGUI.IndentedRect(position)); }

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
                    if (onHeaderGUI != null)
                        onHeaderGUI.Invoke(target, new object[1] {position});
                    else
                        GUI.Label(position, label);
                },
                drawElementCallback = (position, index, isActive, isFocused) =>
                {
                    position.x -= 2;
                    position.y += 2;
                    EditorGUIUtility.labelWidth -= 17;
                    SerializedField field = serializedField.GetArrayElement(index);
                    if (field.IsVisible())
                    {
                        if (getElementLabel != null)
                            field.SetLabel((GUIContent) getElementLabel.Invoke(target, new object[2] {serializedField.GetSerializedProperty(), index}));

                        if (onElementGUI != null)
                            onElementGUI.Invoke(target, new object[3] {position, field.GetSerializedProperty(), field.GetLabel()});
                        else
                            field.OnGUI(position);
                    }

                    EditorGUIUtility.labelWidth += 17;
                },
                drawNoneElementCallback = (rect) =>
                {
                    if (onNoneElementGUI != null)
                        onNoneElementGUI.Invoke(target, new object[1] {rect});
                    else
                        ReorderableList.defaultBehaviours.DrawNoneElement(rect, attribute.Draggable);
                },
                elementHeightCallback = (index) =>
                {
                    SerializedField field = serializedField.GetArrayElement(index);
                    if (getElementHeight == null)
                    {
                        if (field.IsVisible())
                            return serializedField.GetArrayElement(index).GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                        else
                            return 0;
                    }
                    else
                    {
                        return (float) getElementHeight.Invoke(target, new object[1] {field.GetSerializedProperty()}) + EditorGUIUtility.standardVerticalSpacing;
                    }
                },
                onAddCallback = (list) =>
                {
                    serializedField.IncreaseArraySize();
                    if (onAddElement != null)
                    {
                        onAddElement.Invoke(target, new object[1] {serializedField.GetSerializedProperty()});
                    }
                },
                onRemoveCallback = (list) =>
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    serializedField.GetSerializedObject().ApplyModifiedProperties();
                    serializedField.ApplyChildren();
                    if (onRemoveElement != null)
                    {
                        onRemoveElement.Invoke(target, new object[1] {serializedField.GetSerializedProperty()});
                    }
                }
            };

            if (onAddDropdownElement != null)
            {
                reorderableList.onAddDropdownCallback = (rect, list) => { onAddDropdownElement.Invoke(target, new object[2] {rect, list.serializedProperty}); };
            }
        }

        private void FindCallbacks(object target, ReorderableListAttribute attribute)
        {
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);

            foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
            {
                if (onHeaderGUI != null && onElementGUI != null && onNoneElementGUI != null && getElementHeight != null && onAddElement != null &&
                    onAddDropdownElement != null && onRemoveElement != null && getElementLabel != null)
                {
                    break;
                }

                if (onHeaderGUI == null && methodInfo.IsValidCallback(attribute.OnHeaderGUI, typeof(void), typeof(Rect)))
                {
                    onHeaderGUI = methodInfo.DelegateForCall();
                    continue;
                }

                if (onElementGUI == null && methodInfo.IsValidCallback(attribute.OnElementGUI,
                        typeof(void),
                        typeof(Rect),
                        typeof(SerializedProperty),
                        typeof(GUIContent)))
                {
                    onElementGUI = methodInfo.DelegateForCall();
                    continue;
                }

                if (onNoneElementGUI == null && methodInfo.IsValidCallback(attribute.OnNoneElementGUI, typeof(void), typeof(Rect)))
                {
                    onNoneElementGUI = methodInfo.DelegateForCall();
                    continue;
                }

                if (getElementHeight == null && methodInfo.IsValidCallback(attribute.GetElementHeight, typeof(float), typeof(SerializedProperty)))
                {
                    getElementHeight = methodInfo.DelegateForCall<object, object>();
                    continue;
                }

                if (onAddElement == null && methodInfo.IsValidCallback(attribute.OnAddElement, typeof(void), typeof(SerializedProperty)))
                {
                    onAddElement = methodInfo.DelegateForCall<object, object>();
                    continue;
                }

                if (onAddDropdownElement == null && methodInfo.IsValidCallback(attribute.OnAddDropdownElement, typeof(void), typeof(Rect), typeof(SerializedProperty)))
                {
                    onAddDropdownElement = methodInfo.DelegateForCall<object, object>();
                    continue;
                }

                if (onRemoveElement == null && methodInfo.IsValidCallback(attribute.OnRemoveElement, typeof(void), typeof(SerializedProperty)))
                {
                    onRemoveElement = methodInfo.DelegateForCall<object, object>();
                    continue;
                }

                if (getElementLabel == null && methodInfo.IsValidCallback(attribute.GetElementLabel, typeof(GUIContent), typeof(SerializedProperty), typeof(int)))
                {
                    getElementLabel = methodInfo.DelegateForCall<object, object>();
                    continue;
                }
            }
        }
    }
}