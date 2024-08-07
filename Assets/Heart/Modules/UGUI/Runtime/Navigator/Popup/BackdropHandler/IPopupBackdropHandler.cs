using Pancake.Common;

namespace Pancake.UI
{
    public interface IPopupBackdropHandler
    {
        AsyncProcessHandle BeforePopupEnter(Popup popup, bool playAnimation);

        void AfterPopupEnter(Popup popup, bool playAnimation);

        AsyncProcessHandle BeforePopupExit(Popup popup, bool playAnimation);

        void AfterPopupExit(Popup popup, bool playAnimation);
    }
}