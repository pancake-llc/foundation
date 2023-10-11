using System;
using System.Collections;
using UnityEngine;

namespace Pancake
{
    public struct App
    {
        private static GlobalComponent globalComponent;
        private static readonly DateTime UnixEpoch = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var app = new GameObject("App") {hideFlags = HideFlags.HideInHierarchy};
            Init(app.AddComponent<GlobalComponent>());
            UnityEngine.Object.DontDestroyOnLoad(app);
        }

        private static void Init(GlobalComponent globalComponent)
        {
            App.globalComponent = globalComponent;
            Data.Init();
            SetAppFirstOpenTimestamp();
        }

        /// <summary>
        /// Gets the installation timestamp of this app in local timezone.
        /// This timestamp is recorded when the app is initialized for
        /// the first time so it's not really precise but can serve well as a rough approximation
        /// provided that the initialization is done soon after app launch.
        /// </summary>
        /// <returns>The installation timestamp.</returns>
        public static DateTime GetAppFirstOpenTimestamp => Data.Load(Invariant.FIRST_OPEN_TIMESTAMP_KEY, UnixEpoch);

        private static void SetAppFirstOpenTimestamp()
        {
            if (!Data.HasKey(Invariant.FIRST_OPEN_TIMESTAMP_KEY)) Data.Save(Invariant.FIRST_OPEN_TIMESTAMP_KEY, DateTime.Now);
        }

        public static float DeltaTime(TimeMode mode) => mode == TimeMode.Normal ? Time.deltaTime : Time.unscaledDeltaTime;

        public static void Add(UpdateMode mode, Action action)
        {
            switch (mode)
            {
                case UpdateMode.FixedUpdate:
                    globalComponent.FixedUpdateInternal += action;
                    return;
                case UpdateMode.WaitForFixedUpdate:
                    globalComponent.WaitForFixedUpdateInternal += action;
                    return;
                case UpdateMode.Update:
                    globalComponent.UpdateInternal += action;
                    return;
                case UpdateMode.LateUpdate:
                    globalComponent.LateUpdateInternal += action;
                    return;
                case UpdateMode.WaitForEndOfFrame:
                    globalComponent.WaitForEndOfFrameInternal += action;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static void Remove(UpdateMode mode, Action action)
        {
            switch (mode)
            {
                case UpdateMode.FixedUpdate:
                    globalComponent.FixedUpdateInternal -= action;
                    return;
                case UpdateMode.WaitForFixedUpdate:
                    globalComponent.WaitForFixedUpdateInternal -= action;
                    return;
                case UpdateMode.Update:
                    globalComponent.UpdateInternal -= action;
                    return;
                case UpdateMode.LateUpdate:
                    globalComponent.LateUpdateInternal -= action;
                    return;
                case UpdateMode.WaitForEndOfFrame:
                    globalComponent.WaitForEndOfFrameInternal -= action;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static void AddTick(ITickProcess tick) { globalComponent.tickProcesses.Add(tick); }

        public static void AddFixedTick(IFixedTickProcess tick) { globalComponent.fixedTickProcesses.Add(tick); }

        public static void AddLateTick(ILateTickProcess tick) { globalComponent.lateTickProcesses.Add(tick); }

        public static void RemoveTick(ITickProcess tick) { globalComponent.tickProcesses.Remove(tick); }

        public static void RemoveFixedTick(IFixedTickProcess tick) { globalComponent.fixedTickProcesses.Remove(tick); }

        public static void RemoveLateTick(ILateTickProcess tick) { globalComponent.lateTickProcesses.Remove(tick); }

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

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static AsyncProcessHandle StartCoroutine(IEnumerator routine) => globalComponent.StartCoroutineInternal(routine);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void StopCoroutine(AsyncProcessHandle handle) => globalComponent.StopCoroutineInternal(handle);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void DisableThrowException() => globalComponent.ThrowException = false;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void EnableThrowException() => globalComponent.ThrowException = true;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Action ToMainThread(Action action) => globalComponent.ToMainThreadImpl(action);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Action<T> ToMainThread<T>(Action<T> action) => globalComponent.ToMainThreadImpl(action);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2> ToMainThread<T1, T2>(Action<T1, T2> action) => globalComponent.ToMainThreadImpl(action);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2, T3> ToMainThread<T1, T2, T3>(Action<T1, T2, T3> action) => globalComponent.ToMainThreadImpl(action);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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
        /// <param name="useRealTime">Whether the DelayHandle uses real-time(not affected by slow-mo or pausing) or
        /// game-time(affected by time scale changes).</param>
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
        /// Safe Delay call when it had target, progress delay will be cancel when target was destroyed
        /// </summary>
        /// <param name="duration">The duration to wait before the DelayHandle fires.</param>
        /// <param name="onComplete">The action to run when the DelayHandle elapses.</param>
        /// <param name="onUpdate">A function to call each tick of the DelayHandle. Takes the number of seconds elapsed since
        /// the start of the current cycle.</param>
        /// <param name="isLooped">Whether the DelayHandle should restart after executing.</param>
        /// <param name="useRealTime">Whether the DelayHandle uses real-time(not affected by slow-mo or pausing) or
        /// game-time(affected by time scale changes).</param>
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
    }
}