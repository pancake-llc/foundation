using System;
using JetBrains.Annotations;


namespace PancakeEditor.Attribute
{
    public abstract class GroupDrawer
    {
        public abstract PropertyCollectionBaseInspectorElement CreateElementInternal(System.Attribute attribute);
    }

    public abstract class GroupDrawer<TAttribute> : GroupDrawer where TAttribute : System.Attribute
    {
        public sealed override PropertyCollectionBaseInspectorElement CreateElementInternal(System.Attribute attribute) { return CreateElement((TAttribute) attribute); }

        [PublicAPI]
        public abstract PropertyCollectionBaseInspectorElement CreateElement(TAttribute attribute);
    }
}