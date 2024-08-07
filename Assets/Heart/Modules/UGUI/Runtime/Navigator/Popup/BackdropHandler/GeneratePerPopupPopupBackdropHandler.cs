using Pancake.Common;
using UnityEngine;

namespace Pancake.UI
{
    /// <summary>
    ///     en: Implementation of <see cref="IPopupBackdropHandler" /> that generates a backdrop for each modal
    /// </summary>
    internal sealed class GeneratePerPopupPopupBackdropHandler : IPopupBackdropHandler
    {
        private readonly PopupBackdrop _prefab;

        public GeneratePerPopupPopupBackdropHandler(PopupBackdrop prefab) { _prefab = prefab; }

        public AsyncProcessHandle BeforePopupEnter(Popup popup, bool playAnimation)
        {
            var parent = (RectTransform) popup.transform.parent;
            int siblingIndex = popup.transform.GetSiblingIndex();
            var backdrop = Object.Instantiate(_prefab);
            backdrop.Setup(parent);
            backdrop.transform.SetSiblingIndex(siblingIndex);
            return backdrop.Enter(playAnimation);
        }

        public void AfterPopupEnter(Popup popup, bool playAnimation) { }

        public AsyncProcessHandle BeforePopupExit(Popup popup, bool playAnimation)
        {
            int modalSiblingIndex = popup.transform.GetSiblingIndex();
            int backdropSiblingIndex = modalSiblingIndex - 1;
            var backdrop = popup.transform.parent.GetChild(backdropSiblingIndex).GetComponent<PopupBackdrop>();
            return backdrop.Exit(playAnimation);
        }

        public void AfterPopupExit(Popup popup, bool playAnimation)
        {
            int modalSiblingIndex = popup.transform.GetSiblingIndex();
            int backdropSiblingIndex = modalSiblingIndex - 1;
            var backdrop = popup.transform.parent.GetChild(backdropSiblingIndex).GetComponent<PopupBackdrop>();
            Object.Destroy(backdrop.gameObject);
        }
    }
}