using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.SaveData
{
    [UnityEngine.Scripting.Preserve]
    public static class TypeManager
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once InconsistentNaming
        private static object _lock = new object();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Dictionary<Type, CustomType> types;

        // We cache the last accessed type as we quite often use the same type multiple times,
        // so this improves performance as another lookup is not required.
        private static CustomType lastAccessedType;

        public static CustomType GetOrCreateCustomType(Type type, bool throwException = true)
        {
            if (types == null) Init();

            if (type != typeof(object) && lastAccessedType != null && lastAccessedType.type == type) return lastAccessedType;

            // If type doesn't exist, create one.
            if (types.TryGetValue(type, out lastAccessedType)) return lastAccessedType;
            return (lastAccessedType = CreateCustomType(type, throwException));
        }

        public static CustomType GetCustomType(Type type)
        {
            if (types == null) Init();

            if (types.TryGetValue(type, out lastAccessedType)) return lastAccessedType;
            return null;
        }

        internal static void Add(Type type, CustomType customType)
        {
            if (types == null) Init();

            var existingType = GetCustomType(type);
            if (existingType != null && existingType.priority > customType.priority) return;

            lock (_lock)
            {
                types[type] = customType;
            }
        }

        internal static CustomType CreateCustomType(Type type, bool throwException = true)
        {
            CustomType customType;

            if (Reflection.IsEnum(type)) return new CustomTypeEnum(type);
            if (Reflection.TypeIsArray(type))
            {
                int rank = Reflection.GetArrayRank(type);
                if (rank == 1) customType = new CustomArrayType(type);
                else if (rank == 2) customType = new ArrayType2D(type);
                else if (rank == 3) customType = new ArrayType3D(type);
                else if (throwException) throw new NotSupportedException("Only arrays with up to three dimensions are supported");
                else return null;
            }
            else if (Reflection.IsGenericType(type) && Reflection.ImplementsInterface(type, typeof(IEnumerable)))
            {
                Type genericType = Reflection.GetGenericTypeDefinition(type);
                if (genericType == typeof(List<>)) customType = new CustomListType(type);
                else if (genericType == typeof(Dictionary<,>)) customType = new DictionaryType(type);
                else if (genericType == typeof(Queue<>)) customType = new CustomQueueType(type);
                else if (genericType == typeof(Stack<>)) customType = new CustomStackType(type);
                else if (genericType == typeof(HashSet<>)) customType = new CustomHashSetType(type);
                else if (throwException) throw new NotSupportedException("Generic type \"" + type.ToString() + "\" is not supported by");
                else return null;
            }
            else if (Reflection.IsPrimitive(type)) // ERROR: We should not have to create an CustomType for a primitive.
            {
                if (types == null || types.Count == 0) // If the type list is not initialised, it is most likely an initialisation error.
                    throw new TypeLoadException(
                        "CustomType for primitive could not be found, and the type list is empty.");
                throw new TypeLoadException(
                    "CustomType for primitive could not be found, but the type list has been initialised and is not empty.");
            }
            else
            {
                if (Reflection.IsValueType(type)) customType = new CustomReflectedValueType(type);
                else if (Reflection.HasParameterlessConstructor(type) || Reflection.IsAbstract(type) || Reflection.IsInterface(type))
                    customType = new CustomReflectedObjectType(type);
                else if (Reflection.IsAssignableFrom(typeof(ScriptableObject), type))
                    throw new NotSupportedException("Type of " + type +
                                                    " is not supported as it does not have a parameterless constructor. Only value types. However, you may be able to create an CustomType script to add support for it.");
                else if (Reflection.IsAssignableFrom(typeof(Component), type))
                    throw new NotSupportedException("Type of " + type +
                                                    " is not supported as it does not have a parameterless constructor. Only value types. However, you may be able to create an CustomType script to add support for it.");
                else if (throwException)
                    throw new NotSupportedException("Type of " + type +
                                                    " is not supported as it does not have a parameterless constructor. Only value types, Components or ScriptableObjects are supportable without a parameterless constructor. However, you may be able to create an CustomType script to add support for it.");
                else
                    return null;
            }

            if (customType.type == null || customType.isUnsupported)
            {
                if (throwException)
                    throw new NotSupportedException(
                        string.Format("CustomType.type is null when trying to create an CustomType for {0}, possibly because the element type is not supported.", type));
                return null;
            }

            Add(type, customType);
            return customType;
        }

        internal static void Init()
        {
            lock (_lock)
            {
                types = new Dictionary<Type, CustomType>();
                // CustomTypes add themselves to the types Dictionary.
                Reflection.GetInstances<CustomType>();

                // Check that the type list was initialised correctly.
                if (types == null || types.Count == 0)
                    throw new TypeLoadException("Type list could not be initialised.");
            }
        }
    }
}