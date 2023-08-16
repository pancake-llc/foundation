using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace PrimeTween {
    /// Global PrimeTween configuration.
    [PublicAPI]
    public static class PrimeTweenConfig {
        static PrimeTweenManager Instance => PrimeTweenManager.Instance;

        /// <summary>
        /// If <see cref="PrimeTweenManager"/> instance is already created, <see cref="SetTweensCapacity"/> will allocate garbage,
        ///     so it's recommended to use it when no active gameplay is happening. For example, on game launch or when loading a level.
        /// <para>To set initial capacity before <see cref="PrimeTweenManager"/> is created, call <see cref="SetTweensCapacity"/> from a method
        /// with <see cref="RuntimeInitializeOnLoadMethodAttribute"/> and <see cref="RuntimeInitializeLoadType.BeforeSplashScreen"/>. See example below.</para>
        /// </summary>
        /// <example>
        /// <code>
        /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        /// static void beforeSplashScreen() {
        ///     PrimeTweenConfig.SetTweensCapacity(42);
        /// }
        /// </code>
        /// </example>
        public static void SetTweensCapacity(int capacity) {
            Assert.IsTrue(capacity >= 0);
            if (Instance == null) {
                PrimeTweenManager.customInitialCapacity = capacity;
            } else {
                Instance.SetTweensCapacity(capacity);
            }
        }

        public static Ease defaultEase {
            get => Instance.defaultEase;
            set {
                Assert.AreNotEqual(Ease.Custom, value);
                Assert.AreNotEqual(Ease.Default, value);
                Instance.defaultEase = value;
            }
        }
        
        public static bool warnTweenOnDisabledTarget {
            set => Instance.warnTweenOnDisabledTarget = value;
        }
        
        public static bool warnDestroyedTweenHasOnComplete {
            set => Instance.warnDestroyedTweenHasOnComplete = value;
        }
        
        public static bool warnZeroDuration {
            internal get => Instance.warnZeroDuration;
            set => Instance.warnZeroDuration = value;
        }

        public static bool warnStructBoxingAllocationInCoroutine {
            set => Instance.warnStructBoxingAllocationInCoroutine = value;
        }

        public static bool validateCustomCurves {
            set => Instance.validateCustomCurves = value;
        }

        internal const bool defaultUseUnscaledTimeForShakes = false;
    }
}