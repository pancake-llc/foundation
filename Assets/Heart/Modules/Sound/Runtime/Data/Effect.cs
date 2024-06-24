using LitMotion;
using UnityEngine;
using static UnityEngine.Debug;

namespace Pancake.Sound
{
    /// <summary>
    /// Parameters for setting effects. Please use the static factory methods within this class.
    /// </summary>
    [System.Serializable]
    public struct Effect
    {
        // Use these static methods for SetEffect()
        public static Effect HighPass(float frequency, float fadeTime, Ease ease = AudioConstant.HIGH_PASS_IN_EASE) =>
            new(EEffectType.HighPass, frequency, SingleFading(fadeTime, ease));

        public static Effect ResetHighPass(float fadeTime, Ease ease = AudioConstant.HIGH_PASS_OUT_EASE) =>
            new(EEffectType.HighPass, AudioConstant.MIN_FREQUENCY, SingleFading(fadeTime, ease));

        public static Effect LowPass(float frequency, float fadeTime, Ease ease = AudioConstant.LOW_PASS_IN_EASE) =>
            new(EEffectType.LowPass, frequency, SingleFading(fadeTime, ease));

        public static Effect ResetLowPass(float fadeTime, Ease ease = AudioConstant.LOW_PASS_OUT_EASE) =>
            new(EEffectType.LowPass, AudioConstant.MAX_FREQUENCY, SingleFading(fadeTime, ease));

        public static Effect Custom(string exposedParameterName, float value, float fadeTime, Ease ease = Ease.Linear) =>
            new(exposedParameterName, value, SingleFading(fadeTime, ease));

        private static Fading SingleFading(float fadeTime, Ease ease) => new(fadeTime, default, ease, default);


        private float _value;

        public readonly EEffectType type;
        public readonly Fading fading;
        public readonly string customExposedParameter;
        internal readonly bool isDominator;

        // Force user to use static factory method
        internal Effect(EEffectType type, float value, Fading fading, bool isDominator = false)
            : this(type)
        {
            Value = value;
            this.fading = fading;
            this.isDominator = isDominator;
        }

        internal Effect(string exposedParaName, float value, Fading fading)
            : this(EEffectType.Custom, value, fading)
        {
            customExposedParameter = exposedParaName;
        }

        public float Value
        {
            get => _value;
            private set
            {
                switch (type)
                {
                    case EEffectType.None:
                        LogError(AudioConstant.LOG_HEADER + "EffectParameter's EffectType must be set before the Value");
                        break;
                    case EEffectType.Volume:
                        _value = value.ToDecibel();
                        break;
                    case EEffectType.LowPass:
                    case EEffectType.HighPass:
                        if (AudioExtension.IsValidFrequency(value)) _value = value;
                        break;
                    default:
                        _value = value;
                        break;
                }
            }
        }

        public Effect(EEffectType type)
            : this()
        {
            this.type = type;

            switch (type)
            {
                case EEffectType.Volume:
                    Value = AudioConstant.FULL_VOLUME;
                    break;
                case EEffectType.LowPass:
                    Value = AudioConstant.LOW_PASS_FREQUENCY;
                    break;
                case EEffectType.HighPass:
                    Value = AudioConstant.HIGH_PASS_FREQUENCY;
                    break;
            }
        }

        public bool IsDefault()
        {
            switch (type)
            {
                case EEffectType.Volume: return Mathf.Approximately(Value, AudioConstant.FULL_DECIBEL_VOLUME);
                case EEffectType.LowPass: return Mathf.Approximately(Value, AudioConstant.MAX_FREQUENCY);
                case EEffectType.HighPass: return Mathf.Approximately(Value, AudioConstant.MIN_FREQUENCY);
            }

            return false;
        }
    }
}