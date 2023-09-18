using System;

namespace Pancake.UI
{
    public static partial class Extension
    {
        public static UIPopup OnBeforeShow(this UIPopup popup, Action action)
        {
            popup.OnBeforeShowInternal().AddListener(ImplBeforeShow);
            return popup;

            void ImplBeforeShow()
            {
                action?.Invoke();
                popup.OnBeforeShowInternal().RemoveListener(ImplBeforeShow);
            }
        }

        public static UIPopup OnBeforeClose(this UIPopup popup, Action action)
        {
            popup.OnBeforeCloseInternal().AddListener(ImplBeforeClose);
            return popup;

            void ImplBeforeClose()
            {
                action?.Invoke();
                popup.OnBeforeCloseInternal().RemoveListener(ImplBeforeClose);
            }
        }

        public static UIPopup OnAfterShow(this UIPopup popup, Action action)
        {
            popup.OnAfterShowInternal().AddListener(ImplAfterShow);
            return popup;

            void ImplAfterShow()
            {
                action?.Invoke();
                popup.OnAfterShowInternal().RemoveListener(ImplAfterShow);
            }
        }

        public static UIPopup OnAfterClose(this UIPopup popup, Action action)
        {
            popup.OnAfterCloseInternal().AddListener(ImplAfterClose);
            return popup;

            void ImplAfterClose()
            {
                action?.Invoke();
                popup.OnAfterCloseInternal().RemoveListener(ImplAfterClose);
            }
        }
    }
}