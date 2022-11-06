using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public abstract class HeaderGroupBaseInspectorElement : PropertyCollectionBaseInspectorElement
    {
        private const float InsetTop = 4;
        private const float InsetBottom = 4;
        private const float InsetLeft = 18;
        private const float InsetRight = 4;

        protected virtual float GetHeaderHeight(float width) { return 22; }

        protected virtual void DrawHeader(Rect position) { }

        public sealed override float GetHeight(float width) { return base.GetHeight(width) + InsetTop + InsetBottom + GetHeaderHeight(width); }

        public sealed override void OnGUI(Rect position)
        {
            var headerHeight = GetHeaderHeight(position.width);

            var headerBgRect = new Rect(position) {height = headerHeight,};
            var contentBgRect = new Rect(position) {yMin = headerBgRect.yMax,};
            var contentRect = new Rect(contentBgRect)
            {
                xMin = contentBgRect.xMin + InsetLeft,
                xMax = contentBgRect.xMax - InsetRight,
                yMin = contentBgRect.yMin + InsetTop,
                yMax = contentBgRect.yMax - InsetBottom,
            };

            if (headerHeight > 0f)
            {
                DrawHeader(headerBgRect);

                Uniform.DrawBox(contentBgRect, Uniform.ContentBox);
            }
            else
            {
                Uniform.DrawBox(contentBgRect, Uniform.Box);
            }

            using (GuiHelper.PushLabelWidth(EditorGUIUtility.labelWidth - InsetLeft))
            {
                base.OnGUI(contentRect);
            }
        }
    }
}