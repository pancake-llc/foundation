using Pancake.Scriptable;
using PrimeTween;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    public class UIShowAfterWin : GameComponent
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private EFourDirection direction;
        [SerializeField] private ScriptableEventNoParam showEvent;
        [SerializeField] private float duration = 0.5f;

        private Vector2 _defaultPosition;

        protected override void OnEnabled() { showEvent.OnRaised += OnShow; }

        protected override void OnDisabled() { showEvent.OnRaised -= OnShow; }

        private void Start() { _defaultPosition = target.anchoredPosition; }

        private void OnShow()
        {
            switch (direction)
            {
                case EFourDirection.Top:
                    Tween.UIAnchoredPositionY(target, _defaultPosition.y, duration);
                    break;
                case EFourDirection.Down:
                    Tween.UIAnchoredPositionX(target, _defaultPosition.y, duration);
                    break;
                case EFourDirection.Left:
                    Tween.UIAnchoredPositionX(target, _defaultPosition.x, duration);
                    break;
                case EFourDirection.Right:
                    Tween.UIAnchoredPositionX(target, _defaultPosition.x, duration);
                    break;
            }
        }
    }
}