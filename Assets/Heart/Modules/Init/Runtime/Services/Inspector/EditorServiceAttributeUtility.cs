#if !INIT_ARGS_DISABLE_EDITOR_SERVICE_INJECTION && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Sisus.Init.Internal
{
    internal static class EditorServiceAttributeUtility
    {
        private static readonly EditorServiceAttribute[] coroutineRunnerAttributes = { new(typeof(ICoroutineRunner))};

        internal static readonly Dictionary<Type, EditorServiceAttribute[]> concreteTypes = new()
        {
            { typeof(EditorCoroutineRunner), coroutineRunnerAttributes }
        };

        internal static readonly Dictionary<Type, EditorServiceAttribute[]> definingTypes = new()
        {
            { typeof(ICoroutineRunner), coroutineRunnerAttributes }
        };

        static EditorServiceAttributeUtility()
        {
            var typesWithAttribute = TypeCache.GetTypesWithAttribute<EditorServiceAttribute>();
            int count = typesWithAttribute.Count;

            concreteTypes.EnsureCapacity(count);
            definingTypes.EnsureCapacity(count + count);

            foreach(var typeWithAttribute in typesWithAttribute)
            {
                var attributes = typeWithAttribute.GetCustomAttributes<EditorServiceAttribute>().ToArray();
                if(!typeWithAttribute.IsAbstract && !typeWithAttribute.IsGenericTypeDefinition)
                {
                    concreteTypes[typeWithAttribute] = attributes;
                }

                foreach(var attribute in attributes)
                {
                    definingTypes[attribute.definingType] = attributes;
                }
            }
        }
    }
}
#endif