#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.UI
{
    public interface IPopupBackdropHandler
    {
#if PANCAKE_UNITASK
        UniTask BeforePopupEnterAsync(Popup popup, int popupIndex, bool playAnimation);

        UniTask BeforePopupExitAsync(Popup popup, int popupIndex, bool playAnimation);
#endif

        void AfterPopupEnter(Popup popup, int popupIndex, bool playAnimation);

        void AfterPopupExit(Popup popup, int popupIndex, bool playAnimation);
    }
}