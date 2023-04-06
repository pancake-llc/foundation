using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Tween
{
    /// <summary>
    /// Extends the Tween by only public methods.
    /// </summary>
    public static class TweenExtensions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialization()
        {
            App.Add(UpdateMode.Update, TweenManager.Update);
            App.AddQuitCallback(TweenManager.DisposeAllNativeData);
        }

        /// <summary>
        /// Toggles the Tween state between Playing or Rewinding and Pausing.
        /// </summary>
        public static void TogglePause(this Tween tween)
        {
            if (tween.IsPaused())
            {
                tween.Pause(false);
            }
            else
            {
                tween.Pause(true);
            }
        }


        /// <summary>
        /// Sets the callback when the Tween starts by play.
        /// </summary>
        public static Tween OnStartByPlay(this Tween tween, Action onStartByPlay)
        {
            return tween.OnStart(() =>
            {
                if (tween.IsPlaying())
                {
                    onStartByPlay();
                }
            });
        }


        /// <summary>
        /// Sets the callback when the Tween starts by rewind.
        /// </summary>
        public static Tween OnStartByRewind(this Tween tween, Action onStartByRewind)
        {
            return tween.OnStart(() =>
            {
                if (tween.IsRewinding())
                {
                    onStartByRewind();
                }
            });
        }


        /// <summary>
        /// Sets the callback when the Tween stops by play.
        /// </summary>
        public static Tween OnStopByPlay(this Tween tween, Action onStopByPlay)
        {
            return tween.OnStop(() =>
            {
                if (tween.IsStoppedByPlay())
                {
                    onStopByPlay();
                }
            });
        }


        /// <summary>
        /// Sets the callback when the Tween stops by rewind.
        /// </summary>
        public static Tween OnStopByRewind(this Tween tween, Action onStopByRewind)
        {
            return tween.OnStop(() =>
            {
                if (tween.IsStoppedByRewind())
                {
                    onStopByRewind();
                }
            });
        }


        /// <summary>
        /// Sets the callback when the Tween completes by play.
        /// </summary>
        public static Tween OnCompleteByPlay(this Tween tween, Action onCompleteByPlay)
        {
            return tween.OnComplete(() =>
            {
                if (tween.IsCompletedByPlay())
                {
                    onCompleteByPlay();
                }
            });
        }


        /// <summary>
        /// Sets the callback when the Tween completes by rewind.
        /// </summary>
        public static Tween OnCompleteByRewind(this Tween tween, Action onCompleteByRewind)
        {
            return tween.OnComplete(() =>
            {
                if (tween.IsCompletedByRewind())
                {
                    onCompleteByRewind();
                }
            });
        }


        /// <summary>
        /// Sets the number of Tween play repeats.
        /// Repeated calls will add [loops] and [OnCompleteByLoops].
        /// 
        /// [loops]            : -1 means infinite loops.
        /// [isRewindToStart]  : Whether the Tween to rewinds when a loop is completed? — false to restart.
        /// [OnCompleteByLoops]: Callback when all loops are completed.
        /// </summary>
        public static Tween SetLoop(this Tween tween, int loops, bool isRewindToStart = false, Action onCompleteByLoops = null)
        {
            tween.AssertIsNotRecyclable("SetLoop");
            tween.AssertStateIsSetup("SetLoop");

            if (LoopsDataDict.TryGetValue(tween, out LoopsData loopsData) == false)
            {
                LoopsDataDict[tween] = new LoopsData {loops = loops, curLoop = loops, isRewindToStart = isRewindToStart, onCompleteByLoops = onCompleteByLoops};

                return tween.OnCompleteByPlay(() => LoopOnCompleteByPlay(tween))
                    .OnCompleteByRewind(() => LoopOnCompleteByRewind(tween))
                    .OnRecycle(() => LoopsDataDict.Remove(tween));
            }
            else
            {
                loopsData.loops += loops;
                loopsData.curLoop += loops;
                loopsData.isRewindToStart = isRewindToStart;
                loopsData.onCompleteByLoops += onCompleteByLoops;

                // just update loopsData
                return tween;
            }
        }


        #region Private Static Methods

        // binds LoopsData to Tween
        private static readonly Dictionary<Tween, LoopsData> LoopsDataDict = new Dictionary<Tween, LoopsData>(5);

        // records the loops data of the Tween
        private class LoopsData
        {
            public int loops;
            public int curLoop;
            public bool isRewindToStart;
            public Action onCompleteByLoops;
        }


        /// <summary>
        /// Callback when the loop completes by play.
        /// </summary>
        private static void LoopOnCompleteByPlay(Tween tween)
        {
            var loopsData = LoopsDataDict[tween];

            if (loopsData.curLoop != 0)
            {
                if (loopsData.isRewindToStart == false)
                {
                    tween.Restart();
                }
                else
                {
                    tween.Rewind();
                }

                // -1 means infinite loops
                if (loopsData.curLoop > 0)
                {
                    --loopsData.curLoop;
                }
            }
            else
            {
                loopsData.onCompleteByLoops?.Invoke();
                loopsData.curLoop = loopsData.loops;
            }
        }


        /// <summary>
        /// Callback when the loop completes by rewind.
        /// </summary>
        private static void LoopOnCompleteByRewind(Tween tween)
        {
            var loopsData = LoopsDataDict[tween];

            if (loopsData.curLoop < 0 || loopsData.loops != loopsData.curLoop)
            {
                tween.Play();
            }
        }

        #endregion


        #region Internal Static Methods For Editor

        /// <summary>
        /// Gets the loops and curLoop of Tween.
        /// </summary>
        internal static Vector2 GetTweenLoopCount(Tween tween)
        {
            if (LoopsDataDict.TryGetValue(tween, out LoopsData loopsData))
            {
                return new Vector2(loopsData.curLoop, loopsData.loops);
            }

            return Vector2.zero;
        }

        #endregion
    }
}