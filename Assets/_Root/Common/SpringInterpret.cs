using System.Collections;
using UnityEngine;

namespace Pancake.Core
{
    public class SpringInterpret : BaseInterpret
    {
        private float _springVelocity;
        public float springAngularFrequency = 5f;
        public float springDampingRatio = 1f;
        public float maximumVelocity = -1f;
        public float fallAsleepDuration = 1f;

        public float SpringVelocity => _springVelocity;

        protected override void UpdateGoal()
        {
            if (routine == null) routine = StartCoroutine(ExecuteInterpret());
        }

        protected override IEnumerator ExecuteInterpret()
        {
            float sleepT = 0f;
            while (sleepT < fallAsleepDuration)
            {
                yield return null;
                float num = C.AdjustDeltaTime(Time.deltaTime, timeType);
                if (maximumVelocity > 0f)
                {
                    _springVelocity = M.Clamp(_springVelocity, -maximumVelocity, maximumVelocity);
                }

                C.CalcDampedSimpleHarmonicMotion(ref value,
                    ref _springVelocity,
                    goal,
                    num,
                    springAngularFrequency,
                    springDampingRatio);
                if (UpdatePosition())
                {
                    sleepT = 0f;
                }
                else
                {
                    sleepT += num;
                }
            }

            StopInterpret();
        }

        protected override void StopInterpret()
        {
            base.StopInterpret();
            _springVelocity = 0f;
        }

        public override void Nudge(float strength)
        {
            _springVelocity += strength;
            SetGoal(goal);
        }
    }
}