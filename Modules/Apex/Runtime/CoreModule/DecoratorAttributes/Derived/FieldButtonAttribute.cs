using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class FieldButtonAttribute : DecoratorAttribute
    {
        public readonly string name;

        public FieldButtonAttribute(string name)
        {
            this.name = name;
            Label = name;
            Height = 20.0f;
            Style = "@Button";
        }

        #region [Parameters]

        /// <summary>
        /// Custom name for button.
        /// Use the @ prefix to indicate, that a texture will be used instead of the name.
        /// Arguments: @{Default Unity Icon Name}, @{Path to texture}
        /// Example: @_Popup, @Assets/...
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Custom button height.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Custom button style.
        /// </summary>
        public string Style { get; set; }

        #endregion
    }
}