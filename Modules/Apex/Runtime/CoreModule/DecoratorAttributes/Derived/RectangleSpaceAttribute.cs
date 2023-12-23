using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RectangleSpaceAttribute : DecoratorAttribute
    {
        public readonly string name;

        /// <param name="name">Format: <b>void OnGUI(Rect position);</b></param>
        public RectangleSpaceAttribute(string name)
        {
            this.name = name;
            Height = 30;
            GetHeightCallback = string.Empty;
        }

        #region [Optional Parameters]

        /// <summary>
        /// Height of the rectangle space.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Dynamic height for rectangle space.
        /// </summary>
        public string GetHeightCallback { get; set; }

        #endregion
    }
}