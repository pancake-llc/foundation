using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;

namespace Pancake.ApexEditor
{
    [InlineDecoratorTarget(typeof(InlineButtonAttribute))]
    public sealed class InlineButtonDecorator : FieldInlineDecorator
    {
        private InlineButtonAttribute attribute;
        private GUIContent content;
        private GUIStyle style;

        // Stored callback properties.
        private object target;
        private SerializedProperty property;
        private MethodCaller<object, object> onClick;

        /// <summary>
        /// Called once when initializing element inline decorator.
        /// </summary>
        /// <param name="serializedField">serialized field with InlineDecoratorAttribute.</param>
        /// <param name="inlineDecoratorAttribute">InlineDecoratorAttribute of serialized field.</param>
        /// <param name="label">Label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, InlineDecoratorAttribute inlineDecoratorAttribute, GUIContent label)
        {
            attribute = inlineDecoratorAttribute as InlineButtonAttribute;

            property = serializedField.GetSerializedProperty();
            target = serializedField.GetDeclaringObject();
            FindCallback();
            SetButtonLabel();
        }

        /// <summary>
        /// Called for rendering and handling inline decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing inline decorator.</param>
        public override void OnGUI(Rect position)
        {
            if (style == null)
            {
                style = FindStyle();
            }

            if (GUI.Button(position, content, style))
            {
                if (onClick != null)
                {
                    ParameterInfo[] parameters = onClick.Method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        onClick.Invoke(target, null);
                    }
                    else if (parameters.Length == 1)
                    {
                        onClick.Invoke(target, new object[1] {property});
                    }
                    else if (parameters.Length == 2)
                    {
                        onClick.Invoke(target, new object[2] {position, property});
                    }
                }
                else
                {
                    Debug.LogError($"<b>Apex Message:</b> Action with name {attribute.name} is not fonded. The method must be of type void and must have no parameters.");
                }
            }
        }

        /// <summary>
        /// Get the width of the inline decorator, which required to display it.
        /// Calculate only the size of the current inline decorator, not the entire property.
        /// The inline decorator width will be added to the total size of the property with other painters.
        /// </summary>
        public override float GetWidth()
        {
            if (style == null)
            {
                style = FindStyle();
            }

            return attribute.Width == 0 ? style.CalcSize(content).x : attribute.Width;
        }

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public override InlineDecoratorSide GetSide() { return attribute.Side; }

        private void FindCallback()
        {
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
            {
                if (methodInfo.IsValidCallback(attribute.name, typeof(void), null) ||
                    methodInfo.IsValidCallback(attribute.name, typeof(void), typeof(SerializedProperty)) ||
                    methodInfo.IsValidCallback(attribute.name, typeof(void), typeof(Rect), typeof(SerializedProperty)))
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
                GUIStyle customStyle = null;
                if (styleName[0] == '@')
                {
                    customStyle = new GUIStyle(styleName.Remove(0, 1));
                }
                else
                {
                    customStyle = ApexCallbackUtility.GetCallbackResult<GUIStyle>(target, styleName);
                }

                if (customStyle != null)
                {
                    style = customStyle;
                }
                else
                {
                    Debug.LogWarning($"Style not found (used default Button style).");
                }
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