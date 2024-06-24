using UnityEngine;

namespace Pancake.Sound
{
    public class DominatorPlayer : AudioPlayerDecorator, IPlayerEffect
    {
        public DominatorPlayer(AudioPlayer instance)
            : base(instance)
        {
        }
#if !UNITY_WEBGL

        #region Quiet Others

        IPlayerEffect IPlayerEffect.QuietOthers(float othersVol, float fadeTime) { return this.QuietOthers(othersVol, new Fading(fadeTime, EEffectType.Volume)); }

        IPlayerEffect IPlayerEffect.QuietOthers(float othersVol, Fading fading)
        {
            if (othersVol is <= 0f or > 1f)
            {
                Debug.LogWarning(AudioConstant.LOG_HEADER + "othersVol should be less than 1 and greater than 0.");
                return this;
            }

            SetAllEffectExceptDominator(new Effect(EEffectType.Volume, othersVol, fading, true));
            return this;
        }

        #endregion

        #region LowPass Others

        IPlayerEffect IPlayerEffect.LowPassOthers(float freq, float fadeTime) { return this.LowPassOthers(freq, new Fading(fadeTime, EEffectType.LowPass)); }

        IPlayerEffect IPlayerEffect.LowPassOthers(float freq, Fading fading)
        {
            if (!AudioExtension.IsValidFrequency(freq))
            {
                return this;
            }

            SetAllEffectExceptDominator(new Effect(EEffectType.LowPass, freq, fading, true));
            return this;
        }

        #endregion

        #region HighPass Others

        IPlayerEffect IPlayerEffect.HighPassOthers(float freq, float fadeTime) { return this.HighPassOthers(freq, new Fading(fadeTime, EEffectType.HighPass)); }

        IPlayerEffect IPlayerEffect.HighPassOthers(float freq, Fading fading)
        {
            if (!AudioExtension.IsValidFrequency(freq))
            {
                return this;
            }

            SetAllEffectExceptDominator(new Effect(EEffectType.HighPass, freq, fading, true));
            return this;
        }

        #endregion

        private void SetAllEffectExceptDominator(Effect effect)
        {
            SoundManager.Instance.SetEffect(effect).While(PlayerIsPlaying);
            Instance?.SetEffect(EEffectType.None, ESetEffectMode.Override);
        }

        private bool PlayerIsPlaying() => IsAvailable() && IsActive;
#endif
    }
}