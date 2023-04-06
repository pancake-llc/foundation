using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pancake.Tween
{
    /// <summary>
    /// One value that TweenAction will ease to.
    /// </summary>
    [BurstCompile]
    public struct TweenActionValue
    {
        #region Private Fields

        /// <summary>
        /// The final value that will ease to, but the real value depends on isRelative.
        /// </summary>
        private readonly float _finalValue;

        /// <summary>
        /// The value of ease from, set by Tween.
        /// </summary>
        private float _fromValue;

        /// <summary>
        /// The real value will ease to, set by Tween.
        /// </summary>
        private float _toValue;

        #endregion


        #region Internal Fields

        /// <summary>
        /// The value of target.
        /// Sets by TweenAction OnGetTargetValues.
        /// Gets by TweenAction OnSetTargetValues.
        /// </summary>
        public float targetValue;

        /// <summary>
        /// The finalValue is relative (target fromValue) or absolute, default false.
        /// </summary>
        internal bool isRelative;

        /// <summary>
        /// The ease type, default Smooth.
        /// </summary>
        internal Ease ease;

        #endregion


        /// <summary>
        /// Constructs a TweenActionValue with default ease (Smooth) and isRelative (false).
        /// </summary>
        internal TweenActionValue(float finalValue)
        {
            _finalValue = finalValue;
            targetValue = 0.0f;
            _fromValue = 0.0f;
            _toValue = 0.0f;
            isRelative = false;
            ease = Ease.Smooth;
        }


        #region Internal Methods

        /// <summary>
        /// Inits [from, to] values of the TweenActionValues in valueList,
        /// and Init paramList from valueList. 
        /// </summary>
        [BurstCompile]
        internal static unsafe void InitValues(UnsafeList<TweenActionValue>* valueListPtr, int len, UnsafeList<float3>* paramListPtr)
        {
            ref var valueList = ref *valueListPtr;
            ref var paramList = ref *paramListPtr;

            for (var i = 0; i < len; ++i)
            {
                ref var actionValue = ref UnsafeUtility.ArrayElementAsRef<TweenActionValue>(valueList.Ptr, i);

                actionValue._fromValue = actionValue.targetValue;
                // actionValue.isRelative ? actionValue.finalValue + actionValue.fromValue : actionValue.finalValue;
                actionValue._toValue = math.select(actionValue._finalValue, actionValue._finalValue + actionValue._fromValue, actionValue.isRelative);

                paramList.Add(new float3(actionValue._fromValue, actionValue._toValue, (float) actionValue.ease));
            }
        }


        /// <summary>
        /// Sets the fromValue to targetValue of TweenActionValues.
        /// </summary>
        [BurstCompile]
        internal static unsafe void SetFromValues(UnsafeList<TweenActionValue>* valueListPtr, int len)
        {
            ref var valueList = ref *valueListPtr;

            for (var i = 0; i < len; ++i)
            {
                ref var actionValue = ref UnsafeUtility.ArrayElementAsRef<TweenActionValue>(valueList.Ptr, i);
                actionValue.targetValue = actionValue._fromValue;
            }
        }


        /// <summary>
        /// Sets the toValue to targetValue of TweenActionValues.
        /// </summary>
        [BurstCompile]
        internal static unsafe void SetToValues(UnsafeList<TweenActionValue>* valueListPtr, int len)
        {
            ref var valueList = ref *valueListPtr;

            for (var i = 0; i < len; ++i)
            {
                ref var actionValue = ref UnsafeUtility.ArrayElementAsRef<TweenActionValue>(valueList.Ptr, i);
                actionValue.targetValue = actionValue._toValue;
            }
        }


        /// <summary>
        /// Sets the targetValue of TweenActionValues from jobOutputArray.
        /// </summary>
        [BurstCompile]
        internal static unsafe void SetTargetValues(UnsafeList<TweenActionValue>* valueListPtr, float* jobOutputArrayPtr)
        {
            ref var valueList = ref *valueListPtr;

            for (var i = valueList.Length - 1; i > -1; --i)
            {
                ref var actionValue = ref UnsafeUtility.ArrayElementAsRef<TweenActionValue>(valueList.Ptr, i);
                actionValue.targetValue = jobOutputArrayPtr[i];
            }
        }


        /// <summary>
        /// Updates the TweenActionValues by TweenEase.
        /// </summary>
        [BurstCompile]
        internal struct UpdateJob : IJobParallelFor
        {
            [WriteOnly] public NativeArray<float> outputArray;
            [ReadOnly] public NativeArray<float3> inputArray;
            [ReadOnly] public float2 times;

            [BurstCompile]
            public void Execute(int index)
            {
                // calculate the ease value
                outputArray[index] = TweenEaseFn.Step(inputArray[index], times);
            }
        }


        /// <summary>
        /// Updates the TweenActionValues with extra params by TweenEase.
        /// </summary>
        [BurstCompile]
        internal struct UpdateJobWithExtraParams : IJobParallelFor
        {
            [WriteOnly] public NativeArray<float> outputArray;
            [ReadOnly] public NativeArray<float3> inputArray;
            [ReadOnly] public NativeArray<float> extraParamArray;
            [ReadOnly] public float2 times;

            [BurstCompile]
            public unsafe void Execute(int index)
            {
                // calculate the ease value
                outputArray[index] = TweenEaseFn.StepWithExtraParams(inputArray[index], times, (float*) extraParamArray.GetUnsafeReadOnlyPtr(), index);
            }
        }

        #endregion
    }
}