using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UIQuery
{
    public class LeftToRightLayouter : ILayouter
    {
        private readonly float margin;

        public LeftToRightLayouter(float margin = 0f) { this.margin = margin; }

        public LeftToRightLayouter(HorizontalLayoutGroup layout)
        {
            layout.enabled = false;
            margin = layout.spacing;
        }

        public void Layout(IMapper original, IMapper[] elements)
        {
            var position = original.Get<RectTransform>().anchoredPosition;
            foreach (var e in elements)
            {
                var rectTransform = e.Get<RectTransform>();
                rectTransform.anchoredPosition = position;
                position.x += rectTransform.sizeDelta.x;
                position.x += margin;
            }
        }
    }
}