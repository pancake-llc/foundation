using Cysharp.Threading.Tasks;
using Pancake.Component;
using Pancake.DebugView;
using Pancake.Localization;
using Pancake.Sound;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game
{
    [EditorIcon("icon_entry")]
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Button buttonSetting;
        [SerializeField] private Button buttonDailyReward;
        [SerializeField] private Button buttonShop;
        [SerializeField] private Button buttonGoToGameplay;
        [SerializeField] private Button buttonInGameNoti;
        [SerializeField] private Button buttonLeaderboard;
        [SerializeField] private Button buttonDebug;
        [SerializeField] private LocaleText localeTextInGameNoti;

        [Space, SerializeField, PopupPickup] private string settingPopupKey;
        [SerializeField, PopupPickup] private string dailyRewardPopupKey;
        [SerializeField, PopupPickup] private string shopPopupKey;
        [SerializeField, PopupPickup] private string leaderboardPopupKey;

        [Space, SerializeField, AudioPickup] private AudioId bgm;

        private void Start()
        {
            bgm.Play();
            buttonSetting.onClick.AddListener(OnButtonSettingPressed);
            buttonDailyReward.onClick.AddListener(OnButtonDailyRewardPressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            buttonGoToGameplay.onClick.AddListener(OnButtonGotoGameplayPressed);
            buttonInGameNoti.onClick.AddListener(OnButtonInGameNotiPressed);
            buttonLeaderboard.onClick.AddListener(OnButtonLeaderboardPressed);
            buttonDebug.onClick.AddListener(OnButtonDebugPressed);
        }

        private void OnButtonDebugPressed() { Messenger<ShowDebugMessage>.Raise(); }

        private void OnButtonLeaderboardPressed() { MainUIContainer.In.GetMain<PopupContainer>().PushAsync(leaderboardPopupKey, true).Forget(); }

        private void OnButtonInGameNotiPressed() { Messenger<SpawnInGameNotiMessage>.Raise(new SpawnInGameNotiMessage(localeTextInGameNoti)); }

        private void OnButtonShopPressed() { MainUIContainer.In.GetMain<PopupContainer>().PushAsync(shopPopupKey, true).Forget(); }

        private void OnButtonDailyRewardPressed() { MainUIContainer.In.GetMain<PopupContainer>().PushAsync(dailyRewardPopupKey, true).Forget(); }

        private void OnButtonSettingPressed() { MainUIContainer.In.GetMain<PopupContainer>().PushAsync(settingPopupKey, true).Forget(); }

        private async void OnButtonGotoGameplayPressed()
        {
            await Awaitable.NextFrameAsync();
            await SceneLoader.LoadScene(Constant.Scene.GAMEPLAY);
        }

        private void OnDestroy() { AudioStatic.StopAllAndClean(); }
    }
}