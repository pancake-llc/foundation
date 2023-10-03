using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public class RepeatButton : MethodButton
    {
        private Action onClick;

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public RepeatButton(SerializedObject serializedObject, string memberName)
            : base(serializedObject, memberName)
        {
            MethodInfo methodInfo = GetMemberInfo() as MethodInfo;
            onClick = (Action) methodInfo.CreateDelegate(typeof(Action), GetDeclaringObject());
        }

        /// <summary>
        /// Called for rendering and handling button GUI.
        /// </summary>
        /// <param name="position">Rectangle position to draw button GUI.</param>
        protected override void OnButtonGUI(Rect position, GUIStyle style)
        {
            if (GUI.RepeatButton(position, GetLabel(), style))
            {
                Invoke();
                Repaint.Invoke();
            }
        }

        /// <summary>
        /// Invoke method.
        /// </summary>
        public void Invoke()
        {
            try
            {
                onClick.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Button click execution throw exception: {ex.Message}");
            }
        }
    }
}