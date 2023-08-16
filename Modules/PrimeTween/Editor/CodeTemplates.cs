/*// ReSharper disable PossibleNullReferenceException
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace PrimeTween {
    internal static class CodeTemplates {
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => METHOD_NAME(target, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single endValue, float duration, [NotNull] UnityEngine.AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => METHOD_NAME(target, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single startValue, Single endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => METHOD_NAME(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single startValue, Single endValue, float duration, [NotNull] UnityEngine.AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => METHOD_NAME(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single endValue, TweenSettings settings) => METHOD_NAME(target, new TweenSettings<float>(endValue, settings));
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, Single startValue, Single endValue, TweenSettings settings) => METHOD_NAME(target, new TweenSettings<float>(startValue, endValue, settings));
        
        public static Tween METHOD_NAME([NotNull] UnityEngine.Camera target, TweenSettings<float> settings) {
            return animate(target, ref settings, _tween => {
                var _target = _tween.unityTarget as UnityEngine.Camera;
                var val = _tween.FloatVal;
                _target.orthographicSize = val;
            }, t => (t.unityTarget as UnityEngine.Camera).orthographicSize.ToContainer());
        }

        public static Tween Custom_TEMPLATE(Single startValue, Single endValue, float duration, [NotNull] Action<Single> onValueChange, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => Custom_TEMPLATE(new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)), onValueChange);
        public static Tween Custom_TEMPLATE(Single startValue, Single endValue, float duration, [NotNull] UnityEngine.AnimationCurve ease, [NotNull] Action<Single> onValueChange, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => Custom_TEMPLATE(new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)), onValueChange);
        public static Tween Custom_TEMPLATE(Single startValue, Single endValue, TweenSettings settings, [NotNull] Action<Single> onValueChange) => Custom_TEMPLATE(new TweenSettings<float>(startValue, endValue, settings), onValueChange);
        public static Tween Custom_TEMPLATE(TweenSettings<float> settings, [NotNull] Action<Single> onValueChange) {
            Assert.IsNotNull(onValueChange);
            if (settings.startFromCurrent) {
                UnityEngine.Debug.LogWarning(Constants.customTweensDontSupportStartFromCurrentWarning);
            }
            var instance = PrimeTweenManager.Instance;
            var tween = instance.fetchTween();
            tween.startValue.CopyFrom(ref settings.startValue);
            tween.endValue.CopyFrom(ref settings.endValue);
            tween.propType = PropType.Float;
            tween.customOnValueChange = onValueChange;
            tween.Setup(null, ref settings.settings, _tween => {
                var _onValueChange = _tween.customOnValueChange as Action<Single>;
                var val = _tween.FloatVal;
                try {
                    _onValueChange(val);
                } catch (Exception e) {
                    UnityEngine.Debug.LogError($"Tween was stopped because of exception in {nameof(onValueChange)} callback, tween: {_tween.GetDescription()}, exception:\n{e}", _tween.unityTarget);
                    _tween.EmergencyStop();
                }
            }, null, false);
            return instance.Animate(tween);
        }
        public static Tween Custom_TEMPLATE<T>([NotNull] T target, Single startValue, Single endValue, float duration, [NotNull] Action<T, Single> onValueChange, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class 
            => Custom_TEMPLATE(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)), onValueChange);
        public static Tween Custom_TEMPLATE<T>([NotNull] T target, Single startValue, Single endValue, float duration, [NotNull] UnityEngine.AnimationCurve ease, [NotNull] Action<T, Single> onValueChange, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class 
            => Custom_TEMPLATE(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)), onValueChange);
        public static Tween Custom_TEMPLATE<T>([NotNull] T target, Single startValue, Single endValue, TweenSettings settings, [NotNull] Action<T, Single> onValueChange) where T : class 
            => Custom_TEMPLATE(target, new TweenSettings<float>(startValue, endValue, settings), onValueChange);
        public static Tween Custom_TEMPLATE<T>([NotNull] T target, TweenSettings<float> settings, [NotNull] Action<T, Single> onValueChange) where T : class {
            Assert.IsNotNull(onValueChange);
            if (settings.startFromCurrent) {
                UnityEngine.Debug.LogWarning(Constants.customTweensDontSupportStartFromCurrentWarning);
            }
            var instance = PrimeTweenManager.Instance;
            var tween = instance.fetchTween();
            tween.startValue.CopyFrom(ref settings.startValue);
            tween.endValue.CopyFrom(ref settings.endValue);
            tween.propType = PropType.Float;
            tween.customOnValueChange = onValueChange;
            tween.Setup(target, ref settings.settings, _tween => {
                var _onValueChange = _tween.customOnValueChange as Action<T, Single>;
                var _target = _tween.target as T;
                var val = _tween.FloatVal;
                try {
                    _onValueChange(_target, val);
                } catch (Exception e) {
                    UnityEngine.Debug.LogError($"Tween was stopped because of exception in {nameof(onValueChange)} callback, tween: {_tween.GetDescription()}, exception:\n{e}", _tween.unityTarget);
                    _tween.EmergencyStop();
                }
            }, null, false);
            return instance.Animate(tween);
        }
        static Tween animate([NotNull] object target, ref TweenSettings<float> settings, [NotNull] Action<ReusableTween> setter, Func<ReusableTween, ValueContainer> getter) {
            var instance = PrimeTweenManager.Instance;
            var tween = instance.fetchTween();
            tween.startValue.CopyFrom(ref settings.startValue);
            tween.endValue.CopyFrom(ref settings.endValue);
            tween.propType = PropType.Float;
            tween.Setup(target, ref settings.settings, setter, getter, settings.startFromCurrent);
            return instance.Animate(tween);
        }
        static Tween animateWithIntParam([NotNull] object target, int intParam, ref TweenSettings<float> settings, [NotNull] Action<ReusableTween> setter, [NotNull] Func<ReusableTween, ValueContainer> getter) {
            var instance = PrimeTweenManager.Instance;
            var tween = instance.fetchTween();
            tween.intParam = intParam;
            tween.startValue.CopyFrom(ref settings.startValue);
            tween.endValue.CopyFrom(ref settings.endValue);
            tween.propType = PropType.Float;
            tween.Setup(target, ref settings.settings, setter, getter, settings.startFromCurrent);
            return instance.Animate(tween);
        }
    }
}*/