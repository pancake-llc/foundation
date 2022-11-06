using System;
using JetBrains.Annotations;

namespace Pancake.Editor
{
    public abstract class PropertyDisableProcessor : PropertyExtension
    {
        internal Attribute RawAttribute { get; set; }

        [PublicAPI]
        public abstract bool IsDisabled(Property property);
    }

    public abstract class PropertyDisableProcessor<TAttribute> : PropertyDisableProcessor where TAttribute : Attribute
    {
        [PublicAPI] public TAttribute Attribute => (TAttribute) RawAttribute;
    }
}