using System;
using JetBrains.Annotations;


namespace Pancake.AttributeDrawer
{
    public abstract class GroupDrawer
    {
        public abstract PropertyCollectionBaseInspectorElement CreateElementInternal(Attribute attribute);
    }

    public abstract class GroupDrawer<TAttribute> : GroupDrawer where TAttribute : Attribute
    {
        public sealed override PropertyCollectionBaseInspectorElement CreateElementInternal(Attribute attribute) { return CreateElement((TAttribute) attribute); }

        [PublicAPI]
        public abstract PropertyCollectionBaseInspectorElement CreateElement(TAttribute attribute);
    }
}