using UnityEngine;

namespace Pancake.Core
{
    public class Vector2Tweener : Tweener<Vector2>
    {
        public Vector2Tweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, float duration, Validation validation)
            : base(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                Vector2Interpolator.Instance,
                validation)
        {
        }
    }
}