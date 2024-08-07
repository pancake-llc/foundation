using Pancake.Common;
using UnityEngine;

namespace Pancake.UI
{
    /// <summary>
    ///     Implementation of <see cref="IPopupBackdropHandler" /> that changes the drawing order of a single backdrop and
    ///     reuses it
    /// </summary>
    internal sealed class ChangeOrderPopupBackdropHandler : IPopupBackdropHandler
    {
        public enum ChangeTiming
        {
            BeforeAnimation,
            AfterAnimation
        }

        private readonly ChangeTiming _changeTiming;
        private readonly PopupBackdrop _prefab;
        private PopupBackdrop _instance;

        public ChangeOrderPopupBackdropHandler(PopupBackdrop prefab, ChangeTiming changeTiming)
        {
            _prefab = prefab;
            _changeTiming = changeTiming;
        }

        public AsyncProcessHandle BeforePopupEnter(Popup popup, bool playAnimation)
        {
            var parent = (RectTransform) popup.transform.parent;
            int modalSiblingIndex = popup.transform.GetSiblingIndex();

            // If it is the first modal, generate a new backdrop
            if (modalSiblingIndex == 0)
            {
                var backdrop = Object.Instantiate(_prefab);
                backdrop.Setup(parent);
                backdrop.transform.SetSiblingIndex(0);
                _instance = backdrop;
                return backdrop.Enter(playAnimation);
            }

            // For the second and subsequent modals, change the drawing order of the backdrop
            int backdropSiblingIndex = modalSiblingIndex - 1;
            if (_changeTiming == ChangeTiming.BeforeAnimation) _instance.transform.SetSiblingIndex(backdropSiblingIndex);

            return AsyncProcessHandle.Completed();
        }

        public void AfterPopupEnter(Popup popup, bool playAnimation)
        {
            int modalSiblingIndex = popup.transform.GetSiblingIndex();
            int backdropSiblingIndex = modalSiblingIndex - 1;
            // For the second and subsequent modals, change the drawing order of the backdrop
            if (_changeTiming == ChangeTiming.AfterAnimation) _instance.transform.SetSiblingIndex(backdropSiblingIndex);
        }

        public AsyncProcessHandle BeforePopupExit(Popup popup, bool playAnimation)
        {
            int modalSiblingIndex = popup.transform.GetSiblingIndex();

            // If it is the first modal, play the backdrop animation
            if (modalSiblingIndex == 1) return _instance.Exit(playAnimation);

            // For the second and subsequent modals, change the drawing order of the backdrop
            if (_changeTiming == ChangeTiming.BeforeAnimation) _instance.transform.SetSiblingIndex(modalSiblingIndex - 2);

            return AsyncProcessHandle.Completed();
        }

        public void AfterPopupExit(Popup popup, bool playAnimation)
        {
            int modalSiblingIndex = popup.transform.GetSiblingIndex();

            // If it is the first modal, remove the backdrop
            if (modalSiblingIndex == 1)
            {
                Object.Destroy(_instance.gameObject);
                return;
            }

            // For the second and subsequent modals, change the drawing order of the backdrop
            if (_changeTiming == ChangeTiming.AfterAnimation) _instance.transform.SetSiblingIndex(modalSiblingIndex - 2);
        }
    }
}