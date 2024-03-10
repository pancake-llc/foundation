using System;
using System.Threading;
using Pancake.Localization;
using Pancake.Scriptable;
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
        [SerializeField] private Button buttonPet;
        [SerializeField] private Button buttonRoom;

        [Header("POPUP")] [SerializeField, PopupPickup] private string popupShop;
        [SerializeField, PopupPickup] private string popupSetting;
        [SerializeField, PopupPickup] private string popupUpdate;
        [SerializeField, PopupPickup] private string popupDailyReward;
        [SerializeField, PopupPickup] private string popupLeaderboard;
        [SerializeField, PagePickup] private string outfitPageName;

        [Header("OTHER")] [SerializeField] private ScriptableEventString changeSceneEvent;
        [SerializeField] private LocaleText localeTextFeatureLocked;
        [SerializeField] private ScriptableEventLocaleText eventSpawnInGameNotification;
        private CancellationTokenSource _tokenShowUpdate;

        private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
        private PageContainer MainPageContainer => PageContainer.Find(Constant.MAIN_PAGE_CONTAINER);
        private PopupContainer PersistentPopupContainer => PopupContainer.Find(Constant.PERSISTENT_POPUP_CONTAINER);

        private void Start()
        {
            buttonSetting.onClick.AddListener(OnButtonSettingPressed);
            buttonTapToPlay.onClick.AddListener(OnButtonTapToPlayPressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            buttonOutfit.onClick.AddListener(OnButtonOutfitPressed);
            buttonDailyReward.onClick.AddListener(OnButtonDailyRewardPressed);
            buttonRank.onClick.AddListener(OnButtonRankPressed);
            buttonPet.onClick.AddListener(OnButtonPetPressed);
            buttonRoom.onClick.AddListener(OnButtonRoomPressed);
            WaitShowUpdate();
        }

        private void OnButtonRoomPressed() { eventSpawnInGameNotification.Raise(localeTextFeatureLocked); }

        private void OnButtonPetPressed() { eventSpawnInGameNotification.Raise(localeTextFeatureLocked); }

        private void OnButtonRankPressed() { MainPopupContainer.Push<LeaderboardPopup>(popupLeaderboard, false); }

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

        private async void OnButtonTapToPlayPressed()
        {
            PoolHelper.ReturnAllPool();
            await PersistentPopupContainer.Push<SceneTransitionPopup>(nameof(SceneTransitionPopup),
                false,
                onLoad: t => { t.popup.view.Setup(true); },
                popupId: nameof(SceneTransitionPopup)); // show transition
            changeSceneEvent.Raise(Constant.GAMEPLAY_SCENE);
        }

        private void OnButtonShopPressed() { MainPopupContainer.Push<ShopPopup>(popupShop, true); }

        private void OnButtonSettingPressed() { MainPopupContainer.Push<SettingPopup>(popupSetting, true, popupId: popupSetting); }

        protected void OnDisable()
        {
            if (_tokenShowUpdate != null)
            {
                _tokenShowUpdate.Cancel();
                _tokenShowUpdate.Dispose();
            }
        }
    }
}