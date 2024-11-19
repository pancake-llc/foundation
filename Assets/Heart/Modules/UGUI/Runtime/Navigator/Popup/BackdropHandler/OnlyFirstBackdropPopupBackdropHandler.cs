#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace Pancake.UI
{
    /// <summary>
    ///     Implementation of <see cref="IPopupBackdropHandler" /> that generates a backdrop only for the first popup
    /// </summary>
    internal sealed class OnlyFirstBackdropPopupBackdropHandler : IPopupBackdropHandler
    {
        private readonly PopupBackdrop _prefab;

        public OnlyFirstBackdropPopupBackdropHandler(PopupBackdrop prefab) { _prefab = prefab; }

#if PANCAKE_UNITASK
        public async UniTask BeforePopupEnterAsync(Popup popup, int popupIndex, bool playAnimation)
        {
            var parent = (RectTransform) popup.transform.parent;

            // Do not generate a backdrop for the first popup
            if (popupIndex != 0) return;

            var backdrop = Object.Instantiate(_prefab);
            backdrop.Setup(parent, popupIndex);
            backdrop.transform.SetSiblingIndex(0);
            await backdrop.EnterAsync(playAnimation);
        }

        public async UniTask BeforePopupExitAsync(Popup popup, int popupIndex, bool playAnimation)
        {
            // Do not remove the backdrop for the first popup
            if (popupIndex != 0) return;

            var backdrop = popup.transform.parent.GetChild(0).GetComponent<PopupBackdrop>();
            await backdrop.ExitAsync(playAnimation);
        }
#endif

        public void AfterPopupEnter(Popup popup, int popupIndex, bool playAnimation) { }

        public void AfterPopupExit(Popup popup, int popupIndex, bool playAnimation)
        {
            // Do not remove the backdrop for the first popup
            if (popupIndex != 0) return;

            var backdrop = popup.transform.parent.GetChild(0).GetComponent<PopupBackdrop>();
            Object.Destroy(backdrop.gameObject);
        }
    }
}