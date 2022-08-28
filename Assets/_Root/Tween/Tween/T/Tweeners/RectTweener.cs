using UnityEngine;

namespace Pancake.Core.Tween
{
    public class RectTweener : Tweener<Rect>
    {
        public RectTweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, float duration, Validation validation)
            : base(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                RectInterpolator.Instance,
                validation)
        {
        }
    }
}