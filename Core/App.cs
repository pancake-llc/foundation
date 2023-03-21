using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public struct App
    {
        private static event Action FixedUpdateInternal;
        private static event Action WaitForFixedUpdateInternal;
        private static event Action UpdateInternal;
        private static event Action LateUpdateInternal;
        private static event Action WaitForEndOfFrameInternal;

        public static event Action FixedUpdateOnceTime;
        public static event Action WaitForFixedUpdateOnceTime;
        public static event Action UpdateOnceTime;
        public static event Action LateUpdateOnceTime;
        public static event Action WaitForEndOfFrameOnceTime;

        private static readonly List<Action> ToMainThreads = new();
        private static volatile bool isToMainThreadQueueEmpty = true; // Flag indicating whether there's any action queued to be run on game thread.

        private static event Action<bool> OnGamePause;
        private static event Action<bool> OnGameFocus;
        private static event Action OnGameQuit;

        private static readonly List<ITickProcess> TickProcesses = new List<ITickProcess>(1024);
        private static readonly List<IFixedTickProcess> FixedTickProcesses = new List<IFixedTickProcess>(512);
        private static readonly List<ILateTickProcess> LateTickProcesses = new List<ILateTickProcess>(256);

        public static readonly DateTime UnixEpoch = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
        internal const string FIRST_INSTALL_TIMESTAMP_KEY = "first_install_timestamp";

        public static bool IsAppInitialized { get; internal set; }

        public static int FixedFrameCount { get; private set; }

        /// <summary>
        /// Gets the installation timestamp of this app in local timezone.
        /// This timestamp is recorded when the app is initialized for
        /// the first time so it's not really precise but can serve well as a rough approximation
        /// provided that the initialization is done soon after app launch.
        /// </summary>
        /// <returns>The installation timestamp.</returns>
        public static DateTime GetAppInstallTimestamp => Data.Load(FIRST_INSTALL_TIMESTAMP_KEY, UnixEpoch);

        public static float DeltaTime(TimeMode mode) => mode == TimeMode.Normal ? Time.deltaTime : Time.unscaledDeltaTime;

        public static void Add(UpdateMode mode, Action action)
        {
            switch (mode)
            {
                case UpdateMode.FixedUpdate:
                    FixedUpdateInternal += action;
                    return;
                case UpdateMode.WaitForFixedUpdate:
                    WaitForFixedUpdateInternal += action;
                    return;
                case UpdateMode.Update:
                    UpdateInternal += action;
                    return;
                case UpdateMode.LateUpdate:
                    LateUpdateInternal += action;
                    return;
                case UpdateMode.WaitForEndOfFrame:
                    WaitForEndOfFrameInternal += action;
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
                    FixedUpdateInternal -= action;
                    return;
                case UpdateMode.WaitForFixedUpdate:
                    WaitForFixedUpdateInternal -= action;
                    return;
                case UpdateMode.Update:
                    UpdateInternal -= action;
                    return;
                case UpdateMode.LateUpdate:
                    LateUpdateInternal -= action;
                    return;
                case UpdateMode.WaitForEndOfFrame:
                    WaitForEndOfFrameInternal -= action;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static void AddTick(ITickProcess tick) { TickProcesses.Add(tick); }

        public static void AddFixedTick(IFixedTickProcess tick) { FixedTickProcesses.Add(tick); }

        public static void AddLateTick(ILateTickProcess tick) { LateTickProcesses.Add(tick); }

        public static void RemoveTick(ITickProcess tick) { TickProcesses.Remove(tick); }

        public static void RemoveFixedTick(IFixedTickProcess tick) { FixedTickProcesses.Remove(tick); }

        public static void RemoveLateTick(ILateTickProcess tick) { LateTickProcesses.Remove(tick); }

        public static void AddPauseCallback(Action<bool> callback)
        {
            OnGamePause -= callback;
            OnGamePause += callback;
        }

        public static void RemovePauseCallback(Action<bool> callback) { OnGamePause -= callback; }

        public static void AddFocusCallback(Action<bool> callback)
        {
            OnGameFocus -= callback;
            OnGameFocus += callback;
        }

        public static void RemoveFocusCallback(Action<bool> callback) { OnGameFocus -= callback; }

        public static void AddQuitCallback(Action callback)
        {
            OnGameQuit -= callback;
            OnGameQuit += callback;
        }

        public static void RemoveQuitCallback(Action callback) { OnGameQuit -= callback; }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Coroutine RunCoroutine(IEnumerator routine) => GlobalComponent.RunCoroutineImpl(routine);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void EndCoroutine(IEnumerator routine) => GlobalComponent.EndCoroutineImpl(routine);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Action ToMainThread(Action action) => GlobalComponent.ToMainThreadImpl(action);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Action<T> ToMainThread<T>(Action<T> action) => GlobalComponent.ToMainThreadImpl(action);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2> ToMainThread<T1, T2>(Action<T1, T2> action) => GlobalComponent.ToMainThreadImpl(action);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2, T3> ToMainThread<T1, T2, T3>(Action<T1, T2, T3> action) => GlobalComponent.ToMainThreadImpl(action);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void RunOnMainThread(Action action) => GlobalComponent.RunOnMainThreadImpl(action);

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
        /// <inheritdoc cref="GlobalComponent.AttachImpl"/>
        /// </summary>
        /// <param name="prefab"></param>
        public static void Attach(AutoInitialize prefab) { GlobalComponent.AttachImpl(prefab); }

        [DisallowMultipleComponent]
        internal class GlobalComponent : MonoBehaviour
        {
            private static GlobalComponent global;

            private static bool IsInitialized => global != null;

            private List<Action> _localToMainThreads = new();

            private void Awake() { global = this; }

            private void Start()
            {
                StartCoroutine(WaitForFixedUpdate());
                StartCoroutine(WaitForEndOfFrame());

                IEnumerator WaitForFixedUpdate()
                {
                    var wait = new WaitForFixedUpdate();
                    while (true)
                    {
                        yield return wait;
                        FixedFrameCount += 1;
                        WaitForFixedUpdateInternal?.Invoke();
                        C.CallActionClean(ref WaitForFixedUpdateOnceTime);
                    }
                    // ReSharper disable once IteratorNeverReturns
                }

                IEnumerator WaitForEndOfFrame()
                {
                    var wait = new WaitForEndOfFrame();
                    while (true)
                    {
                        yield return wait;
                        WaitForEndOfFrameInternal?.Invoke();
                        C.CallActionClean(ref WaitForEndOfFrameOnceTime);
                    }
                    // ReSharper disable once IteratorNeverReturns
                }
            }

            private void Update()
            {
                for (int i = 0; i < TickProcesses.Count; i++)
                {
                    TickProcesses[i]?.OnTick();
                }

                UpdateInternal?.Invoke();
                C.CallActionClean(ref UpdateOnceTime);

                if (isToMainThreadQueueEmpty) return;
                _localToMainThreads.Clear();
                lock (ToMainThreads)
                {
                    _localToMainThreads.AddRange(ToMainThreads);
                    ToMainThreads.Clear();
                    isToMainThreadQueueEmpty = true;
                }

                for (int i = 0; i < _localToMainThreads.Count; i++)
                {
                    _localToMainThreads[i].Invoke();
                }
            }

            private void FixedUpdate()
            {
                for (int i = 0; i < FixedTickProcesses.Count; i++)
                {
                    FixedTickProcesses[i]?.OnFixedTick();
                }

                FixedUpdateInternal?.Invoke();

                C.CallActionClean(ref FixedUpdateOnceTime);
            }

            private void LateUpdate()
            {
                for (int i = 0; i < LateTickProcesses.Count; i++)
                {
                    LateTickProcesses[i]?.OnLateTick();
                }

                LateUpdateInternal?.Invoke();
                C.CallActionClean(ref LateUpdateOnceTime);
            }

            /// <summary>
            /// Called when the gamme loses or gains focus. 
            /// </summary>
            /// <param name="hasFocus"><c>true</c> if the gameobject has focus, else <c>false</c>.</param>
            /// <remarks>
            /// On Windows Store Apps and Windows Phone 8.1 there's no application quit event,
            /// consider using OnApplicationFocus event when hasFocus equals false.
            /// </remarks>
            private void OnApplicationFocus(bool hasFocus) { OnGameFocus?.Invoke(hasFocus); }

            /// <summary>
            /// Called when the application pauses.
            /// </summary>
            /// <param name="pauseStatus"><c>true</c> if the application is paused, else <c>false</c>.</param>
            private void OnApplicationPause(bool pauseStatus) { OnGamePause?.Invoke(pauseStatus); }

            /// <summary>
            /// Called before the application quits.
            /// </summary>
            /// <remarks>
            /// iOS applications are usually suspended and do not quit.
            /// On Windows Store Apps and Windows Phone 8.1 there's no application quit event,
            /// consider using OnApplicationFocus event when focusStatus equals false.
            /// On WebGL is not possible to implement OnApplicationQuit due to nature of the
            /// browser tabs closing.
            /// </remarks>
            private void OnApplicationQuit() { OnGameQuit?.Invoke(); }

            private void OnDisable()
            {
                if (global == this) global = null;
            }

            #region internal effective

            /// <summary>
            /// Starts a coroutine from non-MonoBehavior objects.
            /// </summary>
            /// <param name="routine">Routine.</param>
            internal static Coroutine RunCoroutineImpl(IEnumerator routine)
            {
                if (routine != null) return global.StartCoroutine(routine);
                return null;
            }

            /// <summary>
            /// Stops a coroutine from non-MonoBehavior objects.
            /// </summary>
            /// <param name="routine">Routine.</param>
            internal static void EndCoroutineImpl(IEnumerator routine)
            {
                if (routine != null) global.StopCoroutine(routine);
            }

            /// <summary>
            /// Schedules the specifies action to be run on the main thread (game thread).
            /// The action will be invoked upon the next Unity Update event.
            /// Only works if initilization has done (<see cref="App.IsAppInitialized"/>).
            /// </summary>
            /// <param name="action">Action.</param>
            internal static void RunOnMainThreadImpl(Action action)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                if (!IsInitialized) return;

                lock (ToMainThreads)
                {
                    ToMainThreads.Add(action);
                    isToMainThreadQueueEmpty = false;
                }
            }

            /// <summary>
            /// Converts the specified action to one that runs on the main thread.
            /// The converted action will be invoked upon the next Unity Update event.
            /// Only works if initilization has done (<see cref="App.IsAppInitialized"/>).
            /// </summary>
            /// <returns>The main thread.</returns>
            /// <param name="action">Act.</param>
            internal static Action ToMainThreadImpl(Action action)
            {
                if (action == null) return delegate { };
                return () => RunOnMainThread(action);
            }

            /// <summary>
            /// Converts the specified action to one that runs on the main thread.
            /// The converted action will be invoked upon the next Unity Update event.
            /// Only works if initilization has done (<see cref="App.IsAppInitialized"/>).
            /// </summary>
            /// <returns>The main thread.</returns>
            /// <param name="action">Act.</param>
            /// <typeparam name="T">The 1st type parameter.</typeparam>
            internal static Action<T> ToMainThreadImpl<T>(Action<T> action)
            {
                if (action == null) return delegate { };
                return (arg) => RunOnMainThread(() => action(arg));
            }

            /// <summary>
            /// Converts the specified action to one that runs on the main thread.
            /// The converted action will be invoked upon the next Unity Update event.
            /// Only works if initilization has done (<see cref="App.IsAppInitialized"/>).
            /// </summary>
            /// <returns>The main thread.</returns>
            /// <param name="action">Act.</param>
            /// <typeparam name="T1">The 1st type parameter.</typeparam>
            /// <typeparam name="T2">The 2nd type parameter.</typeparam>
            internal static Action<T1, T2> ToMainThreadImpl<T1, T2>(Action<T1, T2> action)
            {
                if (action == null) return delegate { };
                return (arg1, arg2) => RunOnMainThread(() => action(arg1, arg2));
            }

            /// <summary>
            /// Converts the specified action to one that runs on the main thread.
            /// The converted action will be invoked upon the next Unity Update event.
            /// Only works if initilization has done (<see cref="App.IsAppInitialized"/>).
            /// </summary>
            /// <returns>The main thread.</returns>
            /// <param name="action">Act.</param>
            /// <typeparam name="T1">The 1st type parameter.</typeparam>
            /// <typeparam name="T2">The 2nd type parameter.</typeparam>
            /// <typeparam name="T3">The 3rd type parameter.</typeparam>
            internal static Action<T1, T2, T3> ToMainThreadImpl<T1, T2, T3>(Action<T1, T2, T3> action)
            {
                if (action == null) return delegate { };
                return (arg1, arg2, arg3) => RunOnMainThread(() => action(arg1, arg2, arg3));
            }

            /// <summary>
            /// add <paramref name="prefab"/> as a children of runtime
            /// </summary>
            /// <param name="prefab"></param>
            internal static void AttachImpl(AutoInitialize prefab)
            {
                var obj = Instantiate(prefab, global.transform, false);
                obj.Init();
            }

            #endregion
        }
    }
}