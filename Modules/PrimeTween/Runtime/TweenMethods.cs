// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace PrimeTween {
    public partial struct Tween {
        /// <summary>Stops all tweens. If onTarget is provided, stops only tweens on this target.<br/>
        /// This method stops tweens, but doesn't stop sequences directly. That is, if a stopped tween was in a sequence, the sequence will only be stopped if it has no more running tweens.</summary>
        /// <seealso cref="PrimeTweenManager.processAll"/>
        public static int StopAll([CanBeNull] object onTarget = null, int? numMinExpected = null, int? numMaxExpected = null) {
            return PrimeTweenManager.processAll(onTarget, tween => {
                tween.kill();
                tween.updateSequenceAfterKill();
                return true;
            }, numMinExpected, numMaxExpected);
        }

        /// <summary>Completes all tweens. If onTarget is provided, completes only tweens on this target.<br/>
        /// This method completes tweens, but doesn't complete sequences directly. That is, if a completed tween was in a sequence, the sequence will only be completed if it has no more running tweens.</summary>
        /// <seealso cref="PrimeTweenManager.processAll"/>
        public static int CompleteAll([CanBeNull] object onTarget = null, int? numMinExpected = null, int? numMaxExpected = null) {
            return PrimeTweenManager.processAll(onTarget, tween => {
                if (tween.tryManipulate()) {
                    tween.ForceComplete();
                    tween.updateSequenceAfterKill();
                    return true;
                }
                return false;
            }, numMinExpected, numMaxExpected);
        }
 
        /// <summary>Sets 'IsPaused' on all tweens. If onTarget is provided, sets 'IsPaused' only on this target.</summary>
        /// <seealso cref="PrimeTweenManager.processAll"/>
        public static int SetPausedAll(bool isPaused, [CanBeNull] object onTarget = null, int? numMinExpected = null, int? numMaxExpected = null) {
            if (isPaused) {
                return PrimeTweenManager.processAll(onTarget, tween => {
                    return tween.trySetPause(true);
                }, numMinExpected, numMaxExpected);
            }
            return PrimeTweenManager.processAll(onTarget, tween => {
                return tween.tryManipulate() && tween.trySetPause(false);
            }, numMinExpected, numMaxExpected);
        }

        /// <summary>Please note: delay may outlive the caller (the calling UnityEngine.Object may already be destroyed).
        /// When using this overload, it's user's responsibility to ensure that <see cref="onComplete"/> is safe to execute once the delay is finished.
        /// It's preferable to use the <see cref="Delay{T}"/> overload because it checks if the UnityEngine.Object target is still alive before calling the <see cref="onComplete"/>.</summary>
        public static Tween Delay(float duration, [CanBeNull] Action onComplete = null, bool useUnscaledTime = false) {
            return delay(null, duration, onComplete, useUnscaledTime);
        }
        public static Tween Delay([NotNull] object target, float duration, [CanBeNull] Action onComplete = null, bool useUnscaledTime = false) {
            Assert.IsNotNull(target);
            return delay(target, duration, onComplete, useUnscaledTime);
        }
        static Tween delay([CanBeNull] object target, float duration, [CanBeNull] Action onComplete, bool useUnscaledTime) {
            var result = delay_internal(target, duration, useUnscaledTime);
            if (onComplete != null) {
                result?.OnComplete(onComplete);
            }
            return result ?? default;
        }
        
        /// <summary> This is the most preferable overload of all Delay functions:<br/>
        /// - It checks if UnityEngine.Object target is still alive before calling the <see cref="onComplete"/> callback.<br/>
        /// - It allows to call any method on <see cref="target"/> without producing garbage.</summary>
        /// <example>
        /// <code>
        /// Tween.Delay(this, duration: 1f, onComplete: _this =&gt; {
        ///     // Please note: we're using '_this' variable from the onComplete callback. Calling DoSomething() directly will implicitly capture 'this' variable (creating a closure) and generate garbage.
        ///     _this.DoSomething();
        /// });
        /// </code>
        /// </example>
        public static Tween Delay<T>([NotNull] T target, float duration, [NotNull] Action<T> onComplete, bool useUnscaledTime = false) where T : class {
            return delay_internal(target, duration, useUnscaledTime)?.OnComplete(target, onComplete) ?? default;
        }

        static Tween? delay_internal([CanBeNull] object target, float duration, bool useUnscaledTime) {
            PrimeTweenManager.checkDuration(target, duration);
            return PrimeTweenManager.delayWithoutDurationCheck(target, duration, useUnscaledTime);
        }

        internal static Tween waitFor(Tween other) {
            Assert.IsTrue(other.IsAlive);
            var result = PrimeTweenManager.createEmpty();
            result.tween.setWaitFor(other);
            return result;
        }
        
        public static Tween MaterialColor([NotNull] Material target, int propertyId, Color endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialColor(target, propertyId, new TweenSettings<Color>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialColor([NotNull] Material target, int propertyId, Color endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialColor(target, propertyId, new TweenSettings<Color>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialColor([NotNull] Material target, int propertyId, Color startValue, Color endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialColor(target, propertyId, new TweenSettings<Color>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialColor([NotNull] Material target, int propertyId, Color startValue, Color endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialColor(target, propertyId, new TweenSettings<Color>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialColor([NotNull] Material target, int propertyId, Color endValue, TweenSettings settings) => MaterialColor(target, propertyId, new TweenSettings<Color>(endValue, settings));
        public static Tween MaterialColor([NotNull] Material target, int propertyId, Color startValue, Color endValue, TweenSettings settings) => MaterialColor(target, propertyId, new TweenSettings<Color>(startValue, endValue, settings));
        public static Tween MaterialColor([NotNull] Material target, int propertyId, TweenSettings<Color> settings) {
            return animateWithIntParam(target, propertyId, ref settings,
                tween => (tween.unityTarget as Material).SetColor(tween.intParam, tween.ColorVal),
                tween => (tween.unityTarget as Material).GetColor(tween.intParam).ToContainer());
        }

        public static Tween MaterialProperty([NotNull] Material target, int propertyId, float endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialProperty(target, propertyId, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, float endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialProperty(target, propertyId, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, float startValue, float endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialProperty(target, propertyId, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, float startValue, float endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialProperty(target, propertyId, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, float endValue, TweenSettings settings) => MaterialProperty(target, propertyId, new TweenSettings<float>(endValue, settings));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, float startValue, float endValue, TweenSettings settings) => MaterialProperty(target, propertyId, new TweenSettings<float>(startValue, endValue, settings));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, TweenSettings<float> settings) {
            return animateWithIntParam(target, propertyId, ref settings,
                tween => (tween.unityTarget as Material).SetFloat(tween.intParam, tween.FloatVal),
                tween => (tween.unityTarget as Material).GetFloat(tween.intParam).ToContainer());
        }

        public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialAlpha(target, propertyId, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialAlpha(target, propertyId, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float startValue, float endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialAlpha(target, propertyId, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float startValue, float endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialAlpha(target, propertyId, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float endValue, TweenSettings settings) => MaterialAlpha(target, propertyId, new TweenSettings<float>(endValue, settings));
        public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float startValue, float endValue, TweenSettings settings) => MaterialAlpha(target, propertyId, new TweenSettings<float>(startValue, endValue, settings));
        public static Tween MaterialAlpha([NotNull] Material target, int propertyId, TweenSettings<float> settings) {
            return animateWithIntParam(target, propertyId, ref settings,
                tween => {
                    var _target = tween.unityTarget as Material;
                    var _propId = tween.intParam;
                    _target.SetColor(_propId, _target.GetColor(_propId).WithAlpha(tween.FloatVal));
                },
                tween => (tween.unityTarget as Material).GetColor(tween.intParam).a.ToContainer());
        }

        public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 endValue, TweenSettings settings) => MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(endValue, settings));
        public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, TweenSettings settings) => MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, settings));
        public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, TweenSettings<Vector2> settings) {
            return animateWithIntParam(target, propertyId, ref settings,
                tween => (tween.unityTarget as Material).SetTextureOffset(tween.intParam, tween.Vector2Val),
                tween => (tween.unityTarget as Material).GetTextureOffset(tween.intParam).ToContainer());
        }

        public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 endValue, TweenSettings settings) => MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(endValue, settings));
        public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, TweenSettings settings) => MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, settings));
        public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, TweenSettings<Vector2> settings) {
            return animateWithIntParam(target, propertyId, ref settings,
                tween => (tween.unityTarget as Material).SetTextureScale(tween.intParam, tween.Vector2Val),
                tween => (tween.unityTarget as Material).GetTextureScale(tween.intParam).ToContainer());
        }

        public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialProperty(target, propertyId, new TweenSettings<Vector4>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialProperty(target, propertyId, new TweenSettings<Vector4>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 startValue, Vector4 endValue, float duration, Ease ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialProperty(target, propertyId, new TweenSettings<Vector4>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 startValue, Vector4 endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => MaterialProperty(target, propertyId, new TweenSettings<Vector4>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 endValue, TweenSettings settings) => MaterialProperty(target, propertyId, new TweenSettings<Vector4>(endValue, settings));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 startValue, Vector4 endValue, TweenSettings settings) => MaterialProperty(target, propertyId, new TweenSettings<Vector4>(startValue, endValue, settings));
        public static Tween MaterialProperty([NotNull] Material target, int propertyId, TweenSettings<Vector4> settings) {
            return animateWithIntParam(target, propertyId, ref settings,
                tween => (tween.unityTarget as Material).SetVector(tween.intParam, tween.Vector4Val),
                tween => (tween.unityTarget as Material).GetVector(tween.intParam).ToContainer());
        }
 
        // No 'startFromCurrent' overload
        public static Tween EulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => EulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween EulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => EulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween EulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings) => EulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, settings));
        public static Tween EulerAngles([NotNull] Transform target, TweenSettings<Vector3> settings) {
            validateEulerAnglesData(ref settings);
            return animate(target, ref settings, _ => { (_.unityTarget as Transform).eulerAngles = _.Vector3Val; }, _ => (_.unityTarget as Transform).eulerAngles.ToContainer());
        }
        
        public static Tween LocalEulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => LocalEulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween LocalEulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => LocalEulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
        public static Tween LocalEulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings) => LocalEulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, settings));
        public static Tween LocalEulerAngles([NotNull] Transform target, TweenSettings<Vector3> settings) {
            validateEulerAnglesData(ref settings);
            return animate(target, ref settings, _ => { (_.unityTarget as Transform).localEulerAngles = _.Vector3Val; }, _ => (_.unityTarget as Transform).localEulerAngles.ToContainer());
        }
        static void validateEulerAnglesData(ref TweenSettings<Vector3> settings) {
            if (settings.startFromCurrent) {
                settings.startFromCurrent = false;
                Debug.LogWarning("Animating euler angles from the current value may produce unexpected results because there is more than one way to represent the current rotation using Euler angles.\n" +
                                 "'" + nameof(TweenSettings<float>.startFromCurrent) + "' was ignored.\n" +
                                 "More info: https://docs.unity3d.com/ScriptReference/Transform-eulerAngles.html\n");
            }
        }
        
        // Called from TweenGenerated.cs
        public static Tween LocalScale([NotNull] Transform target, TweenSettings<float> uniformScaleSettings) {
            var remapped = new TweenSettings<Vector3>(uniformScaleSettings.startValue * Vector3.one, uniformScaleSettings.endValue * Vector3.one, uniformScaleSettings.settings) { startFromCurrent = uniformScaleSettings.startFromCurrent };
            return LocalScale(target, remapped);
        }
        public static Tween Rotation([NotNull] Transform target, TweenSettings<Vector3> eulerAnglesSettings) => Rotation(target, toQuaternion(eulerAnglesSettings));
        public static Tween LocalRotation([NotNull] Transform target, TweenSettings<Vector3> localEulerAnglesSettings) => LocalRotation(target, toQuaternion(localEulerAnglesSettings));
        static TweenSettings<Quaternion> toQuaternion(TweenSettings<Vector3> s) => new TweenSettings<Quaternion>(Quaternion.Euler(s.startValue), Quaternion.Euler(s.endValue), s.settings) { startFromCurrent = s.startFromCurrent };

        
        #if PRIME_TWEEN_EXPERIMENTAL
        public static Tween RotationAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween RotationAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween RotationAdditive([NotNull] Transform target, Vector3 deltaValue, TweenSettings settings) 
            => CustomAdditive(target, deltaValue, settings, (_target, delta) => _target.rotation *= Quaternion.Euler(delta));

        public static Tween LocalRotationAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween LocalRotationAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween LocalRotationAdditive([NotNull] Transform target, Vector3 deltaValue, TweenSettings settings) 
            => CustomAdditive(target, deltaValue, settings, (_target, delta) => _target.localRotation *= Quaternion.Euler(delta));

        public static Tween PositionAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween PositionAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween PositionAdditive([NotNull] Transform target, Vector3 deltaValue, TweenSettings settings) 
            => CustomAdditive(target, deltaValue, settings, (_target, delta) => _target.position += delta);

        public static Tween LocalPositionAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween LocalPositionAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween LocalPositionAdditive([NotNull] Transform target, Vector3 deltaValue, TweenSettings settings) 
            => CustomAdditive(target, deltaValue, settings, (_target, delta) => _target.localPosition += delta);

        public static Tween LocalScaleAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween LocalScaleAdditive([NotNull] Transform target, Vector3 deltaValue, float duration, [NotNull] AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) 
            => RotationAdditive(target, deltaValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public static Tween LocalScaleAdditive([NotNull] Transform target, Vector3 deltaValue, TweenSettings settings) 
            => CustomAdditive(target, deltaValue, settings, (_target, delta) => _target.localScale += delta);
        #endif // PRIME_TWEEN_EXPERIMENTAL
    }
}