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
    [ViewTarget(typeof(CustomViewAttribute))]
    public sealed class CustomView : FieldView
    {
        private object target;
        private MethodCaller<object, object> onInitialization;
        private MethodCaller<object, object> onGUI;
        private MethodCaller<object, object> getHeight;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            target = serializedField.GetDeclaringObject();
            FindCallbacks(target, viewAttribute as CustomViewAttribute);
            onInitialization.SafeInvoke(target, serializedField.GetSerializedProperty(), label);
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            onGUI.SafeInvoke(target, position, serializedField.GetSerializedProperty(), label);
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label)
        {
            if (getHeight != null)
            {
                return (float) getHeight.Invoke(target, new object[2] {serializedField.GetSerializedProperty(), label});
            }

            return base.GetHeight(serializedField, label);
        }

        /// <summary>
        /// Searching custom method callbacks.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        private void FindCallbacks(object target, CustomViewAttribute attribute)
        {
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            
            foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
            {
                if (onInitialization != null && onGUI != null && getHeight != null)
                {
                    break;
                }

                if (onInitialization == null && methodInfo.IsValidCallback(attribute.OnInitialization, typeof(void), typeof(SerializedProperty), typeof(GUIContent)))
                {
                    onInitialization = methodInfo.DelegateForCall();
                    continue;
                }

                if (onGUI == null && methodInfo.IsValidCallback(attribute.OnGUI,
                        typeof(void),
                        typeof(Rect),
                        typeof(SerializedProperty),
                        typeof(GUIContent)))
                {
                    onGUI = methodInfo.DelegateForCall<object, object>();
                    continue;
                }

                if (getHeight == null && methodInfo.IsValidCallback(attribute.GetHeight, typeof(float), typeof(SerializedProperty), typeof(GUIContent)))
                {
                    getHeight = methodInfo.DelegateForCall<object, object>();
                    continue;
                }
            }
        }
    }
}