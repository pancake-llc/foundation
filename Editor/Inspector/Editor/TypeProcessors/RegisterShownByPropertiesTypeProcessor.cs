using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake.Editor;


[assembly: RegisterTypeProcessor(typeof(RegisterShownByPropertiesTypeProcessor), 1)]

namespace Pancake.Editor
{
    public class RegisterShownByPropertiesTypeProcessor : TypeProcessor
    {
        public override void ProcessType(Type type, List<PropertyDefinition> properties)
        {
            const int propertiesOffset = 10001;

            properties.AddRange(ReflectionUtilities.GetAllInstancePropertiesInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => PropertyDefinition.CreateForPropertyInfo(ind + propertiesOffset, it)));
        }

        private static bool IsSerialized(PropertyInfo propertyInfo) { return propertyInfo.GetCustomAttribute<ShowInInspector>() != null; }
    }
}