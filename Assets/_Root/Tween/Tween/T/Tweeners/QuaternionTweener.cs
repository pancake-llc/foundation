using UnityEngine;

namespace Pancake.Core
{
    public class QuaternionTweener : Tweener<Quaternion>
    {
        public QuaternionTweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, float duration, Validation validation)
            : base(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                QuaternionInterpolator.Instance,
                validation)
        {
        }
    }
}