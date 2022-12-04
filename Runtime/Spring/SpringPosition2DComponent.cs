using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public class SpringPosition2DComponent : BaseSpringComponent, ISpringTo<Vector2>, INudgeable<Vector2>
    {
        private SpringVector2 _spring;

        private void Awake()
        {
            var position = transform.position;
            _spring = new SpringVector2() {StartValue = position, EndValue = position, Damping = damping, Stiffness = stiffness};
        }

        public void SpringTo(Vector2 target)
        {
            Timing.KillCoroutines(gameObject.GetInstanceID());
            CheckInspectorChanges();
            Timing.RunCoroutine(IeSpringToTarget(target).CancelWith(gameObject), gameObject.GetInstanceID());
        }

        public void Nudge(Vector2 value)
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

        private IEnumerator<float> IeSpringToTarget(Vector2 target)
        {
            if (M.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                _spring.Reset();
                _spring.StartValue = transform.position;
                _spring.EndValue = target;
            }
            else
            {
                _spring.UpdateEndValue(target, _spring.CurrentVelocity);
            }

            while (!M.Approximately(Vector2.SqrMagnitude(new Vector2(transform.position.x, transform.position.y) - target), 0))
            {
                transform.position = _spring.Evaluate(Time.deltaTime);

                yield return Timing.WaitForOneFrame;
            }

            _spring.Reset();
        }

        private IEnumerator<float> IeHandleNudge(Vector2 value)
        {
            _spring.Reset();
            var position = transform.position;
            _spring.StartValue = position;
            _spring.EndValue = position;
            _spring.InitialVelocity = value;
            transform.position = _spring.Evaluate(Time.deltaTime);

            while (!M.Approximately(0, Vector2.SqrMagnitude(position - transform.position)))
            {
                transform.position = _spring.Evaluate(Time.deltaTime);

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