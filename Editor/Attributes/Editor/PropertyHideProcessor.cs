using System;
using JetBrains.Annotations;

namespace Pancake.AttributeDrawer
{
    public abstract class PropertyHideProcessor : PropertyExtension
    {
        internal Attribute RawAttribute { get; set; }

        [PublicAPI]
        public abstract bool IsHidden(Property property);
    }

    public abstract class PropertyHideProcessor<TAttribute> : PropertyHideProcessor where TAttribute : Attribute
    {
        [PublicAPI] public TAttribute Attribute => (TAttribute) RawAttribute;
    }
}