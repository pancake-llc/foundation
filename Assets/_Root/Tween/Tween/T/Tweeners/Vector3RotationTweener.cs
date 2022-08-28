using UnityEngine;

namespace Pancake.Core.Tween
{
    public class Vector3RotationTweener : Tweener<Vector3>
    {
        public Vector3RotationTweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, RotationMode rotationMode, float duration, Validation validation)
            : base(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                new Vector3RotationInterpolator(rotationMode),
                validation)
        {
        }
    }
}