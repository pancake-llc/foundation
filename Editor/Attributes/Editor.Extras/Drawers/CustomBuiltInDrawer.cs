using PancakeEditor.Attribute;
using InspectorUnityInternalBridge;

[assembly: RegisterValueDrawer(typeof(CustomBuiltInDrawer), DrawerOrder.Fallback - 999)]

namespace PancakeEditor.Attribute
{
    public class CustomBuiltInDrawer : ValueDrawer<object>
    {
        public override InspectorElement CreateElement(InspectorValue<object> propertyValue, InspectorElement next)
        {
            var property = propertyValue.Property;

            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                var handler = ScriptAttributeUtilityProxy.GetHandler(serializedProperty);

                var drawWithHandler = handler.hasPropertyDrawer || property.PropertyType == EPropertyType.Primitive ||
                                      UnityInspectorUtilities.MustDrawWithUnity(property);

                if (drawWithHandler)
                {
                    return new BuiltInPropertyInspectorElement(property, serializedProperty, handler);
                }
            }

            return base.CreateElement(propertyValue, next);
        }
    }
}