using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class SerializedMethod : SerializedMember
    {
        private MethodInfo methodInfo;
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

        /// <summary>
        /// Called for rendering and handling serialized element.
        /// </summary>
        /// <param name="position">Rectangle position to draw member GUI.</param>
        protected override void OnMemberGUI(Rect position)
        {
            if (style == null)
            {
                style = FindStyle(serializedObject);
            }

            if (GUI.Button(position, GetLabel(), style))
            {
                Invoke();
            }
        }

        /// <summary>
        /// Find member info of serialized member. 
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        protected override MemberInfo FindMemberInfo(SerializedObject serializedObject, string memberName)
        {
            methodInfo = serializedObject.targetObject.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.Name == memberName && m.GetParameters().Length == 0)
                .FirstOrDefault();
            return methodInfo;
        }

        /// <summary>
        /// Serialized method height.
        /// </summary>
        protected override float GetMemberHeight() { return EditorGUIUtility.singleLineHeight; }

        /// <summary>
        /// Invoke method.
        /// </summary>
        public virtual void Invoke() { methodInfo?.Invoke(serializedObject.targetObject, null); }

        /// <summary>
        /// Implement this method to override method GUI style.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        protected virtual GUIStyle FindStyle(SerializedObject serializedObject)
        {
            GUIStyleAttribute styleAttribute = GetAttribute<GUIStyleAttribute>();
            if (styleAttribute != null)
            {
                Object target = serializedObject.targetObject;
                MemberInfo[] memberInfos = target.GetType()
                    .GetMember(styleAttribute.member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                for (int i = 0; i < memberInfos.Length; i++)
                {
                    MemberInfo memberInfo = memberInfos[i];
                    if (memberInfo is FieldInfo fieldInfo && fieldInfo.FieldType == typeof(GUIStyle))
                    {
                        return (GUIStyle) fieldInfo.GetValue(target);
                    }
                    else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.PropertyType == typeof(GUIStyle))
                    {
                        return (GUIStyle) propertyInfo.GetValue(target);
                    }
                    else if (memberInfo is MethodInfo methodInfo && methodInfo.ReturnType == typeof(GUIStyle) && methodInfo.GetParameters().Length == 0)
                    {
                        return (GUIStyle) methodInfo.Invoke(target, null);
                    }
                }
            }

            return new GUIStyle(GUI.skin.button);
        }

        #region [Getter / Setter]

        public MethodInfo GetMethodInfo() { return methodInfo; }

        public GUIStyle GetStyle() { return style; }

        public void SetStyle(GUIStyle value) { style = value; }

        #endregion
    }
}