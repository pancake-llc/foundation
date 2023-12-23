using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public abstract class MethodButton : SerializedMember
    {
        private float width;
        private float customWidth;
        private float customHeight;
        private GUIContent prefixLabel;
        private TextAlignment alignment;
        private GUIStyle style;

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public MethodButton(SerializedObject serializedObject, string memberName)
            : base(serializedObject, memberName)
        {
            ButtonWidthAttribute widthAttribute = GetAttribute<ButtonWidthAttribute>();
            customWidth = widthAttribute?.width ?? -1;
            alignment = widthAttribute?.Alignment ?? TextAlignment.Center;

            ButtonHeightAttribute heightAttribute = GetAttribute<ButtonHeightAttribute>();
            customHeight = heightAttribute?.height ?? -1;

            ButtonPrefixLabelAttribute prefixLabelAttribute = GetAttribute<ButtonPrefixLabelAttribute>();
            prefixLabel = prefixLabelAttribute != null && !string.IsNullOrEmpty(prefixLabelAttribute.label)
                ? new GUIContent(prefixLabelAttribute.label)
                : GUIContent.none;

            string label = GetLabel().text;
            if (label != null && label.Length > 1 && label[0] == '@')
            {
                label = label.Remove(0, 1);
                SetLabel(EditorGUIUtility.IconContent(label));
            }
        }

        /// <summary>
        /// Called for rendering and handling button GUI.
        /// </summary>
        /// <param name="position">Rectangle position to draw button GUI.</param>
        protected abstract void OnButtonGUI(Rect position, GUIStyle style);

        /// <summary>
        /// Total height of button.
        /// </summary>
        /// <param name="width">Button width.</param>
        /// <param name="style">Button style.</param>
        protected virtual float GetButtonHeight(float width, GUIStyle style) { return customHeight == -1 ? style.CalcHeight(GetLabel(), width) : customHeight; }

        #region [SerializedMember Implementation]

        /// <summary>
        /// Called for rendering and handling serialized member.
        /// </summary>
        /// <param name="position">Rectangle position to draw member GUI.</param>
        protected sealed override void OnMemberGUI(Rect position)
        {
            if (prefixLabel != GUIContent.none)
            {
                position = EditorGUI.PrefixLabel(position, prefixLabel);
            }

            if (customWidth != -1)
            {
                switch (alignment)
                {
                    case TextAlignment.Center:
                        position.x += (position.width / 2) - (customWidth / 2);
                        break;
                    case TextAlignment.Right:
                        position.x = position.xMax - customWidth;
                        break;
                }

                position.width = customWidth;
            }

            width = position.width;
            OnButtonGUI(position, GetStyle());
        }

        /// <summary>
        /// Serialized member height.
        /// </summary>
        protected sealed override float GetMemberHeight() { return GetButtonHeight(width, GetStyle()); }

        #endregion

        #region [Getter / Setter]

        private GUIStyle GetStyle()
        {
            if (style == null)
            {
                ButtonStyleAttribute attribute = GetAttribute<ButtonStyleAttribute>();
                if (attribute != null)
                {
                    if (attribute.name[0] == '@')
                    {
                        style = new GUIStyle(attribute.name.Remove(0, 1));
                    }
                    else
                    {
                        style = ApexCallbackUtility.GetCallbackResult<GUIStyle>(GetDeclaringObject(), attribute.name);
                    }
                }
                else
                {
                    style = new GUIStyle(GUI.skin.button);
                }
            }

            return style;
        }

        #endregion
    }
}