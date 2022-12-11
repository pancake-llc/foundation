#if PANCAKE_INPUTSYSTEM
using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace Pancake.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Joystick : BaseBehaviour
    {
        private RectTransform _rectTransform;
        public RectTransform knob;
        [OnValueChanged(nameof(OnSizeChanged))] public Vector2 size = new Vector2(200, 200);

        public Finger Finger { get; set; }
        public Vector2 MovementDelta { get; set; }
        
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private void OnSizeChanged() { RectTransform.sizeDelta = size; }
        
        protected virtual void Start() { }

        private Vector2 ClampStartPosition(Vector2 startPosition)
        {
            if (startPosition.x < size.x / 2)
            {
                startPosition.x = size.x / 2;
            }

            if (startPosition.y < size.y / 2)
            {
                startPosition.y = size.y / 2;
            }
            else if (startPosition.y > Screen.height - size.y / 2)
            {
                startPosition.y = Screen.height - size.y / 2;
            }

            return startPosition;
        }

        protected void ClampPosition(Vector2 startPosition) { RectTransform.anchoredPosition = ClampStartPosition(startPosition); }

        public virtual void OnFingerUp() { }

        public virtual void OnFingerDown() { }

        public virtual void OnFingerMove() { }
    }
}
#endif