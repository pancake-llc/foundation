using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public class SpringScaleComponent : BaseSpringComponent, ISpringTo<Vector3>, INudgeable<Vector3>
    {
        private SpringVector3 _spring;

        private void Awake()
        {
            var localScale = transform.localScale;
            _spring = new SpringVector3() {StartValue = localScale, EndValue = localScale, Damping = damping, Stiffness = stiffness};
        }

        public void SpringTo(Vector3 target)
        {
            Timing.KillCoroutines(gameObject.GetInstanceID());
            CheckInspectorChanges();
            Timing.RunCoroutine(IeSpringToTarget(target).CancelWith(gameObject), gameObject.GetInstanceID());
        }

        public void Nudge(Vector3 value)
        {
            CheckInspectorChanges();
            if (Mathf.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                Timing.RunCoroutine(IeHandleNudge(value).CancelWith(gameObject), gameObject.GetInstanceID());
            }
            else
            {
                _spring.UpdateEndValue(_spring.EndValue, _spring.CurrentVelocity + value);
            }
        }

        private IEnumerator<float> IeHandleNudge(Vector3 value)
        {
            _spring.Reset();
            var localScale = transform.localScale;
            _spring.StartValue = localScale;
            _spring.EndValue = localScale;
            _spring.InitialVelocity = value;
            transform.localScale = _spring.Evaluate(Time.deltaTime);

            while (!M.Approximately(0, Vector3.SqrMagnitude(localScale - transform.localScale)))
            {
                transform.localScale = _spring.Evaluate(Time.deltaTime);

                yield return Timing.WaitForOneFrame;
            }

            _spring.Reset();
        }

        private IEnumerator<float> IeSpringToTarget(Vector3 target)
        {
            if (M.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                _spring.Reset();
                _spring.StartValue = transform.localScale;
                _spring.EndValue = target;
            }
            else
            {
                _spring.UpdateEndValue(target, _spring.CurrentVelocity);
            }

            while (!M.Approximately(Vector3.SqrMagnitude(transform.localScale - target), 0))
            {
                transform.localScale = _spring.Evaluate(Time.deltaTime);

                yield return Timing.WaitForOneFrame;
            }

            _spring.Reset();
        }

        private void CheckInspectorChanges()
        {
            _spring.Damping = damping;
            _spring.Stiffness = stiffness;
        }
    }
}