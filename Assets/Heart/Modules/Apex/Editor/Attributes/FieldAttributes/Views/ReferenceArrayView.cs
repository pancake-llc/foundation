using Pancake.Apex;
using Pancake.ExLib.Reflection;
using Pancake.ExLibEditor.Windows;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(ReferenceArrayAttribute))]
    public sealed class ReferenceArrayView : FieldView, ITypeValidationCallback
    {
        private ReferenceArrayAttribute attribute;
        private ReorderableArray reorderableArray;

        // Stored callback properties.
        private object target;
        private MethodCaller<object, object> onElementGUI;
        private MethodCaller<object, object> getElementHeight;
        private MethodCaller<object, object> getElementLabel;
        private MethodCaller<object, object> validateSubtype;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as ReferenceArrayAttribute;
            target = serializedField.GetDeclaringObject();
            FindCallbacks(target, attribute);
            CreateArray(serializedField);
        }

        private void CreateArray(SerializedField serializedField)
        {
            reorderableArray = new ReorderableArray(serializedField, true);
            if (onElementGUI != null)
            {
                reorderableArray.onElementGUICallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedField field = serializedField.GetArrayElement(index);

                    if (getElementLabel != null)
                    {
                        field.SetLabel((GUIContent) getElementLabel.Invoke(target, new object[2] {index, serializedField}));
                    }

                    ApexGUI.RemoveIndentFromRect(ref rect);
                    onElementGUI.Invoke(target, new object[3] {rect, field.GetSerializedProperty(), field.GetLabel()});
                };
            }
            else
            {
                reorderableArray.onElementGUICallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedField field = serializedField.GetArrayElement(index);

                    if (getElementLabel != null)
                    {
                        field.SetLabel((GUIContent) getElementLabel.Invoke(target, new object[2] {index, serializedField}));
                    }

                    // else
                    // {
                    //     object objValue = field.GetManagedReference();
                    //     if (objValue != null)
                    //     {
                    //         field.GetLabel().text = objValue.GetType().Name;
                    //     }
                    // }
                    
                    ApexGUI.RemoveIndentFromRect(ref rect);
                    field.OnGUI(rect);
                };
            }

            if (getElementHeight != null)
            {
                reorderableArray.getElementHeightCallback = (index) =>
                {
                    SerializedField field = serializedField.GetArrayElement(index);
                    return (float) getElementHeight.Invoke(target, new object[1] {field.GetSerializedProperty()});
                };
            }

            reorderableArray.onAddClickCallback = (rect) => { ShowReferences(rect, serializedField); };

            reorderableArray.onRemoveClickCallback = (rect, index) => { serializedField.RemoveArrayElement(index); };
        }

        /// <summary>
        /// Called for drawing serializedField view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized serializedField.</param>
        /// <param name="serializedField">Serialized serializedField with ViewAttribute.</param>
        /// <param name="label">Label of serialized serializedField.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label) { reorderableArray.Draw(position); }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="property">Serialized serializedField with ViewAttribute.</param>
        /// <param name="label">Label of serialized serializedField.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label) { return reorderableArray.GetHeight(); }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        /// <param name="label">Display label of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.isArray && property.propertyType == SerializedPropertyType.Generic; }

        /// <summary>
        /// Show search window with all derived references.
        /// </summary>
        /// <param name="position">Button position.</param>
        /// <param name="serializedField">Serialized field reference.</param>
        public void ShowReferences(Rect position, SerializedField serializedField)
        {
            ExSearchWindow searchWindow = ExSearchWindow.Create("Derived Types");

            Type memberType = serializedField.GetMemberType();
            Type elementType = memberType.GetElementType();
            if (memberType.IsGenericType)
            {
                elementType = memberType.GetGenericArguments()[0];
            }

            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(elementType);
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || !type.HasDefaultConstructor() || (validateSubtype != null && !(bool) validateSubtype.Invoke(target, new object[1] {type})))
                {
                    continue;
                }

                GUIContent content = new GUIContent(type.Name);
                SearchContent attribute = type.GetCustomAttribute<SearchContent>();
                if (attribute != null)
                {
                    if (attribute.Hidden)
                    {
                        continue;
                    }

                    content.text = attribute.name;
                    content.tooltip = attribute.Tooltip;
                    if (SearchContentUtility.TryLoadContentImage(attribute.Image, out Texture2D image))
                    {
                        content.image = image;
                    }
                }

                searchWindow.AddEntry(content, () => CreateReference(type, serializedField));
            }

            searchWindow.Open(position);
        }

        private void CreateReference(Type type, SerializedField serializedField)
        {
            SerializedProperty property = serializedField.GetSerializedProperty();

            int index = property.arraySize;
            property.arraySize++;

            SerializedProperty element = property.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);
            element.serializedObject.ApplyModifiedProperties();

            // if (serializedField.IsExpanded())
            // {
            //     serializedField.ApplyChildren();
            // }
        }

        private void FindCallbacks(object target, ReferenceArrayAttribute attribute)
        {
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            
            foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
            {
                if (onElementGUI != null && getElementHeight != null && getElementLabel != null && validateSubtype != null)
                {
                    break;
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

                if (getElementHeight == null && methodInfo.IsValidCallback(attribute.GetElementHeight, typeof(float), typeof(SerializedProperty)))
                {
                    getElementHeight = methodInfo.DelegateForCall<object, object>();
                    continue;
                }

                if (getElementLabel == null && methodInfo.IsValidCallback(attribute.GetElementLabel, typeof(GUIContent), typeof(int), typeof(SerializedProperty)))
                {
                    getElementLabel = methodInfo.DelegateForCall<object, object>();
                    continue;
                }

                if (validateSubtype == null && methodInfo.IsValidCallback(attribute.ValidateSubtype, typeof(bool), typeof(Type)))
                {
                    validateSubtype = methodInfo.DelegateForCall<object, object>();
                    continue;
                }
            }
        }
    }
}