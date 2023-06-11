using System;
using System.Collections;
using System.Collections.Generic;
using Pancake.Apex;

namespace Pancake
{
    using UnityEngine;

    [DisallowMultipleComponent]
    [HideMonoScript]
    [AddComponentMenu("")]
    public class GlobalComponent : MonoBehaviour
    {
        internal event Action FixedUpdateInternal;
        internal event Action WaitForFixedUpdateInternal;
        internal event Action UpdateInternal;
        internal event Action LateUpdateInternal;
        internal event Action WaitForEndOfFrameInternal;
        private readonly List<Action> _toMainThreads = new();
        private volatile bool _isToMainThreadQueueEmpty = true; // Flag indicating whether there's any action queued to be run on game thread.

        internal event Action<bool> OnGamePause;
        internal event Action<bool> OnGameFocus;
        internal event Action OnGameQuit;

        internal readonly List<ITickProcess> tickProcesses = new List<ITickProcess>(1024);
        internal readonly List<IFixedTickProcess> fixedTickProcesses = new List<IFixedTickProcess>(512);
        internal readonly List<ILateTickProcess> lateTickProcesses = new List<ILateTickProcess>(256);
        private List<Action> _localToMainThreads = new();

        #region delay handle

        private List<DelayHandle> _timers = new List<DelayHandle>();

        // buffer adding timers so we don't edit a collection during iteration
        private List<DelayHandle> _timersToAdd = new List<DelayHandle>();
        private int _fixedFrameCount;

        internal void RegisterDelayHandle(DelayHandle delayHandle) { _timersToAdd.Add(delayHandle); }

        internal void CancelAllDelayHandle()
        {
            foreach (var timer in _timers)
            {
                timer.Cancel();
            }

            _timers = new List<DelayHandle>();
            _timersToAdd = new List<DelayHandle>();
        }

        internal void PauseAllDelayHandle()
        {
            foreach (var timer in _timers)
            {
                timer.Pause();
            }
        }

        internal void ResumeAllDelayHandle()
        {
            foreach (var timer in _timers)
            {
                timer.Resume();
            }
        }

        private void UpdateAllDelayHandle()
        {
            if (_timersToAdd.Count > 0)
            {
                _timers.AddRange(_timersToAdd);
                _timersToAdd.Clear();
            }

            foreach (var timer in _timers)
            {
                timer.Update();
            }

            _timers.RemoveAll(t => t.IsDone);
        }

        #endregion

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
                    _fixedFrameCount += 1;
                    WaitForFixedUpdateInternal?.Invoke();
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
                }
                // ReSharper disable once IteratorNeverReturns
            }
        }

        private void Update()
        {
            for (var i = 0; i < tickProcesses.Count; i++)
            {
                tickProcesses[i]?.OnTick();
            }

            UpdateInternal?.Invoke();
            UpdateAllDelayHandle();

            if (_isToMainThreadQueueEmpty) return;
            _localToMainThreads.Clear();
            lock (_toMainThreads)
            {
                _localToMainThreads.AddRange(_toMainThreads);
                _toMainThreads.Clear();
                _isToMainThreadQueueEmpty = true;
            }

            for (int i = 0; i < _localToMainThreads.Count; i++)
            {
                _localToMainThreads[i].Invoke();
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < fixedTickProcesses.Count; i++)
            {
                fixedTickProcesses[i]?.OnFixedTick();
            }

            FixedUpdateInternal?.Invoke();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < lateTickProcesses.Count; i++)
            {
                lateTickProcesses[i]?.OnLateTick();
            }

            LateUpdateInternal?.Invoke();
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

        #region internal effective

        /// <summary>
        /// Starts a coroutine from non-MonoBehavior objects.
        /// </summary>
        /// <param name="routine">Routine.</param>
        internal Coroutine StartCoroutineImpl(IEnumerator routine)
        {
            if (routine != null) return StartCoroutine(routine);
            return null;
        }

        /// <summary>
        /// Stops a coroutine from non-MonoBehavior objects.
        /// </summary>
        /// <param name="routine">Routine.</param>
        internal void StopCoroutineImpl(IEnumerator routine)
        {
            if (routine != null) StopCoroutine(routine);
        }

        /// <summary>
        /// Schedules the specifies action to be run on the main thread (game thread).
        /// The action will be invoked upon the next Unity Update event.
        /// </summary>
        /// <param name="action">Action.</param>
        internal void RunOnMainThreadImpl(Action action)
        {
            lock (_toMainThreads)
            {
                _toMainThreads.Add(action);
                _isToMainThreadQueueEmpty = false;
            }
        }

        /// <summary>
        /// Converts the specified action to one that runs on the main thread.
        /// The converted action will be invoked upon the next Unity Update event.
        /// </summary>
        /// <returns>The main thread.</returns>
        /// <param name="action">Act.</param>
        internal Action ToMainThreadImpl(Action action)
        {
            if (action == null) return delegate { };
            return () => RunOnMainThreadImpl(action);
        }

        /// <summary>
        /// Converts the specified action to one that runs on the main thread.
        /// The converted action will be invoked upon the next Unity Update event.
        /// </summary>
        /// <returns>The main thread.</returns>
        /// <param name="action">Act.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        internal Action<T> ToMainThreadImpl<T>(Action<T> action)
        {
            if (action == null) return delegate { };
            return (arg) => RunOnMainThreadImpl(() => action(arg));
        }

        /// <summary>
        /// Converts the specified action to one that runs on the main thread.
        /// The converted action will be invoked upon the next Unity Update event.
        /// </summary>
        /// <returns>The main thread.</returns>
        /// <param name="action">Act.</param>
        /// <typeparam name="T1">The 1st type parameter.</typeparam>
        /// <typeparam name="T2">The 2nd type parameter.</typeparam>
        internal Action<T1, T2> ToMainThreadImpl<T1, T2>(Action<T1, T2> action)
        {
            if (action == null) return delegate { };
            return (arg1, arg2) => RunOnMainThreadImpl(() => action(arg1, arg2));
        }

        /// <summary>
        /// Converts the specified action to one that runs on the main thread.
        /// The converted action will be invoked upon the next Unity Update event.
        /// </summary>
        /// <returns>The main thread.</returns>
        /// <param name="action">Act.</param>
        /// <typeparam name="T1">The 1st type parameter.</typeparam>
        /// <typeparam name="T2">The 2nd type parameter.</typeparam>
        /// <typeparam name="T3">The 3rd type parameter.</typeparam>
        internal Action<T1, T2, T3> ToMainThreadImpl<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            if (action == null) return delegate { };
            return (arg1, arg2, arg3) => RunOnMainThreadImpl(() => action(arg1, arg2, arg3));
        }

        #endregion
    }
}