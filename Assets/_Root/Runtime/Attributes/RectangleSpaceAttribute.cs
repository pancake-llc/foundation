using System;
using UnityEngine;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RectangleSpaceAttribute : DecoratorAttribute
    {
        public readonly string method;

        /// <param name="method">Format: <b>void OnGUI(Rect position);</b></param>
        public RectangleSpaceAttribute(string method)
        {
            this.method = method;
            Height = 150;
        }

        #region [Optional Parameters]

        /// <summary>
        /// Height of the preview window.
        /// </summary>
        public float Height { get; set; }

        #endregion
    }
}