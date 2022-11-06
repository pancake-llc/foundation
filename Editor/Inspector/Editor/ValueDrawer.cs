using JetBrains.Annotations;
using UnityEngine;

namespace Pancake.Editor
{
    public abstract class ValueDrawer : CustomDrawer
    {
    }

    public abstract class ValueDrawer<TValue> : ValueDrawer
    {
        public sealed override InspectorElement CreateElementInternal(Property property, InspectorElement next) { return CreateElement(new InspectorValue<TValue>(property), next); }

        [PublicAPI]
        public virtual InspectorElement CreateElement(InspectorValue<TValue> propertyValue, InspectorElement next)
        {
            return new DefaultValueDrawerInspectorElement<TValue>(this, propertyValue, next);
        }

        [PublicAPI]
        public virtual float GetHeight(float width, InspectorValue<TValue> propertyValue, InspectorElement next) { return next.GetHeight(width); }

        [PublicAPI]
        public virtual void OnGUI(Rect position, InspectorValue<TValue> propertyValue, InspectorElement next) { next.OnGUI(position); }

        internal class DefaultValueDrawerInspectorElement<T> : InspectorElement
        {
            private readonly ValueDrawer<T> _drawer;
            private readonly InspectorElement _next;
            private readonly InspectorValue<T> _propertyValue;

            public DefaultValueDrawerInspectorElement(ValueDrawer<T> drawer, InspectorValue<T> propertyValue, InspectorElement next)
            {
                _drawer = drawer;
                _propertyValue = propertyValue;
                _next = next;

                AddChild(next);
            }

            public override float GetHeight(float width) { return _drawer.GetHeight(width, _propertyValue, _next); }

            public override void OnGUI(Rect position) { _drawer.OnGUI(position, _propertyValue, _next); }
        }
    }
}