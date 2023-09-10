using System.Collections;
using UnityEngine;

namespace Pancake.Spring
{
    [EditorIcon("script_spring")]
    public class SpringLocalScaleComponent : BaseSpringComponent, ISpringTo<Vector3>, INudgeable<Vector3>
    {
        private SpringVector3 _spring;
        private CoroutineHandle _handle;
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        private void Awake()
        {
            var localScale = transform.localScale;
            _spring = new SpringVector3() {StartValue = localScale, EndValue = localScale, Damping = damping, Stiffness = stiffness};
        }

        public void SpringTo(Vector3 target)
        {
            if (_handle is {IsDone: false}) StopCoroutine(_handle);
            CheckInspectorChanges();
            _handle = this.RunCoroutine(IeSpringToTarget(target));
        }

        public void Nudge(Vector3 value)
        {
            CheckInspectorChanges();
            if (Mathf.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                if (_handle is {IsDone: false}) StopCoroutine(_handle);
                _handle = this.RunCoroutine(IeHandleNudge(value));
            }
            else
            {
                _spring.UpdateEndValue(_spring.EndValue, _spring.CurrentVelocity + value);
            }
        }

        private IEnumerator IeHandleNudge(Vector3 value)
        {
            _spring.Reset();
            var localScale = transform.localScale;
            _spring.StartValue = localScale;
            _spring.EndValue = localScale;
            _spring.InitialVelocity = value;
            transform.localScale = _spring.Evaluate(Time.deltaTime);

            while (!Math.Approximately(0, Vector3.SqrMagnitude(localScale - transform.localScale)))
            {
                transform.localScale = _spring.Evaluate(Time.deltaTime);

                yield return _waitForEndOfFrame;
            }

            _spring.Reset();
        }

        private IEnumerator IeSpringToTarget(Vector3 target)
        {
            if (Math.Approximately(_spring.CurrentVelocity.sqrMagnitude, 0))
            {
                _spring.Reset();
                _spring.StartValue = transform.localScale;
                _spring.EndValue = target;
            }
            else
            {
                _spring.UpdateEndValue(target, _spring.CurrentVelocity);
            }

            while (!Math.Approximately(Vector3.SqrMagnitude(transform.localScale - target), 0))
            {
                transform.localScale = _spring.Evaluate(Time.deltaTime);

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