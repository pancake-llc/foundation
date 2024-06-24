using System;
using System.Collections;
using System.Collections.Generic;
using LitMotion;
using Pancake.Common;
using UnityEngine;
using UnityEngine.Audio;

namespace Pancake.Sound
{
    public class EffectAutomationHelper : IAutoResetWaitable
    {
        private class Tweaker
        {
            public bool isTweaking;
            public AsyncProcessHandle handle;
            public List<ITweakingWaitable> waitableList;
            public float originValue;
        }

        public interface ITweakingWaitable
        {
            Effect Effect { get; }
            bool IsFinished();
            IEnumerator GetYieldInstruction();
        }

        private class TweakingWaitableBase : ITweakingWaitable
        {
            public Effect Effect { get; set; }
            public TweakingWaitableBase(Effect effect) => Effect = effect;
            public IEnumerator GetYieldInstruction() => null;
            public bool IsFinished() => false;
        }

        private abstract class TweakingWaitableDecorator : ITweakingWaitable
        {
            protected ITweakingWaitable tweakingWaitable;
            public void AttachTo(ITweakingWaitable waitable) => tweakingWaitable = waitable;

            public Effect Effect => tweakingWaitable.Effect;
            public abstract IEnumerator GetYieldInstruction();
            public abstract bool IsFinished();
        }

        private class TweakAndWaitSeconds : TweakingWaitableDecorator
        {
            public readonly float endTime;
            private WaitUntil _waitUntil;

            public TweakAndWaitSeconds(float seconds) { endTime = Time.time + seconds; }

            public override bool IsFinished() => Time.time >= endTime;

            public override IEnumerator GetYieldInstruction()
            {
                _waitUntil ??= new WaitUntil(IsFinished);
                yield return _waitUntil;
            }
        }

        private class TweakAndWaitUntil : TweakingWaitableDecorator
        {
            public readonly IEnumerator enumerator;
            public readonly Func<bool> condition;

            public TweakAndWaitUntil(IEnumerator enumerator, Func<bool> condition)
            {
                this.enumerator = enumerator;
                this.condition = condition;
            }

            public override IEnumerator GetYieldInstruction() => enumerator;
            public override bool IsFinished() => condition();
        }

        private readonly AudioMixer _mixer;
        private readonly Dictionary<EEffectType, Tweaker> _tweakerDict = new();
        private EEffectType _latestEffect;


        public EffectAutomationHelper(AudioMixer mixer) { _mixer = mixer; }

        public WaitForSeconds ForSeconds(float seconds)
        {
            var decoration = new TweakAndWaitSeconds(seconds);
            DecorateTweakingWaitable(decoration);
            return new WaitForSeconds(seconds);
        }

        public WaitUntil Until(Func<bool> condition)
        {
            var instruction = new WaitUntil(condition);
            var decoration = new TweakAndWaitUntil(instruction, condition);
            DecorateTweakingWaitable(decoration);
            return instruction;
        }

        public WaitWhile While(Func<bool> condition)
        {
            var instruction = new WaitWhile(condition);
            var decoration = new TweakAndWaitUntil(instruction, InvertCondition);
            DecorateTweakingWaitable(decoration);
            return instruction;

            bool InvertCondition() => !condition();
        }


        private void DecorateTweakingWaitable(TweakingWaitableDecorator decoration)
        {
            // this should be called after the first tweak is started, the purpose of decorating is to know when will it stop.
            if (_latestEffect == EEffectType.None)
            {
                Debug.LogWarning(AudioConstant.LOG_HEADER + $"AutoResetWaitable on {_latestEffect} is not supported.");
            }
            else if (_tweakerDict.TryGetValue(_latestEffect, out var tweaker))
            {
                int lastIndex = tweaker.waitableList.Count - 1;
                var current = tweaker.waitableList[lastIndex];
                if (current is TweakingWaitableBase)
                {
                    decoration.AttachTo(current);
                    tweaker.waitableList[lastIndex] = decoration;
                }
                else
                {
                    Debug.LogError(AudioConstant.LOG_HEADER + $"The latest waitable isn't the base type:{nameof(TweakingWaitableBase)}");
                }
            }
        }

        public void SetEffectTrackParameter(Effect effect, Action<EEffectType> onReset)
        {
            _latestEffect = effect.type;
            if (_latestEffect == EEffectType.None)
            {
                ResetAllEffect(effect, onReset);
                return;
            }

            if (!_tweakerDict.TryGetValue(effect.type, out var tweaker))
            {
                tweaker = new Tweaker {originValue = GetEffectDefaultValue(effect.type)};
                _tweakerDict.Add(effect.type, tweaker);
            }

            tweaker.waitableList ??= new List<ITweakingWaitable>();
            tweaker.waitableList.Add(new TweakingWaitableBase(effect));

            if (!tweaker.isTweaking)
            {
                App.StopAndReassign(ref tweaker.handle, TweakTrackParameter(tweaker, OnTweakingFinished));
                if (effect.isDominator) SwitchMainTrackMode(true);
            }

            void OnTweakingFinished()
            {
                if (Mathf.Approximately(tweaker.originValue, GetEffectDefaultValue(effect.type)))
                {
                    onReset?.Invoke(effect.type);
                    if (effect.isDominator) SwitchMainTrackMode(false);
                }
            }
        }

        private void SwitchMainTrackMode(bool isDominatorActive)
        {
            string from = isDominatorActive ? AudioConstant.MAIN_TRACK_NAME : AudioConstant.MAIN_DOMINATED_TRACK_NAME;
            string to = isDominatorActive ? AudioConstant.MAIN_DOMINATED_TRACK_NAME : AudioConstant.MAIN_TRACK_NAME;
            _mixer.ChangeChannel(from, to, AudioConstant.FULL_DECIBEL_VOLUME);
        }

        private IEnumerator TweakTrackParameter(Tweaker tweaker, Action onFinished)
        {
            tweaker.isTweaking = true;
            while (tweaker.waitableList.Count > 0)
            {
                int lastIndex = tweaker.waitableList.Count - 1;
                var effect = tweaker.waitableList[lastIndex].Effect;
                string paraName = GetEffectParameterName(effect, out bool hasSecondaryParameter);
                float currentValue = GetCurrentValue(effect);

                yield return Tweak(currentValue,
                    effect.Value,
                    effect.fading.fadeIn,
                    effect.fading.fadeInEase,
                    paraName,
                    hasSecondaryParameter);
                // waitable should be decorated after this tweak

                var waitable = tweaker.waitableList[lastIndex];
                IEnumerator yieldInstruction = waitable.GetYieldInstruction();
                if (yieldInstruction == null)
                {
                    // save value if it's a normal SetEffect 
                    tweaker.originValue = effect.Value;
                    tweaker.waitableList.Clear();
                    break;
                }

                if (!waitable.IsFinished()) yield return yieldInstruction;

                if (tweaker.waitableList.Count == 1)
                {
                    // auto reset to origin after last waitable 
                    yield return Tweak(GetCurrentValue(effect),
                        tweaker.originValue,
                        effect.fading.fadeOut,
                        effect.fading.fadeOutEase,
                        paraName,
                        hasSecondaryParameter);
                }

                tweaker.waitableList.RemoveAt(lastIndex);
            }

            tweaker.isTweaking = false;
            onFinished?.Invoke();
        }

        private IEnumerator Tweak(float from, float to, float fadeTime, Ease ease, string paraName, bool hasSecondaryParameter = false, Action onTweakingFinshed = null)
        {
            if (Mathf.Approximately(from, to)) yield break;

            var values = AudioExtension.GetLerpValuesPerFrame(from, to, fadeTime, ease);
            string secondaryParaName = hasSecondaryParameter ? paraName + "2" : null;

            foreach (float value in values)
            {
                _mixer.SafeSetFloat(paraName, value);
                _mixer.SafeSetFloat(secondaryParaName, value);
                yield return null;
            }

            _mixer.SafeSetFloat(paraName, to);
            _mixer.SafeSetFloat(secondaryParaName, to);
            onTweakingFinshed?.Invoke();
        }

        private bool TryGetCurrentValue(Effect effect, out float value)
        {
            string paraName = GetEffectParameterName(effect, out bool _);
            if (!_mixer.SafeGetFloat(paraName, out value))
            {
                Debug.LogError(AudioConstant.LOG_HEADER + $"Can't get exposed parameter of {effect.type}");
                return false;
            }

            return true;
        }

        private float GetCurrentValue(Effect effect)
        {
            if (TryGetCurrentValue(effect, out float result)) return result;
            return default;
        }

        private void ResetAllEffect(Effect effect, Action<EEffectType> onResetFinished)
        {
            var tweakingCount = 0;
            foreach (var pair in _tweakerDict)
            {
                var tweaker = pair.Value;
                var effectType = pair.Key;
                if (TryGetCurrentValue(effect, out float current))
                {
                    string paraName = GetEffectParameterName(effect, out bool hasSecondaryParameter);
                    App.StopAndClean(ref tweaker.handle);
                    tweaker.handle = App.StartCoroutine(Tweak(current,
                        tweaker.originValue,
                        effect.fading.fadeOut,
                        effect.fading.fadeOutEase,
                        paraName,
                        hasSecondaryParameter,
                        OnTweakingFinished));
                    tweaker.originValue = GetEffectDefaultValue(effectType);
                    tweaker.waitableList.Clear();
                    tweakingCount++;
                }
            }

            void OnTweakingFinished()
            {
                tweakingCount--;
                if (tweakingCount <= 0) onResetFinished?.Invoke(EEffectType.All);
            }
        }

        private string GetEffectParameterName(Effect effect, out bool hasSecondaryParameter)
        {
            hasSecondaryParameter = false;
            switch (effect.type)
            {
                case EEffectType.Volume:
                    if (effect.isDominator) return AudioConstant.MAIN_TRACK_NAME;

                    Debug.LogError(AudioConstant.LOG_HEADER + $"{effect.type} is only supported on Dominator");
                    return string.Empty;
                case EEffectType.LowPass:
                    hasSecondaryParameter = AudioSettings.AudioFilterSlope == EFilterSlope.FourPole;
                    return effect.isDominator ? AudioConstant.DOMINATOR_LOW_PASS_PARA_NAME : AudioConstant.LOW_PASS_PARA_NAME;
                case EEffectType.HighPass:
                    hasSecondaryParameter = AudioSettings.AudioFilterSlope == EFilterSlope.FourPole;
                    return effect.isDominator ? AudioConstant.DOMINATOR_HIGH_PASS_PARA_NAME : AudioConstant.HIGH_PASS_PARA_NAME;
                case EEffectType.Custom: return effect.customExposedParameter;
                default:
                    return string.Empty;
            }
        }

        private float GetEffectDefaultValue(EEffectType effectType)
        {
            switch (effectType)
            {
                case EEffectType.Volume: return AudioConstant.FULL_DECIBEL_VOLUME;
                case EEffectType.LowPass: return AudioConstant.MAX_FREQUENCY;
                case EEffectType.HighPass: return AudioConstant.MIN_FREQUENCY;
                default: return -1f;
            }
        }
    }
}