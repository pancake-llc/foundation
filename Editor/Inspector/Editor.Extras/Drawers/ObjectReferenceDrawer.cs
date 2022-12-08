using Pancake.Editor;
using UnityEditor;
using UnityEngine;

[assembly: RegisterValueDrawer(typeof(ObjectReferenceDrawer), DrawerOrder.Fallback)]

namespace Pancake.Editor
{
    public class ObjectReferenceDrawer : ValueDrawer<Object>
    {
        public override InspectorElement CreateElement(InspectorValue<Object> value, InspectorElement next)
        {
            if (value.Property.IsRootProperty || value.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            return new ObjectReferenceDrawerInspectorElement(value);
        }

        private class ObjectReferenceDrawerInspectorElement : InspectorElement
        {
            private InspectorValue<Object> _propertyValue;
            private readonly bool _allowSceneObjects;

            public ObjectReferenceDrawerInspectorElement(InspectorValue<Object> propertyValue)
            {
                _propertyValue = propertyValue;
                _allowSceneObjects = propertyValue.Property.PropertyTree.TargetIsPersistent == false;
            }

            public override float GetHeight(float width) { return EditorGUIUtility.singleLineHeight; }

            public override void OnGUI(Rect position)
            {
                var hasSerializedProperty = _propertyValue.Property.TryGetSerializedProperty(out var serializedProperty);

                var value = _propertyValue.SmartValue;

                EditorGUI.BeginChangeCheck();

                value = EditorGUI.ObjectField(position,
                    _propertyValue.Property.DisplayNameContent,
                    value,
                    _propertyValue.Property.FieldType,
                    _allowSceneObjects);

                if (EditorGUI.EndChangeCheck())
                {
                    _propertyValue.SmartValue = value;
                }
            }
        }
    }
}