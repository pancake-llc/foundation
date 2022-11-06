using System;
using System.Reflection;
using Pancake.Editor;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(ButtonDrawer), DrawerOrder.Drawer)]

namespace Pancake.Editor
{
    public class ButtonDrawer : AttributeDrawer<ButtonAttribute>
    {
        private ValueResolver<string> _nameResolver;

        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            var isValidMethod = propertyDefinition.TryGetMemberInfo(out var memberInfo) && memberInfo is MethodInfo mi && mi.GetParameters().Length == 0;
            if (!isValidMethod)
            {
                return "[Button] valid only on methods without parameters";
            }

            _nameResolver = ValueResolver.ResolveString(propertyDefinition, Attribute.Name);
            if (_nameResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return ExtensionInitializationResult.Ok;
        }

        public override float GetHeight(float width, Property property, InspectorElement next)
        {
            if (Attribute.ButtonSize != 0)
            {
                return Attribute.ButtonSize;
            }

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, Property property, InspectorElement next)
        {
            var name = _nameResolver.GetValue(property);

            if (string.IsNullOrEmpty(name))
            {
                name = property.DisplayName;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = property.RawName;
            }

            if (GUI.Button(position, name))
            {
                InvokeButton(property, Array.Empty<object>());
            }
        }

        private static void InvokeButton(Property property, object[] parameters)
        {
            if (property.TryGetMemberInfo(out var memberInfo) && memberInfo is MethodInfo methodInfo)
            {
                property.ModifyAndRecordForUndo(targetIndex =>
                {
                    try
                    {
                        var parentValue = property.Parent.GetValue(targetIndex);
                        methodInfo.Invoke(parentValue, parameters);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
            }
        }
    }
}