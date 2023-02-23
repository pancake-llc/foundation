using Pancake.AttributeDrawer;

[assembly: RegisterPropertyHideProcessor(typeof(HideIfProcessor))]

namespace Pancake.AttributeDrawer
{
    public class HideIfProcessor : PropertyHideProcessor<HideIfAttribute>
    {
        private ValueResolver<object> _conditionResolver;

        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _conditionResolver = ValueResolver.Resolve<object>(propertyDefinition, Attribute.Condition);

            if (_conditionResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return ExtensionInitializationResult.Ok;
        }

        public sealed override bool IsHidden(Property property)
        {
            var val = _conditionResolver.GetValue(property);
            var equal = val?.Equals(Attribute.Value) ?? Attribute.Value == null;
            return equal != Attribute.Inverse;
        }
    }
}