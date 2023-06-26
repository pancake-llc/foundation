using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Pancake.Tween
{
    /// <summary>
    /// Tween can play both queued and concurrent TweenActions.
    /// The queued play one by one.
    /// The concurrent play concurrently.
    /// 
    /// Note: starts in reverse order when TweenActions have the same start time.
    /// </summary>
    public class Tween
    {
        /// <summary>
        /// The timeline duration.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// Whether the Tween auto recycled after it is completed, default true by Create.
        /// </summary>
        public bool IsRecyclable { get; private set; }


        #region Private Fields

        /// <summary>
        /// All TweenActions sorted by timelineStart.
        /// </summary>
        private TweenAction[] _startSortedActions;

        /// <summary>
        /// All TweenActions sorted by timelineEnd.
        /// </summary>
        private TweenAction[] _endSortedActions;

        /// <summary>
        /// Callback when the Tween (Play or Rewind) starts.
        /// </summary>
        private Action _onStart;

        /// <summary>
        /// Callback when the Tween (Play or Rewind) completes.
        /// </summary>
        private Action _onComplete;

        /// <summary>
        /// Callback when the Tween (Play or Rewind) stops.
        /// </summary>
        private Action _onStop;

        /// <summary>
        /// Callback when the Tween recycles.
        /// </summary>
        private Action _onRecycle;

        /// <summary>
        /// The duration of the TweenActions queue.
        /// </summary>
        private float _queueTime;

        /// <summary>
        /// The duration of the last TweenAction added to the concurrent array.
        /// </summary>
        private float _concurrentTime;

        /// <summary>
        /// The current timeline of the Tween.
        /// </summary>
        private float _curTimeline;

        /// <summary>
        /// The current time in duration, set by TweenManager.
        /// </summary>
        private float _curTime;

        /// <summary>
        /// The current state.
        /// </summary>
        private State _curState;

        /// <summary>
        /// The previous run state which is Playing or Rewinding.
        /// </summary>
        private State _preRunState;

        /// <summary>
        /// The current operation.
        /// </summary>
        private Operation _curOperation;

        /// <summary>
        /// The index of TweenAction wait to Play or Rwind.
        /// 
        /// If Playing,    forward  in startSortedActions.
        /// If Rewingding, backward in endSortedActions.
        /// If out of actionList means the Tween runs completed.
        /// </summary>
        private int _waitIndex;

        /// <summary>
        /// The default TweenEase for Add or Append TweenAction.
        /// </summary>
        private Ease _defaultEase = Ease.Smooth;

        /// <summary>
        /// The scale for timeline sort.
        /// </summary>
        private const int TIMELINE_SORT_SCALE = 10000;

        /// <summary>
        /// The default isRelative for Add or Append TweenAction.
        /// </summary>
        private bool _defaultIsRelative;

        /// <summary>
        /// The list of TweenActions currently Playing or Rewinding.
        /// These TweenActions are also in actionList.
        /// </summary>
        private readonly List<TweenAction> _actionUpdateList = new List<TweenAction>(8);

        /// <summary>
        /// All TweenActions including queued and concurrent.
        /// </summary>
        private readonly List<TweenAction> _actionList = new List<TweenAction>(8);

        /// <summary>
        /// The cached Tweens.
        /// </summary>
        private static readonly Stack<Tween> TweenCachedStack = new Stack<Tween>(8);

        /// <summary>
        /// The unrecycled Tweens.
        /// </summary>
        private static readonly List<Tween> TweenUnrecycledList = new List<Tween>(8);

        #endregion


        /// <summary>
        /// The Tween states.
        /// </summary>
        private enum State
        {
            /// <summary>
            /// Set the parameters.
            /// </summary>
            Setup,

            /// <summary>
            /// Playing forward.
            /// </summary>
            Playing,

            /// <summary>
            /// Playing backward.
            /// </summary>
            Rewinding,

            /// <summary>
            /// Play or Rewind is paused.
            /// </summary>
            Paused,

            /// <summary>
            /// Play or Rewind is Stopping, and the next state is Stopped.
            /// </summary>
            Stopping,

            /// <summary>
            /// Play or Rewind is stopped.
            /// </summary>
            Stopped,

            /// <summary>
            /// Play or Rewind is completed.
            /// </summary>
            Completed,

            /// <summary>
            /// Cached for reuse.
            /// </summary>
            Recycled,
        }


        /// <summary>
        /// The Tween operations.
        /// 
        /// Tip: can use it in callback to synchronously controls the other related Tweens.
        /// </summary>
        private enum Operation
        {
            /// <summary>
            /// Restarts the Tween (Play or Rewind).
            /// </summary>
            Restart,

            /// <summary>
            /// Goto the start of Tween (Play or Rewind).
            /// </summary>
            GotoStart,

            /// <summary>
            /// Goto the end of Tween (PLay or Rewind).
            /// </summary>
            GotoEnd,

            /// <summary>
            /// Calls the OnComplete.
            /// </summary>
            CallOnComplete,

            /// <summary>
            /// Calls the OnStop.
            /// </summary>
            CallOnStop,

            /// <summary>
            /// No operation.
            /// </summary>
            None,
        }


        /// <summary>
        /// Prevents new Tween.
        /// </summary>
        private Tween() { }


        #region Public Static Methods

        /// <summary>
        /// Creates a Tween.
        /// 
        /// If [isRecyclable] is true then the Tween will be auto recycled when it is completed
        /// — so don't hold a Tween and always create a new one.
        ///
        /// If [isRecyclable] is false then the Tween needs to be recycled manually by SetRecyclable
        /// — so the Tween can be Restart or Rewind.
        /// </summary>
        public static Tween Create(bool isRecyclable = true)
        {
            Tween tween;

            if (TweenCachedStack.Count > 0)
            {
                tween = TweenCachedStack.Pop();
            }
            else
            {
                tween = new Tween();
            }

            // these values same as new and cached

            tween._curState = State.Setup;
            tween._curOperation = Operation.None;
            tween.IsRecyclable = isRecyclable;
            tween._waitIndex = -1;

            if (isRecyclable == false)
            {
                TweenUnrecycledList.Add(tween);
            }

            return tween;
        }


        /// <summary>
        /// Plays the delay callback.
        /// </summary>
        public static Tween PlayDelayCallback(float delay, Action callback)
        {
            return Create()
                // delay in timeline, use one action
                .AddDelayCallback(delay, callback)
                // delay in action, use one action
                //.Add(TweenAction.CreateDelayCallback(delay, Callback))
                // delay in action, use two actions
                //.AppendInterval(delay).AppendCallback(Callback)
                .Play();
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Appends the TweenAction to the queue.
        /// </summary>
        public Tween Append(TweenAction action)
        {
            action.AssertCanBeAttached("in [Tween Append]");
            AssertStateIsNotRecycled("Append");
            AddQueueAction(action);

            return this;
        }


        /// <summary>
        /// Appends the interval time to the queue.
        /// </summary>
        public Tween AppendInterval(float interval)
        {
            AssertStateIsNotRecycled("AppendInterval");
            AddQueueAction(TweenAction.Create(null, null, interval));

            return this;
        }


        /// <summary>
        /// Appends the callback to the queue.
        /// </summary>
        public Tween AppendCallback(Action callback)
        {
            AssertStateIsNotRecycled("AppendCallback");
            AddQueueAction(TweenAction.CreateCallback(callback));

            return this;
        }


        /// <summary>
        /// Appends the interval time with callback to the queue.
        /// </summary>
        public Tween AppendIntervalCallback(float interval, Action callback)
        {
            AssertStateIsNotRecycled("AppendIntervalCallback");
            AddQueueAction(TweenAction.CreateDelayCallback(interval, callback));

            return this;
        }


        /// <summary>
        /// Adds the TweenAction to the concurrent array.
        /// </summary>
        public Tween Add(TweenAction action)
        {
            action.AssertCanBeAttached("in [Tween Add]");
            AssertStateIsNotRecycled("Add");
            AddConcurrentAction(0.0f, action);

            return this;
        }


        /// <summary>
        /// Adds the delay TweenAction to the concurrent array.
        /// </summary>
        public Tween AddDelay(float delay, TweenAction action)
        {
            action.AssertCanBeAttached("in [Tween AddDelay]");
            AssertStateIsNotRecycled("AddDelay");
            AddConcurrentAction(delay, action);

            return this;
        }


        /// <summary>
        /// Adds the delay TweenAction after the last Appended to the concurrent array.
        /// </summary>
        public Tween AddDelayAfterAppend(float delay, TweenAction action)
        {
            action.AssertCanBeAttached("in [Tween AddDelayAfterAppend]");
            AssertStateIsNotRecycled("AddDelayAfterAppend");
            AddConcurrentAction(_queueTime + delay, action);

            return this;
        }


        /// <summary>
        /// Adds the delay TweenAction after the last Added to the concurrent array.
        /// </summary>
        public Tween AddDelayAfterAdd(float delay, TweenAction action)
        {
            action.AssertCanBeAttached("in [Tween AddAfterAddDelay]");
            AssertStateIsNotRecycled("AddAfterAddDelay");
            AddConcurrentAction(_concurrentTime + delay, action);

            return this;
        }


        /// <summary>
        /// Adds the TweenAction after the last Appended to the concurrent array.
        /// </summary>
        public Tween AddAfterAppend(TweenAction action)
        {
            action.AssertCanBeAttached("in [Tween AddAfterAppend]");
            AssertStateIsNotRecycled("AddAfterAppend");
            AddConcurrentAction(_queueTime, action);

            return this;
        }


        /// <summary>
        /// Adds the TweenAction after the last Added to the concurrent array.
        /// </summary>
        public Tween AddAfterAdd(TweenAction action)
        {
            action.AssertCanBeAttached("in [Tween AddAfterAdd]");
            AssertStateIsNotRecycled("AddAfterAdd");
            AddConcurrentAction(_concurrentTime, action);

            return this;
        }


        /// <summary>
        /// Adds the callback to the concurrent array.
        /// </summary>
        public Tween AddCallback(Action callback)
        {
            AssertStateIsNotRecycled("AddCallback");
            AddConcurrentAction(0.0f, TweenAction.CreateCallback(callback));

            return this;
        }


        /// <summary>
        /// Adds the callback after the last Appended to the concurrent array.
        /// </summary>
        public Tween AddCallbackAfterAppend(Action callback)
        {
            AssertStateIsNotRecycled("AddCallbackAfterAppend");
            AddConcurrentAction(_queueTime, TweenAction.CreateCallback(callback));

            return this;
        }


        /// <summary>
        /// Adds the callback after the last Added to the concurrent array.
        /// </summary>
        public Tween AddCallbackAfterAdd(Action callback)
        {
            AssertStateIsNotRecycled("AddCallbackAfterAdd");
            AddConcurrentAction(_concurrentTime, TweenAction.CreateCallback(callback));

            return this;
        }


        /// <summary>
        /// Adds the delay callback to the concurrent array.
        /// </summary>
        public Tween AddDelayCallback(float delay, Action callback)
        {
            AssertStateIsNotRecycled("AddDelayCallback");
            AddConcurrentAction(delay, TweenAction.CreateCallback(callback));

            return this;
        }


        /// <summary>
        /// Adds the delay callback after the last Appended to the concurrent array.
        /// </summary>
        public Tween AddDelayCallbackAfterAppend(float delay, Action callback)
        {
            AssertStateIsNotRecycled("AddDelayCallbackAfterAppend");
            AddConcurrentAction(_queueTime + delay, TweenAction.CreateCallback(callback));

            return this;
        }


        /// <summary>
        /// Adds the delay callback after the last Added to the concurrent array.
        /// </summary>
        public Tween AddDelayCallbackAfterAdd(float delay, Action callback)
        {
            AssertStateIsNotRecycled("AddDelayCallbackAfterAdd");
            AddConcurrentAction(_concurrentTime + delay, TweenAction.CreateCallback(callback));

            return this;
        }


        /// <summary>
        /// Sets the [OnStart] callback which is called when the Tween starts (Play or Rewind).
        /// </summary>
        public Tween OnStart(Action onStart)
        {
            AssertStateIsNotRecycled("OnStart");
            _onStart += onStart;

            return this;
        }


        /// <summary>
        /// Sets the [OnComplete] callback which is called when the Tween completes (Play or Rewind).
        /// </summary>
        public Tween OnComplete(Action onComplete)
        {
            AssertStateIsNotRecycled("OnComplete");
            _onComplete += onComplete;

            return this;
        }


        /// <summary>
        /// Sets the [OnStop] callback which is called when the Tween stops (Play or Rewind).
        /// </summary>
        public Tween OnStop(Action onStop)
        {
            AssertStateIsNotRecycled("OnStop");
            _onStop += onStop;

            return this;
        }


        /// <summary>
        /// Sets the [OnRecycle] callback which is called when the Tween recycles.
        /// Tip: can use it to clear data bound to Tweens.
        /// </summary>
        public Tween OnRecycle(Action onRecycle)
        {
            AssertStateIsNotRecycled("OnRecycle");
            _onRecycle += onRecycle;

            return this;
        }


        /// <summary>
        /// Sets the [ease] of Add or Append TweenAction, default Smooth.
        /// Only sets the TweenAction whose [ease] is Smooth.
        /// </summary>
        public Tween SetEase(Ease ease)
        {
            AssertStateIsSetup("SetDefaultEase");
            _defaultEase = ease;

            return this;
        }


        /// <summary>
        /// Sets the [isRelative] of Add or Append TweenActions, default false.
        /// Only sets the TweenAction whose [isRelative] is false.
        /// </summary>
        public Tween SetRelative(bool isRelative)
        {
            AssertStateIsSetup("SetDefaultRelative");
            _defaultIsRelative = isRelative;

            return this;
        }


        /// <summary>
        /// Sets the Tween to recyclable.
        /// 
        /// If true and the Tween State is Setup or Completed or Stopped then recycle it immediately,
        /// else wait until it is completed and recycle it.
        /// </summary>
        public Tween SetRecyclable(bool isRecyclable)
        {
            if (IsRecyclable != isRecyclable)
            {
                IsRecyclable = isRecyclable;

                if (isRecyclable)
                {
                    RecycleIfNotUpdating();
                    TweenUnrecycledList.RemoveSwapBack(this);
                }
                else
                {
                    AssertStateIsNotRecycled("SetRecyclable(false)");
                    TweenUnrecycledList.Add(this);
                }
            }

            return this;
        }


        /// <summary>
        /// Plays the Tween.
        /// </summary>
        public Tween Play()
        {
            AssertStateCanRun("Play");

            switch (_curState)
            {
                case State.Setup:
                    GenerateStartSortedActions();
                    goto case State.Stopped;

                case State.Playing:
                    Debug.LogWarning("Tween Play, the state is already [Playing]!");
                    break;

                case State.Rewinding:
                    // reverse rewinding
                    Reverse();
                    break;

                case State.Paused:
                    Pause(false);
                    break;

                case State.Stopped:
                    _curState = State.Playing;
                    InitPlay();
                    TweenManager.AddTween(this);
                    break;

                case State.Completed:
                    if (_waitIndex == -1)
                    {
                        // rewind is Completed
                        goto case State.Stopped;
                    }
                    else if (_waitIndex == _startSortedActions.Length)
                    {
                        // play is Completed
                        Restart();
                    }

                    break;

                default:
                    WarnUnhandledState("Play");
                    break;
            }

            return this;
        }


        /// <summary>
        /// Rewinds the Tween.
        /// The Tween cannot be recyclable!
        /// </summary>
        public Tween Rewind()
        {
            AssertIsNotRecyclable("Rewind");
            AssertStateCanRun("Rewind");

            switch (_curState)
            {
                case State.Playing:
                    // reverse Playing
                    Reverse();
                    break;

                case State.Rewinding:
                    Debug.LogWarning("Tween Rewind, the state is already [Rewinding]!");
                    break;

                case State.Paused:
                    Pause(false);
                    break;

                case State.Setup:
                case State.Stopped:
                case State.Completed:
                    GenerateEndSortedActions();

                    if (
                        // play is Completed
                        _waitIndex == _endSortedActions.Length || _curState == State.Setup || _curState == State.Stopped)
                    {
                        _curState = State.Rewinding;
                        InitRewind();
                        TweenManager.AddTween(this);
                    }
                    else if (_waitIndex == -1)
                    {
                        // rewind is Completed
                        Restart();
                    }

                    break;

                default:
                    WarnUnhandledState("Rewind");
                    break;
            }

            return this;
        }


        /// <summary>
        /// Restarts the Tween (Play or Rewind).
        /// The Tween cannot be recyclable!
        /// </summary>
        public Tween Restart()
        {
            AssertIsNotRecyclable("Restart");
            AssertStateCanRun("Restart");
            _curOperation = Operation.Restart;

            switch (_curState)
            {
                case State.Setup:
                    return Play();

                case State.Playing:
                    ReversePlayedActions();
                    InitPlay();
                    break;

                case State.Rewinding:
                    ReverseRewindedActions();
                    InitRewind();
                    break;

                case State.Paused:
                    if (_preRunState == State.Playing)
                    {
                        _curState = State.Playing;
                        goto case State.Playing;
                    }
                    else if (_preRunState == State.Rewinding)
                    {
                        _curState = State.Rewinding;
                        goto case State.Rewinding;
                    }

                    break;

                case State.Stopped:
                case State.Completed:
                    // tween has been or will be (Restart in OnComplete or OnStop) removed from TweenManager
                    TweenManager.AddTween(this);
                    goto case State.Paused;

                default:
                    WarnUnhandledState("Restart");
                    break;
            }

            _curOperation = Operation.None;
            return this;
        }


        /// <summary>
        /// Goto the start of Tween (Play or Rewind).
        /// The Tween cannot be recyclable!
        /// </summary>
        public Tween GotoStart()
        {
            AssertIsNotRecyclable("GotoStart");
            AssertStateCanRun("GotoStart");
            _curOperation = Operation.GotoStart;

            switch (_curState)
            {
                case State.Playing:
                case State.Rewinding:
                    // goto start by curState
                    _preRunState = _curState;
                    goto case State.Paused;

                case State.Paused:
                    // handle Stopping by UpdateActions
                    _curState = State.Stopping;
                    goto case State.Stopping;

                case State.Stopped:
                case State.Completed:
                    // already removed or
                    // in OnComplete or OnStop
                    goto case State.Stopping;

                // can only goto here
                case State.Stopping:
                    if (_preRunState == State.Playing)
                    {
                        ReversePlayedActions();
                        // ready to play again
                        _waitIndex = -1;
                    }
                    else if (_preRunState == State.Rewinding)
                    {
                        ReverseRewindedActions();
                        // ready to rewind again
                        _waitIndex = _endSortedActions.Length;
                    }

                    break;

                default:
                    WarnUnhandledState("GotoStart");
                    break;
            }

            _curOperation = Operation.None;
            return this;
        }


        /// <summary>
        /// Goto the end of Tween (Play or Rewind).
        /// The Tween cannot be recyclable!
        /// </summary>
        public Tween GotoEnd()
        {
            AssertIsNotRecyclable("GotoEnd");
            AssertStateCanRun("GotoEnd");
            _curOperation = Operation.GotoEnd;

            switch (_curState)
            {
                case State.Playing:
                case State.Rewinding:
                    // goto end by curState
                    _preRunState = _curState;
                    goto case State.Paused;

                case State.Paused:
                    // handle Stopping by UpdateActions
                    _curState = State.Stopping;
                    goto case State.Stopping;

                case State.Stopped:
                case State.Completed:
                    // already removed or in OnStop or OnComplete 
                    goto case State.Stopping;

                // can only goto here
                case State.Stopping:
                    if (_preRunState == State.Playing)
                    {
                        for (var i = _actionUpdateList.Count - 1; i > -1; --i)
                        {
                            _actionUpdateList[i].CompletePlay();
                        }

                        var len = _startSortedActions.Length;

                        for (var i = _waitIndex > -1 ? _waitIndex : 0; i < len; ++i)
                        {
                            _startSortedActions[i].CompletePlay();
                        }

                        _waitIndex = len;
                    }
                    else if (_preRunState == State.Rewinding)
                    {
                        for (var i = _actionUpdateList.Count - 1; i > -1; --i)
                        {
                            _actionUpdateList[i].CompleteRewind();
                        }

                        var len = _endSortedActions.Length;

                        for (var i = _waitIndex < len ? _waitIndex : len - 1; i > -1; --i)
                        {
                            _endSortedActions[i].CompleteRewind();
                        }

                        _waitIndex = -1;
                    }

                    // the run actions have been completed
                    _actionUpdateList.Clear();
                    break;

                default:
                    WarnUnhandledState("GotoEnd");
                    break;
            }

            _curOperation = Operation.None;
            return this;
        }


        /// <summary>
        /// Reverses the Tween (Play or Rewind).
        /// 
        /// If Tween is Completed then reverse the previous Play or Rewind,
        /// else reverse the Playing or Rewinding.
        /// </summary>
        public Tween Reverse()
        {
            switch (_curState)
            {
                case State.Playing:
                    AssertIsNotRecyclable("Reverse [Playing]");
                    GenerateEndSortedActions();

                    if (_waitIndex != _startSortedActions.Length)
                    {
                        var waitAction = _startSortedActions[_waitIndex];

                        if (_waitIndex != 0)
                        {
                            // change timelineStart to timelineEnd
                            _curTimeline = waitAction.TimelineEnd;
                            // waitIndex in endSortedActions impossible to run
                            _waitIndex = Array.IndexOf(_endSortedActions, waitAction) - 1;

                            // find new waitIndex with previous waitIndex
                            while (_waitIndex > -1)
                            {
                                waitAction = _endSortedActions[_waitIndex];

                                // if timelineEnd >= curTime then the waitAction still in actionUpdateList
                                if (waitAction.TimelineEnd < _curTime)
                                {
                                    _curTimeline = waitAction.TimelineEnd;
                                    break;
                                }

                                --_waitIndex;
                            }
                        }
                        else
                        {
                            // the waitAction is first action to Play
                            // if count == 0 then the waitAction is not start
                            // else the waitAction is Playing  
                            if (_actionUpdateList.Count == 0)
                            {
                                waitAction.InitValues();
                                // if not add waitAction then the Tween will no change to stop
                                // so add the waitAction as the last to Rewind
                                _actionUpdateList.Add(waitAction);
                            }

                            // no actions in play list are waiting to Rewind
                            _waitIndex = -1;
                        }
                    }
                    else
                    {
                        // to startLen - 1
                        --_waitIndex;
                    }

                    _preRunState = State.Playing;
                    _curState = State.Rewinding;
                    break;

                case State.Rewinding:
                    GenerateStartSortedActions();

                    if (_waitIndex != -1)
                    {
                        var actionLen = _endSortedActions.Length;
                        var waitAction = _endSortedActions[_waitIndex];

                        if (_waitIndex != actionLen - 1)
                        {
                            // change timelineEnd to timelineStart
                            _curTimeline = waitAction.TimelineStart;
                            // waitIndex in startSortedActions impossible to run
                            _waitIndex = Array.IndexOf(_startSortedActions, waitAction) + 1;

                            // find new waitIndex with next waitIndex
                            while (_waitIndex < actionLen)
                            {
                                waitAction = _startSortedActions[_waitIndex];

                                // if timelineStart <= curTime then the waitAction still in actionUpdateList
                                if (waitAction.TimelineStart > _curTime)
                                {
                                    _curTimeline = waitAction.TimelineStart;
                                    break;
                                }

                                ++_waitIndex;
                            }
                        }
                        else
                        {
                            // the waitAction is last action to Rewind
                            // if count == 0 then the waitAction is not start
                            // else the waitAction is Rewinding 
                            if (_actionUpdateList.Count == 0)
                            {
                                // if not add waitAction then the Tween will no change to stop
                                // so add the waitAction as the first to Play
                                _actionUpdateList.Add(waitAction);
                            }

                            // no actions in rewind list are waiting to Play
                            _waitIndex = actionLen;
                        }
                    }
                    else
                    {
                        // to 0
                        ++_waitIndex;
                    }

                    _preRunState = State.Rewinding;
                    _curState = State.Playing;
                    break;

                case State.Paused:
                    if (_preRunState == State.Playing)
                    {
                        goto case State.Playing;
                    }
                    else if (_preRunState == State.Rewinding)
                    {
                        goto case State.Rewinding;
                    }

                    break;

                case State.Stopped:
                case State.Completed:
                    if (_preRunState == State.Playing)
                    {
                        AssertIsNotRecyclable($"Reverse [{_curState}]");
                        Rewind();
                    }
                    else if (_preRunState == State.Rewinding)
                    {
                        Play();
                    }

                    break;

                default:
                    WarnUnhandledState("Reverse");
                    break;
            }

            return this;
        }


        /// <summary>
        /// Stops the Tween Playing or Rewinding.
        /// If the Tween is recyclable then it will be recycled.
        /// </summary>
        public Tween Stop()
        {
            AssertStateCanRun("Stop");

            switch (_curState)
            {
                case State.Playing:
                case State.Rewinding:
                    _preRunState = _curState;
                    goto case State.Paused;

                case State.Paused:
                    // keep preRunState as before Pause
                    // handle Stopping by UpdateActions
                    _curState = State.Stopping;
                    _actionUpdateList.Clear();
                    break;

                default:
                    WarnUnhandledState("Stop");
                    break;
            }

            return this;
        }


        /// <summary>
        /// Pauses or resumes the Tween Playing or Rewinding.
        /// </summary>
        public void Pause(bool isPause)
        {
            if (isPause)
            {
                if (_curState != State.Paused)
                {
                    if (_curState == State.Playing || _curState == State.Rewinding)
                    {
                        _preRunState = _curState;
                        _curState = State.Paused;
                    }
                    else
                    {
                        Debug.LogWarning($"Tween Pause, the state [{_curState}] not be [Playing] or [Rewinding]!");
                    }
                }
                else
                {
                    Debug.LogWarning("Tween Pause, the state is already Paused!");
                }
            }
            else
            {
                if (_curState == State.Paused)
                {
                    _curState = _preRunState;
                }
                else
                {
                    Debug.LogWarning("Tween Pause, the state is already not Paused!");
                }
            }
        }


        /// <summary>
        /// Whether the Tween state is Setup?
        /// </summary>
        public bool IsSetup() { return _curState == State.Setup; }


        /// <summary>
        /// Whether the Tween state is Playing?
        /// </summary>
        public bool IsPlaying() { return _curState == State.Playing; }


        /// <summary>
        /// Whether the Tween state is Rewinding?
        /// </summary>
        public bool IsRewinding() { return _curState == State.Rewinding; }


        /// <summary>
        /// Whether the Tween state is Playing or Rewinding? 
        /// </summary>
        public bool IsRunning() { return _curState == State.Playing || _curState == State.Rewinding; }


        /// <summary>
        /// Whether the Tween state is Paused?
        /// </summary>
        public bool IsPaused() { return _curState == State.Paused; }


        /// <summary>
        /// Whether the Tween state is Stopping?
        /// </summary>
        public bool IsStopping() { return _curState == State.Stopping; }


        /// <summary>
        /// Whether the Tween state is Stopped?
        /// </summary>
        public bool IsStopped() { return _curState == State.Stopped; }


        /// <summary>
        /// Whether the Tween state is Stopped by play? 
        /// </summary>
        public bool IsStoppedByPlay() { return _curState == State.Stopped && _preRunState == State.Playing; }


        /// <summary>
        /// Whether the Tween state is Stopped by rewind?
        /// </summary>
        public bool IsStoppedByRewind() { return _curState == State.Stopped && _preRunState == State.Rewinding; }


        /// <summary>
        /// Whether the Tween state is Completed?
        /// </summary>
        public bool IsCompleted() { return _curState == State.Completed; }


        /// <summary>
        /// Whether the Tween state is Completed by play? 
        /// </summary>
        public bool IsCompletedByPlay() { return _curState == State.Completed && _preRunState == State.Playing; }


        /// <summary>
        /// Whether the Tween state is Completed by rewind?
        /// </summary>
        public bool IsCompletedByRewind() { return _curState == State.Completed && _preRunState == State.Rewinding; }


        /// <summary>
        /// Whether the Tween is Recycled?
        /// </summary>
        public bool IsRecycled() { return _curState == State.Recycled; }


        /// <summary>
        /// Whether the Tween operation is Restart?
        /// Uses in TweenAction callback.
        /// </summary>
        public bool IsOpRestart() { return _curOperation == Operation.Restart; }


        /// <summary>
        /// Whether the Tween operation is GotoStart?
        /// Uses in TweenAction callback.
        /// </summary>
        public bool IsOpGotoStart() { return _curOperation == Operation.GotoStart; }


        /// <summary>
        /// Whether the Tween operation is GotoEnd?
        /// Uses in TweenAction callback.
        /// </summary>
        public bool IsOpGotoEnd() { return _curOperation == Operation.GotoEnd; }

        #endregion


        #region Internal Methods

        /// <summary>
        /// Updates all TweenActions.
        /// If return false then the Tween is completed.
        /// </summary>
        internal bool UpdateActions(float deltaSeconds)
        {
            TweenAction waitAction;

            // add run actions and check the tween whether is completed
            switch (_curState)
            {
                case State.Playing:
                    if (_waitIndex < _startSortedActions.Length && _curTimeline <= _curTime)
                    {
                        // remove complete play actions
                        for (var j = _actionUpdateList.Count - 1; j > -1; --j)
                        {
                            if (_actionUpdateList[j].CheckPlayCompleted(_curTime))
                            {
                                _actionUpdateList.RemoveAtSwapBack(j);
                            }
                        }

                        // add waiting action 
                        _actionUpdateList.Add(_startSortedActions[_waitIndex].InitPlay());

                        while (true)
                        {
                            // get next wait action
                            if (++_waitIndex == _startSortedActions.Length)
                            {
                                break;
                            }

                            waitAction = _startSortedActions[_waitIndex];

                            if (waitAction.TimelineStart > _curTime)
                            {
                                // record the timeline of the waiting action
                                _curTimeline = waitAction.TimelineStart;
                                break;
                            }

                            // add runnable action 
                            _actionUpdateList.Add(waitAction.InitPlay());
                        }
                    }

                    // update all play actions
                    for (var j = _actionUpdateList.Count - 1; j > -1; --j)
                    {
                        if (_actionUpdateList[j].Play(_curTime) == false)
                        {
                            _actionUpdateList.RemoveAtSwapBack(j);

                            // check whether the Tween is completed
                            // TweenAction OnComplete may add new action
                            // so the Tween may still has action when j == 0
                            if (j == 0 && _curTime >= Duration)
                            {
                                goto Completed;
                            }
                        }
                    }

                    _curTime += deltaSeconds;
                    return true;

                case State.Rewinding:
                    if (_waitIndex > -1 && _curTimeline >= _curTime)
                    {
                        // remove complete rewind actions
                        for (var j = _actionUpdateList.Count - 1; j > -1; --j)
                        {
                            if (_actionUpdateList[j].CheckRewindCompleted(_curTime))
                            {
                                _actionUpdateList.RemoveAtSwapBack(j);
                            }
                        }

                        // add waiting action 
                        _actionUpdateList.Add(_endSortedActions[_waitIndex].InitRewind());

                        while (true)
                        {
                            // get next wait action
                            if (--_waitIndex == -1)
                            {
                                break;
                            }

                            waitAction = _endSortedActions[_waitIndex];

                            if (waitAction.TimelineEnd < _curTime)
                            {
                                // record the timeline of the waiting action
                                _curTimeline = waitAction.TimelineEnd;
                                break;
                            }

                            // add runnable action 
                            _actionUpdateList.Add(waitAction.InitRewind());
                        }
                    }

                    // update all rewind actions
                    for (var j = _actionUpdateList.Count - 1; j > -1; --j)
                    {
                        if (_actionUpdateList[j].Rewind(_curTime) == false)
                        {
                            _actionUpdateList.RemoveAtSwapBack(j);

                            // check whether the Tween is completed
                            // TweenAction OnComplete may add new action
                            // so the Tween may still has action when j == 0
                            if (j == 0 && _curTime <= 0.0f)
                            {
                                goto Completed;
                            }
                        }
                    }

                    _curTime -= deltaSeconds;
                    return true;

                case State.Paused:
                    return true;

                case State.Stopping:
                    // keep preRunState as before Stopping
                    _curState = State.Stopped;
                    _curOperation = Operation.CallOnStop;
                    _onStop?.Invoke();
                    _curOperation = Operation.None;
                    goto Stopped;

                default:
                    WarnUnhandledState("UpdateActions");
                    break;
            }

            // the curTime and curTimeline will reset in Play or Rewind
            Completed:
            _preRunState = _curState;
            _curState = State.Completed;
            _curOperation = Operation.CallOnComplete;
            _onComplete?.Invoke();
            _curOperation = Operation.None;

            Stopped:
            if (IsRecyclable)
            {
                Recycle();
            }

            return false;
        }


        /// <summary>
        /// Whether the Tween is in TweenManager tweenUpdateList?
        /// Multithreading is not considered.
        /// </summary>
        internal bool IsUpdating()
        {
            switch (_curState)
            {
                case State.Playing:
                case State.Rewinding:
                case State.Paused:
                case State.Stopping:
                    return true;

                case State.Completed:
                    if (_curOperation == Operation.CallOnComplete)
                    {
                        // in the OnComplete
                        return true;
                    }

                    break;

                case State.Stopped:
                    if (_curOperation == Operation.CallOnStop)
                    {
                        // in the OnStop
                        return true;
                    }

                    break;
            }

            return false;
        }


        /// <summary>
        /// Recycles the Tween if it not in TweenManager tweenUpdateList.
        /// Multithreading is not considered.
        /// </summary>
        internal void RecycleIfNotUpdating()
        {
            switch (_curState)
            {
                case State.Setup:
                    Recycle();
                    break;

                case State.Stopped:
                    if (_curOperation != Operation.CallOnStop)
                    {
                        Recycle();
                    }

                    break;

                case State.Completed:
                    if (_curOperation != Operation.CallOnComplete)
                    {
                        Recycle();
                    }

                    break;
            }
        }


        /// <summary>
        /// Disposes the native data with Allocator.Persistent from TweenActions, called by TweenManager.
        /// If the Tween is recycled, it has no TweenAtions to DisposeNativeData.
        /// </summary>
        internal void DisposeNativeData()
        {
            for (var i = _actionList.Count - 1; i > -1; --i)
            {
                _actionList[i].DisposeNativeData();
            }
        }


        /// <summary>
        /// Asserts the Tween state is Setup.
        /// </summary>
        internal void AssertStateIsSetup(string tag) { Debug.Assert(_curState == State.Setup, $"Tween {tag}, the state [{_curState}] must be [Setup]!"); }


        /// <summary>
        /// Asserts the Tween state is not Recycled.
        /// </summary>
        internal void AssertStateIsNotRecycled(string tag)
        {
            Debug.Assert(_curState != State.Recycled, $"Tween {tag}, the state [{_curState}] cannot be [Recycled]!");
        }


        /// <summary>
        /// Asserts the Tween is not Recyclable.
        /// </summary>
        internal void AssertIsNotRecyclable(string tag)
        {
            Debug.Assert(IsRecyclable == false, $"Tween {tag}, the Tween cannot be recyclable! Change it by [Create] or [SetRecyclable].");
        }


        /// <summary>
        /// Asserts the Tween state can Play or Rewind.
        /// </summary>
        internal void AssertStateCanRun(string tag)
        {
            Debug.Assert(_curState != State.Recycled && _curState != State.Stopping,
                $"Tween {tag}, the state [{_curState}] cannot be [Recycled] or [Stopping]!");
        }


        /// <summary>
        /// Warns the unhandled state.
        /// </summary>
        internal void WarnUnhandledState(string tag) { Debug.LogWarning($"Tween {tag}, why the state is [{_curState}]? This is an unhandled state."); }

        #endregion


        #region Internal Static Methods

        /// <summary>
        /// Recycles unrecycled Tweens.
        /// </summary>
        internal static void RecycleUnrecycled()
        {
            for (var i = TweenUnrecycledList.Count - 1; i > -1; --i)
            {
                var tween = TweenUnrecycledList[i];
                tween.IsRecyclable = true;
                tween.RecycleIfNotUpdating();
            }

            TweenUnrecycledList.Clear();
        }

        /// <summary>
        /// Disposes the native data with Allocator.Persistent from tweenUnrecycledList, called by TweenManger.
        /// </summary>
        internal static void DisposeNativeDataFromUnrecycled()
        {
            for (var i = TweenUnrecycledList.Count - 1; i > -1; --i)
            {
                var tween = TweenUnrecycledList[i];

                // if in TweenManager's tweenUpdateList, leave it to TweenManager's DisposeAllNativeData
                if (tween.IsUpdating() == false)
                {
                    tween.DisposeNativeData();
                }
            }

            // if not clear then the TweensInfoMenu will visit the disposed native data
            TweenUnrecycledList.Clear();
        }


        /// <summary>
        /// Disposes the native data with Allocator.Persistent from tweenCachedStack, called by TweenManger.
        /// </summary>
        internal static void DisposeNativeDataFromCached()
        {
            while (TweenCachedStack.Count > 0)
            {
                TweenCachedStack.Pop().DisposeNativeData();
            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Generates the sorted timelineStart TweenActions to startSortedActions.
        /// </summary>
        private void GenerateStartSortedActions()
        {
            if (_startSortedActions == null)
            {
                // sort by timelineStart ASC
                _actionList.Sort((a1, a2) => (int) ((a1.TimelineStart - a2.TimelineStart) * TIMELINE_SORT_SCALE));
                _startSortedActions = _actionList.ToArray();
            }
        }


        /// <summary>
        /// Generates the sorted timelineEnd TweenActions to endSortedActions.
        /// </summary>
        private void GenerateEndSortedActions()
        {
            if (_endSortedActions == null)
            {
                // sort by timelineEnd ASC
                _actionList.Sort((a1, a2) => (int) ((a1.TimelineEnd - a2.TimelineEnd) * TIMELINE_SORT_SCALE));
                _endSortedActions = _actionList.ToArray();
            }
        }


        /// <summary>
        /// Reverses all play completed actions.
        /// </summary>
        private void ReversePlayedActions()
        {
            for (var i = _waitIndex - 1; i > -1; --i)
            {
                _startSortedActions[i].CompleteRewind().InitPlay();
            }

            // the run actions have been Rewinded in startSortedActions
            _actionUpdateList.Clear();
        }


        /// <summary>
        /// Reverses all rewind completed actions.
        /// </summary>
        private void ReverseRewindedActions()
        {
            for (int i = _waitIndex + 1, len = _endSortedActions.Length; i < len; ++i)
            {
                _endSortedActions[i].CompletePlay().InitRewind();
            }

            // the run actions have been Played in endSortedActions
            _actionUpdateList.Clear();
        }


        /// <summary>
        /// Inits the Play.
        /// </summary>
        private void InitPlay()
        {
            Debug.Assert(_startSortedActions.Length > 0, "Tween InitPlay, there are no actions to Play!");

            _curTimeline = _startSortedActions[0].TimelineStart;
            _waitIndex = 0;
            _curTime = 0.0f;
            _onStart?.Invoke();
        }


        /// <summary>
        /// Inits the Rewind.
        /// </summary>
        private void InitRewind()
        {
            var last = _endSortedActions.Length - 1;
            _curTimeline = _endSortedActions[last].TimelineEnd;
            _waitIndex = last;
            _curTime = Duration;
            _onStart?.Invoke();
        }


        /// <summary>
        /// Adds the TweenAction to the queue.
        /// </summary>
        private void AddQueueAction(TweenAction action)
        {
            AddAction(action, _queueTime, _queueTime + action.Duration);
            _queueTime = action.TimelineEnd;
        }


        /// <summary>
        /// Adds the TweenAction to the concurrent array.
        /// </summary>
        private void AddConcurrentAction(float timelineStart, TweenAction action)
        {
            AddAction(action, timelineStart, timelineStart + action.Duration);
            _concurrentTime = action.TimelineEnd;
        }


        /// <summary>
        /// Adds the TweenAction to the sorted list.
        /// </summary>
        private void AddAction(TweenAction action, float timelineStart, float timelineEnd)
        {
            if (Duration < timelineEnd)
            {
                Duration = timelineEnd;
            }

            if (_defaultEase != Ease.Smooth)
            {
                action.SetDefaultEase(_defaultEase);
            }

            if (_defaultIsRelative)
            {
                action.SetDefaultRelativeFalse();
            }

            action.AttachTo(this, timelineStart, timelineEnd);

            // actionList be sorted before play or rewind
            _actionList.Add(action);
        }


        /// <summary>
        /// Resets the values as the constructor, and push to cached stack.
        /// </summary>
        private void Recycle()
        {
            AssertStateIsNotRecycled("Recycle");

            Debug.Assert(_actionUpdateList.Count == 0, $"Tween Recycle, the actionUpdateList[count = {_actionUpdateList.Count}] must be completed!");

            _onRecycle?.Invoke();

            for (var i = _actionList.Count - 1; i > -1; --i)
            {
                _actionList[i].Recycle();
            }

            _actionList.Clear();

            // these values set to default as constructor dose but not creator dose

            _startSortedActions = null;
            _endSortedActions = null;
            Duration = 0.0f;
            _queueTime = 0.0f;
            _concurrentTime = 0.0f;
            _onStart = null;
            _onComplete = null;
            _onStop = null;
            _onRecycle = null;
            _defaultEase = Ease.Smooth;
            _defaultIsRelative = false;
            _curState = State.Recycled;

            TweenCachedStack.Push(this);
        }

        #endregion


        #region Internal Methods For Editor

        /// <summary>
        /// Gets the curTime.
        /// </summary>
        internal float GetCurTime() { return _curTime; }


        /// <summary>
        /// Gets the curState name.
        /// </summary>
        internal string GetCurStateName() { return _curState.ToString(); }


        /// <summary>
        /// Gets the actionList.
        /// </summary>
        internal List<TweenAction> GetActionList() { return _actionList; }


        /// <summary>
        /// Gets the tweenCachedStack.
        /// </summary>
        internal static Stack<Tween> GetTweenCachedStack() { return TweenCachedStack; }


        /// <summary>
        /// Gets the tweenUnrecycledList.
        /// </summary>
        internal static List<Tween> GetTweenUnrecycledList() { return TweenUnrecycledList; }

        #endregion
    }
}