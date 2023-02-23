using System;
using System.Collections.Generic;


namespace Pancake.AttributeDrawer
{
    public class TypeDefinition
    {
        private static readonly Dictionary<Type, TypeDefinition> Cache = new Dictionary<Type, TypeDefinition>();

        private TypeDefinition(IReadOnlyList<PropertyDefinition> properties) { Properties = properties; }

        public IReadOnlyList<PropertyDefinition> Properties { get; }

        public static TypeDefinition GetCached(Type type)
        {
            if (Cache.TryGetValue(type, out var definition))
            {
                return definition;
            }

            var processors = DrawersUtilities.AllTypeProcessors;
            var properties = new List<PropertyDefinition>();

            foreach (var processor in processors)
            {
                processor.ProcessType(type, properties);
            }

            return Cache[type] = new TypeDefinition(properties);
        }
    }
}