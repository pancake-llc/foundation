using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ReorderableListAttribute : ViewAttribute
    {
        public ReorderableListAttribute()
        {
            HeaderHeight = 20;
            Draggable = true;
            ShowAddButton = true;
            ShowRemoveButton = true;
            OnHeaderGUI = string.Empty;
            OnElementGUI = string.Empty;
            OnNoneElementGUI = string.Empty;
            GetElementHeight = string.Empty;
            OnAddElement = string.Empty;
            OnAddDropdownElement = string.Empty;
            OnRemoveElement = string.Empty;
            GetElementLabel = string.Empty;
        }

        /// <summary>
        /// Height of the header.
        /// </summary>
        public float HeaderHeight { get; set; }

        /// <summary>
        /// Elements is draggable?
        /// </summary>
        public bool Draggable { get; set; }

        /// <summary>
        /// Show add element button?
        /// </summary>
        public bool ShowAddButton { get; set; }

        /// <summary>
        /// Show remove element button?
        /// </summary>
        public bool ShowRemoveButton { get; set; }

        /// <summary>
        /// Called to draw header GUI.
        /// <br>Method format: <b>void OnHeaderGUI(Rect position)</b></br>
        /// </summary>
        public string OnHeaderGUI { get; set; }

        /// <summary>
        /// Called to draw list element GUI.
        /// <br>Method format: <b>void OnElementGUI(Rect position, SerializedProperty element, GUIContent label)</b></br>
        /// </summary>
        public string OnElementGUI { get; set; }

        /// <summary>
        /// Called when list is empty.
        /// <br>Method format: <b>void OnNoneElementGUI(Rect position)</b></br>
        /// </summary>
        public string OnNoneElementGUI { get; set; }

        /// <summary>
        /// Called to calculate list element height.
        /// <br>Method format: <b>float GetElementHeight(SerializedProperty element)</b></br>
        /// </summary>
        public string GetElementHeight { get; set; }

        /// <summary>
        /// Called when added new element to list.
        /// <br>Method format: <b>void OnAddElement(SerializedProperty array)</b></br>
        /// </summary>
        public string OnAddElement { get; set; }

        /// <summary>
        /// Called when added new element to list through dropdown menu.
        /// <br>Method format: <b>void OnDropdownAddElement(Rect position, SerializedProperty list)</b></br>
        /// </summary>
        public string OnAddDropdownElement { get; set; }

        /// <summary>
        /// Called when removed element from list.
        /// <br>Method format: <b>void OnRemoveElement(SerializedProperty array)</b></br>
        /// </summary>
        public string OnRemoveElement { get; set; }

        /// <summary>
        /// Called to draw element label.
        /// <br>Method format: <b>GUIContent GetElementLabel(SerializedProperty array, int index)</b></br>
        /// </summary>
        public string GetElementLabel { get; set; }
    }
}