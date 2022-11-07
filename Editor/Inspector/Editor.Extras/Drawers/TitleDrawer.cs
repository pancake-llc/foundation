using Pancake.Editor;
using UnityEditor;
using UnityEngine;

[assembly: RegisterAttributeDrawer(typeof(TitleDrawer), DrawerOrder.Inspector)]

namespace Pancake.Editor
{
    public class TitleDrawer : AttributeDrawer<TitleAttribute>
    {
        private const int SpaceBeforeTitle = 9;
        private const int SpaceBeforeLine = 2;
        private const int LineHeight = 2;
        private const int SpaceBeforeContent = 3;

        private ValueResolver<string> _titleResolver;

        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _titleResolver = ValueResolver.ResolveString(propertyDefinition, Attribute.Title);

            if (_titleResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return ExtensionInitializationResult.Ok;
        }

        public override float GetHeight(float width, Property property, InspectorElement next)
        {
            var extraHeight = SpaceBeforeTitle + EditorGUIUtility.singleLineHeight + SpaceBeforeLine + LineHeight + SpaceBeforeContent;

            return next.GetHeight(width) + extraHeight;
        }

        public override void OnGUI(Rect position, Property property, InspectorElement next)
        {
            var titleRect = new Rect(position) {y = position.y + SpaceBeforeTitle, height = EditorGUIUtility.singleLineHeight,};

            var lineRect = new Rect(position) {y = titleRect.yMax + SpaceBeforeLine, height = LineHeight,};

            var contentRect = new Rect(position) {yMin = lineRect.yMax + SpaceBeforeContent,};

            var title = _titleResolver.GetValue(property, "Error");
            GUI.Label(titleRect, title, EditorStyles.boldLabel);
            EditorGUI.DrawRect(lineRect, Color.gray);

            next.OnGUI(contentRect);
        }
    }
}