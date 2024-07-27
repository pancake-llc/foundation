using Pancake.Common;
using Pancake.UI;

namespace Pancake.Game
{
    using UnityEngine;

    [EditorIcon("icon_default")]
    public class RequireInternetChecker : MonoBehaviour
    {
        [SerializeField] private float timeCheckAgain = 5f;
        [SerializeField, PopupPickup] private string noInternetPopupKey;

        public void Start()
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
                    var popupContainer = MainUIContainer.In.GetMain<PopupContainer>();
                    popupContainer.Popups.TryGetValue(noInternetPopupKey, out var popup);
                    if (popup == null) popupContainer.Push<NoInternetPopup>(noInternetPopupKey, true, popupId: noInternetPopupKey);
                }
            });
        }
    }
}