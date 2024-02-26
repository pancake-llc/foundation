using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;

namespace Pancake.DebugView
{
    public class StatefulDrawer : Drawer
    {
        [SerializeField] [Range(0.0f, 1.0f)] private float minProgress = 0.1f;
        [SerializeField] private bool useMiddleState;
        [SerializeField] [Range(0.0f, 1.0f)] private float middleProgress = 0.5f;
        [SerializeField] [Range(0.0f, 1.0f)] private float maxProgress = 1.0f;

        public float MinProgress { get => minProgress; set => minProgress = value; }

        public bool UseMiddleState { get => useMiddleState; set => useMiddleState = value; }

        public float MiddleProgress { get => middleProgress; set => middleProgress = value; }

        public float MaxProgress { get => maxProgress; set => maxProgress = value; }

        protected override void Start()
        {
            if (Application.isPlaying)
                SetState(OpenOnStart ? DrawerState.Max : DrawerState.Min);
        }

        public void SetState(DrawerState state) { Progress = GetStateProgress(state); }

        public void SetStateWithAnimation(DrawerState to, float durationSec, Ease easeType, Action completed = null)
        {
            PlayProgressAnimation(GetStateProgress(to), durationSec, easeType, completed: completed);
        }

        /// <summary>
        ///     Returns the state with a nearest progress with the current one.
        /// </summary>
        /// <returns></returns>
        public DrawerState GetNearestState()
        {
            var nearestState = DrawerState.Min;
            float nearestDistance = 1;
            foreach (var state in GetValidStates())
            {
                var distance = Mathf.Abs(GetStateProgress(state) - Progress);
                if (distance <= nearestDistance)
                {
                    nearestState = state;
                    nearestDistance = distance;
                }
            }

            return nearestState;
        }

        /// <summary>
        ///     Returns the state with a greater progress than the current one.
        ///     If current progress is 1.0, returns DrawerState.Max.
        /// </summary>
        /// <returns></returns>
        public DrawerState GetUpperState()
        {
            var upperState = DrawerState.Max;
            foreach (var state in GetValidStates().OrderByDescending(GetStateProgress))
            {
                if (GetStateProgress(state) <= Progress)
                    break;

                upperState = state;
            }

            return upperState;
        }

        /// <summary>
        ///     Returns the state with a lower progress than the current one.
        ///     If current progress is 0.0, returns DrawerState.Min.
        /// </summary>
        /// <returns></returns>
        public DrawerState GetLowerState()
        {
            var lowerState = DrawerState.Min;
            foreach (var state in GetValidStates().OrderBy(GetStateProgress))
            {
                if (GetStateProgress(state) >= Progress)
                    break;

                lowerState = state;
            }

            return lowerState;
        }

        public float GetStateProgress(DrawerState state)
        {
            switch (state)
            {
                case DrawerState.Min:
                    var min = Mathf.Min(minProgress, maxProgress);
                    if (useMiddleState) min = Mathf.Min(min, middleProgress);
                    return min;
                case DrawerState.Middle:
                    if (useMiddleState)
                    {
                        var middle = Mathf.Max(minProgress, middleProgress);
                        middle = Mathf.Min(middle, maxProgress);
                        return middle;
                    }
                    else
                    {
                        throw new Exception("The middle state progress is requested, but the Middle state is not enabled.");
                    }
                case DrawerState.Max:
                    var max = Mathf.Max(minProgress, maxProgress);
                    if (useMiddleState) max = Mathf.Max(max, middleProgress);
                    return max;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private IEnumerable<DrawerState> GetValidStates()
        {
            return Enum.GetValues(typeof(DrawerState)).Cast<DrawerState>().Where(x => useMiddleState || x != DrawerState.Middle);
        }
    }
}