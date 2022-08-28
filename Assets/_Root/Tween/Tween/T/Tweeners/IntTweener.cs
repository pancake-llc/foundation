namespace Pancake.Core
{
    public class IntTweener : Tweener<int>
    {
        internal IntTweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, float duration, Validation validation)
            : base(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                IntInterpolator.Instance,
                validation)
        {
        }
    }
}