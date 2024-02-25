using UnityEngine;

namespace Pancake.DebugView
{
    public abstract class InputBasedFlickListenerBase : FlickListenerBase
    {
        protected override bool ClickStarted()
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (Input.touchCount >= 1) return Input.GetTouch(0).phase == TouchPhase.Began;
            return false;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        protected override bool ClickFinished()
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (Input.touchCount >= 1)return Input.GetTouch(0).phase == TouchPhase.Ended;

            return false;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        protected override bool TryGetClickedPosition(out Vector2 position)
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (Input.touchCount >= 1)
            {
                position = Input.GetTouch(0).position;
                return true;
            }
#else
            if (Input.GetMouseButton(0))
            {
                position = Input.mousePosition;
                return true;
            }
#endif

            position = default;
            return false;
        }
    }
}