using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public class ToggleButton : MethodButton
    {
        private int index;
        private Action<bool> onClick;

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public ToggleButton(SerializedObject serializedObject, string memberName)
            : base(serializedObject, memberName)
        {
            MethodInfo methodInfo = GetMemberInfo() as MethodInfo;
            onClick = (Action<bool>) methodInfo.CreateDelegate(typeof(Action<bool>), GetDeclaringObject());
            index = -1;
        }

        /// <summary>
        /// Called for rendering and handling button GUI.
        /// </summary>
        /// <param name="position">Rectangle position to draw button GUI.</param>
        protected override void OnButtonGUI(Rect position, GUIStyle style)
        {
            EditorGUI.BeginChangeCheck();
            int prevIndex = index;
            index = GUI.Toolbar(position, index, new[] {GetLabel()}, style);
            if (EditorGUI.EndChangeCheck())
            {
                if (index == prevIndex)
                {
                    index = -1;
                }

                Invoke(index > -1);
            }
        }

        /// <summary>
        /// Invoke method.
        /// </summary>
        public void Invoke(bool value)
        {
            try
            {
                onClick.Invoke(value);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Button click execution throw exception: {ex.Message}");
            }
        }
    }
}