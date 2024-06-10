using LitMotion;
using LitMotion.Extensions;
using VitalRouter;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [Routes]
    public partial class UIHideBeforeWin : GameComponent
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private EFourDirection direction;
        [SerializeField] private float value;
        [SerializeField] private float duration = 0.5f;

        public void OnHide(HideUIBeforeWinCommand cmd)
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

    public struct HideUIBeforeWinCommand : ICommand
    {
    }
}