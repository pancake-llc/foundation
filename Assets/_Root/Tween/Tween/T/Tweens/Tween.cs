using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Pancake.Core
{
    public abstract partial class Tween : ITween
    {
        private int _loopsRemaining;
        private event TweenCallback OnStartCallback;
        private event TweenCallback OnLoopCallback;
        private event TweenCallback OnResetCallback;
        private event TweenCallback OnCompleteCallback;
        private event TweenCallback OnKillCallback;
        private event TweenCallback OnCompleteOrKillCallback;
        private event TweenCallback<float> OnTimeScaleChangedCallback;
        protected EaseDelegate EaseFunction { get; set; }
        public float TimeScale { get; private set; }

        public int Loops { get; private set; }
        public ResetMode LoopResetMode { get; private set; }
        public TimeMode TimeMode { get; private set; }
        public UpdateMode UpdateMode { get; private set; }

        public bool IsNested { get; set; }

        public bool IsPlaying { get; protected set; }
        public bool IsCompleted { get; protected set; }

        protected float timeDelay;
        protected float elapsedDelay;
        protected bool markDelayCompelted;

        public ITween OnTimeScaleChanged(TweenCallback<float> onTimeScaleChange)
        {
            OnTimeScaleChangedCallback = onTimeScaleChange;
            return this;
        }

        public ITween OnStart(TweenCallback onStart)
        {
            OnStartCallback = onStart;
            return this;
        }

        public ITween OnLoop(TweenCallback onLoop)
        {
            OnLoopCallback = onLoop;
            return this;
        }

        public ITween OnReset(TweenCallback onReset)
        {
            OnResetCallback = onReset;
            return this;
        }

        public ITween OnComplete(TweenCallback onComplete)
        {
            OnCompleteCallback = onComplete;
            return this;
        }

        public ITween OnKill(TweenCallback onKill)
        {
            OnKillCallback = onKill;
            return this;
        }

        public ITween OnCompleteOrKill(TweenCallback onCompleteOrKill)
        {
            OnCompleteOrKillCallback = onCompleteOrKill;
            return this;
        }


        public bool IsAlive { get; set; }


        internal Tween()
        {
            SetEase(Ease.Linear);
            SetTimeScale(1.0f);
            SetUpdateMode(UpdateMode.Update);
        }

        public void Start(bool isCompletingInstantly = false)
        {
            if (IsPlaying)
            {
                Kill();
            }

            IsPlaying = true;
            IsCompleted = false;
            elapsedDelay = 0.0f;
            markDelayCompelted = false;

            OnStartCallback?.Invoke();

            OnTweenStart(isCompletingInstantly);
        }

        public void Update()
        {
            if (!IsPlaying) return;

            OnTweenUpdate();
        }

        public void Complete()
        {
            if (!IsPlaying && !IsCompleted)
            {
                Start(isCompletingInstantly: true);
            }

            OnTweenComplete();
            
            _loopsRemaining = 0;

            NewMarkCompleted();
        }

        public void Kill()
        {
            if (!IsPlaying)
            {
                return;
            }

            IsPlaying = false;
            IsCompleted = true;

            OnTweenKill();

            OnKillCallback?.Invoke();
            OnCompleteOrKillCallback?.Invoke();
        }

        public void Reset(bool kill, ResetMode resetMode = ResetMode.InitialValues)
        {
            if (kill)
            {
                Kill();

                IsPlaying = false;
            }

            IsCompleted = false;
            elapsedDelay = 0f;
            markDelayCompelted = false;

            OnTweenReset(kill, resetMode);

            OnResetCallback?.Invoke();
        }

        public float GetDuration() { return OnGetDuration(); }

        public float GetElapsed()
        {
            if (!IsPlaying && !IsCompleted)
            {
                return 0.0f;
            }

            if (!IsPlaying && IsCompleted)
            {
                return GetDuration();
            }

            return OnGetElapsed();
        }

        public float GetNormalizedProgress()
        {
            float duration = GetDuration();

            if (duration <= 0)
            {
                return 0.0f;
            }

            float elapsed = GetElapsed();

            return (1.0f / duration) * elapsed;
        }

        public int GetTweensCount() { return OnGetTweensCount(); }

        public int GetPlayingTweensCount() { return OnGetPlayingTweensCount(); }

        public ITween SetTimeScale(float timeScale = 1f, TimeMode timeMode = TimeMode.Unscaled)
        {
            TimeMode = timeMode;
            OnTimeModeChange(timeMode);
            if (M.Approximately(TimeScale, timeScale)) return this;
            TimeScale = timeScale;
            OnTimeScaleChange(timeScale);
            OnTimeScaleChangedCallback?.Invoke(timeScale);
            return this;
        }

        public void SetEase(EaseDelegate easeFunction)
        {
            EaseFunction = easeFunction;
            OnEaseDelegateChange(EaseFunction);
        }

        public ITween SetUpdateMode(UpdateMode updateMode)
        {
            UpdateMode = updateMode;
            return this;
        }

        public ITween SetEase(Ease ease)
        {
            SetEase(Interpolator.Get(ease));
            return this;
        }

        public ITween SetEase(AnimationCurve animationCurve)
        {
            if (animationCurve == null)
            {
                throw new ArgumentNullException($"Tried to {nameof(SetEase)} " + $"with a null {nameof(AnimationCurve)} on {nameof(Tween)}");
            }

            SetEase(Interpolator.Get(animationCurve));
            return this;
        }

        public ITween SetEase(Interpolator interpolator)
        {
            if (interpolator.ease == Ease.CustomCurve) return SetEase(interpolator.customCurve);
          
            return SetEase(interpolator.ease);
        }

        public ITween SetLoops(int loops, ResetMode resetMode)
        {
            Loops = M.Max(loops, -1);
            _loopsRemaining = Loops;
            LoopResetMode = resetMode;
            return this;
        }

        public virtual ITween Delay(float timeDelay)
        {
            this.timeDelay = timeDelay;
            OnTimeDelayChange(this.timeDelay);
            return this;
        }

        public void Replay()
        {
            Reset(kill: true);
            Play();
        }

        public void Play() { TweenManager.Add(this); }

        public void Pause() { IsPlaying = false; }

        private bool NewLoop(ResetMode loopResetMode)
        {
            bool infinity = _loopsRemaining == -1;
            bool positiveNumber = _loopsRemaining > 0;

            if (!(positiveNumber || infinity) || !Loopable)  return false;

            if (positiveNumber) --_loopsRemaining;

            Reset(kill: false, loopResetMode);

            Start();

            OnLoopCallback?.Invoke();

            return true;
        }

        protected void NewMarkCompleted()
        {
            if (!IsPlaying)
            {
                return;
            }

            bool loops = NewLoop(LoopResetMode);

            if (loops)
            {
                return;
            }

            IsPlaying = false;
            IsCompleted = true;

            OnTweenComplete();

            OnCompleteCallback?.Invoke();
            OnCompleteOrKillCallback?.Invoke();
        }

        public async Task AwaitCompleteOrKill(CancellationToken cancellationToken)
        {
            if (!IsPlaying)
            {
                return;
            }

            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

            void Callback() => taskCompletionSource.TrySetResult(default);

            OnCompleteOrKillCallback += Callback;

            cancellationToken.Register(Kill);

            await taskCompletionSource.Task;

            OnCompleteOrKillCallback -= Callback;
        }

        protected bool ValidateDelay()
        {
            float deltaTime = RuntimeUtilities.GetUnitedDeltaTime(TimeMode);
            float dt = deltaTime * TweenManager.TimeScale * TimeScale;

            if (timeDelay > 0.0f && !markDelayCompelted)
            {
                elapsedDelay += dt;
                if (elapsedDelay >= timeDelay)
                {
                    elapsedDelay = timeDelay;
                    markDelayCompelted = true;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        protected abstract bool Loopable { get; }

        protected abstract void OnTweenStart(bool isCompletingInstantly);
        protected abstract void OnTweenUpdate();
        protected abstract void OnTweenKill();
        protected abstract void OnTweenComplete();
        protected abstract void OnTweenReset(bool kill, ResetMode loopResetMode);
        protected abstract void OnTweenStartLoop(ResetMode loopResetMode);

        internal abstract void OnTimeDelayChange(float timeDelay);
        internal abstract void OnEaseDelegateChange(EaseDelegate easeFunction);
        internal abstract void OnTimeScaleChange(float timeScale);
        internal abstract void OnTimeModeChange(TimeMode timeMode);

        public abstract float OnGetDuration();
        public abstract float OnGetElapsed();
        public abstract int OnGetTweensCount();
        public abstract int OnGetPlayingTweensCount();
    }
}