using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Pancake.Tween
{
    public interface ITween
    {
        float TimeScale { get; }

        int Loops { get; }
        ResetMode LoopResetMode { get; }
        TimeMode TimeMode { get; }
        UpdateMode UpdateMode { get; }

        bool IsPlaying { get; }
        bool IsCompleted { get; }

        ITween OnTimeScaleChanged(TweenCallback<float> onTimeScaleChange);
        ITween OnStart(TweenCallback onStart);
        ITween OnLoop(TweenCallback onLoop);
        ITween OnReset(TweenCallback onReset);
        ITween OnComplete(TweenCallback onComplete);
        ITween OnKill(TweenCallback onKill);
        ITween OnCompleteOrKill(TweenCallback onCompleteOrKill);

        float GetDuration();
        float GetElapsed();
        float GetNormalizedProgress();
        int GetTweensCount();
        int GetPlayingTweensCount();

        ITween SetTimeScale(float timeScale = 1f, TimeMode timeMode = TimeMode.Unscaled);
        ITween SetUpdateMode(UpdateMode updateMode);
        ITween SetEase(Ease ease);
        ITween SetEase(AnimationCurve animationCurve);
        ITween SetEase(Interpolator interpolator);
        ITween SetLoops(int loops, ResetMode resetMode);
        ITween Delay(float timeDelay);

        void Complete();
        void Kill();
        void Reset(bool kill, ResetMode resetMode = ResetMode.InitialValues);

        void Replay();
        void Play();
        void Pause();

        Task AwaitCompleteOrKill(CancellationToken cancellationToken);
    }
}