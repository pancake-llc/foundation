using Pancake.Apex;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(DisplayAsStringAttribute))]
    public sealed class DisplayAsStringView : FieldView
    {
        private DisplayAsStringAttribute attribute;
        private GUIStyle style;
        private float width;

        // Stored callback properties.
        private object target;
        private MemberGetter<object, object> valueGetter;

        /// <summary>
        /// Called once when initializing FieldView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as DisplayAsStringAttribute;

            FieldInfo fieldInfo = serializedField.GetMemberInfo() as FieldInfo;
            valueGetter = fieldInfo.DelegateForGet();
            target = serializedField.GetDeclaringObject();
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (style == null)
            {
                style = FindStyle(target, attribute.Style);
            }

            width = position.width;

            object value = valueGetter.Invoke(target);
            GUIContent content = new GUIContent(value?.ToString() ?? string.Empty);
            EditorGUI.LabelField(position, label, content, style);
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label)
        {
            if (style == null)
            {
                style = FindStyle(target, attribute.Style);
            }

            object value = valueGetter.Invoke(target);
            return style.CalcHeight(new GUIContent(value?.ToString() ?? string.Empty), width);
        }

        /// <summary>
        /// Find style of text.
        /// </summary>
        private GUIStyle FindStyle(object target, string styleName)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.wordWrap = true;
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
    }
}