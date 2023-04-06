using System;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Attribute
{
    internal class InlineGenericInspectorElement : PropertyCollectionBaseInspectorElement
    {
        private readonly Props _props;
        private readonly Property _property;

        [Serializable]
        public struct Props
        {
            public bool drawPrefixLabel;
            public float labelWidth;
        }

        public InlineGenericInspectorElement(Property property, Props props = default)
        {
            _property = property;
            _props = props;

            DeclareGroups(property.ValueType);

            foreach (var childProperty in property.ChildrenProperties)
            {
                AddProperty(childProperty);
            }
        }

        public override void OnGUI(Rect position)
        {
            if (_props.drawPrefixLabel)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);
            }

            using (GuiHelper.PushLabelWidth(_props.labelWidth))
            {
                base.OnGUI(position);
            }
        }
    }
}