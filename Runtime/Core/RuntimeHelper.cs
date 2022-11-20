using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// This class contains helper methods for use in runtime. Once initialized, this
    /// is attached to a game object that persists across scenes and allows
    /// accessing MonoBehaviour methods from any classes.
    /// </summary>
    [AddComponentMenu("")]
    internal class RuntimeHelper : MonoBehaviour
    {
        /// <summary>
        /// The singleton instance of this class. Upon the first access, a new game object
        /// attached with this class instance will be created if none exists. Therefore the first
        /// access to this property should always be made from the main thread. In general, we should
        /// always access this instace from the main thread, unless we're certain that a game object
        /// has been created before
        /// </summary>
        /// <value>The instance.</value>
        public static RuntimeHelper Instance { get; private set; }

        // List of actions to run on the game thread
        private static List<Action> toMainThreadQueue = new List<Action>();

        // Member variable used to copy actions from mToMainThreadQueue and
        // execute them on the game thread.
        private List<Action> _localToMainThreadQueue = new List<Action>();

        // Flag indicating whether there's any action queued to be run on game thread.
        private static volatile bool isToMainThreadQueueEmpty = true;

        // List of actions to be invoked upon application pause event.
        private static List<Action<bool>> pauseCallbackQueue = new List<Action<bool>>();

        // List of actions to be invoked upon application focus event.
        private static List<Action<bool>> focusCallbackQueue = new List<Action<bool>>();

        // List of action to be invoked upon application quit event
        private static List<Action> quitCallbackQueue = new List<Action>();

        #region Public API

        /// <summary>
        /// Determines if a game object attached with this class singleton instance exists.
        /// 
        /// </summary>
        /// <returns><c>true</c> if is initialized; otherwise, <c>false</c>.</returns>
        public static bool IsInitialized => Instance != null;

        /// <summary>
        /// Gets the (roughly accurate) app installation timestamp in local timezone. 
        /// This timestamp is recorded when the app is initalized for the first time.
        /// If this method is called before that, Epoch time (01/01/1970) will be returned since
        /// no value was stored.
        /// </summary>
        /// <returns>The app installation time in local timezone.</returns>
        public static DateTime GetAppInstallationTime => RuntimeManager.GetAppInstallationTimestamp;

        /// <summary>
        /// Starts a coroutine from non-MonoBehavior objects.
        /// </summary>
        /// <param name="routine">Routine.</param>
        public static Coroutine RunCoroutine(IEnumerator routine)
        {
            if (routine != null) return Instance.StartCoroutine(routine);
            return null;
        }

        /// <summary>
        /// Stops a coroutine from non-MonoBehavior objects.
        /// </summary>
        /// <param name="routine">Routine.</param>
        public static void EndCoroutine(IEnumerator routine)
        {
            if (routine != null)
                Instance.StopCoroutine(routine);
        }

        /// <summary>
        /// Converts the specified action to one that runs on the main thread.
        /// The converted action will be invoked upon the next Unity Update event.
        /// Only works if initilization has done (<see cref="RuntimeManager.Init"/>).
        /// </summary>
        /// <returns>The main thread.</returns>
        /// <param name="act">Act.</param>
        public static Action ToMainThread(Action act)
        {
            if (act == null)
            {
                return delegate { };
            }

            return () => RunOnMainThread(() => act());
        }

        /// <summary>
        /// Converts the specified action to one that runs on the main thread.
        /// The converted action will be invoked upon the next Unity Update event.
        /// Only works if initilization has done (<see cref="RuntimeManager.Init"/>).
        /// </summary>
        /// <returns>The main thread.</returns>
        /// <param name="act">Act.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static Action<T> ToMainThread<T>(Action<T> act)
        {
            if (act == null)
            {
                return delegate { };
            }

            return (arg) => RunOnMainThread(() => act(arg));
        }

        /// <summary>
        /// Converts the specified action to one that runs on the main thread.
        /// The converted action will be invoked upon the next Unity Update event.
        /// Only works if initilization has done (<see cref="RuntimeManager.Init"/>).
        /// </summary>
        /// <returns>The main thread.</returns>
        /// <param name="act">Act.</param>
        /// <typeparam name="T1">The 1st type parameter.</typeparam>
        /// <typeparam name="T2">The 2nd type parameter.</typeparam>
        public static Action<T1, T2> ToMainThread<T1, T2>(Action<T1, T2> act)
        {
            if (act == null)
            {
                return delegate { };
            }

            return (arg1, arg2) => RunOnMainThread(() => act(arg1, arg2));
        }

        /// <summary>
        /// Converts the specified action to one that runs on the main thread.
        /// The converted action will be invoked upon the next Unity Update event.
        /// Only works if initilization has done (<see cref="RuntimeManager.Init"/>).
        /// </summary>
        /// <returns>The main thread.</returns>
        /// <param name="act">Act.</param>
        /// <typeparam name="T1">The 1st type parameter.</typeparam>
        /// <typeparam name="T2">The 2nd type parameter.</typeparam>
        /// <typeparam name="T3">The 3rd type parameter.</typeparam>
        public static Action<T1, T2, T3> ToMainThread<T1, T2, T3>(Action<T1, T2, T3> act)
        {
            if (act == null)
            {
                return delegate { };
            }

            return (arg1, arg2, arg3) => RunOnMainThread(() => act(arg1, arg2, arg3));
        }

        /// <summary>
        /// Schedules the specifies action to be run on the main thread (game thread).
        /// The action will be invoked upon the next Unity Update event.
        /// Only works if initilization has done (<see cref="RuntimeManager.Init"/>).
        /// </summary>
        /// <param name="action">Action.</param>
        public static void RunOnMainThread(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Note that this requires the singleton game object to be created first (for Update() to run).
            if (!IsInitialized)
            {
                Debug.LogError("Using RunOnMainThread without initializing Helper.");
                return;
            }

            lock (toMainThreadQueue)
            {
                toMainThreadQueue.Add(action);
                isToMainThreadQueueEmpty = false;
            }
        }

        /// <summary>
        /// Adds a callback that is invoked upon the Unity event OnApplicationFocus.
        /// Only works if initilization has done (<see cref="RuntimeManager.Init"/>).
        /// </summary>
        /// <see cref="OnApplicationFocus"/>
        /// <param name="callback">Callback.</param>
        public static void AddFocusCallback(Action<bool> callback)
        {
            if (!focusCallbackQueue.Contains(callback))
            {
                focusCallbackQueue.Add(callback);
            }
        }

        /// <summary>
        /// Removes the callback from the list to call upon OnApplicationFocus event.
        /// is called.
        /// </summary>
        /// <returns><c>true</c>, if focus callback was removed, <c>false</c> otherwise.</returns>
        /// <param name="callback">Callback.</param>
        public static bool RemoveFocusCallback(Action<bool> callback) { return focusCallbackQueue.Remove(callback); }

        /// <summary>
        /// Adds a callback that is invoked upon the Unity event OnApplicationPause.
        /// Only works if initilization has done (<see cref="RuntimeManager.Init"/>).
        /// </summary>
        /// <see cref="OnApplicationPause"/>
        /// <param name="callback">Callback.</param>
        public static void AddPauseCallback(Action<bool> callback)
        {
            if (!pauseCallbackQueue.Contains(callback))
            {
                pauseCallbackQueue.Add(callback);
            }
        }

        /// <summary>
        /// Removes the callback from the list to invoke upon OnApplicationPause event.
        /// is called.
        /// </summary>
        /// <returns><c>true</c>, if focus callback was removed, <c>false</c> otherwise.</returns>
        /// <param name="callback">Callback.</param>
        public static bool RemovePauseCallback(Action<bool> callback) { return pauseCallbackQueue.Remove(callback); }

        /// <summary>
        /// Removes the callback from the list to invoke upon OnApplicationQuit event.
        /// is called.
        /// </summary>
        /// <returns><c>true</c>, if focus callback was removed, <c>false</c> otherwise.</returns>
        /// <param name="callback">Callback.</param>
        public static bool RemoveQuitCallback(Action callback) { return quitCallbackQueue.Remove(callback); }

        /// <summary>
        /// Adds a callback that is invoked upon the Unity event OnApplicationQuit.
        /// Only works if initilization has done (<see cref="RuntimeManager.Init"/>).
        /// </summary>
        /// <see cref="OnApplicationQuit"/>
        /// <param name="callback">Callback.</param>
        public static void AddQuitCallback(Action callback)
        {
            if (!quitCallbackQueue.Contains(callback))
            {
                quitCallbackQueue.Add(callback);
            }
        }

        /// <summary>
        /// Gets the key associated with the specified value in the given dictionary.
        /// </summary>
        /// <returns>The key for value.</returns>
        /// <param name="dict">Dict.</param>
        /// <param name="val">Value.</param>
        /// <typeparam name="TKey">The 1st type parameter.</typeparam>
        /// <typeparam name="TVal">The 2nd type parameter.</typeparam>
        public static TKey GetKeyForValue<TKey, TVal>(IDictionary<TKey, TVal> dict, TVal val)
        {
            foreach (KeyValuePair<TKey, TVal> entry in dict)
            {
                if (entry.Value.Equals(val))
                {
                    return entry.Key;
                }
            }

            return default(TKey);
        }

        #endregion // Public API

        #region Internal Stuff

        // Destroys the proxy game object that carries the instance of this class if one exists.
        // ReSharper disable once UnusedMember.Local
        private static void DestroyProxy()
        {
            if (!IsInitialized) return;

            if (!isToMainThreadQueueEmpty || pauseCallbackQueue.Count > 0 || focusCallbackQueue.Count > 0) return;

            Destroy(Instance.gameObject);
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (isToMainThreadQueueEmpty) return;

            // Copy the shared queue into a local queue while
            // preventing other threads to modify it.
            _localToMainThreadQueue.Clear();
            lock (toMainThreadQueue)
            {
                _localToMainThreadQueue.AddRange(toMainThreadQueue);
                toMainThreadQueue.Clear();
                isToMainThreadQueueEmpty = true;
            }

            // Execute queued actions (from local queue).
            for (int i = 0; i < _localToMainThreadQueue.Count; i++)
            {
                _localToMainThreadQueue[i].Invoke();
            }
        }

        /// <summary>
        /// Called when the gamme loses or gains focus. 
        /// </summary>
        /// <param name="focused"><c>true</c> if the gameobject has focus, else <c>false</c>.</param>
        /// <remarks>
        /// On Windows Store Apps and Windows Phone 8.1 there's no application quit event,
        /// consider using OnApplicationFocus event when hasFocus equals false.
        /// </remarks>
        private void OnApplicationFocus(bool focused)
        {
            for (int i = 0; i < focusCallbackQueue.Count; i++)
            {
                var act = focusCallbackQueue[i];
                try
                {
                    act(focused);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception executing action in OnApplicationFocus:" + e.Message + "\n" + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Called when the application pauses.
        /// </summary>
        /// <param name="paused"><c>true</c> if the application is paused, else <c>false</c>.</param>
        private void OnApplicationPause(bool paused)
        {
            for (int i = 0; i < pauseCallbackQueue.Count; i++)
            {
                var act = pauseCallbackQueue[i];
                try
                {
                    act(paused);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception executing action in OnApplicationPause:" + e.Message + "\n" + e.StackTrace);
                }
            }
        }

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
        private void OnApplicationQuit()
        {
            for (int i = 0; i < quitCallbackQueue.Count; i++)
            {
                var act = quitCallbackQueue[i];
                try
                {
                    act();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception executing action in OnApplicationQuit:" + e.Message + "\n" + e.StackTrace);
                }
            }
        }

        #endregion
    }
}