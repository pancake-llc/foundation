using System;
using System.Threading;
using Pancake.Localization;
using Cysharp.Threading.Tasks;
using Pancake.Component;
using Pancake.Sound;
using Pancake.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VitalRouter;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_controller")]
    public class MenuController : GameComponent
    {
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
        [SerializeField] private Audio buttonAudio;

        [Header("OTHER")]
        [SerializeField] private LocaleText localeTextFeatureLocked;
        private CancellationTokenSource _tokenShowUpdate;
        [Inject] private readonly AudioManager _soundManager;
        [Inject] private readonly SceneLoader _sceneLoader;

        //private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
        //private PageContainer MainPageContainer => PageContainer.Find(Constant.MAIN_PAGE_CONTAINER);
        //private PopupContainer PersistentPopupContainer => PopupContainer.Find(Constant.PERSISTENT_POPUP_CONTAINER);

        private void Start()
        {
            buttonSetting.OnClickAsObservable().Subscribe(OnSettingButtonPressed);
            buttonTapToPlay.onClick.AddListener(OnButtonTapToPlayPressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            buttonOutfit.onClick.AddListener(OnButtonOutfitPressed);
            buttonDailyReward.onClick.AddListener(OnButtonDailyRewardPressed);
            buttonRank.onClick.AddListener(OnButtonRankPressed);
            buttonPet.onClick.AddListener(OnButtonPetPressed);
            buttonRoom.onClick.AddListener(OnButtonRoomPressed);
            WaitShowUpdate();
        }

        private void OnSettingButtonPressed(Unit _)
        {
            ModalContainer.Main.NextAsync<SettingModalContext>().Forget();
            _soundManager.PlaySfx(buttonAudio);
        }

        private void OnButtonRoomPressed() { Router.Default.PublishAsync(new SpawnInGameNotiCommand(localeTextFeatureLocked)); }

        private void OnButtonPetPressed() { Router.Default.PublishAsync(new SpawnInGameNotiCommand(localeTextFeatureLocked)); }

        private void OnButtonRankPressed()
        {
            //MainPopupContainer.Push<LeaderboardPopup>(popupLeaderboard, false);
        }

        private void OnButtonDailyRewardPressed()
        {
            //MainPopupContainer.Push<DailyRewardPopup>(popupDailyReward, true);
        }

        private void OnButtonOutfitPressed()
        {
            //MainPageContainer.Push(outfitPageName, true);
        }

        private async void WaitShowUpdate()
        {
            // if (remoteConfigFetchCompleted == null) return;
            // _tokenShowUpdate = new CancellationTokenSource();
            // try
            // {
            //     await UniTask.WaitUntil(() => remoteConfigFetchCompleted, PlayerLoopTiming.Update, _tokenShowUpdate.Token);
            //     var version = new Version(remoteConfigNewVersion.Value);
            //     int result = version.CompareTo(new Version(Application.version));
            //     // is new version
            //     //if (result > 0 && !dontShowUpdateAgain) await MainPopupContainer.Push<UpdatePopup>(popupUpdate, true);
            // }
            // catch (OperationCanceledException)
            // {
            //     // ignored
            // }
        }

        private async void OnButtonTapToPlayPressed()
        {
            PoolHelper.ReturnAllPool();
            // await PersistentPopupContainer.Push<SceneTransitionPopup>(nameof(SceneTransitionPopup),
            //     false,
            //     onLoad: t => { t.popup.view.Setup(true); },
            //     popupId: nameof(SceneTransitionPopup)); // show transition
            _sceneLoader.ChangeScene(Constant.GAMEPLAY_SCENE);
        }

        private void OnButtonShopPressed()
        {
            //MainPopupContainer.Push<ShopPopup>(popupShop, true);
        }

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