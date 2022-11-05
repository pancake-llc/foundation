namespace Pancake.Tween
{
    public interface ITweener
    {
        float Duration { get; }
        float Elapsed { get; }

        bool UseGeneralTimeScale { get; set; }
        float TimeScale { get; set; }
        TimeMode TimeMode { get; set; }

        bool IsPlaying { get; }

        void SetEase(EaseDelegate easeFunction);

        void Reset(ResetMode mode);
        void Start();
        void Update();
        void Complete();
        void Kill();
    }
}