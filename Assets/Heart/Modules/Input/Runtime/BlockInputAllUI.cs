using Sisus.Init;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pancake.MobileInput
{
    [EditorIcon("icon_default")]
    public class BlockInputAllUI : MonoBehaviour<TouchInput>
    {
        private TouchInput _touchInput;
        private bool _wasTouchingLastFrame;

        protected override void Init(TouchInput argument) { _touchInput = argument; }

        private void Update()
        {
            if (IsPointerOverGameObject())
            {
                if (TouchWrapper.IsFingerDown && _wasTouchingLastFrame == false) _touchInput.IsTouchOnLockedArea = true;
            }

            _wasTouchingLastFrame = TouchWrapper.IsFingerDown;
        }

        private bool IsPointerOverGameObject()
        {
            if (EventSystem.current == null) return false;

            // Check mouse
            if (EventSystem.current.IsPointerOverGameObject()) return true;

            // Check touches
            for (var i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return true;
                }
            }

            return false;
        }
    }
}