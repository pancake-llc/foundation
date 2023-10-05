using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ArrayAttribute : ViewAttribute
    {
        /// <summary>
        /// Array attribute constructor.
        /// </summary>
        public ArrayAttribute()
        {
            OnGUI = string.Empty;
            GetHeight = string.Empty;
            GetLabel = string.Empty;
            OnAdd = string.Empty;
            OnRemove = string.Empty;
        }

        #region [Optional Parameters]

        /// <summary>
        /// Override array element GUI.
        /// <br>Example: <b>void OnElementGUI(Rect position, SerializedProperty property, GUIContent label)</b></br>
        /// </summary>
        public string OnGUI { get; set; }

        /// <summary>
        /// Override array element height.
        /// <br>Example: <b>float GetElementHeight(SerializedProperty array, int index)</b></br>
        /// </summary>
        public string GetHeight { get; set; }

        /// <summary>
        /// Override array element label.
        /// <br>Example: <b>GUIContent GetElementLabel(SerializedProperty array, int index)</b></br>
        /// </summary>
        public string GetLabel { get; set; }

        /// <summary>
        /// Callback on add new element.
        /// <br>Called after adding new element.</br>
        /// <br>Example: <b>void OnAdd(SerializedProperty array, int index)</b></br>
        /// </summary>
        public string OnAdd { get; set; }

        /// <summary>
        /// Callback on remove selected element.
        /// <br>Called before removing selected element.</br>
        /// <br>Example: <b>void OnRemove(SerializedProperty array, int index)</b></br>
        /// </summary>
        public string OnRemove { get; set; }

        /// <summary>
        /// Callback on reorder array list.
        /// <br>Called after list has been reordered.</br>
        /// <br>Example: <b>void OnReorder(SerializedProperty array)</b></br>
        /// </summary>
        public string OnReorder { get; set; }

        #endregion
    }
}