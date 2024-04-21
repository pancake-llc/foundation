using LitMotion;
using LitMotion.Extensions;
using Pancake.Scriptable;

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

        protected void OnEnable() { showEvent.OnRaised += OnShow; }

        protected void OnDisable() { showEvent.OnRaised -= OnShow; }

        private void Start() { _defaultPosition = target.anchoredPosition; }

        private void OnShow()
        {
            switch (direction)
            {
                case EFourDirection.Top:
                    LMotion.Create(target.anchoredPosition.y, _defaultPosition.y, duration).BindToAnchoredPositionY(target);
                    break;
                case EFourDirection.Down:
                    LMotion.Create(target.anchoredPosition.y, _defaultPosition.y, duration).BindToAnchoredPositionY(target);
                    break;
                case EFourDirection.Left:
                    LMotion.Create(target.anchoredPosition.x, _defaultPosition.x, duration).BindToAnchoredPositionX(target);
                    break;
                case EFourDirection.Right:
                    LMotion.Create(target.anchoredPosition.x, _defaultPosition.x, duration).BindToAnchoredPositionX(target);
                    break;
            }
        }
    }
}