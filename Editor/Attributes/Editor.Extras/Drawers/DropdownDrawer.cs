using System.Collections.Generic;
using System.Linq;
using Pancake.Attribute;
using PancakeEditor.Attribute;


[assembly: RegisterAttributeDrawer(typeof(DropdownDrawer<>), DrawerOrder.Decorator)]

namespace PancakeEditor.Attribute
{
    public class DropdownDrawer<T> : AttributeDrawer<DropdownAttribute>
    {
        private DropdownValuesResolver<T> _valuesResolver;

        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            _valuesResolver = DropdownValuesResolver<T>.Resolve(propertyDefinition, Attribute.Values);

            if (_valuesResolver.TryGetErrorString(out string error)) return error;

            return ExtensionInitializationResult.Ok;
        }

        public override InspectorElement CreateElement(Property property, InspectorElement next)
        {
            return new DropdownInspectorElement(property, _valuesResolver.GetDropdownItems);
        }
    }
}