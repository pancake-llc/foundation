using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public class SerializedCsProperty : SerializedMember
    {
        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public SerializedCsProperty(SerializedObject serializedObject, string memberName)
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
            position = EditorGUI.PrefixLabel(position, GetLabel());
            EditorGUI.SelectableLabel(position, GetValue()?.ToString() ?? "null", EditorStyles.textField);
        }

        /// <summary>
        /// Serialized method height.
        /// </summary>
        protected override float GetMemberHeight() { return EditorGUIUtility.singleLineHeight; }

        #endregion

        /// <summary>
        /// Get value from serialized property.
        /// </summary>
        public virtual object GetValue()
        {
            if (GetMemberInfo() is PropertyInfo propertyInfo && propertyInfo.CanRead)
            {
                return propertyInfo.GetValue(GetDeclaringObject());
            }

            return null;
        }

        /// <summary>
        /// Set value to serialized property.
        /// </summary>
        public virtual void SetValue(object value)
        {
            if (GetMemberInfo() is PropertyInfo propertyInfo && propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GetDeclaringObject(), value);
            }
        }
    }
}