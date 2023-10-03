using Pancake.Component;
using Pancake.IAP;
using Pancake.Monetization;
using Pancake.Scriptable;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class PopupShop : GameComponent
    {
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


        private bool _initialized;

        // public override void Init()
        // {
        //     if (!_initialized)
        //     {
        //         _initialized = true;
        //         buttonCoinPack1.onClick.AddListener(OnCoinPack1Purchase);
        //         buttonCoinPack2.onClick.AddListener(OnCoinPack2Purchase);
        //         buttonDoubleCoin.onClick.AddListener(OnDoubleCoinPurchase);
        //         buttonRemoveAds.onClick.AddListener(OnRemoveAdsPurchase);
        //         buttonAllPinSkin.onClick.AddListener(OnAllSkinPinPurchase);
        //         buttonVip.onClick.AddListener(OnVipPurchase);
        //         buttonFreeCoin.onClick.AddListener(OnGetFreeCoin);
        //     }
        //
        //     OnRefreshUI();
        // }
        //
        // private void OnGetFreeCoin()
        // {
        //     if (Application.isMobilePlatform)
        //     {
        //         rewardAd.Context().OnCompleted(OnCompleteAdGetFreeCoin).Show();
        //     }
        //     else
        //     {
        //         OnCompleteAdGetFreeCoin();
        //     }
        // }
        //
        // private void OnCompleteAdGetFreeCoin()
        // {
        //     Data.Save(Constant.USER_CURRENT_COIN, Data.Load(Constant.USER_CURRENT_COIN, 0) + 500);
        //     fxCoinSpawnEvent.Raise(buttonFreeCoin.transform.position, 500);
        //     //noticeUpdateCoinEvent.Raise(); // use coin collision insteaded so dont need use noticeUpdateCoinEvent
        // }
        //
        // private void OnVipPurchase()
        // {
        //     vip.OnPurchaseCompleted(() =>
        //         {
        //             OnRefreshUI();
        //             fxCoinSpawnEvent.Raise(buttonVip.transform.parent.position, 0);
        //         })
        //         .Purchase(purchaseEvent);
        // }
        //
        // private void OnAllSkinPinPurchase() { allPinSkin.OnPurchaseCompleted(OnRefreshUI).Purchase(purchaseEvent); }
        // private void OnRemoveAdsPurchase() { removeAds.OnPurchaseCompleted(OnRefreshUI).Purchase(purchaseEvent); }
        // private void OnDoubleCoinPurchase() { doubleCoin.OnPurchaseCompleted(OnRefreshUI).Purchase(purchaseEvent); }
        //
        // private void OnCoinPack1Purchase()
        // {
        //     coinPack1.OnPurchaseCompleted(() =>
        //         {
        //             OnRefreshUI();
        //             fxCoinSpawnEvent.Raise(buttonCoinPack1.transform.parent.position, 0);
        //         })
        //         .Purchase(purchaseEvent);
        // }
        //
        // private void OnCoinPack2Purchase()
        // {
        //     coinPack2.OnPurchaseCompleted(() =>
        //         {
        //             OnRefreshUI();
        //             fxCoinSpawnEvent.Raise(buttonCoinPack2.transform.parent.position, 0);
        //         })
        //         .Purchase(purchaseEvent);
        // }
        //
        // /// <summary>
        // /// Call when purchase completed
        // /// it only update user interface
        // /// </summary>
        // private void OnRefreshUI()
        // {
        //     bool checkDoubleCoin;
        //     if (!Application.isMobilePlatform) checkDoubleCoin = Data.Load(Constant.IAP_DOUBLE_COIN, false);
        //     else checkDoubleCoin = checkOwnerProductEvent.Raise(doubleCoin) || Data.Load(Constant.IAP_DOUBLE_COIN, false);
        //
        //     purchasedDoubleCoin.SetActive(checkDoubleCoin);
        //     buttonDoubleCoin.gameObject.SetActive(!checkDoubleCoin);
        //
        //
        //     bool checkRemoveAds;
        //     if (!Application.isMobilePlatform) checkRemoveAds = AdStatic.IsRemoveAd;
        //     else checkRemoveAds = checkOwnerProductEvent.Raise(removeAds) || AdStatic.IsRemoveAd;
        //
        //     purchasedRemoveAds.SetActive(checkRemoveAds);
        //     buttonRemoveAds.gameObject.SetActive(!checkRemoveAds);
        //
        //     bool checkAllPinSkin;
        //     if (!Application.isMobilePlatform) checkAllPinSkin = Data.Load(Constant.IAP_UNLOCK_ALL_SKIN, false);
        //     else checkAllPinSkin = checkOwnerProductEvent.Raise(allPinSkin) || Data.Load(Constant.IAP_UNLOCK_ALL_SKIN, false);
        //
        //     purchasedAllPinSkin.SetActive(checkAllPinSkin);
        //     buttonAllPinSkin.gameObject.SetActive(!checkAllPinSkin);
        //
        //
        //     bool checkVip;
        //     if (!Application.isMobilePlatform) checkVip = Data.Load(Constant.IAP_VIP, false);
        //     else checkVip = checkOwnerProductEvent.Raise(vip) || Data.Load(Constant.IAP_VIP, false);
        //
        //     purchasedVip.SetActive(checkVip);
        //     buttonVip.gameObject.SetActive(!checkVip);
        // }
    }
}