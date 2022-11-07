using Pancake.Editor;

[assembly: RegisterPropertyDisableProcessor(typeof(DisableIfProcessor))]

namespace Pancake.Editor
{
    public class DisableIfProcessor : PropertyDisableProcessor<DisableIfAttribute>
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

        public sealed override bool IsDisabled(Property property)
        {
            var val = _conditionResolver.GetValue(property);
            var equal = val?.Equals(Attribute.Value) ?? Attribute.Value == null;
            return equal != Attribute.Inverse;
        }
    }
}