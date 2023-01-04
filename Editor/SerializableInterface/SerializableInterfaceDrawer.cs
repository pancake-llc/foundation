using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Pancake.Editor
{
    [CustomPropertyDrawer(typeof(SerializableInterface<>), true)]
    internal sealed class SerializableInterfaceDrawer : PropertyDrawer
    {
        private readonly SI_RawReferenceDrawer _rawReferenceDrawer = new SI_RawReferenceDrawer();
        private readonly SI_UnityReferenceDrawer _unityReferenceDrawer = new SI_UnityReferenceDrawer();

        private SerializedProperty _serializedProperty;
        private Type _genericType;

        /// <inheritdoc />
        public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

        private void Initialize(SerializedProperty property)
        {
            if (_serializedProperty == property) return;

            _serializedProperty = property;
            _genericType = GetGenericArgument();
            Assert.IsNotNull(_genericType, "Unable to find generic argument, are you doing some shady inheritance?");
        }

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            return GetReferenceDrawer(property, label).GetHeight();
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            GetReferenceDrawer(property, label).OnGUI(position);
        }

        private Type GetGenericArgument()
        {
            Type type = fieldInfo.FieldType;

            while (type != null)
            {
                if (type.IsArray) type = type.GetElementType();

                if (type == null) return null;

                if (type.IsGenericType)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        type = type.GetGenericArguments()[0];
                    }
                    else if (type.GetGenericTypeDefinition() == typeof(SerializableInterface<>))
                    {
                        return type.GetGenericArguments()[0];
                    }
                    else
                    {
                        type = type.BaseType;
                    }
                }
                else
                {
                    type = type.BaseType;
                }
            }

            return null;
        }

        private SI_IReferenceDrawer GetReferenceDrawer(SerializedProperty property, GUIContent label)
        {
            SerializedProperty modeProperty = _serializedProperty.FindPropertyRelative("mode");
            InterfaceRefMode referenceMode = (InterfaceRefMode) modeProperty.enumValueIndex;

            switch (referenceMode)
            {
                case InterfaceRefMode.Raw:
                    _rawReferenceDrawer.Initialize(property, _genericType, label, fieldInfo);
                    return _rawReferenceDrawer;
                case InterfaceRefMode.Unity:
                    _unityReferenceDrawer.Initialize(property, _genericType, label, fieldInfo);
                    return _unityReferenceDrawer;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}