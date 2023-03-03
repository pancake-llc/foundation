using System.Collections.Generic;
using System.Linq;
using Pancake.Attribute;
using PancakeEditor.Attribute;


[assembly: RegisterAttributeDrawer(typeof(DropdownDrawer<>), DrawerOrder.Decorator)]

namespace PancakeEditor.Attribute
{
    public class DropdownDrawer<T> : AttributeDrawer<DropdownAttribute>
    {
        private ValueResolver<IEnumerable<DropdownItem<T>>> _itemsResolver;
        private ValueResolver<IEnumerable<T>> _valuesResolver;

        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            _valuesResolver = ValueResolver.Resolve<IEnumerable<T>>(propertyDefinition, Attribute.Values);

            if (_valuesResolver.TryGetErrorString(out _))
            {
                _itemsResolver = ValueResolver.Resolve<IEnumerable<DropdownItem<T>>>(propertyDefinition, Attribute.Values);

                if (_itemsResolver.TryGetErrorString(out var itemResolverError))
                {
                    return itemResolverError;
                }
            }

            return ExtensionInitializationResult.Ok;
        }

        public override InspectorElement CreateElement(Property property, InspectorElement next) { return new DropdownInspectorElement(property, GetDropdownItems); }

        private IEnumerable<IDropdownItem> GetDropdownItems(Property property)
        {
            if (_valuesResolver != null)
            {
                var values = _valuesResolver.GetValue(property, Enumerable.Empty<T>());

                foreach (var value in values)
                {
                    yield return new DropdownItem {Text = $"{value}", Value = value,};
                }
            }

            if (_itemsResolver != null)
            {
                var values = _itemsResolver.GetValue(property, Enumerable.Empty<DropdownItem<T>>());

                foreach (var value in values)
                {
                    yield return value;
                }
            }
        }
    }
}