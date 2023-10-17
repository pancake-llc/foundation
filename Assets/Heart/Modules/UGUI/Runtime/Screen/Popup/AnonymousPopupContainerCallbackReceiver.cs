using System;

namespace Pancake.UI
{
    public sealed class AnonymousPopupContainerCallbackReceiver : IPopupContainerCallbackReceiver
    {
        public event Action<(Popup enter, Popup exit)> OnAfterPop;
        public event Action<(Popup enter, Popup exit)> OnAfterPush;
        public event Action<(Popup enter, Popup exit)> OnBeforePop;
        public event Action<(Popup enter, Popup exit)> OnBeforePush;

        public AnonymousPopupContainerCallbackReceiver(
            Action<(Popup enter, Popup exit)> onBeforePush = null,
            Action<(Popup enter, Popup exit)> onAfterPush = null,
            Action<(Popup enter, Popup exit)> onBeforePop = null,
            Action<(Popup enter, Popup exit)> onAfterPop = null)
        {
            OnBeforePush = onBeforePush;
            OnAfterPush = onAfterPush;
            OnBeforePop = onBeforePop;
            OnAfterPop = onAfterPop;
        }

        public void BeforePush(Popup enter, Popup exit) { OnBeforePush?.Invoke((enter, exit)); }

        public void AfterPush(Popup enter, Popup exit) { OnAfterPush?.Invoke((enter, exit)); }

        public void BeforePop(Popup enter, Popup exit) { OnBeforePop?.Invoke((enter, exit)); }

        public void AfterPop(Popup enter, Popup exit) { OnAfterPop?.Invoke((enter, exit)); }
    }
}