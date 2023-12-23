using System.Collections;
using UnityEngine;

namespace Pancake.Spring
{
    [EditorIcon("script_spring")]
    public class SpringLocalPosition2DComponent : BaseSpringComponent, ISpringTo<Vector2>, INudgeable<Vector2>
    {
        private SpringVector2 _spring;
        private Coroutine _handle;
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        private void Awake()
        {
            var position = transform.localPosition;
            _spring = new SpringVector2() {StartValue = position, EndValue = position, Damping = damping, Stiffness = stiffness};
        }

        public void SpringTo(Vector2 target)
        {
            if (_handle != null) StopCoroutine(_handle);

            CheckInspectorChanges();
            _handle = StartCoroutine(IeSpringToTarget(target));
        }

        public void Nudge(Vector2 value)
        {
            CheckInspectorChanges();
            if (_spring.CurrentVelocity.sqrMagnitude.Approximately(0))
            {
                if (_handle != null) StopCoroutine(_handle);
                _handle = StartCoroutine(IeHandleNudge(value));
            }
            else
            {
                _spring.UpdateEndValue(_spring.EndValue, _spring.CurrentVelocity + value);
            }
        }

        private IEnumerator IeSpringToTarget(Vector2 target)
        {
            if (Math.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                _spring.Reset();
                _spring.StartValue = transform.localPosition;
                _spring.EndValue = target;
            }
            else
            {
                _spring.UpdateEndValue(target, _spring.CurrentVelocity);
            }

            while (!Math.Approximately(Vector2.SqrMagnitude(new Vector2(transform.localPosition.x, transform.localPosition.y) - target), 0))
            {
                transform.localPosition = _spring.Evaluate(Time.deltaTime);

                yield return _waitForEndOfFrame;
            }

            _spring.Reset();
        }

        private IEnumerator IeHandleNudge(Vector2 value)
        {
            _spring.Reset();
            var position = transform.localPosition;
            _spring.StartValue = position;
            _spring.EndValue = position;
            _spring.InitialVelocity = value;
            transform.localPosition = _spring.Evaluate(Time.deltaTime);

            while (!Math.Approximately(0, Vector2.SqrMagnitude(position - transform.localPosition)))
            {
                transform.localPosition = _spring.Evaluate(Time.deltaTime);

                yield return _waitForEndOfFrame;
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