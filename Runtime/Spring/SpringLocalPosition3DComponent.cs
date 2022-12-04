using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public class SpringLocalPosition3DComponent: BaseSpringComponent, ISpringTo<Vector3>, INudgeable<Vector3>
    {
        private SpringVector3 _spring;

        private void Awake()
        {
            var position = transform.localPosition;
            _spring = new SpringVector3() {StartValue = position, EndValue = position, Damping = damping, Stiffness = stiffness};
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
            if (M.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                Timing.RunCoroutine(HandleNudge(value).CancelWith(gameObject), gameObject.GetInstanceID());
            }
            else
            {
                _spring.UpdateEndValue(_spring.EndValue, _spring.CurrentVelocity + value);
            }
        }

        private IEnumerator<float> IeSpringToTarget(Vector3 target)
        {
            if (M.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                _spring.Reset();
                _spring.StartValue = transform.localPosition;
                _spring.EndValue = target;
            }
            else
            {
                _spring.UpdateEndValue(target, _spring.CurrentVelocity);
            }

            while (!M.Approximately(Vector3.SqrMagnitude(transform.localPosition - target), 0))
            {
                transform.localPosition = _spring.Evaluate(Time.deltaTime);

                yield return Timing.WaitForOneFrame;
            }

            _spring.Reset();
        }

        private IEnumerator<float> HandleNudge(Vector3 value)
        {
            _spring.Reset();
            var position = transform.localPosition;
            _spring.StartValue = position;
            _spring.EndValue = position;
            _spring.InitialVelocity = value;
            transform.localPosition = _spring.Evaluate(Time.deltaTime);

            while (!M.Approximately(0, Vector3.SqrMagnitude(position - transform.localPosition)))
            {
                transform.localPosition = _spring.Evaluate(Time.deltaTime);

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