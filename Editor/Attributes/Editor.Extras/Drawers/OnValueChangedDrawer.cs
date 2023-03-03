using Pancake.Attribute;
using PancakeEditor.Attribute;

[assembly: RegisterAttributeDrawer(typeof(OnValueChangedDrawer), DrawerOrder.System)]

namespace PancakeEditor.Attribute
{
    public class OnValueChangedDrawer : AttributeDrawer<OnValueChangedAttribute>
    {
        private ActionResolver _actionResolver;

        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _actionResolver = ActionResolver.Resolve(propertyDefinition, Attribute.Method);
            if (_actionResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return ExtensionInitializationResult.Ok;
        }

        public override InspectorElement CreateElement(Property property, InspectorElement next) { return new OnValueChangedListenerInspectorElement(property, next, _actionResolver); }

        private class OnValueChangedListenerInspectorElement : InspectorElement
        {
            private readonly Property _property;
            private readonly ActionResolver _actionResolver;

            public OnValueChangedListenerInspectorElement(Property property, InspectorElement next, ActionResolver actionResolver)
            {
                _property = property;
                _actionResolver = actionResolver;

                AddChild(next);
            }

            protected override void OnAttachToPanel()
            {
                base.OnAttachToPanel();

                _property.ValueChanged += OnValueChanged;
                _property.ChildValueChanged += OnValueChanged;
            }

            protected override void OnDetachFromPanel()
            {
                _property.ChildValueChanged -= OnValueChanged;
                _property.ValueChanged -= OnValueChanged;

                base.OnDetachFromPanel();
            }

            private void OnValueChanged(Property obj)
            {
                _property.PropertyTree.ApplyChanges();
                _actionResolver.InvokeForAllTargets(_property);
                _property.PropertyTree.Update();
            }
        }
    }
}