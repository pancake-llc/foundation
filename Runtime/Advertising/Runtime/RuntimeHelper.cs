using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeTypeMemberModifiers
namespace Pancake.Monetization
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
        public static RuntimeHelper Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Init();
                }

                return mInstance;
            }
        }

        // The singleton instance.
        private static RuntimeHelper mInstance;

        // List of actions to run on the game thread
        private static List<Action> mToMainThreadQueue = new List<Action>();

        // Member variable used to copy actions from mToMainThreadQueue and
        // execute them on the game thread.
        List<Action> localToMainThreadQueue = new List<Action>();

        // Flag indicating whether there's any action queued to be run on game thread.
        private static volatile bool mIsToMainThreadQueueEmpty = true;

        // List of actions to be invoked upon application pause event.
        private static List<Action<bool>> mPauseCallbackQueue = new List<Action<bool>>();

        // List of actions to be invoked upon application focus event.
        private static List<Action<bool>> mFocusCallbackQueue = new List<Action<bool>>();

        // Flag indicating whether this is a dummy instance.
        private static bool mIsDummy;

        #region Public API

        /// <summary>
        /// Creates the singleton instance of this class and a game object that carries it.
        /// This must be called once from the main thread.
        /// You can call it before accessing the <see cref="Instance"/> singleton,
        /// though <see cref="Instance"/> automatically calls this method if needed, so you can bypass this
        /// and access that property directly, provided that you're on the main thread.
        /// Also note that this method does nothing if initialization has been done before,
        /// so it's safe to call it multiple times.
        /// </summary>
        public static void Init()
        {
            if (mInstance != null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                var go = new GameObject("RuntimeHelper") {hideFlags = HideFlags.HideAndDontSave};
                mInstance = go.AddComponent<RuntimeHelper>();
                DontDestroyOnLoad(go);
            }
            else
            {
                // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
                mInstance = new RuntimeHelper();
                mIsDummy = true;
            }
        }

        /// <summary>
        /// Internally calls <see cref="Init"/>. Basically we're just giving
        /// the method another name to make things clearer. 
        /// </summary>
        public static void InitIfNeeded() { Init(); }

        /// <summary>
        /// Determines if a game object attached with this class singleton instance exists.
        /// 
        /// </summary>
        /// <returns><c>true</c> if is initialized; otherwise, <c>false</c>.</returns>
        public static bool IsInitialized() { return mInstance != null; }

        /// <summary>
        /// Gets the (roughly accurate) app installation timestamp in local timezone. 
        /// This timestamp is recorded when the app is initalized for the first time.
        /// If this method is called before that, Epoch time (01/01/1970) will be returned since
        /// no value was stored.
        /// </summary>
        /// <returns>The app installation time in local timezone.</returns>
        public static DateTime GetAppInstallationTime() { return RuntimeManager.GetAppInstallationTimestamp(); }

        /// <summary>
        /// Starts a coroutine from non-MonoBehavior objects.
        /// </summary>
        /// <param name="routine">Routine.</param>
        public static Coroutine RunCoroutine(IEnumerator routine)
        {
            if (routine != null)
                return Instance.StartCoroutine(routine);
            else
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
        /// Only works if initilization has done (<see cref="Init"/>).
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
        /// Only works if initilization has done (<see cref="Init"/>).
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
        /// Only works if initilization has done (<see cref="Init"/>).
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
        /// Only works if initilization has done (<see cref="Init"/>).
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
        /// Only works if initilization has done (<see cref="Init"/>).
        /// </summary>
        /// <param name="action">Action.</param>
        public static void RunOnMainThread(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (mIsDummy)
            {
                return;
            }

            // Note that this requires the singleton game object to be created first (for Update() to run).
            if (!IsInitialized())
            {
                Debug.LogError("Using RunOnMainThread without initializing Helper.");
                return;
            }

            lock (mToMainThreadQueue)
            {
                mToMainThreadQueue.Add(action);
                mIsToMainThreadQueueEmpty = false;
            }
        }

        /// <summary>
        /// Adds a callback that is invoked upon the Unity event OnApplicationFocus.
        /// Only works if initilization has done (<see cref="Init"/>).
        /// </summary>
        /// <see cref="OnApplicationFocus"/>
        /// <param name="callback">Callback.</param>
        public static void AddFocusCallback(Action<bool> callback)
        {
            if (!mFocusCallbackQueue.Contains(callback))
            {
                mFocusCallbackQueue.Add(callback);
            }
        }

        /// <summary>
        /// Removes the callback from the list to call upon OnApplicationFocus event.
        /// is called.
        /// </summary>
        /// <returns><c>true</c>, if focus callback was removed, <c>false</c> otherwise.</returns>
        /// <param name="callback">Callback.</param>
        public static bool RemoveFocusCallback(Action<bool> callback) { return mFocusCallbackQueue.Remove(callback); }

        /// <summary>
        /// Adds a callback that is invoked upon the Unity event OnApplicationPause.
        /// Only works if initilization has done (<see cref="Init"/>).
        /// </summary>
        /// <see cref="OnApplicationPause"/>
        /// <param name="callback">Callback.</param>
        public static void AddPauseCallback(Action<bool> callback)
        {
            if (!mPauseCallbackQueue.Contains(callback))
            {
                mPauseCallbackQueue.Add(callback);
            }
        }

        /// <summary>
        /// Removes the callback from the list to invoke upon OnApplicationPause event.
        /// is called.
        /// </summary>
        /// <returns><c>true</c>, if focus callback was removed, <c>false</c> otherwise.</returns>
        /// <param name="callback">Callback.</param>
        public static bool RemovePauseCallback(Action<bool> callback) { return mPauseCallbackQueue.Remove(callback); }

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
        static void DestroyProxy()
        {
            if (mInstance == null)
                return;

            if (!mIsToMainThreadQueueEmpty || mPauseCallbackQueue.Count > 0 || mFocusCallbackQueue.Count > 0)
                return;

            if (!mIsDummy)
                Destroy(mInstance.gameObject);

            mInstance = null;
        }

        void Awake() { DontDestroyOnLoad(gameObject); }

        void OnDisable()
        {
            if (mInstance == this)
            {
                mInstance = null;
            }
        }

        void Update()
        {
            if (mIsDummy || mIsToMainThreadQueueEmpty)
            {
                return;
            }

            // Copy the shared queue into a local queue while
            // preventing other threads to modify it.
            localToMainThreadQueue.Clear();
            lock (mToMainThreadQueue)
            {
                localToMainThreadQueue.AddRange(mToMainThreadQueue);
                mToMainThreadQueue.Clear();
                mIsToMainThreadQueueEmpty = true;
            }

            // Execute queued actions (from local queue).
            for (int i = 0; i < localToMainThreadQueue.Count; i++)
            {
                localToMainThreadQueue[i].Invoke();
            }
        }

        void OnApplicationFocus(bool focused)
        {
            for (int i = 0; i < mFocusCallbackQueue.Count; i++)
            {
                var act = mFocusCallbackQueue[i];
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

        void OnApplicationPause(bool paused)
        {
            for (int i = 0; i < mPauseCallbackQueue.Count; i++)
            {
                var act = mPauseCallbackQueue[i];
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

        #endregion // Internal Stuff
    }
}