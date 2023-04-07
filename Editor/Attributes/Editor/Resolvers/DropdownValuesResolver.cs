using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pancake.Attribute;

namespace PancakeEditor.Attribute
{
    public class DropdownValuesResolver<T>
    {
        [CanBeNull] private ValueResolver<IEnumerable<DropdownItem<T>>> _itemsResolver;
        [CanBeNull] private ValueResolver<IEnumerable<T>> _valuesResolver;

        [PublicAPI]
        public static DropdownValuesResolver<T> Resolve(PropertyDefinition propertyDefinition, string expression)
        {
            var valuesResolver = ValueResolver.Resolve<IEnumerable<T>>(propertyDefinition, expression);
            if (!valuesResolver.TryGetErrorString(out _))
            {
                return new DropdownValuesResolver<T> {_valuesResolver = valuesResolver,};
            }

            var itemsResolver = ValueResolver.Resolve<IEnumerable<DropdownItem<T>>>(propertyDefinition, expression);

            return new DropdownValuesResolver<T> {_itemsResolver = itemsResolver,};
        }

        [PublicAPI]
        public bool TryGetErrorString(out string error) { return ValueResolver.TryGetErrorString(_valuesResolver, _itemsResolver, out error); }

        [PublicAPI]
        public IEnumerable<IDropdownItem> GetDropdownItems(Property property)
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