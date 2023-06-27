using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Pancake.Tween
{
    /// <summary>
    /// Note: starts the Tweens in reverse order of addition.
    /// </summary>
    public static class TweenManager
    {
        #region Private Fields

        /// <summary>
        /// The list of Tweens currently updating (Playing or Rewinding or Paused or Stopping).
        /// </summary>
        private static readonly List<Tween> TweenUpdateList = new List<Tween>(15);

        /// <summary>
        /// The list of TweenActions that wait for the jobs to complete to update targets.
        /// </summary>
        private static readonly List<TweenAction> TweenActionUpdateList = new List<TweenAction>(25);

        /// <summary>
        /// The list of JobHandles of TweenActions that need to be updated.
        /// </summary>
        private static readonly NativeList<JobHandle> JobHandleUpdateList = new NativeList<JobHandle>(25, Allocator.Persistent);

        #endregion


        #region Public Methods

        /// <summary>
        /// Is there any Tween updating?
        /// </summary>
        public static bool IsAnyUpdating() { return TweenUpdateList.Count > 0; }


        /// <summary>
        /// Stops all updating Tweens Playing or Rewinding.
        /// If the Tween is recyclable then it will be recycled on completion.
        /// </summary>
        public static void StopAll()
        {
            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                TweenUpdateList[i].Stop();
            }
        }


        /// <summary>
        /// Restarts all updating Tweens Playing or Rewinding.
        /// </summary>
        public static void RestartAll()
        {
            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                TweenUpdateList[i].Restart();
            }
        }


        /// <summary>
        /// Reverses all updating Tweens Playing or Rewinding.
        /// </summary>
        public static void ReverseAll()
        {
            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                TweenUpdateList[i].Reverse();
            }
        }


        /// <summary>
        /// Rewinds all updating Tweens Playing or Rewinding.
        /// </summary>
        public static void RewindAll()
        {
            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                TweenUpdateList[i].Rewind();
            }
        }


        /// <summary>
        /// Pauses or resumes all updating Tweens Playing or Rewinding.
        /// </summary>
        public static void PauseAll(bool isPause)
        {
            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                TweenUpdateList[i].Pause(isPause);
            }
        }


        /// <summary>
        /// Toggles all updating Tweens state between Playing or Rewinding and Paused.
        /// </summary>
        public static void TogglePauseAll()
        {
            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                TweenUpdateList[i].TogglePause();
            }
        }


        /// <summary>
        /// Sets all updating Tweens to recyclable.
        /// </summary>
        public static void SetRecyclableAll(bool isRecyclable)
        {
            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                TweenUpdateList[i].SetRecyclable(isRecyclable);
            }
        }


        /// <summary>
        /// Stops all updating Tweens and Recycles all unrecycled Tweens.
        /// </summary>
        public static void RecycleAll()
        {
            StopAll();
            Tween.RecycleUnrecycled();
        }


        /// <summary>
        /// Updates all Tweens, called every frame.
        /// </summary>
        public static void Update()
        {
            var deltaSeconds = Time.deltaTime;

            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                if (TweenUpdateList[i].UpdateActions(deltaSeconds) == false)
                {
                    TweenUpdateList.RemoveAtSwapBack(i);
                    continue;
                }
            }

            if (JobHandleUpdateList.Length > 0)
            {
                // complete all update jobs of TweenActionValues
                JobHandle.CompleteAll(JobHandleUpdateList.AsArray());
                JobHandleUpdateList.Clear();
            }

            var tweenActionUpdateListCount = TweenActionUpdateList.Count;
            if (tweenActionUpdateListCount > 0)
            {
                // update all targets by the results of update jobs
                for (var i = tweenActionUpdateListCount - 1; i > -1; --i)
                {
                    TweenActionUpdateList[i].UpdateTargetValues();
                }

                TweenActionUpdateList.Clear();
            }

#if !ENABLE_BURST_AOT
            the ENABLE_BURST_AOT is not enabled when this line error
            add it by [Edit] -> [Project Settting] -> [Player] ->
                      [Other Settings] -> [Script Compilation] -> [Script Define Symbols]
#endif
        }


        /// <summary>
        /// Disposes all native data with Allocator.Persistent, called when ApplicationQuit.
        /// 
        /// If not dispose the native data, calling the Finalize of DisposeSentinel by GC,
        /// will cause an editor error when app quit.
        /// </summary>
        public static void DisposeAllNativeData()
        {
            JobHandleUpdateList.Dispose();

            TweenAction.DisposeNativeDataFromCached();
            Tween.DisposeNativeDataFromCached();
            Tween.DisposeNativeDataFromUnrecycled();

            for (var i = TweenUpdateList.Count - 1; i > -1; --i)
            {
                TweenUpdateList[i].DisposeNativeData();
            }

            // if not clear then the TweensInfoMenu will visit the disposed native data
            TweenUpdateList.Clear();
        }

        #endregion


        #region Internal Methods

        /// <summary>
        /// Removes the Tween from the update list.
        /// </summary>
        internal static void RemoveTween(Tween tween) { TweenUpdateList.RemoveSwapBack(tween); }


        /// <summary>
        /// Adds the Tween to the update list.
        /// </summary>
        internal static void AddTween(Tween tween) { TweenUpdateList.Add(tween); }


        /// <summary>
        /// Adds the TweenAction and JobHandle to update list.
        /// </summary>
        internal static void AddActionAndJob(TweenAction action, in JobHandle jobHandle)
        {
            TweenActionUpdateList.Add(action);
            JobHandleUpdateList.Add(jobHandle);
        }

        #endregion


        #region Internal Methods For Editor

        /// <summary>
        /// Gets the tweenUpdateList.
        /// </summary>
        internal static List<Tween> GettweenUpdateList() { return TweenUpdateList; }

        #endregion
    }
}