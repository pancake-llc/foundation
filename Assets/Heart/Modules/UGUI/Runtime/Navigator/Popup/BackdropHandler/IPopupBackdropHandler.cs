using Cysharp.Threading.Tasks;

namespace Pancake.UI
{
    public interface IPopupBackdropHandler
    {
        UniTask BeforePopupEnterAsync(Popup popup, int popupIndex, bool playAnimation);

        void AfterPopupEnter(Popup popup, int popupIndex, bool playAnimation);

        UniTask BeforePopupExitAsync(Popup popup, int popupIndex, bool playAnimation);

        void AfterPopupExit(Popup popup, int popupIndex, bool playAnimation);
    }
}