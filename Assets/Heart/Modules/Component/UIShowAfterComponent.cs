#if PANCAKE_LITMOTION
using LitMotion;
using LitMotion.Extensions;
#endif
using UnityEngine;
#if PANCAKE_ROUTER
using VitalRouter;
#endif

namespace Pancake.Component
{
#if PANCAKE_ROUTER
    [Routes]
#endif
    [EditorIcon("icon_default")]
    public partial class UIShowAfterComponent : MonoBehaviour
    {
        [SerializeField] private StringConstant group;
        [SerializeField] private RectTransform target;
        [SerializeField] private EFourDirection direction;
#if PANCAKE_LITMOTION
        [SerializeField] private Ease ease;
#endif
        [SerializeField] private float duration = 0.5f;

        private Vector2 _defaultPosition;

#if PANCAKE_ROUTER
        private void OnEnable() { MapTo(Router.Default); }

        private void OnDisable() { UnmapRoutes(); }

        private void Start() { _defaultPosition = target.anchoredPosition; }

        public void OnShow(UIShowAfterCommand cmd)
        {
            if (!cmd.Group.Equals(group.Value)) return;
#if PANCAKE_LITMOTION
            switch (direction)
            {
                case EFourDirection.Top:
                    LMotion.Create(target.anchoredPosition.y, _defaultPosition.y, duration).WithEase(ease).BindToAnchoredPositionY(target).AddTo(target);
                    break;
                case EFourDirection.Down:
                    LMotion.Create(target.anchoredPosition.y, _defaultPosition.y, duration).WithEase(ease).BindToAnchoredPositionY(target).AddTo(target);
                    break;
                case EFourDirection.Left:
                    LMotion.Create(target.anchoredPosition.x, _defaultPosition.x, duration).WithEase(ease).BindToAnchoredPositionX(target).AddTo(target);
                    break;
                case EFourDirection.Right:
                    LMotion.Create(target.anchoredPosition.x, _defaultPosition.x, duration).WithEase(ease).BindToAnchoredPositionX(target).AddTo(target);
                    break;
            }
#endif
        }
#endif
    }
}