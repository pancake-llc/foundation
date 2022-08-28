using UnityEngine;

namespace Pancake.Core
{
    public class Vector4Tweener : Tweener<Vector4>
    {
        public Vector4Tweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, float duration, Validation validation)
            : base(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                Vector4Interpolator.Instance,
                validation)
        {
        }
    }
}