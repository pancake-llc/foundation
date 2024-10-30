#if PANCAKE_LITMOTION
using LitMotion;
using LitMotion.Extensions;
#endif
using UnityEngine;

namespace Pancake.Component
{
    [EditorIcon("icon_default")]
    public class UIHideBeforeComponent : MonoBehaviour
    {
        [SerializeField] private StringConstant group;
        [SerializeField] private RectTransform target;
        [SerializeField] private EFourDirection direction;
        [SerializeField] private float value;
#if PANCAKE_LITMOTION
        [SerializeField] private Ease ease;
#endif
        [SerializeField] private float duration = 0.5f;

        private MessageBinding<UIHideBeforeMessage> _binding;

        private void OnEnable()
        {
            _binding ??= new MessageBinding<UIHideBeforeMessage>(OnHide);
            _binding.Listen = true;
        }

        private void OnDisable() { _binding.Listen = false; }

        private void OnHide(UIHideBeforeMessage msg)
        {
            if (!msg.Group.Equals(group.Value)) return;
#if PANCAKE_LITMOTION
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
#endif
        }
    }
}