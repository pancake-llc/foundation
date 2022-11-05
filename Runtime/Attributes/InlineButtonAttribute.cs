using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InlineButtonAttribute : InlineDecoratorAttribute
    {
        public readonly string method;

        public InlineButtonAttribute(string method)
        {
            this.method = method;
            Label = method;
            Width = -1.0f;
            Style = "Button";
            Side = InlineDecoratorSide.Right;
        }

        #region [Parameters]

        /// <summary>
        /// Custom label for button.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Custom width for button.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Custom style for button.
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public InlineDecoratorSide Side { get; set; }

        #endregion
    }
}