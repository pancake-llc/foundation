using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
#if PANCAKE_ROUTER
using VitalRouter;
#endif

namespace Pancake.Component
{
    [Routes]
    [EditorIcon("icon_default")]
    public partial class UIShowAfterComponent : MonoBehaviour
    {
        [SerializeField] private StringConstant group;
        [SerializeField] private RectTransform target;
        [SerializeField] private EFourDirection direction;
        [SerializeField] private Ease ease;
        [SerializeField] private float duration = 0.5f;

        private Vector2 _defaultPosition;

#if PANCAKE_ROUTER
        private void OnEnable() { MapTo(Router.Default); }

        private void OnDisable() { UnmapRoutes(); }

        private void Start() { _defaultPosition = target.anchoredPosition; }

        public void OnShow(UIShowAfterCommand cmd)
        {
            if (!cmd.Group.Equals(group.Value)) return;
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
        }
#endif
    }
}