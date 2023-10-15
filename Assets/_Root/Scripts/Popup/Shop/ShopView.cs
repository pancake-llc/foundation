using Pancake.Component;
using Pancake.IAP;
using Pancake.Monetization;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class ShopView : View
    {
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonCoinPack1;
        [SerializeField] private Button buttonCoinPack2;
        [SerializeField] private Button buttonDoubleCoin;
        [SerializeField] private Button buttonRemoveAds;
        [SerializeField] private Button buttonAllPinSkin;
        [SerializeField] private Button buttonVip;
        [SerializeField] private Button buttonFreeCoin;
        [SerializeField] private GameObject purchasedDoubleCoin;
        [SerializeField] private GameObject purchasedRemoveAds;
        [SerializeField] private GameObject purchasedAllPinSkin;
        [SerializeField] private GameObject purchasedVip;
        [SerializeField] private ScriptableEventVfxMagnet fxCoinSpawnEvent;
        [SerializeField] private RewardVariable rewardAd;
        [SerializeField] private ScriptableEventIAPProduct purchaseEvent;
        [SerializeField] private ScriptableEventIAPFuncProduct checkOwnerProductEvent;
        [SerializeField] private IAPDataVariable coinPack1;
        [SerializeField] private IAPDataVariable coinPack2;
        [SerializeField] private IAPDataVariable doubleCoin;
        [SerializeField] private IAPDataVariable removeAds;
        [SerializeField] private IAPDataVariable allPinSkin;
        [SerializeField] private IAPDataVariable vip;

        [Space] [SerializeField] private int coinFreeValue = 500;

        [SerializeField]
        private int coinPack1Value = 50000; // For display purposes only, the actual value added to the user's data will be via the index in IAPPurchaseSuccess.

        [SerializeField]
        private int coinPack2Value = 500000; // For display purposes only, the actual value added to the user's data will be via the index in IAPPurchaseSuccess.


        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonCoinPack1.onClick.AddListener(OnButtonCoinPack1Pressed);
            buttonCoinPack2.onClick.AddListener(OnButtonCoinPack2Pressed);
            buttonDoubleCoin.onClick.AddListener(OnButtonDoubleCoinPressed);
            buttonRemoveAds.onClick.AddListener(OnButtonRemoveAdsPressed);
            buttonAllPinSkin.onClick.AddListener(OnButtonAllSkinPinPressed);
            buttonVip.onClick.AddListener(OnButtonVipPressed);
            buttonFreeCoin.onClick.AddListener(OnButtonGetFreeCoinPressed);
            Refresh();
            return UniTask.CompletedTask;
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private void Refresh()
        {
            bool checkDoubleCoin;
            if (!Application.isMobilePlatform) checkDoubleCoin = Data.Load(Constant.IAP_DOUBLE_COIN, false);
            else checkDoubleCoin = checkOwnerProductEvent.Raise(doubleCoin) || Data.Load(Constant.IAP_DOUBLE_COIN, false);

            purchasedDoubleCoin.SetActive(checkDoubleCoin);
            buttonDoubleCoin.gameObject.SetActive(!checkDoubleCoin);


            bool checkRemoveAds;
            if (!Application.isMobilePlatform) checkRemoveAds = AdStatic.IsRemoveAd;
            else checkRemoveAds = checkOwnerProductEvent.Raise(removeAds) || AdStatic.IsRemoveAd;

            purchasedRemoveAds.SetActive(checkRemoveAds);
            buttonRemoveAds.gameObject.SetActive(!checkRemoveAds);

            bool checkAllPinSkin;
            if (!Application.isMobilePlatform) checkAllPinSkin = Data.Load(Constant.IAP_UNLOCK_ALL_SKIN, false);
            else checkAllPinSkin = checkOwnerProductEvent.Raise(allPinSkin) || Data.Load(Constant.IAP_UNLOCK_ALL_SKIN, false);

            purchasedAllPinSkin.SetActive(checkAllPinSkin);
            buttonAllPinSkin.gameObject.SetActive(!checkAllPinSkin);


            bool checkVip;
            if (!Application.isMobilePlatform) checkVip = Data.Load(Constant.IAP_VIP, false);
            else checkVip = checkOwnerProductEvent.Raise(vip) || Data.Load(Constant.IAP_VIP, false);

            purchasedVip.SetActive(checkVip);
            buttonVip.gameObject.SetActive(!checkVip);
        }

        private void OnButtonGetFreeCoinPressed()
        {
            if (Application.isMobilePlatform)
            {
                rewardAd.Context().OnCompleted(OnCompleteAdGetFreeCoin).Show();
            }
            else
            {
                OnCompleteAdGetFreeCoin();
            }
        }

        private void OnCompleteAdGetFreeCoin()
        {
            UserData.AddCoin(coinFreeValue);
            fxCoinSpawnEvent.Raise(buttonFreeCoin.transform.position, coinFreeValue);
            //noticeUpdateCoinEvent.Raise(); // use coin collision insteaded so dont need use noticeUpdateCoinEvent
        }

        private void OnButtonVipPressed()
        {
            vip.OnPurchaseCompleted(() =>
                {
                    Refresh();
                    fxCoinSpawnEvent.Raise(buttonVip.transform.parent.position, coinPack1Value);
                })
                .Purchase(purchaseEvent);
        }

        private void OnButtonAllSkinPinPressed() { allPinSkin.OnPurchaseCompleted(Refresh).Purchase(purchaseEvent); }
        private void OnButtonRemoveAdsPressed() { removeAds.OnPurchaseCompleted(Refresh).Purchase(purchaseEvent); }
        private void OnButtonDoubleCoinPressed() { doubleCoin.OnPurchaseCompleted(Refresh).Purchase(purchaseEvent); }

        private void OnButtonCoinPack1Pressed()
        {
            coinPack1.OnPurchaseCompleted(() =>
                {
                    Refresh();
                    fxCoinSpawnEvent.Raise(buttonCoinPack1.transform.parent.position, coinPack1Value);
                })
                .Purchase(purchaseEvent);
        }

        private void OnButtonCoinPack2Pressed()
        {
            coinPack2.OnPurchaseCompleted(() =>
                {
                    Refresh();
                    fxCoinSpawnEvent.Raise(buttonCoinPack2.transform.parent.position, coinPack2Value);
                })
                .Purchase(purchaseEvent);
        }
    }
}