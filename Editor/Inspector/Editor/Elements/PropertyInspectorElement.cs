using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class PropertyInspectorElement : InspectorElement
    {
        private readonly Property _property;

        [Serializable]
        public struct Props
        {
            public bool forceInline;
        }

        public PropertyInspectorElement(Property property, Props props = default)
        {
            _property = property;

            foreach (var error in _property.ExtensionErrors)
            {
                AddChild(new InfoBoxInspectorElement(error, EMessageType.Error));
            }

            var element = CreateElement(property, props);

            var drawers = property.AllDrawers;
            for (var index = drawers.Count - 1; index >= 0; index--)
            {
                element = drawers[index].CreateElementInternal(property, element);
            }

            AddChild(element);
        }

        public override float GetHeight(float width)
        {
            if (!_property.IsVisible)
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            return base.GetHeight(width);
        }

        public override void OnGUI(Rect position)
        {
            if (!_property.IsVisible)
            {
                return;
            }

            var oldShowMixedValue = EditorGUI.showMixedValue;
            var oldEnabled = GUI.enabled;

            GUI.enabled &= _property.IsEnabled;
            EditorGUI.showMixedValue = _property.IsValueMixed;

            using (PropertyOverrideContext.BeginProperty())
            {
                base.OnGUI(position);
            }

            EditorGUI.showMixedValue = oldShowMixedValue;
            GUI.enabled = oldEnabled;
        }

        private static InspectorElement CreateElement(Property property, Props props)
        {
            switch (property.PropertyType)
            {
                case EPropertyType.Array:
                {
                    return CreateArrayElement(property);
                }

                case EPropertyType.Reference:
                {
                    return CreateReferenceElement(property, props);
                }

                case EPropertyType.Generic:
                {
                    return CreateGenericElement(property, props);
                }

                default:
                {
                    return new NoDrawerInspectorElement(property);
                }
            }
        }

        private static InspectorElement CreateArrayElement(Property property) { return new ListInspectorElement(property); }

        private static InspectorElement CreateReferenceElement(Property property, Props props)
        {
            if (property.TryGetAttribute(out InlinePropertyAttribute inlineAttribute))
            {
                return new ReferenceInspectorElement(property,
                    new ReferenceInspectorElement.Props {inline = true, drawPrefixLabel = !props.forceInline, labelWidth = inlineAttribute.LabelWidth,});
            }

            if (props.forceInline)
            {
                return new ReferenceInspectorElement(property, new ReferenceInspectorElement.Props {inline = true, drawPrefixLabel = false,});
            }

            return new ReferenceInspectorElement(property, new ReferenceInspectorElement.Props {inline = false, drawPrefixLabel = false,});
        }

        private static InspectorElement CreateGenericElement(Property property, Props props)
        {
            if (property.TryGetAttribute(out InlinePropertyAttribute inlineAttribute))
            {
                return new InlineGenericInspectorElement(property,
                    new InlineGenericInspectorElement.Props {drawPrefixLabel = !props.forceInline, labelWidth = inlineAttribute.LabelWidth,});
            }

            if (props.forceInline)
            {
                return new InlineGenericInspectorElement(property, new InlineGenericInspectorElement.Props {drawPrefixLabel = false,});
            }

            return new FoldoutInspectorElement(property);
        }
    }
}