using Pancake.Common;
using UnityEngine;

namespace Pancake.UI
{
    /// <summary>
    ///     Implementation of <see cref="IPopupBackdropHandler" /> that generates a backdrop only for the first modal
    /// </summary>
    internal sealed class OnlyFirstBackdropPopupBackdropHandler : IPopupBackdropHandler
    {
        private readonly PopupBackdrop _prefab;

        public OnlyFirstBackdropPopupBackdropHandler(PopupBackdrop prefab) { _prefab = prefab; }

        public AsyncProcessHandle BeforePopupEnter(Popup popup, bool playAnimation)
        {
            var parent = (RectTransform) popup.transform.parent;
            int modalSiblingIndex = popup.transform.GetSiblingIndex();

            // Do not generate a backdrop for the first modal
            if (modalSiblingIndex != 0) return AsyncProcessHandle.Completed();

            var backdrop = Object.Instantiate(_prefab);
            backdrop.Setup(parent);
            backdrop.transform.SetSiblingIndex(0);
            return backdrop.Enter(playAnimation);
        }

        public void AfterPopupEnter(Popup popup, bool playAnimation) { }

        public AsyncProcessHandle BeforePopupExit(Popup popup, bool playAnimation)
        {
            int modalSiblingIndex = popup.transform.GetSiblingIndex();

            // Do not remove the backdrop for the first modal
            if (modalSiblingIndex != 1) return AsyncProcessHandle.Completed();

            var backdrop = popup.transform.parent.GetChild(0).GetComponent<PopupBackdrop>();
            return backdrop.Exit(playAnimation);
        }

        public void AfterPopupExit(Popup popup, bool playAnimation)
        {
            int modalSiblingIndex = popup.transform.GetSiblingIndex();

            // Do not remove the backdrop for the first modal
            if (modalSiblingIndex != 1) return;

            var backdrop = popup.transform.parent.GetChild(0).GetComponent<PopupBackdrop>();
            Object.Destroy(backdrop.gameObject);
        }
    }
}