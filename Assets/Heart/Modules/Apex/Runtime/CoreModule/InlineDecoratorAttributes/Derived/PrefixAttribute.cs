using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class PrefixAttribute : InlineDecoratorAttribute
    {
        public readonly string label;

        public PrefixAttribute(string label)
        {
            this.label = label;
            Style = "Label";
            BeforeField = false;
        }

        #region [Optional Parameters]

        public string Style { get; set; }

        public bool BeforeField { get; set; }

        #endregion
    }
}