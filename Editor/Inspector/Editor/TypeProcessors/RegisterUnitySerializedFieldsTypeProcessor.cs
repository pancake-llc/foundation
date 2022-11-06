using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake.Editor;


[assembly: RegisterTriTypeProcessor(typeof(RegisterUnitySerializedFieldsTypeProcessor), 0)]

namespace Pancake.Editor
{
    public class RegisterUnitySerializedFieldsTypeProcessor : TypeProcessor
    {
        public override void ProcessType(Type type, List<PropertyDefinition> properties)
        {
            const int fieldsOffset = 1;

            properties.AddRange(ReflectionUtilities.GetAllInstanceFieldsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => PropertyDefinition.CreateForFieldInfo(ind + fieldsOffset, it)));
        }

        private static bool IsSerialized(FieldInfo fieldInfo) { return UnitySerializationUtilities.IsSerializableByUnity(fieldInfo); }
    }
}