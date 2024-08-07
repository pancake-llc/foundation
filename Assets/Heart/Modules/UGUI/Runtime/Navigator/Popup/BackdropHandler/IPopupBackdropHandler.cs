using Pancake.Common;

namespace Pancake.UI
{
    public interface IPopupBackdropHandler
    {
        AsyncProcessHandle BeforePopupEnter(Popup popup, int popupIndex, bool playAnimation);

        void AfterPopupEnter(Popup popup, int popupIndex, bool playAnimation);

        AsyncProcessHandle BeforePopupExit(Popup popup, int popupIndex, bool playAnimation);

        void AfterPopupExit(Popup popup, int popupIndex, bool playAnimation);
    }
}