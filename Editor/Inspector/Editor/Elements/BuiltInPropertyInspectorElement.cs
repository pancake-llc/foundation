using InspectorUnityInternalBridge;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    internal class BuiltInPropertyInspectorElement : InspectorElement
    {
        private readonly Property _property;
        private readonly PropertyHandlerProxy _propertyHandler;
        private readonly SerializedProperty _serializedProperty;

        public BuiltInPropertyInspectorElement(Property property, SerializedProperty serializedProperty, PropertyHandlerProxy propertyHandler)
        {
            _property = property;
            _serializedProperty = serializedProperty;
            _propertyHandler = propertyHandler;
        }

        public override float GetHeight(float width) { return _propertyHandler.GetHeight(_serializedProperty, _property.DisplayNameContent, true); }

        public override void OnGUI(Rect position)
        {
            EditorGUI.BeginChangeCheck();

            if (_property.IsArrayElement && _serializedProperty.propertyType == SerializedPropertyType.Generic && _serializedProperty.hasVisibleChildren)
            {
                position.xMin += 12;
            }

            _propertyHandler.OnGUI(position, _serializedProperty, _property.DisplayNameContent, true);

            if (EditorGUI.EndChangeCheck())
            {
                _property.NotifyValueChanged();
            }
        }
    }
}