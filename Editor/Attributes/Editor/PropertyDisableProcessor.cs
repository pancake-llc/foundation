using System;
using JetBrains.Annotations;

namespace PancakeEditor.Attribute
{
    public abstract class PropertyDisableProcessor : PropertyExtension
    {
        internal System.Attribute RawAttribute { get; set; }

        [PublicAPI]
        public abstract bool IsDisabled(Property property);
    }

    public abstract class PropertyDisableProcessor<TAttribute> : PropertyDisableProcessor where TAttribute : System.Attribute
    {
        [PublicAPI] public TAttribute Attribute => (TAttribute) RawAttribute;
    }
}