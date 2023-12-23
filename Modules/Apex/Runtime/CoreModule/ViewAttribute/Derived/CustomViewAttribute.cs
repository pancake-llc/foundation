using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CustomViewAttribute : ViewAttribute
    {
        /// <summary>
        /// Called once when element view initialization.
        /// <br>Method format: <b>void OnInitialization(SerializedProperty property, GUIContent label)</b></br>
        /// </summary>
        public string OnInitialization { get; set; }

        /// <summary>
        /// Called for drawing element.
        /// <br>Method format: <b>void OnGUI(Rect position, SerializedProperty property, GUIContent label)</b></br>
        /// </summary>
        public string OnGUI { get; set; }

        /// <summary>
        /// Called for calculation element height.
        /// <br>Method format: <b>float GetHeight(SerializedProperty property, GUIContent label)</b></br>
        /// </summary>
        public string GetHeight { get; set; }
    }
}