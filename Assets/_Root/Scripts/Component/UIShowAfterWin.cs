using LitMotion;
using LitMotion.Extensions;
using VitalRouter;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [Routes]
    public partial class UIShowAfterWin : GameComponent
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private EFourDirection direction;
        [SerializeField] private float duration = 0.5f;

        private Vector2 _defaultPosition;

        private void Start() { _defaultPosition = target.anchoredPosition; }

        public void OnShow(UIShowAfterWinCommand cmd)
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

    public struct UIShowAfterWinCommand : ICommand
    {
    }
}