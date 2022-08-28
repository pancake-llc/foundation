namespace Pancake.Core
{
    public class FloatTweener : Tweener<float>
    {
        public FloatTweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, float duration, Validation validation)
            : base(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                FloatInterpolator.Instance,
                validation)
        {
        }
    }
}