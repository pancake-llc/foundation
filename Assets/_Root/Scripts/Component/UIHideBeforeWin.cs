using Pancake.Scriptable;
using PrimeTween;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    public class UIHideBeforeWin : GameComponent
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private EFourDirection direction;
        [SerializeField] private float value;
        [SerializeField] private ScriptableEventNoParam hideEvent;
        [SerializeField] private float duration = 0.5f;

        protected override void OnEnabled() { hideEvent.OnRaised += OnHide; }

        protected override void OnDisabled() { hideEvent.OnRaised -= OnHide; }

        private void OnHide()
        {
            switch (direction)
            {
                case EFourDirection.Top:
                    Tween.UIAnchoredPositionY(target, value, duration);
                    break;
                case EFourDirection.Down:
                    Tween.UIAnchoredPositionX(target, -value, duration);
                    break;
                case EFourDirection.Left:
                    Tween.UIAnchoredPositionX(target, -value, duration);
                    break;
                case EFourDirection.Right:
                    Tween.UIAnchoredPositionX(target, value, duration);
                    break;
            }
        }
    }
}