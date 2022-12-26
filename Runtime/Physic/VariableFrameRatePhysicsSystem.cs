using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Scripting;

namespace Pancake
{
    /// <summary>
    /// Part to be registered in the PlayerLoopSystem update process
    /// </summary>
    [Preserve]
    public static partial class VariableFrameRatePhysicsSystem
    {
        public struct RecordCurrentDynamicUnscaledTime
        {
        }

        public struct AdjustFixedDeltaTime
        {
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [Preserve]
        private static void Initialize()
        {
            SetVariableFrameRatePhysicsSystem();
            defaultFixedDeltaTime = VariableFrameRatePreset.GetDefaultFixedDeltaTimeStep();
            Type = VariableFrameRatePreset.GetFixedDeltaTimeType();
        }

        /// <summary>
        /// Adjust the value of fixedDeltaTime to synchronize fixedTime with time every frame.
        /// Idea: Adjust fixedDeltaTime to be the same as deltaTime.
        /// Example: 0.051 (ms) => 0.02 + 0.02 + 0.011 (ms)
        /// </summary>
        private static void SetVariableFrameRatePhysicsSystem()
        {
            var rootPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < rootPlayerLoopSystem.subSystemList.Length; i++)
            {
                var subSystem = rootPlayerLoopSystem.subSystemList[i];
                bool isTimeUpdateSystem = subSystem.type == typeof(TimeUpdate);
                bool isFixedUpdateSystem = subSystem.type == typeof(FixedUpdate);

                if (isTimeUpdateSystem || isFixedUpdateSystem)
                {
                    // * TimeUpdate: System for updating Time.Time and Time.deltaTime
                    // It is necessary to adjust fixedDeltaTime before deciding whether FixedUpdate can be executed, so add processing immediately after TimeUpdate
                    var updateSubSystemList = new List<PlayerLoopSystem>(subSystem.subSystemList);
                    if (isTimeUpdateSystem)
                    {
                        // System that records Time.unscaledTime for use inside FixedUpdate
                        updateSubSystemList.Add(CreateRecordCurrentDynamicUnscaledTimeSystem());
                        updateSubSystemList.Add(CreateUpdateFixedTimeAfterTimeUpdateSystem());
                    }
                    else
                    {
                        updateSubSystemList.Add(CreateUpdateFixedDeltaTimeOnFixedUpdateEndSystem());
                    }

                    subSystem.subSystemList = updateSubSystemList.ToArray();
                    rootPlayerLoopSystem.subSystemList[i] = subSystem;
                }
            }

            // Apply above changes
            PlayerLoop.SetPlayerLoop(rootPlayerLoopSystem);
        }

        /// <summary> Create a system to synchronize fixedDeltaTime with deltaTime </summary>
        private static PlayerLoopSystem CreateUpdateFixedTimeAfterTimeUpdateSystem()
        {
            return new PlayerLoopSystem() {type = typeof(AdjustFixedDeltaTime), updateDelegate = UpdateFixedDeltaTimeAfterTimeUpdate,};
        }

        /// <summary> Create a system to synchronize fixedDeltaTime with deltaTime </summary>
        private static PlayerLoopSystem CreateUpdateFixedDeltaTimeOnFixedUpdateEndSystem()
        {
            return new PlayerLoopSystem() {type = typeof(AdjustFixedDeltaTime), updateDelegate = UpdateFixedDeltaTimeOnFixedUpdateEnd,};
        }

        /// <summary> Create a system that records Time.unscaledTime </summary>
        private static PlayerLoopSystem CreateRecordCurrentDynamicUnscaledTimeSystem()
        {
            return new PlayerLoopSystem() {type = typeof(RecordCurrentDynamicUnscaledTime), updateDelegate = SetCurrentDynamicUnscaledTime,};
        }
    }


    /// <summary>
    /// FixedDeltaTime adjustment processor
    /// </summary>
    public static partial class VariableFrameRatePhysicsSystem
    {
        /// <summary>
        /// 
        /// </summary>
        public enum FixedDeltaTimeType
        {
            /// <summary>
            /// fixedDeltaTime fixed value; existing method <br/>
            /// FixedUpdate 0~ times per frame <br/>
            /// Physics is stable
            /// </summary>
            Fixed = 0,

            /// <summary>
            /// fixedDeltaTime = deltaTime <br/>
            /// FixedUpdate exactly once per frame <br/>
            /// Works well with the rest of the game
            /// </summary>
            Variable = 1,

            /// <summary>
            /// Divide deltaTime by DefaultFixedDeltaTime value and set to fixedDeltaTime <br/>
            /// FixedUpdate 1~ times per frame <br/>
            /// A compromise between Fixed and Variable (stability & compatibility)
            /// </summary>
            VariableWithSubStep = 2,
        }

        private static float defaultFixedDeltaTime = 0.02f;
        private const float CHECK_TIME_EPSILON = 0.00001f;
        private const float SAFE_MARGIN_TIME_EPSILON = CHECK_TIME_EPSILON * 0.1f;

        /// <summary>
        /// DeltaTime mode switch (false = Unity default update handling)
        /// </summary>
        [Preserve] public static FixedDeltaTimeType Type
        {
            get => type;
            set
            {
                if (type == value) return;

                type = value;

                if (!Application.isPlaying) return;
                // Revert fixedDeltaTime if existing
                if (type == FixedDeltaTimeType.Fixed)
                {
                    Time.fixedDeltaTime = Time.timeScale * defaultFixedDeltaTime;
                }
            }
        }

        private static FixedDeltaTimeType type = FixedDeltaTimeType.Fixed;

        /// <summary> Revert to Time.unscaledTime </summary> for this frame
        private static float currentDynamicUnscaledTime;


        /// <summary>
        /// After TimeUpdate look at the type and update fixedDeltaTime
        /// </summary>
        private static void UpdateFixedDeltaTimeAfterTimeUpdate()
        {
            if (!Application.isPlaying) return;

            switch (Type)
            {
                case FixedDeltaTimeType.Variable:
                    Time.fixedDeltaTime = Time.deltaTime;
                    break;

                case FixedDeltaTimeType.VariableWithSubStep:
                    AdjustFixedDeltaTimeClamped();
                    break;

                case FixedDeltaTimeType.Fixed:
                default:
                    break;
            }
        }

        /// <summary>
        /// At the end of FixedUpdate, look at the type and update the fixedDeltaTime
        /// </summary>
        private static void UpdateFixedDeltaTimeOnFixedUpdateEnd()
        {
            if (!Application.isPlaying) return;

            switch (Type)
            {
                case FixedDeltaTimeType.VariableWithSubStep:
                    AdjustFixedDeltaTimeClamped();
                    break;

                case FixedDeltaTimeType.Variable:
                case FixedDeltaTimeType.Fixed:
                default:
                    break;
            }
        }

        /// <summary>
        /// Adjust fixedDeltaTime so that Time.time and Time.fixedTime are in sync
        /// </summary>
        private static void AdjustFixedDeltaTimeClamped()
        {
            var remainDeltaTime = currentDynamicUnscaledTime - Time.fixedUnscaledTime;
            // If fixedTime and Time are 0 or greater than default fixedDeltaTime, restore default fixedDeltaTime
            if (remainDeltaTime <= CHECK_TIME_EPSILON || remainDeltaTime > defaultFixedDeltaTime)
            {
                Time.fixedDeltaTime = Time.timeScale * defaultFixedDeltaTime;
            }
            else
            {
                // There is a possibility that a float error may occur in the subtraction, and the execution judgment of FixedUpdate may not pass, so
                // do it with a number slightly lower than the true deltaTime
                Time.fixedDeltaTime = Time.timeScale * (remainDeltaTime - SAFE_MARGIN_TIME_EPSILON);
            }
        }

        /// <summary>
        /// In FixedUpdate, Time.unscaledTime becomes Time.fixedUnscaledTime, so
        /// Record Time.unscaledTime outside
        /// </summary>
        private static void SetCurrentDynamicUnscaledTime() { currentDynamicUnscaledTime = Time.unscaledTime; }
    }
}