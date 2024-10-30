#if PANCAKE_LITMOTION
using LitMotion;
using LitMotion.Extensions;
#endif
using UnityEngine;

namespace Pancake.Component
{
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
        private MessageBinding<UIShowAfterMessage> _binding;

        private void OnEnable()
        {
            _binding ??= new MessageBinding<UIShowAfterMessage>(OnShow);
            _binding.Listen = true;
        }

        private void OnDisable() { _binding.Listen = false; }

        private void Start() { _defaultPosition = target.anchoredPosition; }

        public void OnShow(UIShowAfterMessage msg)
        {
            if (!msg.Group.Equals(group.Value)) return;
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
    }
}