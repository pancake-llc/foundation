using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Pancake.Editor
{
    public abstract class AttributeDrawer : CustomDrawer
    {
        internal Attribute RawAttribute { get; set; }
    }

    public abstract class AttributeDrawer<TAttribute> : AttributeDrawer where TAttribute : Attribute
    {
        [PublicAPI] public TAttribute Attribute => (TAttribute) RawAttribute;

        public sealed override InspectorElement CreateElementInternal(Property property, InspectorElement next) { return CreateElement(property, next); }

        [PublicAPI]
        public virtual InspectorElement CreateElement(Property property, InspectorElement next) { return new DefaultAttributeDrawerInspectorElement(this, property, next); }

        [PublicAPI]
        public virtual float GetHeight(float width, Property property, InspectorElement next) { return next.GetHeight(width); }

        [PublicAPI]
        public virtual void OnGUI(Rect position, Property property, InspectorElement next) { next.OnGUI(position); }

        internal class DefaultAttributeDrawerInspectorElement : InspectorElement
        {
            private readonly AttributeDrawer<TAttribute> _drawer;
            private readonly InspectorElement _next;
            private readonly Property _property;

            public DefaultAttributeDrawerInspectorElement(AttributeDrawer<TAttribute> drawer, Property property, InspectorElement next)
            {
                _drawer = drawer;
                _property = property;
                _next = next;

                AddChild(next);
            }

            public override float GetHeight(float width) { return _drawer.GetHeight(width, _property, _next); }

            public override void OnGUI(Rect position) { _drawer.OnGUI(position, _property, _next); }
        }
    }
}