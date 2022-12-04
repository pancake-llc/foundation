using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public class SpringRotationComponent : BaseSpringComponent, ISpringTo<Vector3>, ISpringTo<Quaternion>, INudgeable<Vector3>, INudgeable<Quaternion>
    {
        private SpringVector3 _spring;

        private void Awake()
        {
            var rotation = transform.rotation;
            _spring = new SpringVector3() {StartValue = rotation.eulerAngles, EndValue = rotation.eulerAngles, Damping = damping, Stiffness = stiffness};
        }

        public void SpringTo(Vector3 target) { SpringTo(Quaternion.Euler(target)); }

        public void SpringTo(Quaternion target)
        {
            Timing.KillCoroutines(gameObject.GetInstanceID());
            CheckInspectorChanges();
            Timing.RunCoroutine(IeSpringToTarget(target).CancelWith(gameObject), gameObject.GetInstanceID());
        }

        public void Nudge(Vector3 value)
        {
            CheckInspectorChanges();
            if (M.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                Timing.RunCoroutine(IeHandleNudge(value).CancelWith(gameObject), gameObject.GetInstanceID());
            }
            else
            {
                _spring.UpdateEndValue(_spring.EndValue, _spring.CurrentVelocity + value);
            }
        }

        public void Nudge(Quaternion value) { Nudge(value.eulerAngles); }

        private IEnumerator<float> IeSpringToTarget(Quaternion target)
        {
            if (M.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                _spring.Reset();
                _spring.StartValue = transform.eulerAngles;
                _spring.EndValue = target.eulerAngles;
            }
            else
            {
                _spring.UpdateEndValue(target.eulerAngles, _spring.CurrentVelocity);
            }

            while (!M.Approximately(0, 1f - Quaternion.Dot(transform.rotation, target)))
            {
                transform.rotation = Quaternion.Euler(_spring.Evaluate(Time.deltaTime));
                yield return Timing.WaitForOneFrame;
            }

            _spring.Reset();
        }

        private IEnumerator<float> IeHandleNudge(Vector3 value)
        {
            _spring.Reset();
            var rotation = transform.rotation;
            _spring.StartValue = rotation.eulerAngles;
            _spring.EndValue = rotation.eulerAngles;
            _spring.InitialVelocity = value;
            transform.rotation = Quaternion.Euler(_spring.Evaluate(Time.deltaTime));

            while (!M.Approximately(0, 1f - Quaternion.Dot(rotation, transform.rotation)))
            {
                transform.rotation = Quaternion.Euler(_spring.Evaluate(Time.deltaTime));

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