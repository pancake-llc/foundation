using System;
using System.Collections.Generic;
using Pancake.Attribute;
using PancakeEditor.Attribute;


[assembly: RegisterTypeProcessor(typeof(SortPropertiesTypeProcessor), 10000)]

namespace PancakeEditor.Attribute
{
    public class SortPropertiesTypeProcessor : TypeProcessor
    {
        public override void ProcessType(Type type, List<PropertyDefinition> properties)
        {
            foreach (var propertyDefinition in properties)
            {
                if (propertyDefinition.Attributes.TryGet(out PropertyOrderAttribute orderAttribute))
                {
                    propertyDefinition.Order = orderAttribute.Order;
                }
            }

            properties.Sort(PropertyOrderComparer.Instance);
        }

        private class PropertyOrderComparer : IComparer<PropertyDefinition>
        {
            public static readonly PropertyOrderComparer Instance = new PropertyOrderComparer();

            public int Compare(PropertyDefinition x, PropertyDefinition y) { return x.Order.CompareTo(y.Order); }
        }
    }
}