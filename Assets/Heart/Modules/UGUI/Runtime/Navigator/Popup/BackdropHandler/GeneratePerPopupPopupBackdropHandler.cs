using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    /// <summary>
    ///     Implementation of <see cref="IPopupBackdropHandler" /> that generates a backdrop for each popup
    /// </summary>
    internal sealed class GeneratePerPopupPopupBackdropHandler : IPopupBackdropHandler
    {
        private readonly PopupBackdrop _prefab;

        public GeneratePerPopupPopupBackdropHandler(PopupBackdrop prefab) { _prefab = prefab; }

        public async UniTask BeforePopupEnterAsync(Popup popup, int popupIndex, bool playAnimation)
        {
            var parent = (RectTransform) popup.transform.parent;
            var backdrop = Object.Instantiate(_prefab);
            backdrop.Setup(parent, popupIndex);
            int backdropSiblingIndex = popupIndex * 2;
            backdrop.transform.SetSiblingIndex(backdropSiblingIndex);
            await backdrop.EnterAsync(playAnimation);
        }

        public void AfterPopupEnter(Popup popup, int popupIndex, bool playAnimation) { }

        public async UniTask BeforePopupExitAsync(Popup popup, int popupIndex, bool playAnimation)
        {
            var backdropSiblingIndex = popupIndex * 2;
            var backdrop = popup.transform.parent.GetChild(backdropSiblingIndex).GetComponent<PopupBackdrop>();
            await backdrop.ExitAsync(playAnimation);
        }

        public void AfterPopupExit(Popup popup, int popupIndex, bool playAnimation)
        {
            var backdropSiblingIndex = popupIndex * 2;
            var backdrop = popup.transform.parent.GetChild(backdropSiblingIndex).GetComponent<PopupBackdrop>();
            Object.Destroy(backdrop.gameObject);
        }
    }
}