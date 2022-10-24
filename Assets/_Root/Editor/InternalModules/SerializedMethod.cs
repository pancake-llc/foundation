using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class SerializedMethod : SerializedMember
    {
        private GUIStyle style;

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public SerializedMethod(SerializedObject serializedObject, string memberName)
            : base(serializedObject, memberName)
        {
        }

        #region [SerializedMember Implementation]

        /// <summary>
        /// Called for rendering and handling serialized element.
        /// </summary>
        /// <param name="position">Rectangle position to draw member GUI.</param>
        protected override void OnMemberGUI(Rect position)
        {
            if (style == null)
            {
                style = FindStyle();
            }

            if (GUI.Button(EditorGUI.IndentedRect(position), GetLabel(), style))
            {
                Invoke();
            }
        }

        /// <summary>
        /// Serialized method height.
        /// </summary>
        protected override float GetMemberHeight() { return EditorGUIUtility.singleLineHeight; }

        #endregion

        /// <summary>
        /// Invoke method.
        /// </summary>
        public virtual void Invoke()
        {
            if (GetMemberInfo() is MethodInfo methodInfo)
            {
                methodInfo?.Invoke(GetMemberTarget(), null);
            }
        }

        /// <summary>
        /// Implement this method to override method GUI style.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        private GUIStyle FindStyle()
        {
            GUIStyleAttribute styleAttribute = GetAttribute<GUIStyleAttribute>();
            if (styleAttribute != null)
            {
                MemberInfo[] memberInfos = GetMemberTarget()
                    .GetType()
                    .GetMember(styleAttribute.member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                for (int i = 0; i < memberInfos.Length; i++)
                {
                    MemberInfo memberInfo = memberInfos[i];
                    if (memberInfo is FieldInfo fieldInfo && fieldInfo.FieldType == typeof(GUIStyle))
                    {
                        return (GUIStyle) fieldInfo.GetValue(GetMemberTarget());
                    }
                    else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.PropertyType == typeof(GUIStyle))
                    {
                        return (GUIStyle) propertyInfo.GetValue(GetMemberTarget());
                    }
                    else if (memberInfo is MethodInfo methodInfo && methodInfo.ReturnType == typeof(GUIStyle) && methodInfo.GetParameters().Length == 0)
                    {
                        return (GUIStyle) methodInfo.Invoke(GetMemberTarget(), null);
                    }
                }
            }

            return new GUIStyle(GUI.skin.button);
        }

        #region [Getter / Setter]

        public GUIStyle GetStyle() { return style; }

        public void SetStyle(GUIStyle value) { style = value; }

        #endregion
    }
}