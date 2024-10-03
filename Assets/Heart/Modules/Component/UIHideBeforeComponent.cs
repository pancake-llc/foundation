using LitMotion;
using LitMotion.Extensions;
#if PANCAKE_ROUTER
using VitalRouter;
#endif

namespace Pancake.Component
{
    using UnityEngine;

#if PANCAKE_ROUTER
    [Routes]
#endif
    [EditorIcon("icon_default")]
    public partial class UIHideBeforeComponent : MonoBehaviour
    {
        [SerializeField] private StringConstant group;
        [SerializeField] private RectTransform target;
        [SerializeField] private EFourDirection direction;
        [SerializeField] private float value;
        [SerializeField] private Ease ease;
        [SerializeField] private float duration = 0.5f;

#if PANCAKE_ROUTER
        private void OnEnable() { MapTo(Router.Default); }

        private void OnDisable() { UnmapRoutes(); }

        public void OnHide(UIHideBeforeCommand cmd)
        {
            if (!cmd.Group.Equals(group.Value)) return;
            switch (direction)
            {
                case EFourDirection.Top:
                    LMotion.Create(target.anchoredPosition.y, value, duration).WithEase(ease).BindToAnchoredPositionY(target).AddTo(target);
                    break;
                case EFourDirection.Down:
                    LMotion.Create(target.anchoredPosition.y, -value, duration).WithEase(ease).BindToAnchoredPositionY(target).AddTo(target);
                    break;
                case EFourDirection.Left:
                    LMotion.Create(target.anchoredPosition.x, -value, duration).WithEase(ease).BindToAnchoredPositionX(target).AddTo(target);
                    break;
                case EFourDirection.Right:
                    LMotion.Create(target.anchoredPosition.x, value, duration).WithEase(ease).BindToAnchoredPositionX(target).AddTo(target);
                    break;
            }
        }
#endif
    }
}