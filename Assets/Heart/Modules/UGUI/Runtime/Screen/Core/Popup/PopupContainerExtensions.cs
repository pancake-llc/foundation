using System;

namespace Pancake.UI
{
    public static class PopupContainerExtensions
    {
        /// <summary>
        ///     Add callbacks.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="onBeforePush"></param>
        /// <param name="onAfterPush"></param>
        /// <param name="onBeforePop"></param>
        /// <param name="onAfterPop"></param>
        public static void AddCallbackReceiver(
            this PopupContainer self,
            Action<(Popup enter, Popup exit)> onBeforePush = null,
            Action<(Popup enter, Popup exit)> onAfterPush = null,
            Action<(Popup enter, Popup exit)> onBeforePop = null,
            Action<(Popup enter, Popup exit)> onAfterPop = null)
        {
            var callbackReceiver = new AnonymousPopupContainerCallbackReceiver(onBeforePush, onAfterPush, onBeforePop, onAfterPop);
            self.AddCallbackReceiver(callbackReceiver);
        }

        /// <summary>
        ///     Add callbacks.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="popup"></param>
        /// <param name="onBeforePush"></param>
        /// <param name="onAfterPush"></param>
        /// <param name="onBeforePop"></param>
        /// <param name="onAfterPop"></param>
        public static void AddCallbackReceiver(
            this PopupContainer self,
            Popup popup,
            Action<Popup> onBeforePush = null,
            Action<Popup> onAfterPush = null,
            Action<Popup> onBeforePop = null,
            Action<Popup> onAfterPop = null)
        {
            var callbackReceiver = new AnonymousPopupContainerCallbackReceiver();
            callbackReceiver.OnBeforePush += x =>
            {
                var (enterPopup, exitPopup) = x;
                if (enterPopup.Equals(popup)) onBeforePush?.Invoke(exitPopup);
            };
            callbackReceiver.OnAfterPush += x =>
            {
                var (enterPopup, exitPopup) = x;
                if (enterPopup.Equals(popup)) onAfterPush?.Invoke(exitPopup);
            };
            callbackReceiver.OnBeforePop += x =>
            {
                var (enterPopup, exitPopup) = x;
                if (exitPopup.Equals(popup)) onBeforePop?.Invoke(enterPopup);
            };
            callbackReceiver.OnAfterPop += x =>
            {
                var (enterPopup, exitPopup) = x;
                if (exitPopup.Equals(popup)) onAfterPop?.Invoke(enterPopup);
            };

            var gameObj = self.gameObject;
            if (!gameObj.TryGetComponent<MonoBehaviourDestroyedTracker>(out var destroyedEventDispatcher))
            {
                destroyedEventDispatcher = gameObj.AddComponent<MonoBehaviourDestroyedTracker>();
            }

            destroyedEventDispatcher.Callback += () => self.RemoveCallbackReceiver(callbackReceiver);

            self.AddCallbackReceiver(callbackReceiver);
        }
    }
}