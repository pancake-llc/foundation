using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pancake.Editor
{
    internal class DrawersUtilities
    {
        private static readonly GenericTypeMatcher GroupDrawerMatcher = typeof(GroupDrawer<>);
        private static readonly GenericTypeMatcher ValueDrawerMatcher = typeof(ValueDrawer<>);
        private static readonly GenericTypeMatcher AttributeDrawerMatcher = typeof(AttributeDrawer<>);
        private static readonly GenericTypeMatcher ValueValidatorMatcher = typeof(ValueValidator<>);
        private static readonly GenericTypeMatcher AttributeValidatorMatcher = typeof(AttributeValidator<>);
        private static readonly GenericTypeMatcher HideProcessorMatcher = typeof(PropertyHideProcessor<>);
        private static readonly GenericTypeMatcher DisableProcessorMatcher = typeof(PropertyDisableProcessor<>);

        private static IDictionary<Type, GroupDrawer> _allGroupDrawersCacheBackingField;
        private static IReadOnlyList<RegisterTriAttributeDrawerAttribute> _allAttributeDrawerTypesBackingField;
        private static IReadOnlyList<RegisterTriValueDrawerAttribute> _allValueDrawerTypesBackingField;
        private static IReadOnlyList<RegisterTriAttributeValidatorAttribute> _allAttributeValidatorTypesBackingField;
        private static IReadOnlyList<RegisterTriValueValidatorAttribute> _allValueValidatorTypesBackingField;
        private static IReadOnlyList<RegisterTriPropertyHideProcessor> _allHideProcessorTypesBackingField;
        private static IReadOnlyList<RegisterTriPropertyDisableProcessor> _allDisableProcessorTypesBackingField;

        private static IReadOnlyList<TypeProcessor> _allTypeProcessorBackingField;

        private static IDictionary<Type, GroupDrawer> AllGroupDrawersCache
        {
            get
            {
                if (_allGroupDrawersCacheBackingField == null)
                {
                    _allGroupDrawersCacheBackingField =
                        (from asm in ReflectionUtilities.Assemblies
                            from attr in asm.GetCustomAttributes<RegisterTriGroupDrawerAttribute>()
                            let groupAttributeType = GroupDrawerMatcher.MatchOut(attr.DrawerType, out var t) ? t : null
                            where groupAttributeType != null
                            select new KeyValuePair<Type, RegisterTriGroupDrawerAttribute>(groupAttributeType, attr)).ToDictionary(it => it.Key,
                            it => (GroupDrawer) Activator.CreateInstance(it.Value.DrawerType));
                }

                return _allGroupDrawersCacheBackingField;
            }
        }

        public static IReadOnlyList<TypeProcessor> AllTypeProcessors
        {
            get
            {
                if (_allTypeProcessorBackingField == null)
                {
                    _allTypeProcessorBackingField = (from asm in ReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriTypeProcessorAttribute>()
                        orderby attr.Order
                        select (TypeProcessor) Activator.CreateInstance(attr.ProcessorType)).ToList();
                }

                return _allTypeProcessorBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriValueDrawerAttribute> AllValueDrawerTypes
        {
            get
            {
                if (_allValueDrawerTypesBackingField == null)
                {
                    _allValueDrawerTypesBackingField = (from asm in ReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriValueDrawerAttribute>()
                        where ValueDrawerMatcher.Match(attr.DrawerType)
                        select attr).ToList();
                }

                return _allValueDrawerTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriAttributeDrawerAttribute> AllAttributeDrawerTypes
        {
            get
            {
                if (_allAttributeDrawerTypesBackingField == null)
                {
                    _allAttributeDrawerTypesBackingField = (from asm in ReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriAttributeDrawerAttribute>()
                        where AttributeDrawerMatcher.Match(attr.DrawerType)
                        select attr).ToList();
                }

                return _allAttributeDrawerTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriValueValidatorAttribute> AllValueValidatorTypes
        {
            get
            {
                if (_allValueValidatorTypesBackingField == null)
                {
                    _allValueValidatorTypesBackingField = (from asm in ReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriValueValidatorAttribute>()
                        where ValueValidatorMatcher.Match(attr.ValidatorType)
                        select attr).ToList();
                }

                return _allValueValidatorTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriAttributeValidatorAttribute> AllAttributeValidatorTypes
        {
            get
            {
                if (_allAttributeValidatorTypesBackingField == null)
                {
                    _allAttributeValidatorTypesBackingField = (from asm in ReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriAttributeValidatorAttribute>()
                        where AttributeValidatorMatcher.Match(attr.ValidatorType)
                        select attr).ToList();
                }

                return _allAttributeValidatorTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriPropertyHideProcessor> AllHideProcessors
        {
            get
            {
                if (_allHideProcessorTypesBackingField == null)
                {
                    _allHideProcessorTypesBackingField = (from asm in ReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriPropertyHideProcessor>()
                        where HideProcessorMatcher.Match(attr.ProcessorType)
                        select attr).ToList();
                }

                return _allHideProcessorTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriPropertyDisableProcessor> AllDisableProcessors
        {
            get
            {
                if (_allDisableProcessorTypesBackingField == null)
                {
                    _allDisableProcessorTypesBackingField = (from asm in ReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriPropertyDisableProcessor>()
                        where DisableProcessorMatcher.Match(attr.ProcessorType)
                        select attr).ToList();
                }

                return _allDisableProcessorTypesBackingField;
            }
        }

        public static PropertyCollectionBaseInspectorElement TryCreateGroupElementFor(DeclareGroupBaseAttribute attribute)
        {
            if (!AllGroupDrawersCache.TryGetValue(attribute.GetType(), out var attr))
            {
                return null;
            }

            return attr.CreateElementInternal(attribute);
        }

        public static IEnumerable<ValueDrawer> CreateValueDrawersFor(Type valueType)
        {
            return from drawer in AllValueDrawerTypes
                where ValueDrawerMatcher.Match(drawer.DrawerType, valueType)
                select CreateInstance<ValueDrawer>(drawer.DrawerType,
                    valueType,
                    it =>
                    {
                        it.ApplyOnArrayElement = drawer.ApplyOnArrayElement;
                        it.Order = drawer.Order;
                    });
        }

        public static IEnumerable<AttributeDrawer> CreateAttributeDrawersFor(Type valueType, IReadOnlyList<Attribute> attributes)
        {
            return from attribute in attributes
                from drawer in AllAttributeDrawerTypes
                where AttributeDrawerMatcher.Match(drawer.DrawerType, attribute.GetType())
                select CreateInstance<AttributeDrawer>(drawer.DrawerType,
                    valueType,
                    it =>
                    {
                        it.ApplyOnArrayElement = drawer.ApplyOnArrayElement;
                        it.Order = drawer.Order;
                        it.RawAttribute = attribute;
                    });
        }

        public static IEnumerable<ValueValidator> CreateValueValidatorsFor(Type valueType)
        {
            return from validator in AllValueValidatorTypes
                where ValueValidatorMatcher.Match(validator.ValidatorType, valueType)
                select CreateInstance<ValueValidator>(validator.ValidatorType,
                    valueType,
                    it =>
                    {
                        //
                        it.ApplyOnArrayElement = validator.ApplyOnArrayElement;
                    });
        }

        public static IEnumerable<AttributeValidator> CreateAttributeValidatorsFor(Type valueType, IReadOnlyList<Attribute> attributes)
        {
            return from attribute in attributes
                from validator in AllAttributeValidatorTypes
                where AttributeValidatorMatcher.Match(validator.ValidatorType, attribute.GetType())
                select CreateInstance<AttributeValidator>(validator.ValidatorType,
                    valueType,
                    it =>
                    {
                        it.ApplyOnArrayElement = validator.ApplyOnArrayElement;
                        it.RawAttribute = attribute;
                    });
        }

        public static IEnumerable<PropertyHideProcessor> CreateHideProcessorsFor(Type valueType, IReadOnlyList<Attribute> attributes)
        {
            return from attribute in attributes
                from processor in AllHideProcessors
                where HideProcessorMatcher.Match(processor.ProcessorType, attribute.GetType())
                select CreateInstance<PropertyHideProcessor>(processor.ProcessorType,
                    valueType,
                    it =>
                    {
                        //
                        it.RawAttribute = attribute;
                    });
        }

        public static IEnumerable<PropertyDisableProcessor> CreateDisableProcessorsFor(Type valueType, IReadOnlyList<Attribute> attributes)
        {
            return from attribute in attributes
                from processor in AllDisableProcessors
                where DisableProcessorMatcher.Match(processor.ProcessorType, attribute.GetType())
                select CreateInstance<PropertyDisableProcessor>(processor.ProcessorType,
                    valueType,
                    it =>
                    {
                        //
                        it.RawAttribute = attribute;
                    });
        }

        private static T CreateInstance<T>(Type type, Type argType, Action<T> setup)
        {
            if (type.IsGenericType)
            {
                type = type.MakeGenericType(argType);
            }

            var instance = (T) Activator.CreateInstance(type);
            setup(instance);
            return instance;
        }

        private class GenericTypeMatcher
        {
            private readonly Dictionary<Type, (bool, Type)> _cache = new Dictionary<Type, (bool, Type)>();
            private readonly Type _expectedGenericType;

            private GenericTypeMatcher(Type expectedGenericType) { _expectedGenericType = expectedGenericType; }

            public static implicit operator GenericTypeMatcher(Type expectedGenericType) { return new GenericTypeMatcher(expectedGenericType); }

            public bool Match(Type type, Type targetType) { return MatchOut(type, out var constraint) && constraint.IsAssignableFrom(targetType); }

            public bool Match(Type type) { return MatchOut(type, out _); }

            public bool MatchOut(Type type, out Type targetType)
            {
                if (_cache.TryGetValue(type, out var cachedResult))
                {
                    targetType = cachedResult.Item2;
                    return cachedResult.Item1;
                }

                var succeed = MatchInternal(type, out targetType);
                _cache[type] = (succeed, targetType);
                return succeed;
            }

            private bool MatchInternal(Type type, out Type targetType)
            {
                targetType = null;

                if (type.IsAbstract)
                {
                    Debug.LogError($"{type.Name} must be non abstract");
                    return false;
                }

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    Debug.LogError($"{type.Name} must have a parameterless constructor");
                    return false;
                }

                Type genericArgConstraints = null;
                if (type.IsGenericType)
                {
                    var genericArg = type.GetGenericArguments().SingleOrDefault();

                    if (genericArg == null || genericArg.GenericParameterAttributes != GenericParameterAttributes.None)
                    {
                        Debug.LogError($"{type.Name} must contains only one generic arg with simple constant e.g. <where T : bool>");
                        return false;
                    }

                    genericArgConstraints = genericArg.GetGenericParameterConstraints().SingleOrDefault();
                }

                var drawerType = type.BaseType;

                while (drawerType != null)
                {
                    if (drawerType.IsGenericType && drawerType.GetGenericTypeDefinition() == _expectedGenericType)
                    {
                        targetType = drawerType.GetGenericArguments()[0];

                        if (targetType.IsGenericParameter)
                        {
                            if (genericArgConstraints == null)
                            {
                                Debug.LogError($"{type.Name} must contains only one generic arg with simple constant e.g. <where T : bool>");
                                return false;
                            }

                            targetType = genericArgConstraints;
                        }

                        return true;
                    }

                    drawerType = drawerType.BaseType;
                }

                Debug.LogError($"{type.Name} must implement {_expectedGenericType}");
                return false;
            }
        }
    }
}