using LitMotion;
using LitMotion.Extensions;
using Pancake.Scriptable;

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

        protected void OnEnable() { hideEvent.OnRaised += OnHide; }

        protected void OnDisable() { hideEvent.OnRaised -= OnHide; }

        private void OnHide()
        {
            switch (direction)
            {
                case EFourDirection.Top:
                    LMotion.Create(target.anchoredPosition.y, value, duration).BindToAnchoredPositionY(target);
                    break;
                case EFourDirection.Down:
                    LMotion.Create(target.anchoredPosition.y, -value, duration).BindToAnchoredPositionY(target);
                    break;
                case EFourDirection.Left:
                    LMotion.Create(target.anchoredPosition.x, -value, duration).BindToAnchoredPositionX(target);
                    break;
                case EFourDirection.Right:
                    LMotion.Create(target.anchoredPosition.x, value, duration).BindToAnchoredPositionX(target);
                    break;
            }
        }
    }
}