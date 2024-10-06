using UnityEditor;
using UnityEngine;
using static Sisus.Init.EditorOnly.Internal.ScriptGenerator;

namespace Sisus.Init.EditorOnly.Internal
{
    internal static class ScriptableObjectMenuItems
    {
        [MenuItem("CONTEXT/ScriptableObject/Generate Initializer")]
        private static void GenerateInitializerFromScript(MenuCommand command)
        {
            var scriptableObject = command.context as ScriptableObject;
            var script = MonoScript.FromScriptableObject(scriptableObject);
            CreateInitializer(script);
        }

        [MenuItem("CONTEXT/ScriptableObject/Generate Initializer", validate = true)]
        private static bool GenerateInitializerFromScriptEnabled(MenuCommand command)
        {
            return GenerateInitializerEnabled(command.context as ScriptableObject);
        }

        private static bool GenerateInitializerEnabled(ScriptableObject scriptableObject)
        {
            if(scriptableObject == null)
            {
                return false;
            }

            var type = scriptableObject.GetType();
            if(type == null)
            {
                return false;
            }

            foreach(var @interface in type.GetInterfaces())
            {
                if(!@interface.IsGenericType)
                {
                    continue;
                }

                var genericTypeDefinition = @interface.GetGenericTypeDefinition();
                if(genericTypeDefinition == typeof(IInitializable<>)
                   || genericTypeDefinition == typeof(IInitializable<,>)
                   || genericTypeDefinition == typeof(IInitializable<,,>)
                   || genericTypeDefinition == typeof(IInitializable<,,,>)
                   || genericTypeDefinition == typeof(IInitializable<,,,,>)
                   || genericTypeDefinition == typeof(IInitializable<,,,,,>))
                {
                    return true;
                }
            }

            return false;
        }
    }
}