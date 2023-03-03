using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake.Attribute;
using PancakeEditor.Attribute;


[assembly: RegisterTypeProcessor(typeof(RegisterButtonsTypeProcessor), 3)]

namespace PancakeEditor.Attribute
{
    public class RegisterButtonsTypeProcessor : TypeProcessor
    {
        public override void ProcessType(Type type, List<PropertyDefinition> properties)
        {
            const int methodsOffset = 20001;

            properties.AddRange(ReflectionUtilities.GetAllInstanceMethodsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => PropertyDefinition.CreateForMethodInfo(ind + methodsOffset, it)));
        }

        private static bool IsSerialized(MethodInfo methodInfo) { return methodInfo.GetCustomAttribute<ButtonAttribute>() != null; }
    }
}