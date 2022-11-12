using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class BoxGroupInspectorElement : HeaderGroupBaseInspectorElement
    {
        private readonly GUIContent _headerLabel;

        public BoxGroupInspectorElement(DeclareBoxGroupAttribute attribute)
        {
            _headerLabel = attribute.Title == null ? GUIContent.none : new GUIContent(attribute.Title);
        }
        
        protected override float GetHeaderHeight(float width)
        {
            if (string.IsNullOrEmpty(_headerLabel.text))
            {
                return 0f;
            }

            return base.GetHeaderHeight(width);
        }

        protected override void DrawHeader(Rect position)
        {
            Uniform.DrawBox(position, Uniform.TabOnlyOne);

            var headerLabelRect = new Rect(position)
            {
                xMin = position.xMin + 6, xMax = position.xMax - 6, yMin = position.yMin + 2, yMax = position.yMax - 2,
            };

            EditorGUI.LabelField(headerLabelRect, _headerLabel);
        }
    }
}