using System;
using JetBrains.Annotations;

namespace PancakeEditor.Attribute
{
    public abstract class PropertyHideProcessor : PropertyExtension
    {
        internal System.Attribute RawAttribute { get; set; }

        [PublicAPI]
        public abstract bool IsHidden(Property property);
    }

    public abstract class PropertyHideProcessor<TAttribute> : PropertyHideProcessor where TAttribute : System.Attribute
    {
        [PublicAPI] public TAttribute Attribute => (TAttribute) RawAttribute;
    }
}