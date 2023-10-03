namespace Pancake.UI
{
    public interface IAnimation
    {
        float Duration { get; }
        void SetTime(float time);
    }
}