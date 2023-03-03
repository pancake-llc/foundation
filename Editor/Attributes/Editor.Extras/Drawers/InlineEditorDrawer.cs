using Pancake.Attribute;
using PancakeEditor.Attribute;
using PancakeEditor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: RegisterAttributeDrawer(typeof(InlineEditorDrawer), DrawerOrder.Drawer - 100, ApplyOnArrayElement = true)]

namespace PancakeEditor.Attribute
{
    public class InlineEditorDrawer : AttributeDrawer<InlineEditorAttribute>
    {
        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            if (!typeof(Object).IsAssignableFrom(propertyDefinition.FieldType))
            {
                return "[InlineEditor] valid only on Object fields";
            }

            return ExtensionInitializationResult.Ok;
        }

        public override InspectorElement CreateElement(Property property, InspectorElement next)
        {
            var element = new BoxGroupInspectorElement(new BoxGroupInspectorElement.Props() {titleMode = BoxGroupInspectorElement.TitleMode.Hidden});
            element.AddChild(new ObjectReferenceFoldoutDrawerInspectorElement(property));
            element.AddChild(new InlineEditorInspectorElement(property));
            return element;
        }

        private class ObjectReferenceFoldoutDrawerInspectorElement : InspectorElement
        {
            private readonly Property _property;

            public ObjectReferenceFoldoutDrawerInspectorElement(Property property) { _property = property; }

            public override float GetHeight(float width) { return EditorGUIUtility.singleLineHeight; }

            public override void OnGUI(Rect position)
            {
                var prefixRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight, xMax = position.xMin + EditorGUIUtility.labelWidth,};
                var pickerRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight, xMin = prefixRect.xMax,};

                AttributeSkin.Foldout(prefixRect, _property);

                EditorGUI.BeginChangeCheck();

                var allowSceneObjects = _property.PropertyTree.TargetIsPersistent == false;

                var value = (Object) _property.Value;
                value = EditorGUI.ObjectField(pickerRect,
                    GUIContent.none,
                    value,
                    _property.FieldType,
                    allowSceneObjects);

                if (EditorGUI.EndChangeCheck())
                {
                    _property.SetValue(value);
                }
            }
        }
    }
}