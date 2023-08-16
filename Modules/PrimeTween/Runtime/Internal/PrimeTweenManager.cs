// ReSharper disable Unity.RedundantHideInInspectorAttribute
#if PRIME_TWEEN_SAFETY_CHECKS && UNITY_ASSERTIONS
#define SAFETY_CHECKS
using System.Linq;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace PrimeTween {
    [AddComponentMenu("")]
    internal class PrimeTweenManager : MonoBehaviour {
        internal static PrimeTweenManager Instance;
        #if UNITY_EDITOR
        static bool isHotReload = true;
        #endif
        internal static int customInitialCapacity = -1;
        const int defaultInitialCapacity = 200;

        /// Item can be null if the list is accessed from the <see cref="ReusableTween.updateAndCheckIfRunning"/> via onValueChange() or onComplete()
        #if UNITY_2021_3_OR_NEWER
        [ItemCanBeNull]
        #endif
        [SerializeField] internal List<ReusableTween> tweens;
        [NonSerialized] internal List<ReusableTween> pool;
        [HideInInspector] internal List<ReusableTween> buffer;
        internal int currentPoolCapacity;
        internal int maxSimultaneousTweensCount;
        
        [HideInInspector] 
        internal int lastId;
        [HideInInspector]
        internal int lastSequenceId;
        internal Ease defaultEase = Ease.OutQuad;
        internal const Ease defaultShakeEase = Ease.OutSine;
        internal bool warnTweenOnDisabledTarget = true;
        internal bool warnDestroyedTweenHasOnComplete = true;
        internal bool warnZeroDuration = true;
        internal bool warnStructBoxingAllocationInCoroutine = true;
        internal bool validateCustomCurves = true;
        int processedCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void beforeSceneLoad() {
            #if UNITY_EDITOR
            isHotReload = false;
            #endif
            Assert.IsNull(Instance);
            var go = new GameObject(nameof(PrimeTweenManager));
            DontDestroyOnLoad(go);
            var instance = go.AddComponent<PrimeTweenManager>();
            instance.init();
            Instance = instance;
        }

        void init() {
            var capacity = customInitialCapacity != -1 ? customInitialCapacity : defaultInitialCapacity;
            tweens = new List<ReusableTween>(capacity);
            buffer = new List<ReusableTween>(capacity);
            pool = new List<ReusableTween>(capacity);
            currentPoolCapacity = capacity;
        }

        const string manualInstanceCreationIsNotAllowedMessage = "Please don't create the " + nameof(PrimeTweenManager) + " instance manually.";
        void Awake() => Assert.IsNull(Instance, manualInstanceCreationIsNotAllowedMessage);

        #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void iniOnLoad() {
            if (!isHotReload) {
                return;
            }
            if (!Application.isPlaying) {
                return;
            }
            Assert.IsNull(Instance);
            var foundInScene = FindObjectOfType<PrimeTweenManager>();
            Assert.IsNotNull(foundInScene);
            #if PRIME_TWEEN_INSPECTOR_DEBUGGING
            Debug.LogError("PRIME_TWEEN_INSPECTOR_DEBUGGING doesn't work with 'Recompile And Continue Playing' because Tween.id is serializable but Tween.tween is not.");
            return;
            #endif
            var count = foundInScene.tweens.Count;
            if (count > 0) {
                Debug.Log($"All tweens ({count}) were stopped because of 'Recompile And Continue Playing'.");
            }
            foundInScene.tweens = new List<ReusableTween>();
            foundInScene.pool = new List<ReusableTween>();
            foundInScene.SetTweensCapacity(foundInScene.currentPoolCapacity);
            Instance = foundInScene;
        }

        void Reset() {
            Assert.IsFalse(Application.isPlaying);
            Debug.LogError(manualInstanceCreationIsNotAllowedMessage);
            DestroyImmediate(this);
        }
        #endif

        void Start() => Assert.AreEqual(Instance, this, manualInstanceCreationIsNotAllowedMessage);

        void OnDestroy() {
            customInitialCapacity = -1;
        }
        
        /// <summary>
        /// The most common tween lifecycle:
        /// 1. User's script creates a tween in Update() in frame N.
        /// 2. PrimeTweenManager.LateUpdate() applies the 'startValue' to the tween in the SAME FRAME N. This guarantees that the animation is rendered at the 'startValue' in the same frame the tween is created.
        /// 3. PrimeTweenManager.Update() executes the first animation step on frame N+1. PrimeTweenManager's execution order is -2000, this means that
        ///     all tweens created in previous frames will already be updated before user's script Update() (if user's script execution order is greater than -2000). 
        /// 4. PrimeTweenManager.Update() completes the tween on frame N+(duration*targetFrameRate) given that targetFrameRate is stable.
        /// </summary>
        void Update() {
            var deltaTime = Time.deltaTime;
            var unscaledDeltaTime = Time.unscaledDeltaTime;
            // onComplete and onValueChange can create new tweens. Cache count to process only those tweens that were present when the update started
            var oldCount = tweens.Count;
            var numRemoved = 0;
            // Process tweens in the order of creation.
            // This allows to create tween duplicates because the latest tween on the same value will overwrite the previous ones.
            for (int i = 0; i < oldCount; i++) {
                var tween = tweens[i];
                var newIndex = i - numRemoved;
                #if SAFETY_CHECKS
                Assert.IsNotNull(tween);
                if (numRemoved > 0) {
                    Assert.IsNull(tweens[newIndex]);
                }
                #endif
                // ReSharper disable once PossibleNullReferenceException
                // don't release a tween until all tweens in a sequence are completed
                if (tween.updateAndCheckIfRunning(tween.settings.useUnscaledTime ? unscaledDeltaTime : deltaTime) || tween.sequence.IsCreated) {
                    if (i != newIndex) {
                        tweens[i] = null;
                        tweens[newIndex] = tween;
                    }
                    continue;
                }
                releaseTweenToPool(tween);
                tweens[i] = null; // set to null after releaseTweenToPool() so in case of an exception, the tween will stay inspectable via Inspector 
                numRemoved++;
            }
            processedCount = oldCount - numRemoved;
            #if SAFETY_CHECKS
            Assert.IsTrue(tweens.Skip(oldCount - numRemoved).Take(numRemoved).All(_ => _ == null));
            Assert.IsTrue(tweens.Skip(oldCount).All(_ => _ != null));
            #endif
            if (numRemoved == 0) {
                return;
            }
            var newCount = tweens.Count;
            for (int i = oldCount; i < newCount; i++) {
                var tween = tweens[i];
                var newIndex = i - numRemoved;
                #if SAFETY_CHECKS
                Assert.IsNotNull(tween);
                #endif
                tweens[newIndex] = tween;
            }
            tweens.RemoveRange(newCount - numRemoved, numRemoved);
            Assert.AreEqual(tweens.Count, newCount - numRemoved);
            #if SAFETY_CHECKS
            Assert.IsFalse(tweens.Any(_ => _ == null));
            Assert.AreEqual(tweens.Count, tweens.Distinct().Count());
            #endif
        }

        void LateUpdate() {
            var cachedCount = tweens.Count;
            for (int i = processedCount; i < cachedCount; i++) {
                var tween = tweens[i];
                // ReSharper disable once PossibleNullReferenceException
                if (tween.isAlive && !tween.startFromCurrent && !tween.waitFor.IsAlive && tween.settings.startDelay == 0 && !tween.isUnityTargetDestroyed()) {
                    Assert.AreEqual(0, tween.elapsedTime);
                    tween.ReportOnValueChange(0);
                }
            }
        }

        void releaseTweenToPool([NotNull] ReusableTween tween) {
            #if SAFETY_CHECKS
            foreach (var t in tweens) {
                if (t != null) {
                    Assert.AreNotEqual(tween.id, t.nextInSequence.id);
                }
            }
            #endif
            tween.Reset();
            pool.Add(tween);
        }

        internal static Tween createEmpty() {
            var result = delayWithoutDurationCheck(null, 0, false);
            Assert.IsTrue(result.HasValue);
            return result.Value;
        }

        // Returns null if target is a destroyed UnityEngine.Object
        internal static Tween? delayWithoutDurationCheck([CanBeNull] object target, float duration, bool useUnscaledTime) {
            var tween = Instance.fetchTween();
            tween.propType = PropType.Float;
            tween.tweenType = TweenType.Delay;
            var settings = new TweenSettings {
                duration = duration,
                ease = Ease.Linear,
                useUnscaledTime = useUnscaledTime
            };
            tween.Setup(target, ref settings, _ => {}, null, false);
            var result = Instance.addTween(tween);
            // ReSharper disable once RedundantCast
            return result.IsCreated ? result : (Tween?)null;
        }

        [NotNull]
        internal ReusableTween fetchTween() {
            if (pool.Count == 0) {
                pool.Add(new ReusableTween());
            }
            var lastIndex = pool.Count - 1;
            var result = pool[lastIndex];
            pool.RemoveAt(lastIndex);
            lastId++;
            result.id = lastId;
            return result;
        }

        internal Tween Animate([NotNull] ReusableTween tween) {
            checkDuration(tween.target, tween.settings.duration);
            return addTween(tween);
        }

        internal static void checkDuration<T>([CanBeNull] T target, float duration) where T : class {
            if (duration < 0 || (duration == 0 && Instance.warnZeroDuration)) {
                Debug.LogWarning($"Tween duration ({duration}) <= 0. {Constants.buildWarningCanBeDisabledMessage(nameof(warnZeroDuration))}", target as UnityEngine.Object);
            }
        }

        Tween addTween([NotNull] ReusableTween tween) {
            Assert.IsNotNull(tween);
            Assert.IsTrue(tween.id > 0);
            if (!(tween.unityTarget is null) && tween.unityTarget == null) {
                Debug.LogError($"Tween's UnityEngine.Object target is null: {tween.GetDescription()}. This error can mean that:\n" +
                               "- The target reference is not populated in the Inspector.\n" +
                               "- The target has been destroyed.\n");
                tween.kill();
                releaseTweenToPool(tween);
                return default;
            }
            if (warnTweenOnDisabledTarget) {
                if (tween.unityTarget is Component comp && !comp.gameObject.activeInHierarchy) {
                    Debug.LogWarning($"Tween is started on GameObject that is not active in hierarchy: {comp.name}. {Constants.buildWarningCanBeDisabledMessage(nameof(warnTweenOnDisabledTarget))}", comp);
                }
            }
            // Debug.Log($"add tween {tween.id}, {tween.GetDescription()}", tween.unityTarget);
            tweens.Add(tween);
            #if UNITY_ASSERTIONS
            maxSimultaneousTweensCount = Math.Max(maxSimultaneousTweensCount, tweens.Count);
            var newCapacity = Math.Max(pool.Capacity, tweens.Capacity);
            if (currentPoolCapacity != newCapacity) {
                Debug.LogWarning($"Tweens capacity has been increased from {currentPoolCapacity} to {newCapacity}. Please increase the capacity manually to prevent memory allocations at runtime by calling {Constants.setTweensCapacityMethod}.\n" +
                                 $"To know the highest number of simultaneously running tweens, please observe the '{nameof(PrimeTweenManager)}/{Constants.maxAliveTweens}' in Inspector.\n");
                currentPoolCapacity = newCapacity;
            }
            #endif
            return new Tween(tween);
        }

        /// <param name="onTarget">If specified, processes only tweens with such target. If null, processes all running tweens.</param>
        /// <param name="minExpected">If specified, asserts that the number of processed tweens is greater than or equal to <see cref="minExpected"/>.</param>
        /// <param name="numMaxExpected">If specified, asserts that the number of processed tweens is less than or equal to <see cref="numMaxExpected"/>.</param>
        /// <returns>The number of processed tweens.</returns>
        internal int processAll([CanBeNull] object onTarget, [NotNull] Predicate<ReusableTween> predicate, int? minExpected, int? numMaxExpected) {
            int numProcessed = 0;
            foreach (var tween in tweens) {
                if (tween == null) {
                    continue;
                }
                if (onTarget != null && tween.target != onTarget) {
                    continue;
                }
                if (tween.isAlive && predicate(tween)) {
                    numProcessed++;
                }
            }
            #if UNITY_ASSERTIONS
            if (minExpected.HasValue && numProcessed < minExpected) {
                throw new Exception($"the number of processed tweens ({numProcessed}) is less than '{nameof(minExpected)}' ({minExpected.Value})");
            }
            if (numMaxExpected.HasValue && numProcessed > numMaxExpected) {
                throw new Exception($"the number of processed tweens ({numProcessed}) is greater than '{nameof(numMaxExpected)}' ({numMaxExpected.Value})");
            }
            #endif
            return numProcessed;
        }
        
        internal void SetTweensCapacity(int capacity) {
            var runningTweens = tweens.Count;
            if (capacity < runningTweens) {
                Debug.LogError($"New capacity ({capacity}) should be greater than the number of currently running tweens ({runningTweens}).\n" +
                        $"You can use {nameof(Tween)}.{nameof(Tween.StopAll)}() to stop all running tweens.");
                return;
            }
            tweens.Capacity = capacity;
            Assert.AreEqual(0, buffer.Count);
            buffer.Capacity = capacity;
            resizeAndSetCapacity(pool, capacity - runningTweens, capacity);
            currentPoolCapacity = capacity;
            Assert.AreEqual(capacity, tweens.Capacity);
            Assert.AreEqual(capacity, pool.Capacity);
        }

        internal static void resizeAndSetCapacity([NotNull] List<ReusableTween> list, int newCount, int newCapacity) {
            Assert.IsTrue(newCapacity >= newCount);
            int curCount = list.Count;
            if (curCount > newCount) {
                var numToRemove = curCount - newCount;
                list.RemoveRange(newCount, numToRemove);
                list.Capacity = newCapacity;
            } else {
                list.Capacity = newCapacity;
                if (newCount > curCount) {
                    var numToCreate = newCount - curCount;
                    for (int i = 0; i < numToCreate; i++) {
                        list.Add(new ReusableTween());
                    }
                }
            }
            Assert.AreEqual(newCount, list.Count);
            Assert.AreEqual(newCapacity, list.Capacity);
        }
        
        [Conditional("UNITY_ASSERTIONS")]
        internal void warnStructBoxingInCoroutineOnce() {
            if (!warnStructBoxingAllocationInCoroutine) {
                return;
            }
            warnStructBoxingAllocationInCoroutine = false;
            Debug.LogWarning("Please use Tween/Sequence." + nameof(Tween.ToYieldInstruction) + "() when waiting for a Tween/Sequence in coroutines to prevent struct boxing.\n" + 
                             Constants.buildWarningCanBeDisabledMessage(nameof(PrimeTweenConfig.warnStructBoxingAllocationInCoroutine)) + "\n");
        }
    }
    
    internal enum TweenType {
        None,
        Delay,
        ShakeLocalPosition,
        ShakeLocalRotation,
        ShakeLocalScale,
        ShakeCustom
    }
}