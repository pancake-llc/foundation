using UnityEngine;

namespace Pancake.Tween
{
    public class ColorTweener : Tweener<Color>
    {
        public ColorTweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, float duration, Validation validation)
            : base(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                ColorInterpolator.Instance,
                validation)
        {
        }
    }
}