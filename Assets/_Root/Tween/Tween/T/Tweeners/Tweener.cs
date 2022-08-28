
using UnityEngine;

namespace Pancake.Core.Tween
{
    public abstract class Tweener<T> : ITweener
    {
        public delegate void Setter(T currentValue);

        public delegate T Getter();

        public delegate bool Validation();

        private readonly IInterpolator<T> interpolator;

        private readonly Getter currValueGetter;
        private readonly Setter setter;
        private readonly Getter finalValueGetter;
        private readonly Validation validation;

        private bool firstTime;
        private T firstTimeInitialValue;
        private T firstTimeFinalValue;

        private T initialValue;
        private T finalValue;
        private T currentValue;

        private EaseDelegate easeFunction;

        public TimeMode TimeMode { get; set; }
        public bool IsPlaying { get; protected set; }

        public float Duration { get; }
        public float Elapsed { get; private set; }

        public bool UseGeneralTimeScale { get; set; }
        public float TimeScale { get; set; }

        public Tweener(Getter currValueGetter, Setter setter, Getter finalValueGetter, float duration, IInterpolator<T> interpolator, Validation validation)
        {
            this.currValueGetter = currValueGetter;
            this.setter = setter;
            this.finalValueGetter = finalValueGetter;
            Duration = Mathf.Max(duration, 0.0f);
            this.interpolator = interpolator;
            this.validation = validation;

            firstTime = true;

            UseGeneralTimeScale = true;
            TimeScale = 1.0f;
        }

        public void Start()
        {
            if (IsPlaying) return;

            IsPlaying = true;

            Elapsed = 0.0f;

            bool valid = Validate();

            if (!valid)
            {
                Kill();

                return;
            }

            initialValue = currValueGetter.Invoke();

            if (firstTime)
            {
                firstTime = false;

                finalValue = finalValueGetter.Invoke();

                firstTimeInitialValue = initialValue;
                firstTimeFinalValue = finalValue;
            }

            CompleteIfInstant();
        }

        public void Reset(ResetMode mode)
        {
            if (firstTime)
            {
                return;
            }

            bool valid = Validate();

            if (!valid)
            {
                Kill();

                return;
            }

            Elapsed = 0.0f;

            switch (mode)
            {
                case ResetMode.InitialValues:
                {
                    setter(firstTimeInitialValue);
                    finalValue = firstTimeFinalValue;
                }
                    break;

                case ResetMode.IncrementalValues:
                {
                    T difference = interpolator.Subtract(firstTimeInitialValue, firstTimeFinalValue);

                    finalValue = interpolator.Add(currentValue, difference);
                }
                    break;

                case ResetMode.CurrentValues:
                {
                    finalValue = firstTimeFinalValue;
                }
                    break;
            }
        }

        public void Update()
        {
            if (!IsPlaying) return;

            bool valid = Validate();

            if (!valid)
            {
                Kill();

                return;
            }

            float generalTimeScale = UseGeneralTimeScale ? TweenManager.TimeScale : 1.0f;
            float deltaTime = RuntimeUtilities.GetUnitedDeltaTime(TimeMode);
            float dt = deltaTime * generalTimeScale * TimeScale;

            Elapsed += dt;

            if (Elapsed < Duration)
            {
                float timeNormalized = Elapsed / Duration;

                currentValue = interpolator.Evaluate(initialValue, finalValue, timeNormalized, easeFunction);

                setter(currentValue);
            }
            else
            {
                Complete();
            }
        }

        public void Complete()
        {
            T newValue = interpolator.Evaluate(initialValue, finalValue, 1.0f, easeFunction);

            setter(newValue);

            IsPlaying = false;
        }

        public void Kill() { IsPlaying = false; }

        public void SetEase(EaseDelegate easeFunction) { this.easeFunction = easeFunction; }

        private void CompleteIfInstant()
        {
            if (!IsPlaying)
            {
                return;
            }

            bool isInstant = Duration == 0.0f;

            if (isInstant)
            {
                Complete();
            }
        }

        private bool Validate()
        {
            if (validation == null)
            {
                return true;
            }

            return validation.Invoke();
        }
    }
}