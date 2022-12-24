using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public struct Runtime
    {
        public static event Action OnFixedTick;
        public static event Action OnWaitForFixedTick;
        public static event Action OnTick;
        public static event Action OnLateTick;
        public static event Action OnWaitForEndOfFrame;

        public static event Action OnFixedTickOnceTime;
        public static event Action OnWaitForFixedTickOnceTime;
        public static event Action OnTickOnceTime;
        public static event Action OnLateTickOnceTime;
        public static event Action OnWaitForEndOfFrameOnceTime;

        public static event Action OnEditorTick; // An update callback can be used in edit-mode
        public static event Action OnEditorTickOnceTime; // An update callback (will be executed once) can be used in edit-mode

        public static List<Action> toMainThreads = new();
        private static volatile bool isToMainThreadQueueEmpty = true; // Flag indicating whether there's any action queued to be run on game thread.

        public static event Action<bool> OnPause;
        public static event Action<bool> OnFocus;
        public static event Action OnQuit;

        public static readonly DateTime UnixEpoch = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
        private const string APP_INSTALLATION_TIMESTAMP_PPKEY = "APP_INSTALLATION_TIMESTAMP";

        private static float UnitedDeltaTime
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return Editortime.UnscaledDeltaTime * Time.timeScale;
                return Time.deltaTime;
#else
                return Time.deltaTime;
#endif
            }
        }

        private static float UnitedUnscaledDeltaTime
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return Editortime.UnscaledDeltaTime;
                return Time.unscaledDeltaTime;
#else
                return Time.unscaledDeltaTime;
#endif
            }
        }

        public static bool IsRuntimeInitialized { get; private set; }

        public static int FixedFrameCount { get; private set; }

        /// <summary>
        /// Gets the installation timestamp of this app in local timezone.
        /// This timestamp is recorded when the app is initialized for
        /// the first time so it's not really precise but can serve well as a rough approximation
        /// provided that the initialization is done soon after app launch.
        /// </summary>
        /// <returns>The installation timestamp.</returns>
        public static DateTime GetAppInstallTimestamp => Storage.GetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, UnixEpoch);

        public static float GetDeltaTime(TimeMode mode) => mode == TimeMode.Normal ? UnitedDeltaTime : UnitedUnscaledDeltaTime;

        public static void Add(UpdateMode mode, Action action)
        {
            switch (mode)
            {
                case UpdateMode.FixedUpdate:
                    OnFixedTick += action;
                    return;
                case UpdateMode.WaitForFixedUpdate:
                    OnWaitForFixedTick += action;
                    return;
                case UpdateMode.Update:
                    OnTick += action;
                    return;
                case UpdateMode.LateUpdate:
                    OnLateTick += action;
                    return;
                case UpdateMode.WaitForEndOfFrame:
                    OnWaitForEndOfFrame += action;
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
                    OnFixedTick -= action;
                    return;
                case UpdateMode.WaitForFixedUpdate:
                    OnWaitForFixedTick -= action;
                    return;
                case UpdateMode.Update:
                    OnTick -= action;
                    return;
                case UpdateMode.LateUpdate:
                    OnLateTick -= action;
                    return;
                case UpdateMode.WaitForEndOfFrame:
                    OnWaitForEndOfFrame -= action;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static void AddPauseCallback(Action<bool> callback)
        {
            OnPause -= callback;
            OnPause += callback;
        }

        public static void RemovePauseCallback(Action<bool> callback) { OnPause -= callback; }

        public static void AddFocusCallback(Action<bool> callback)
        {
            OnFocus -= callback;
            OnFocus += callback;
        }

        public static void RemoveFocusCallback(Action<bool> callback) { OnFocus -= callback; }

        public static void AddQuitCallback(Action callback)
        {
            OnQuit -= callback;
            OnQuit += callback;
        }

        public static void RemoveQuitCallback(Action callback) { OnQuit -= callback; }

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (IsRuntimeInitialized) return;

            if (Application.isPlaying)
            {
                var runtime = new GameObject("Runtime") {hideFlags = HideFlags.HideInHierarchy};
                runtime.AddComponent<GlobalComponent>();
                UnityEngine.Object.DontDestroyOnLoad(runtime);

                Data.Init();
                DeviceLogTracking.Init();

                if (Monetization.AdSettings.RuntimeAutoInitialize) runtime.AddComponent<Monetization.Advertising>();
#if PANCAKE_INPUTSYSTEM
                runtime.AddComponent<Pancake.MyInput>();
#endif
#if PANCAKE_IRONSOURCE_ENABLE
                runtime.AddComponent<Monetization.IronSourceStateHandler>();
#endif

                // Store the timestamp of the *first* init which can be used 
                // as a rough approximation of the installation time.
                if (Storage.GetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, UnixEpoch) == UnixEpoch) Storage.SetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, DateTime.Now);

                IsRuntimeInitialized = true;
                Debug.Log("Runtime has been initialized!");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            UnityEditor.EditorApplication.update += () =>
            {
                if (!Application.isPlaying)
                {
                    OnEditorTick?.Invoke();

                    if (OnEditorTickOnceTime != null)
                    {
                        var call = OnEditorTickOnceTime;
                        OnEditorTickOnceTime = null;
                        call();
                    }
                }
            };
        }
#endif

        public class GlobalComponent : MonoBehaviour
        {
            private static GlobalComponent global;

            private static bool IsInitialized => global != null;

            private List<Action> _localToMainThreads = new();

            private void Awake()
            {
                if (global != null) Destroy(this);
                else global = this;

                DontDestroyOnLoad(gameObject);
            }

            private void Start()
            {
                StartCoroutine(WaitForFixedTick());
                StartCoroutine(WaitForEndOfFrame());

                IEnumerator WaitForFixedTick()
                {
                    var wait = new WaitForFixedUpdate();
                    while (true)
                    {
                        yield return wait;
                        FixedFrameCount += 1;
                        OnWaitForFixedTick?.Invoke();

                        if (OnWaitForFixedTickOnceTime == null) continue;
                        var call = OnWaitForFixedTickOnceTime;
                        OnWaitForFixedTickOnceTime = null;
                        call();
                    }
                    // ReSharper disable once IteratorNeverReturns
                }

                IEnumerator WaitForEndOfFrame()
                {
                    var wait = new WaitForEndOfFrame();
                    while (true)
                    {
                        yield return wait;
                        OnWaitForEndOfFrame?.Invoke();

                        if (OnWaitForEndOfFrameOnceTime == null) continue;
                        var call = OnWaitForEndOfFrameOnceTime;
                        OnWaitForEndOfFrameOnceTime = null;
                        call();
                    }
                    // ReSharper disable once IteratorNeverReturns
                }
            }

            private void Update()
            {
                OnTick?.Invoke();
                OnEditorTick?.Invoke();

                if (OnTickOnceTime != null)
                {
                    var call = OnTickOnceTime;
                    OnTickOnceTime = null;
                    call();
                }

                if (OnEditorTickOnceTime != null)
                {
                    var call = OnEditorTickOnceTime;
                    OnEditorTickOnceTime = null;
                    call();
                }


                if (isToMainThreadQueueEmpty) return;
                _localToMainThreads.Clear();
                lock (toMainThreads)
                {
                    _localToMainThreads.AddRange(toMainThreads);
                    toMainThreads.Clear();
                    isToMainThreadQueueEmpty = true;
                }

                for (int i = 0; i < _localToMainThreads.Count; i++)
                {
                    _localToMainThreads[i].Invoke();
                }
            }

            private void FixedUpdate()
            {
                OnFixedTick?.Invoke();

                if (OnFixedTickOnceTime != null)
                {
                    var call = OnFixedTickOnceTime;
                    OnFixedTickOnceTime = null;
                    call();
                }
            }

            private void LateUpdate()
            {
                OnLateTick?.Invoke();

                if (OnLateTickOnceTime != null)
                {
                    var call = OnLateTickOnceTime;
                    OnLateTickOnceTime = null;
                    call();
                }
            }

            /// <summary>
            /// Called when the gamme loses or gains focus. 
            /// </summary>
            /// <param name="hasFocus"><c>true</c> if the gameobject has focus, else <c>false</c>.</param>
            /// <remarks>
            /// On Windows Store Apps and Windows Phone 8.1 there's no application quit event,
            /// consider using OnApplicationFocus event when hasFocus equals false.
            /// </remarks>
            private void OnApplicationFocus(bool hasFocus) { OnFocus?.Invoke(hasFocus); }

            /// <summary>
            /// Called when the application pauses.
            /// </summary>
            /// <param name="pauseStatus"><c>true</c> if the application is paused, else <c>false</c>.</param>
            private void OnApplicationPause(bool pauseStatus) { OnPause?.Invoke(pauseStatus); }

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
            private void OnApplicationQuit() { OnQuit?.Invoke(); }

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
            /// Only works if initilization has done (<see cref="Runtime.AutoInitialize"/>).
            /// </summary>
            /// <param name="action">Action.</param>
            internal static void RunOnMainThreadImpl(Action action)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                if (!IsInitialized)
                {
                    Debug.LogError("Using RunOnMainThread without initializing Runtime");
                    return;
                }

                lock (toMainThreads)
                {
                    toMainThreads.Add(action);
                    isToMainThreadQueueEmpty = false;
                }
            }

            /// <summary>
            /// Converts the specified action to one that runs on the main thread.
            /// The converted action will be invoked upon the next Unity Update event.
            /// Only works if initilization has done (<see cref="Runtime.AutoInitialize"/>).
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
            /// Only works if initilization has done (<see cref="Runtime.AutoInitialize"/>).
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
            /// Only works if initilization has done (<see cref="Runtime.AutoInitialize"/>).
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
            /// Only works if initilization has done (<see cref="Runtime.AutoInitialize"/>).
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

            #endregion
        }
    }

#if UNITY_EDITOR
    internal struct Editortime
    {
        private static float unscaledDeltaTime;
        private static double lastTimeSinceStartup = -0.01;

        [UnityEditor.InitializeOnLoadMethod]
        private static void Init()
        {
            UnityEditor.EditorApplication.update += () =>
            {
                unscaledDeltaTime = (float) (UnityEditor.EditorApplication.timeSinceStartup - lastTimeSinceStartup);
                lastTimeSinceStartup = UnityEditor.EditorApplication.timeSinceStartup;
            };
        }

        public static float UnscaledDeltaTime => unscaledDeltaTime;
    }
#endif
}