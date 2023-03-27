using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Pancake.Tween
{
    /// <summary>
    /// Each TweenAction can have a list of TweenActionValues that will ease concurrently.
    /// Don't hold a TweenAction and always create a new one, system will recycle it.
    /// </summary>
    public class TweenAction
    {
        /// <summary>
        /// The timeline duration.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// Which the TweenAction is attached to.
        /// </summary>
        public Tween Tween { get; private set; }


        #region Internal Fields

        /// <summary>
        /// The start time in Tween timeline.
        /// </summary>
        internal float TimelineStart { get; private set; }

        /// <summary>
        /// The end time in Tween timeline.
        /// </summary>
        internal float TimelineEnd { get; private set; }

        #endregion


        #region Private Fields

        /// <summary>
        /// Callback when TweenAction needs to get the target values.
        /// </summary>
        private OnSetTargetValuesFunc _onGetTargetValues;

        /// <summary>
        /// Callback when TweenAction needs to set the target values.
        /// </summary>
        private OnSetTargetValuesFunc _onSetTargetValues;

        /// <summary>
        /// Whether all TweenActionValues are init [from, to] values?
        /// </summary>
        private bool _isInitValues;

        /// <summary>
        /// Callback when TweenAction starts (Play or Rewind).
        /// </summary>
        private Action _onStart;

        /// <summary>
        /// Callback When TweenAction completes (Play or Rewind).
        /// </summary>
        private Action _onComplete;

        /// <summary>
        /// The JobHandle of UpdateJob.
        /// </summary>
        private JobHandle _jobHandle;

        /// <summary>
        /// The input NativeArray of UpdateJob.
        /// </summary>
        private NativeArray<float3> _jobInputArray;

        /// <summary>
        /// The output NativeArray of UpdateJob.
        /// </summary>
        private NativeArray<float> _jobOutputArray;

        /// <summary>
        /// The extra param NativeArray of UpdateJob. 
        /// </summary>
        private NativeArray<float> _jobExtraPramArray;

        /// <summary>
        /// The values to jobInputArray.
        /// [float3]: [x:fromValue, y:toValue, z:ease]
        /// </summary>                                                 
        private NativeList<float3> _paramList = new NativeList<float3>(4, Allocator.Persistent);

        /// <summary>                                                  
        /// The values to jobExtraPramArray.                              
        /// </summary>                                                 
        private NativeList<float> _extraParamList = new NativeList<float>(4, Allocator.Persistent);

        /// <summary>                                                  
        /// The list of TweenActionValues that ease concurrently.      
        /// </summary>                                                 
        private NativeList<TweenActionValue> _valueList = new NativeList<TweenActionValue>(4, Allocator.Persistent);

        /// <summary>                                                  
        /// The cached TweenActions.                                   
        /// </summary>
        private static readonly Stack<TweenAction> ActionCachedStack = new Stack<TweenAction>(8);

        #endregion


        /// <summary>
        /// Prevents new TweenAction.
        /// </summary>
        private TweenAction() { }


        #region Public Static Methods

        public delegate void OnGetTargetValuesFunc(in NativeList<TweenActionValue> valueList);

        public delegate void OnSetTargetValuesFunc(in NativeList<TweenActionValue> valueList);

        public delegate void OnSetValueList(in NativeList<TweenActionValue> valueList);


        /// <summary>
        /// Creates a TweenAction ease to float.
        /// 
        /// [OnGetTargetFloat]: get the target values.
        /// [OnSetTargetFloat]: set the target values.
        /// [finalValue]      : the final value that will ease to. 
        /// </summary>
        public static TweenAction CreateFloat(Func<float> onGetTargetFloat, Action<float> onSetTargetFloat, float finalValue, float duration)
        {
            var action = Create((in NativeList<TweenActionValue> valueList) => valueList.ElementAt(0).targetValue = onGetTargetFloat(),
                (in NativeList<TweenActionValue> valueList) => onSetTargetFloat(valueList.ElementAt(0).targetValue),
                duration);

            action._valueList.Add(new TweenActionValue(finalValue));

            return action;
        }


        public delegate void OnGetTargetVector2(out Vector2 v2);

        public delegate void OnSetTargetVector2(in Vector2 v2);


        /// <summary>
        /// Creates a TweenAction ease to vector2.
        /// 
        /// [OnGetTargetVector2]: get the target values.
        /// [OnSetTargetVector2]: set the target values.
        /// [finalValues]       : the final values that will ease to. 
        /// </summary>
        public static TweenAction CreateVector2(OnGetTargetVector2 onGetTargetVector2, OnSetTargetVector2 onSetTargetVector2, in Vector2 finalValues, float duration)
        {
            var action = Create((in NativeList<TweenActionValue> valueList) =>
                {
                    onGetTargetVector2(out Vector2 v2);
                    valueList.ElementAt(0).targetValue = v2.x;
                    valueList.ElementAt(1).targetValue = v2.y;
                },
                (in NativeList<TweenActionValue> valueList) =>
                {
                    onSetTargetVector2(new Vector2(valueList.ElementAt(0).targetValue, valueList.ElementAt(1).targetValue));
                },
                duration);

            action._valueList.Add(new TweenActionValue(finalValues.x));
            action._valueList.Add(new TweenActionValue(finalValues.y));

            return action;
        }


        public delegate void OnGetTargetVector3(out Vector3 v3);

        public delegate void OnSetTargetVector3(in Vector3 v3);


        /// <summary>
        /// Creates a TweenAction ease to vector3.
        /// 
        /// [OnGetTargetVector3]: get the target values.
        /// [OnSetTargetVector3]: set the target values.
        /// [finalValues]       : the final values that will ease to. 
        /// </summary>
        public static TweenAction CreateVector3(OnGetTargetVector3 onGetTargetVector3, OnSetTargetVector3 onSetTargetVector3, in Vector3 finalValues, float duration)
        {
            var action = Create((in NativeList<TweenActionValue> valueList) =>
                {
                    onGetTargetVector3(out Vector3 v3);
                    valueList.ElementAt(0).targetValue = v3.x;
                    valueList.ElementAt(1).targetValue = v3.y;
                    valueList.ElementAt(2).targetValue = v3.z;
                },
                (in NativeList<TweenActionValue> valueList) =>
                {
                    onSetTargetVector3(new Vector3(valueList.ElementAt(0).targetValue, valueList.ElementAt(1).targetValue, valueList.ElementAt(2).targetValue));
                },
                duration);

            action._valueList.Add(new TweenActionValue(finalValues.x));
            action._valueList.Add(new TweenActionValue(finalValues.y));
            action._valueList.Add(new TweenActionValue(finalValues.z));

            return action;
        }


        public delegate void OnGetTargetVector4(out Vector4 v4);

        public delegate void OnSetTargetVector4(in Vector4 v4);


        /// <summary>
        /// Creates a TweenAction ease to vector4.
        /// 
        /// [OnGetTargetVector4]: get the target values.
        /// [OnSetTargetVector4]: set the target values.
        /// [finalValues]       : the final values that will ease to. 
        /// </summary>
        public static TweenAction CreateVector4(OnGetTargetVector4 onGetTargetVector4, OnSetTargetVector4 onSetTargetVector4, in Vector4 finalValues, float duration)
        {
            var action = Create((in NativeList<TweenActionValue> valueList) =>
                {
                    onGetTargetVector4(out Vector4 v4);
                    valueList.ElementAt(0).targetValue = v4.x;
                    valueList.ElementAt(1).targetValue = v4.y;
                    valueList.ElementAt(2).targetValue = v4.z;
                    valueList.ElementAt(3).targetValue = v4.w;
                },
                (in NativeList<TweenActionValue> valueList) =>
                {
                    onSetTargetVector4(new Vector4(valueList.ElementAt(0).targetValue,
                        valueList.ElementAt(1).targetValue,
                        valueList.ElementAt(2).targetValue,
                        valueList.ElementAt(3).targetValue));
                },
                duration);

            action._valueList.Add(new TweenActionValue(finalValues.x));
            action._valueList.Add(new TweenActionValue(finalValues.y));
            action._valueList.Add(new TweenActionValue(finalValues.z));
            action._valueList.Add(new TweenActionValue(finalValues.w));

            return action;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Sets the [OnStart] callback which is called when the TweenAction starts (Play or Rewind).
        /// </summary>
        public TweenAction OnStart(Action onStart)
        {
            AssertCanBeAttached("OnStart");
            _onStart += onStart;

            return this;
        }


        /// <summary>
        /// Sets the [OnComplete] callback which is called when the TweenAction completes (Play or Rewind).
        /// </summary>
        public TweenAction OnComplete(Action onComplete)
        {
            AssertCanBeAttached("OnComplete");
            _onComplete += onComplete;

            return this;
        }


        /// <summary>
        /// Sets the [isRelative] of all TweenActionValues, default false.
        /// </summary>
        public TweenAction SetRelative(bool isRelative)
        {
            AssertCanBeAttached("SetRelative");

            for (var i = _valueList.Length - 1; i > -1; --i)
            {
                _valueList.ElementAt(i).isRelative = isRelative;
            }

            return this;
        }


        /// <summary>
        /// Sets the [isRelative] of TweenActionValue at index, default false.
        /// </summary>
        public TweenAction SetRelativeAt(int index, bool isRelative)
        {
            AssertCanBeAttached("SetRelativeAt");
            _valueList.ElementAt(index).isRelative = isRelative;

            return this;
        }


        /// <summary>
        /// Sets the [ease] of all TweenActionValues, default Smooth.
        /// </summary>
        public TweenAction SetEase(Ease ease)
        {
            AssertCanBeAttached("SetEase");

            for (var i = _valueList.Length - 1; i > -1; --i)
            {
                _valueList.ElementAt(i).ease = ease;
            }

            return this;
        }


        /// <summary>
        /// Sets the [ease] of TweenActionValue at index, default Smooth.
        /// </summary>
        public TweenAction SetEaseAt(int index, Ease ease)
        {
            AssertCanBeAttached("SetEaseAt");
            _valueList.ElementAt(index).ease = ease;

            return this;
        }


        /// <summary>
        /// Sets the [extraParams] to TweenEase.
        /// </summary>
        public TweenAction SetExtraParams(params float[] extraParams)
        {
            AssertCanBeAttached("SetExtraParams");

            for (int i = 0, count = extraParams.Length; i < count; ++i)
            {
                _extraParamList.Add(extraParams[i]);
            }

            return this;
        }

        #endregion


        #region Internal Methods

        /// <summary>
        /// Creates a TweenAction.
        /// [OnGetTargetValues]: pure callback TweenAction can be null.
        /// [OnSetTargetValues]: pure callback TweenAction can be null.
        /// </summary>
        internal static TweenAction Create(OnSetTargetValuesFunc onGetTargetValues, OnSetTargetValuesFunc onSetTargetValues, float duration)
        {
            TweenAction action;

            if (ActionCachedStack.Count > 0)
            {
                action = ActionCachedStack.Pop();
            }
            else
            {
                action = new TweenAction();
            }

            // these values same as new and cached

            action._onGetTargetValues = onGetTargetValues;
            action._onSetTargetValues = onSetTargetValues;
            action.Duration = duration;

            return action;
        }


        /// <summary>
        /// Attaches the TweenAction to Tween.
        /// </summary>
        internal void AttachTo(Tween tween, float timelineStart, float timelineEnd)
        {
            Tween = tween;
            TimelineStart = timelineStart;
            TimelineEnd = timelineEnd;
        }


        /// <summary>
        /// Sets the [ease] of all TweenActionValues, when the ease of TweenActionValue is default Smooth.
        /// </summary>
        internal TweenAction SetDefaultEase(Ease ease)
        {
            AssertCanBeAttached("SetDefaultEase");

            for (var i = _valueList.Length - 1; i > -1; --i)
            {
                ref var value = ref _valueList.ElementAt(i);

                if (value.ease == Ease.Smooth)
                {
                    value.ease = ease;
                }
            }

            return this;
        }


        /// <summary>
        /// Sets the [isRelative] of all TweenActionValues, when the [isRelative] of TweenActionValue is default false.
        /// </summary>
        internal TweenAction SetDefaultRelativeFalse()
        {
            AssertCanBeAttached("SetDefaultRelativeFalse");

            for (var i = _valueList.Length - 1; i > -1; --i)
            {
                ref var value = ref _valueList.ElementAt(i);

                if (value.isRelative == false)
                {
                    value.isRelative = true;
                }
            }

            return this;
        }


        /// <summary>
        /// Plays the TweenAction.
        /// If return false then the play is completed.
        /// </summary>
        internal bool Play(float tweenCurTime)
        {
            var curTime = tweenCurTime - TimelineStart;

            if (curTime < Duration)
            {
                UpdateValues(curTime);
                return true;
            }
            else
            {
                CompletePlay();
                return false;
            }
        }


        /// <summary>
        /// Inits the Play.
        /// </summary>
        internal TweenAction InitPlay()
        {
            InitValues();
            _onStart?.Invoke();
            return this;
        }


        /// <summary>
        /// Completes the Play — set the toValue to targetValue of TweenActionValues.
        /// </summary>
        internal TweenAction CompletePlay()
        {
            var len = _valueList.Length;

            if (len > 0)
            {
                InitValuesWithLen(len);

                unsafe
                {
                    TweenActionValue.SetToValues(_valueList.GetUnsafeList(), len);
                }

                // set the tagert values to toValues by valueList
                _onSetTargetValues(_valueList);
            }

            _onComplete?.Invoke();

            return this;
        }


        /// <summary>
        /// Checks whether the Play is completed.
        /// </summary>
        internal bool CheckPlayCompleted(float tweenCurTime)
        {
            if (tweenCurTime - TimelineStart >= Duration)
            {
                CompletePlay();
                return true;
            }

            return false;
        }


        /// <summary>
        /// Rewinds the TweenAction.
        /// If return false then the rewind is completed.
        /// </summary>
        internal bool Rewind(float tweenCurTime)
        {
            var curTime = tweenCurTime - TimelineStart;

            if (curTime > 0.0f)
            {
                UpdateValues(curTime);
                return true;
            }
            else
            {
                CompleteRewind();
                return false;
            }
        }


        /// <summary>
        /// Inits the Rewind.
        /// </summary>
        internal TweenAction InitRewind()
        {
            // if come from Setup to Rewind then need to InitValues
            InitValues();
            _onStart?.Invoke();
            return this;
        }


        /// <summary>
        /// Completes the Rewind — set the fromValue to targetValue of TweenActionValues.
        /// </summary>
        internal TweenAction CompleteRewind()
        {
            var len = _valueList.Length;

            if (len > 0)
            {
                unsafe
                {
                    TweenActionValue.SetFromValues(_valueList.GetUnsafeList(), len);
                }

                // set the tagert values to fromValues by valueList
                _onSetTargetValues(_valueList);
            }

            _onComplete?.Invoke();

            return this;
        }


        /// <summary>
        /// Checks whether the Reinwd is completed.
        /// </summary>
        internal bool CheckRewindCompleted(float tweenCurTime)
        {
            if (tweenCurTime - TimelineStart <= 0.0f)
            {
                CompleteRewind();
                return true;
            }

            return false;
        }


        /// <summary>
        /// Calls the InitValuesWithLen.
        /// </summary>
        internal void InitValues()
        {
            var len = _valueList.Length;

            if (len > 0)
            {
                InitValuesWithLen(len);
            }
        }


        /// <summary>
        /// Callback OnSetTargetValues to update the target.
        /// </summary>
        internal void UpdateTargetValues()
        {
            // the Tween state may be changed after update
            // if not running, do not update target values, but still dispose the job data
            if (Tween.IsRunning())
            {
                unsafe
                {
                    TweenActionValue.SetTargetValues(_valueList.GetUnsafeList(), (float*) _jobOutputArray.GetUnsafePtr());
                }

                _onSetTargetValues(_valueList);
            }

            _jobInputArray.Dispose();
            _jobOutputArray.Dispose();

            if (_jobExtraPramArray.IsCreated)
            {
                _jobExtraPramArray.Dispose();
            }
        }


        /// <summary>
        /// Resets the values as the constructor, push to cached stack, called by Tween.
        /// </summary>
        internal void Recycle()
        {
            // these values set to default as constructor dose but not creator dose

            Tween = null;
            _onStart = null;
            _onComplete = null;
            _onGetTargetValues = null;
            _onSetTargetValues = null;
            _isInitValues = false;

            _paramList.Clear();
            _extraParamList.Clear();
            _valueList.Clear();

            ActionCachedStack.Push(this);
        }


        /// <summary>
        /// Disposes the native data with Allocator.Persistent,
        /// called by TweenAction (dispose cached) and Tween (dispose Unrecycled).
        /// </summary>
        internal void DisposeNativeData()
        {
            _paramList.Dispose();
            _extraParamList.Dispose();
            _valueList.Dispose();
        }


        /// <summary>
        /// Asserts the TweenAction can be attached to the Tween.
        /// </summary>
        internal void AssertCanBeAttached(string tag) { Debug.Assert(Tween == null, $"TweenAction {tag}, the TweenAction has been attached to the Tween!"); }

        #endregion


        #region Internal Static Methods

        /// <summary>
        /// Creates a TweenAction that has only a callback.
        /// </summary>
        internal static TweenAction CreateCallback(Action callback) { return Create(null, null, 0.0f).OnComplete(callback); }

        /// <summary>
        /// Creates a TweenAction that has only a delay callback.
        /// </summary>
        internal static TweenAction CreateDelayCallback(float delay, Action callback) { return Create(null, null, delay).OnComplete(callback); }


        /// <summary>
        /// Disposes the native data with Allocator.Persistent from tweenCachedStack, called by TweenManger.
        /// </summary>
        internal static void DisposeNativeDataFromCached()
        {
            while (ActionCachedStack.Count > 0)
            {
                ActionCachedStack.Pop().DisposeNativeData();
            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Inits [from, to] values of the TweenActionValues in valueList,
        /// and Init the paramList from valueList. 
        /// </summary>
        private void InitValuesWithLen(int len)
        {
            if (_isInitValues == false)
            {
                // get tagert values to the valueList
                _onGetTargetValues(_valueList);

                unsafe
                {
                    TweenActionValue.InitValues(_valueList.GetUnsafeList(), len, _paramList.GetUnsafeList());
                }

                _isInitValues = true;
            }
        }


        /// <summary>
        /// Updates the TweenActionValues by UpdateJob.
        /// </summary>
        private void UpdateValues(float curTime)
        {
            var len = _valueList.Length;

            if (len > 0)
            {
                _jobOutputArray = new NativeArray<float>(len, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                _jobInputArray = new NativeArray<float3>(len, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                _jobInputArray.CopyFrom(_paramList);

                if (_extraParamList.Length == 0)
                {
                    _jobHandle = new TweenActionValue.UpdateJob()
                    {
                        outputArray = _jobOutputArray, inputArray = _jobInputArray, times = new float2(curTime, Duration),
                    }.Schedule(len, 1);
                }
                else
                {
                    _jobExtraPramArray = new NativeArray<float>(_extraParamList.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                    _jobExtraPramArray.CopyFrom(_extraParamList);

                    _jobHandle = new TweenActionValue.UpdateJobWithExtraParams()
                    {
                        outputArray = _jobOutputArray,
                        inputArray = _jobInputArray,
                        extraParamArray = _jobExtraPramArray,
                        times = new float2(curTime, Duration),
                    }.Schedule(len, 1);
                }

                TweenManager.AddActionAndJob(this, in _jobHandle);
            }
        }

        #endregion


        #region Internal Methods For Editor

        /// <summary>
        /// Gets the actionCachedStack.
        /// </summary>
        internal static ICollection GetActionCachedStack() { return ActionCachedStack; }

        #endregion
    }
}