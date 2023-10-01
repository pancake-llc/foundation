using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI.Popup
{
    public class ModalBackdrop : GameComponent
    {
        [SerializeField] private ModalBackdropTransitionContainer animationContainer;
        [SerializeField] private bool closeModalWhenClicked;

        private CanvasGroup _canvasGroup;
        private RectTransform _parentTransform;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform) transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            if (closeModalWhenClicked)
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
                    var modalContainer = ModalContainer.Of(transform);
                    if (modalContainer.IsInTransition) return;
                    modalContainer.Pop(true);
                });
            }
        }

        public void Setup(RectTransform parentTransform)
        {
            _parentTransform = parentTransform;
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.interactable = closeModalWhenClicked;
            gameObject.SetActive(false);
        }

        internal AsyncProcessHandle Enter(bool playAnimation) { return App.StartCoroutine(EnterRoutine(playAnimation)); }

        private IEnumerator EnterRoutine(bool playAnimation)
        {
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.alpha = 1;

            if (playAnimation)
            {
                var anim = animationContainer.GetAnimation(true);
                if (anim == null) anim = DefaultPopupSetting.ModalBackdropEnter;

                if (anim.Duration > 0)
                {
                    anim.Setup(_rectTransform);
                    yield return App.StartCoroutine(anim.CreateRoutine());
                }
            }

            _rectTransform.FillWithParent(_parentTransform);
        }

        internal AsyncProcessHandle Exit(bool playAnimation) { return App.StartCoroutine(ExitRoutine(playAnimation)); }

        private IEnumerator ExitRoutine(bool playAnimation)
        {
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.alpha = 1;

            if (playAnimation)
            {
                var anim = animationContainer.GetAnimation(false);
                if (anim == null) anim = DefaultPopupSetting.ModalBackdropExit;

                if (anim.Duration > 0)
                {
                    anim.Setup(_rectTransform);
                    yield return App.StartCoroutine(anim.CreateRoutine());
                }
            }

            _canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
    }
}