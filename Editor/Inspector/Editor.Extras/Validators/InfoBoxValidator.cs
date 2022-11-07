using Pancake.Editor;

[assembly: RegisterAttributeValidator(typeof(InfoBoxValidator))]

namespace Pancake.Editor
{
    public class InfoBoxValidator : AttributeValidator<InfoBoxAttribute>
    {
        private ValueResolver<string> _resolver;
        private ValueResolver<bool> _visibleIfResolver;

        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            _resolver = ValueResolver.ResolveString(propertyDefinition, Attribute.Text);
            _visibleIfResolver = Attribute.VisibleIf != null ? ValueResolver.Resolve<bool>(propertyDefinition, Attribute.VisibleIf) : null;

            if (ValueResolver.TryGetErrorString(_resolver, _visibleIfResolver, out var error))
            {
                return error;
            }

            return ExtensionInitializationResult.Ok;
        }

        public override ValidationResult Validate(Property property)
        {
            if (_visibleIfResolver != null && !_visibleIfResolver.GetValue(property))
            {
                return ValidationResult.Valid;
            }

            var message = _resolver.GetValue(property, "");
            return new ValidationResult(false, message, Attribute.MessageType);
        }
    }
}