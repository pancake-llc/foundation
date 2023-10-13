using System;
using System.Threading;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    [EditorIcon("script_controller")]
    public class MenuController : GameComponent
    {
        [SerializeField] private BoolVariable remoteConfigFetchCompleted;
        [SerializeField] private StringVariable remoteConfigNewVersion;
        [SerializeField] private BoolVariable dontShowUpdateAgain;
        [Header("BUTTON")] [SerializeField] private Button buttonSetting;
        [SerializeField] private Button buttonTapToPlay;
        [SerializeField] private Button buttonShop;
        [SerializeField] private Button buttonOutfit;
        [SerializeField] private Button buttonDailyReward;
        [SerializeField] private Button buttonRank;

        [Header("POPUP")] [SerializeField, PopupPickup] private string popupShop;
        [SerializeField, PopupPickup] private string popupSetting;
        [SerializeField, PopupPickup] private string popupUpdate;
        [SerializeField, PopupPickup] private string popupDailyReward;
        [SerializeField, PopupPickup] private string popupNotification;
        [SerializeField, PagePickup] private string outfitPageName;

        [Header("OTHER")] [SerializeField] private AudioComponent buttonAudio;
        [SerializeField] private ScriptableEventString changeSceneEvent;
        private CancellationTokenSource _tokenShowUpdate;

        private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
        private PageContainer MainPageContainer => PageContainer.Find(Constant.MAIN_PAGE_CONTAINER);

        private void Start()
        {
            buttonSetting.onClick.AddListener(OnButtonSettingPressed);
            buttonTapToPlay.onClick.AddListener(OnButtonTapToPlayPressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            buttonOutfit.onClick.AddListener(OnButtonOutfitPressed);
            buttonDailyReward.onClick.AddListener(OnButtonDailyRewardPressed);
            buttonRank.onClick.AddListener(OnButtonRankPressed);
            WaitShowUpdate();
        }

        private void OnButtonRankPressed() { MainPopupContainer.Push<NotificationPopup>(popupNotification, true, onLoad: _ => _.popup.view.SetMessage("Comming soon!")); }

        private void OnButtonDailyRewardPressed() { MainPopupContainer.Push<DailyRewardPopup>(popupDailyReward, true); }

        private void OnButtonOutfitPressed() { MainPageContainer.Push(outfitPageName, true); }

        private async void WaitShowUpdate()
        {
            if (remoteConfigFetchCompleted == null) return;
            _tokenShowUpdate = new CancellationTokenSource();
            try
            {
                await UniTask.WaitUntil(() => remoteConfigFetchCompleted, PlayerLoopTiming.Update, _tokenShowUpdate.Token);
                var version = new Version(remoteConfigNewVersion.Value);
                int result = version.CompareTo(new Version(Application.version));
                // is new version
                if (result > 0 && !dontShowUpdateAgain) await MainPopupContainer.Push<UpdatePopup>(popupUpdate, true);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

        private void OnButtonTapToPlayPressed() { changeSceneEvent.Raise(Constant.GAMEPLAY_SCENE); }

        private void OnButtonShopPressed() { MainPopupContainer.Push<ShopPopup>(popupShop, true); }

        private void OnButtonSettingPressed() { MainPopupContainer.Push<SettingPopup>(popupSetting, true); }

        protected override void OnDisabled()
        {
            if (_tokenShowUpdate != null)
            {
                _tokenShowUpdate.Cancel();
                _tokenShowUpdate.Dispose();
            }
        }
    }
}