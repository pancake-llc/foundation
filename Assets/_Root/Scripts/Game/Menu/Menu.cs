using System;
using Alchemy.Inspector;
using Pancake.Component;
using Pancake.Localization;
using Pancake.Sound;
using Pancake.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VitalRouter;

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
        [SerializeField] private LocaleText localeTextInGameNoti;

        [HorizontalLine, SerializeField, PopupPickup] private string settingPopupKey;
        [SerializeField, PopupPickup] private string dailyRewardPopupKey;
        [SerializeField, PopupPickup] private string shopPopupKey;
        [SerializeField, PopupPickup] private string leaderboardPopupKey;

        [HorizontalLine, SerializeField, AudioPickup] private AudioId bgm;

        private void Start()
        {
            bgm.Play();
            buttonSetting.onClick.AddListener(OnButtonSettingPressed);
            buttonDailyReward.onClick.AddListener(OnButtonDailyRewardPressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            buttonGoToGameplay.onClick.AddListener(OnButtonGotoGameplayPressed);
            buttonInGameNoti.onClick.AddListener(OnButtonInGameNotiPressed);
            buttonLeaderboard.onClick.AddListener(OnButtonLeaderboardPressed);
        }

        private void OnButtonLeaderboardPressed() { MainUIContainer.In.GetMain<PopupContainer>().Push(leaderboardPopupKey, true); }

        private void OnButtonInGameNotiPressed() { Router.Default.PublishAsync(new SpawnInGameNotiCommand(localeTextInGameNoti)); }

        private void OnButtonShopPressed() { MainUIContainer.In.GetMain<PopupContainer>().Push(shopPopupKey, true); }

        private void OnButtonDailyRewardPressed() { MainUIContainer.In.GetMain<PopupContainer>().Push(dailyRewardPopupKey, true); }

        private void OnButtonSettingPressed() { MainUIContainer.In.GetMain<PopupContainer>().Push(settingPopupKey, true); }

        private async void OnButtonGotoGameplayPressed()
        {
            AudioStatic.StopAll(); // todo: check issue when click button play sound meanwhile call StopAll (StopAll call before sound click played)
            SceneManager.sceneLoaded += OnGameplaySceneLoaded;
            Static.sceneHolder.Remove(Constant.Scene.MENU);
            await SceneManager.UnloadSceneAsync(Constant.Scene.MENU);
            await SceneManager.LoadSceneAsync(Constant.Scene.GAMEPLAY, LoadSceneMode.Additive);
        }

        private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
            Static.sceneHolder.Add(scene.name, scene);
            SceneManager.SetActiveScene(scene);
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            try
            {
#endif
                bgm.Stop();
#if UNITY_EDITOR
            }
            catch (Exception)
            {
                // ingored
            }
#endif
        }
    }
}