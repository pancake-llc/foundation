using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(FieldButtonAttribute))]
    sealed class FieldButtonDecorator : FieldDecorator
    {
        private FieldButtonAttribute attribute;
        private GUIContent content;
        private GUIStyle style;
        private float width;

        // Stored callbacks.
        private object target;
        private MethodCaller<object, object> onClick;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="serializedField">Serialized field reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized field decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override void Initialize(SerializedField serializedField, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            target = serializedField.GetDeclaringObject();
            attribute = decoratorAttribute as FieldButtonAttribute;
            FindCallback();
            SetButtonLabel();
        }

        /// <summary>
        /// Called for rendering and handling GUI events.
        /// </summary>
        public override void OnGUI(Rect position)
        {
            if (style == null)
            {
                style = FindStyle();
            }

            width = position.width;
            if (GUI.Button(position, content, style))
            {
                onClick.SafeInvoke(target, null);
                Debug.Assert(onClick != null,
                    $"<b>Apex Message:</b> Action with name {attribute.name} is not fonded. The method must be of type void and must have no parameters.");
            }
        }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight()
        {
            if (style == null)
            {
                style = FindStyle();
            }

            return attribute.Height != 0 ? attribute.Height : style.CalcHeight(content, width);
        }

        private void FindCallback()
        {
            foreach (MethodInfo methodInfo in target.GetType().AllMethods())
            {
                if (methodInfo.IsValidCallback(attribute.name, typeof(void), null))
                {
                    onClick = methodInfo.DelegateForCall();
                    break;
                }
            }
        }

        private GUIStyle FindStyle()
        {
            GUIStyle style = GUI.skin.button;
            string styleName = attribute.Style;
            if (!string.IsNullOrEmpty(styleName))
            {
                if (styleName[0] == '@')
                {
                    return new GUIStyle(styleName.Remove(0, 1));
                }

                return ApexCallbackUtility.GetCallbackResult<GUIStyle>(target, styleName);
            }

            return style;
        }

        private void SetButtonLabel()
        {
            content = new GUIContent();
            if (!string.IsNullOrEmpty(attribute.Label))
            {
                content.text = attribute.Label;
                if (attribute.Label[0] == '@')
                {
                    content = EditorGUIUtility.IconContent(attribute.Label.Remove(0, 1));
                }
            }
            else if (onClick != null)
            {
                content.text = onClick.GetMethodInfo().Name;
            }
            else
            {
                content = EditorGUIUtility.IconContent("console.erroricon.sml");
            }
        }
    }
}