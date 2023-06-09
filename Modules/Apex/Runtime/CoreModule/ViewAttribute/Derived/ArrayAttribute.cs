using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ArrayAttribute : ViewAttribute
    {
        /// <summary>
        /// Apex array attribute.
        /// </summary>
        public ArrayAttribute()
        {
            OnElementGUI = string.Empty;
            GetElementHeight = string.Empty;
            GetElementLabel = string.Empty;
        }

        #region [Optional Parameters]

        /// <summary>
        /// Custom element name display format. Arguments: {index}, {niceIndex}
        /// </summary>
        [System.Obsolete("Use GetElementLabel callback instead.")] public string ElementLabel { get; set; }

        /// <summary>
        /// Custom element GUI.
        /// <br>Example: <b>void OnElementGUI(Rect position, SerializedProperty property, GUIContent label)</b></br>
        /// </summary>
        public string OnElementGUI { get; set; }

        /// <summary>
        /// Custom element GUI.
        /// <br>Example: <b>float GetElementHeight(SerializedProperty property, GUIContent label)</b></br>
        /// </summary>
        public string GetElementHeight { get; set; }

        /// <summary>
        /// Custom element GUI.
        /// <br>Example: <b>string GetElementLabel(SerializedProperty array, int index)</b></br>
        /// </summary>
        public string GetElementLabel { get; set; }

        #endregion
    }
}