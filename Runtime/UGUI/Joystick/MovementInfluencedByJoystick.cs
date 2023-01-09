#if PANCAKE_INPUTSYSTEM
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace Pancake.UI
{
    public class MovementInfluencedByJoystick : BaseMono
    {
        public Pancake.UI.Joystick joystick;
        public float speed;
        private Transform _cacheTransform;

        private void Awake() { _cacheTransform = transform; }

        private void OnEnable()
        {
            MyInput.FingerDownEvent += OnFingerDown;
            MyInput.FingerUpEvent += OnFingerUp;
            MyInput.FingerMoveEvent += OnFingerMove;
        }

        private void OnDisable()
        {
            MyInput.FingerDownEvent -= OnFingerDown;
            MyInput.FingerUpEvent -= OnFingerUp;
            MyInput.FingerMoveEvent -= OnFingerMove;
        }

        protected void OnFingerMove(Finger finger)
        {
            if (joystick.Finger == finger)
            {
                Vector2 knobPosition;
                float radius = joystick.size.x / 2f;
                ETouch.Touch currentTouch = finger.currentTouch;
                if (Vector2.Distance(currentTouch.screenPosition, joystick.RectTransform.anchoredPosition) > radius)
                {
                    knobPosition = (currentTouch.screenPosition - joystick.RectTransform.anchoredPosition).normalized * radius;
                }
                else
                {
                    knobPosition = currentTouch.screenPosition - joystick.RectTransform.anchoredPosition;
                }

                joystick.knob.anchoredPosition = knobPosition;
                joystick.MovementDelta = knobPosition / radius;
                joystick.OnFingerMove();
            }
        }

        protected void OnFingerUp(Finger finger)
        {
            if (joystick.Finger == finger)
            {
                joystick.Finger = null;
                joystick.knob.anchoredPosition = Vector2.zero;
                joystick.MovementDelta = Vector2.zero;
                joystick.OnFingerUp();
            }
        }

        protected void OnFingerDown(Finger finger)
        {
            if (joystick.Finger == null)
            {
                joystick.Finger = finger;
                joystick.MovementDelta = Vector2.zero;
                joystick.OnFingerDown();
            }
        }

        private void Update()
        {
            var scaledMovement = speed * Time.deltaTime * new Vector3(joystick.MovementDelta.x, 0, joystick.MovementDelta.y);
            var position = _cacheTransform.position;
            transform.LookAt(position + scaledMovement, Vector3.up);
            position += scaledMovement;
            _cacheTransform.position = position;
        }
    }
}
#endif