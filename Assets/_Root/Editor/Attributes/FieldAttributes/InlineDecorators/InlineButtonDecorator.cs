using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [InlineDecoratorTarget(typeof(InlineButtonAttribute))]
    sealed class InlineButtonDecorator : FieldInlineDecorator
    {
        private object target;
        private InlineButtonAttribute attribute;
        private MethodInfo action;
        private GUIContent content;
        private GUIStyle style;

        /// <summary>
        /// Called once when initializing element inline decorator.
        /// </summary>
        /// <param name="element">Serialized element with InlineDecoratorAttribute.</param>
        /// <param name="inlineDecoratorAttribute">InlineDecoratorAttribute of serialized element.</param>
        /// <param name="label">Label of serialized property.</param>
        public override void Initialize(SerializedField element, InlineDecoratorAttribute inlineDecoratorAttribute, GUIContent label)
        {
            target = element.serializedObject.targetObject;
            attribute = inlineDecoratorAttribute as InlineButtonAttribute;
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            action = methods.Where(m => m.Name == attribute.method && m.GetParameters().Length == 0).FirstOrDefault();

            content = new GUIContent();
            if (!string.IsNullOrEmpty(attribute.Label))
            {
                content.text = attribute.Label;
                if (attribute.Label[0] == '@')
                {
                    content = EditorGUIUtility.IconContent(attribute.Label.Remove(0, 1));
                }
            }
            else if (action != null)
            {
                content.text = action.Name;
            }
            else
            {
                content = GUIContent.none;
            }
        }

        /// <summary>
        /// Called for rendering and handling inline decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing inline decorator.</param>
        public override void OnGUI(Rect position)
        {
            if (GUI.Button(position, content, style))
            {
                action?.Invoke(target, null);
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
                style = new GUIStyle(attribute.Style);

            return attribute.Width <= 0 ? style.CalcSize(content).x : attribute.Width;
        }

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public override InlineDecoratorSide GetSide() { return attribute.Side; }
    }
}