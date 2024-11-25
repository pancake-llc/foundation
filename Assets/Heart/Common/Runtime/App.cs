using System;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Pancake.Common
{
    public static class App
    {
        private static GlobalComponent globalComponent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var app = new GameObject("App") {hideFlags = HideFlags.HideInHierarchy};
            globalComponent = app.AddComponent<GlobalComponent>();
            globalComponent.EntryPoint();
            UnityEngine.Object.DontDestroyOnLoad(app);
            Data.Init();
        }

        public static float DeltaTime(ETimeMode mode) => mode == ETimeMode.Normal ? Time.deltaTime : Time.unscaledDeltaTime;

        public static void AddListener(EUpdateMode mode, Action action)
        {
            switch (mode)
            {
                case EUpdateMode.FixedUpdate:
                    globalComponent.OnFixedUpdate += action;
                    return;
                case EUpdateMode.Update:
                    globalComponent.OnUpdate += action;
                    return;
                case EUpdateMode.LateUpdate:
                    globalComponent.OnLateUpdate += action;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static void RemoveListener(EUpdateMode mode, Action action)
        {
            switch (mode)
            {
                case EUpdateMode.FixedUpdate:
                    globalComponent.OnFixedUpdate -= action;
                    return;
                case EUpdateMode.Update:
                    globalComponent.OnUpdate -= action;
                    return;
                case EUpdateMode.LateUpdate:
                    globalComponent.OnLateUpdate -= action;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static void AddPauseCallback(Action<bool> callback)
        {
            globalComponent.OnGamePause -= callback;
            globalComponent.OnGamePause += callback;
        }

        public static void RemovePauseCallback(Action<bool> callback) { globalComponent.OnGamePause -= callback; }

        public static void AddFocusCallback(Action<bool> callback)
        {
            globalComponent.OnGameFocus -= callback;
            globalComponent.OnGameFocus += callback;
        }

        public static void RemoveFocusCallback(Action<bool> callback) { globalComponent.OnGameFocus -= callback; }

        public static void AddQuitCallback(Action callback)
        {
            globalComponent.OnGameQuit -= callback;
            globalComponent.OnGameQuit += callback;
        }

        public static void RemoveQuitCallback(Action callback) { globalComponent.OnGameQuit -= callback; }

        public static void AddLowMemoryCallback(Action callback)
        {
            globalComponent.OnLowMemory -= callback;
            globalComponent.OnLowMemory += callback;
        }

        public static void RemoveLowMemoryCallback(Action callback) { globalComponent.OnLowMemory -= callback; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action ToMainThread(Action action) => globalComponent.ToMainThreadImpl(action);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T> ToMainThread<T>(Action<T> action) => globalComponent.ToMainThreadImpl(action);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2> ToMainThread<T1, T2>(Action<T1, T2> action) => globalComponent.ToMainThreadImpl(action);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2, T3> ToMainThread<T1, T2, T3>(Action<T1, T2, T3> action) => globalComponent.ToMainThreadImpl(action);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunOnMainThread(Action action) => globalComponent.RunOnMainThreadImpl(action);

        /// <summary>
        /// Enables or disables Unity debug log.
        /// </summary>
        /// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
        public static void EnableUnityDebugLog(bool isEnabled)
        {
#if UNITY_2017_1_OR_NEWER
            Debug.unityLogger.logEnabled = isEnabled;
#else
            Debug.logger.logEnabled = isEnabled;
#endif
        }

        /// <summary>
        /// Delay call
        /// </summary>
        /// <param name="duration">The duration to wait before the DelayHandle fires.</param>
        /// <param name="onComplete">The action to run when the DelayHandle elapses.</param>
        /// <param name="onUpdate">A function to call each tick of the DelayHandle. Takes the number of seconds elapsed since
        /// the start of the current cycle.</param>
        /// <param name="isLooped">Whether the DelayHandle should restart after executing.</param>
        /// <param name="useRealTime">Whether the DelayHandle uses real-time(not affected by slow motion or pausing) or
        /// game-time(affected by timescale changes).</param>
        /// <returns></returns>
        public static DelayHandle Delay(float duration, Action onComplete, Action<float> onUpdate = null, bool isLooped = false, bool useRealTime = false)
        {
            var timer = new DelayHandle(duration,
                onComplete,
                onUpdate,
                isLooped,
                useRealTime,
                null);
            globalComponent.RegisterDelayHandle(timer);
            return timer;
        }


        /// <summary>
        /// Safe Delay call when it had target, progress delay will be canceled when target was destroyed
        /// </summary>
        /// <param name="duration">The duration to wait before the DelayHandle fires.</param>
        /// <param name="onComplete">The action to run when the DelayHandle elapses.</param>
        /// <param name="onUpdate">A function to call each tick of the DelayHandle. Takes the number of seconds elapsed since
        /// the start of the current cycle.</param>
        /// <param name="isLooped">Whether the DelayHandle should restart after executing.</param>
        /// <param name="useRealTime">Whether the DelayHandle uses real-time(not affected by slow motion or pausing) or
        /// game-time(affected by timescale changes).</param>
        /// <param name="target">The target (behaviour) to attach this DelayHandle to.</param>
        public static DelayHandle Delay(
            MonoBehaviour target,
            float duration,
            Action onComplete,
            Action<float> onUpdate = null,
            bool isLooped = false,
            bool useRealTime = false)
        {
            var timer = new DelayHandle(duration,
                onComplete,
                onUpdate,
                isLooped,
                useRealTime,
                target);
            globalComponent.RegisterDelayHandle(timer);
            return timer;
        }

        public static void CancelDelay(DelayHandle delayHandle) { delayHandle?.Cancel(); }

        public static void PauseDelay(DelayHandle delayHandle) { delayHandle?.Pause(); }

        public static void ResumeDelay(DelayHandle delayHandle) { delayHandle?.Resume(); }

        public static void CancelAllDelay() { globalComponent.CancelAllDelayHandle(); }

        public static void PauseAllDelay() { globalComponent.PauseAllDelayHandle(); }

        public static void ResumeAllDelay() { globalComponent.ResumeAllDelayHandle(); }

        public static void StopAndClean(ref DelayHandle handle)
        {
            if (handle == null || !handle.IsDone) return;

            CancelDelay(handle);
            handle = null;
        }

        public static void DontDestroy(UnityEngine.Object target) { globalComponent.DontDestroy(target); }
    }
}