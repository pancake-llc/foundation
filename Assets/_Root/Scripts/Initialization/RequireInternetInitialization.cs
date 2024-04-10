using Pancake.Common;
using Pancake.UI;
using UnityEngine;

namespace Pancake.SceneFlow
{
    using Pancake;

    public class RequireInternetInitialization : Initialize
    {
        [SerializeField] private float timeCheckAgain = 5f;
        [SerializeField, PopupPickup] private string popupNoInternet;

        public override void Init()
        {
            if (!HeartSettings.RequireInternet) return;
            App.Delay(this, timeCheckAgain, OnUpdateCallback, isLooped: true);
        }

        private void OnUpdateCallback()
        {
            C.Network.CheckConnection(network =>
            {
                if (network != ENetworkStatus.Connected)
                {
                    var popupContainer = PopupContainer.Find(Constant.PERSISTENT_POPUP_CONTAINER);
                    popupContainer.Popups.TryGetValue(popupNoInternet, out var popup);
                    if (popup == null) popupContainer.Push<NoInternetPopup>(popupNoInternet, true, popupId: popupNoInternet);
                }
            });
        }
    }
}