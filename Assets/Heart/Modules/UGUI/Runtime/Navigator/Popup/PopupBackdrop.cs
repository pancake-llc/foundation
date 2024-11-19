#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using Pancake.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public class PopupBackdrop : GameComponent
    {
        [SerializeField] private TransitionAnimation enterAnimation;
        [SerializeField] private TransitionAnimation exitAnimation;

        [SerializeField] private bool closePopupWhenClicked;

        private CanvasGroup _canvasGroup;
        private RectTransform _parentTransform;
        private RectTransform _rectTransform;

        public ITransitionAnimation GetTransitionAnimation(bool enter)
        {
            var transitionAnimation = enter ? enterAnimation : exitAnimation;
            return transitionAnimation.GetAnimation();
        }

        private void Awake()
        {
            _rectTransform = (RectTransform) transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            if (closePopupWhenClicked)
            {
                if (!TryGetComponent<Image>(out var image))
                {
                    image = gameObject.AddComponent<Image>();
                    image.color = Color.clear;
                }

                if (!TryGetComponent<Button>(out var button))
                {
                    button = gameObject.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                }

                button.onClick.AddListener(() =>
                {
                    var popupContainer = PopupContainer.Of(transform);
                    if (popupContainer.IsInTransition) return;
#if PANCAKE_UNITASK
                    popupContainer.PopAsync(true).Forget();
#endif
                });
            }
        }

        public void Setup(RectTransform parent, int popupIndex)
        {
            _parentTransform = parent;
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.interactable = closePopupWhenClicked;
            OnSetup(_parentTransform, popupIndex);
            gameObject.SetActive(false);
        }

        protected virtual void OnSetup(RectTransform parentTransform, int popupIndex) { }

#if PANCAKE_UNITASK
        internal async UniTask EnterAsync(bool playAnimation)
        {
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.alpha = 1;

            if (playAnimation)
            {
                var anim = GetTransitionAnimation(true);
                if (anim == null) anim = DefaultNavigatorSetting.PopupBackdropEnter;

                if (anim.Duration > 0)
                {
                    anim.Setup(_rectTransform);
                    await anim.PlayWith();
                }
            }

            _rectTransform.FillWithParent(_parentTransform);
        }

        internal async UniTask ExitAsync(bool playAnimation)
        {
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.alpha = 1;

            if (playAnimation)
            {
                var anim = GetTransitionAnimation(false);
                if (anim == null) anim = DefaultNavigatorSetting.PopupBackdropExit;

                if (anim.Duration > 0)
                {
                    anim.Setup(_rectTransform);
                    await anim.PlayWith();
                }
            }

            _canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
#endif
    }
}