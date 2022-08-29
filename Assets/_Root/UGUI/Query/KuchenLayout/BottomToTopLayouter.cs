using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UIQuery
{
    public class BottomToTopLayouter : ILayouter
    {
        private readonly float margin;

        public BottomToTopLayouter(float margin)
        {
            this.margin = margin;
        }

        public BottomToTopLayouter(VerticalLayoutGroup layout)
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
                position.y += rectTransform.sizeDelta.y;
                position.y += margin;
            }
        }
    }
}