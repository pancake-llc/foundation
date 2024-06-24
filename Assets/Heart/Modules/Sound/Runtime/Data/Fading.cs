using LitMotion;

namespace Pancake.Sound
{
    public struct Fading
    {
        public readonly float fadeIn;
        public readonly float fadeOut;
        public readonly Ease fadeInEase;
        public readonly Ease fadeOutEase;

        public Fading(float fadeIn, float fadeOut, EEffectType effectType)
            : this(effectType)
        {
            this.fadeIn = fadeIn;
            this.fadeOut = fadeOut;
        }

        public Fading(float fadeTime, EEffectType effectType)
            : this(effectType)
        {
            fadeIn = fadeTime;
            fadeOut = fadeTime;
        }

        public Fading(EEffectType effectType)
        {
            switch (effectType)
            {
                case EEffectType.LowPass:
                    fadeInEase = AudioConstant.LOW_PASS_IN_EASE;
                    fadeOutEase = AudioConstant.LOW_PASS_OUT_EASE;
                    break;
                case EEffectType.HighPass:
                    fadeInEase = AudioConstant.HIGH_PASS_IN_EASE;
                    fadeOutEase = AudioConstant.HIGH_PASS_OUT_EASE;
                    break;
                default:
                    fadeInEase = Ease.Linear;
                    fadeOutEase = Ease.Linear;
                    break;
            }

            fadeIn = default;
            fadeOut = default;
        }

        public Fading(float fadeIn, float fadeOut, Ease fadeInEase, Ease fadeOutEase)
        {
            this.fadeOut = fadeOut;
            this.fadeIn = fadeIn;
            this.fadeInEase = fadeInEase;
            this.fadeOutEase = fadeOutEase;
        }
    }
}