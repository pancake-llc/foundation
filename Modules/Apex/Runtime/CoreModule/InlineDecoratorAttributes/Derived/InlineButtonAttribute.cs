using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InlineButtonAttribute : InlineDecoratorAttribute
    {
        public readonly string name;

        public InlineButtonAttribute(string method)
        {
            this.name = method;
            Label = method;
            Width = 0;
            Style = "@Button";
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