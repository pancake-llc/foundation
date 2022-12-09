using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake.Editor;

[assembly: RegisterTypeProcessor(typeof(RegisterShownByFieldsTypeProcessor), 1)]

namespace Pancake.Editor
{
    public class RegisterShownByFieldsTypeProcessor : TypeProcessor
    {
        public override void ProcessType(Type type, List<PropertyDefinition> properties)
        {
            const int fieldsOffset = 5001;

            properties.AddRange(ReflectionUtilities.GetAllInstanceFieldsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => PropertyDefinition.CreateForFieldInfo(ind + fieldsOffset, it)));
        }

        private static bool IsSerialized(FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttribute<ShowInInspector>() != null && UnitySerializationUtilities.IsSerializableByUnity(fieldInfo) == false;
        }
    }
}