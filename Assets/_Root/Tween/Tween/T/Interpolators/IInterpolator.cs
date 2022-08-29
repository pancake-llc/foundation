namespace Pancake.Tween
{
    public interface IInterpolator<T>
    {
        T Evaluate(T initialValue, T finalValue, float time, EaseDelegate interpolator);
        T Subtract(T initialValue, T finalValue);
        T Add(T initialValue, T finalValue);
    }
}