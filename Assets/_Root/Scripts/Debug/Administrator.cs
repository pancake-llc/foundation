using System.Globalization;
using Pancake.Apex;
using Pancake.Component;
using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.Scriptable;
using Pancake.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    /// <summary>
    /// Used for debugging purposes, some performance rules may be ignored
    /// </summary>
    public class Administrator : GameComponent
    {
        [SerializeField] private RectTransform holder;
        [SerializeField] private RectTransform virtualHolder;
        [SerializeField] private Button buttonSlide;
        [SerializeField] private TMP_InputField inputPassword;
        [SerializeField] private Button buttonJoin;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private string password;
        [SerializeField, Array] private GameObject[] locks;
        [SerializeField] private Button buttonNextLevel;
        [SerializeField] private Button buttonPreviousLevel;
        [SerializeField] private TMP_InputField inputLevelIndex;
        [SerializeField] private Button buttonJumpToLevel;
        [SerializeField] private Button buttonWinLevel;
        [SerializeField] private Button buttonLoseLevel;
        [SerializeField] private Button buttonAdd10KCoin;
        [SerializeField] private Button buttonAdd1MCoin;
        [SerializeField] private Button buttonEnableAds;
        [SerializeField] private Button buttonDisableAds;
        [SerializeField] private Button buttonShowBanner;
        [SerializeField] private Button buttonHideBanner;
        [SerializeField] private Button buttonShowInter;
        [SerializeField] private Button buttonShowRewarded;
        [SerializeField] private Button buttonShowAppOpen;

        [Header("Event")] [SerializeField] private ScriptableEventVfxMagnet fxCoinSpawnEvent;
        [SerializeField] private BannerVariable bannerVariable;
        [SerializeField] private InterVariable interVariable;
        [SerializeField] private RewardVariable rewardVariable;
        [SerializeField] private AppOpenVariable appOpenVariable;
        [SerializeField] private ScriptableEventLoadLevel loadLevelEvent;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private ScriptableEventNoParam hideUiGameplayEvent;
        [SerializeField] private IntVariable currentLevelIndex;

        [SerializeField, PopupPickup] private string popupWin;
        [SerializeField, PopupPickup] private string popupLose;

        private bool _stateSlide;

        private void Start()
        {
            Refresh();
            buttonSlide.onClick.AddListener(OnButtonSlideClicked);
            buttonJoin.onClick.AddListener(OnButtonJoinClicked);
            buttonNextLevel.onClick.AddListener(OnButtonNextLevelClicked);
            buttonPreviousLevel.onClick.AddListener(OnButtonPreviousLevelClicked);
            buttonJumpToLevel.onClick.AddListener(OnButtonJumpToLevelClicked);
            buttonWinLevel.onClick.AddListener(OnButtonWinLevelClicked);
            buttonLoseLevel.onClick.AddListener(OnButtonLoseLevelClicked);
            buttonAdd10KCoin.onClick.AddListener(OnButtonAdd10KCoinClicked);
            buttonAdd1MCoin.onClick.AddListener(OnButtonAdd1MCoinClicked);
            buttonEnableAds.onClick.AddListener(OnButtonEnableAdsClicked);
            buttonDisableAds.onClick.AddListener(OnButtonDisableAdsClicked);
            buttonShowBanner.onClick.AddListener(OnButtonShowBannerClicked);
            buttonHideBanner.onClick.AddListener(OnButtonHideBannerClicked);
            buttonShowInter.onClick.AddListener(OnButtonShowInterClicked);
            buttonShowRewarded.onClick.AddListener(OnButtonShowRewardedClicked);
            buttonShowAppOpen.onClick.AddListener(OnButtonShowAppOpenClicked);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { Hide(); }

        private void Refresh()
        {
            bool state = Data.Load($"{Application.identifier}_admin_state", false);
            Change(state);
            if (!state) return;
            bool isInGameplay = SceneManager.GetActiveScene().name.Equals(Constant.GAMEPLAY_SCENE);
            for (var i = 0; i < 6; i++)
            {
                locks[i].SetActive(isInGameplay);
            }
        }

        private void OnButtonLoseLevelClicked()
        {
            hideUiGameplayEvent.Raise();
            var win = FindObjectOfType<WinPopup>();
            if (win != null)
            {
                DebugEditor.Log("Popup Lose now cannot be displayed because Popup Win is showing!");
                return;
            }

            var popupContainer = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
            popupContainer.Push<LosePopup>("LosePopup", true);
        }

        private void OnButtonWinLevelClicked()
        {
            hideUiGameplayEvent.Raise();
            var lose = FindObjectOfType<LosePopup>();
            if (lose != null)
            {
                DebugEditor.Log("Popup Win now cannot be displayed because Popup Lose is showing!");
                return;
            }

            var popupContainer = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
            popupContainer.Push<WinPopup>("WinPopup", true);
        }

        private void OnButtonDisableAdsClicked() { AdStatic.IsRemoveAd = true; }

        private void OnButtonEnableAdsClicked() { AdStatic.IsRemoveAd = false; }

        private void OnButtonShowAppOpenClicked() { appOpenVariable.Context().Show(); }

        private void OnButtonShowRewardedClicked() { rewardVariable.Context().Show(); }

        private void OnButtonShowInterClicked() { interVariable.Context().Show(); }

        private void OnButtonHideBannerClicked() { bannerVariable.Context().Destroy(); }

        private void OnButtonShowBannerClicked() { bannerVariable.Context().Show(); }

        private void OnButtonAdd10KCoinClicked()
        {
            UserData.AddCoin(10000);
            fxCoinSpawnEvent.Raise(Vector2.zero, 10000);
        }

        private void OnButtonAdd1MCoinClicked()
        {
            UserData.AddCoin(1000000);
            fxCoinSpawnEvent.Raise(Vector2.zero, 1000000);
        }

        private async void OnButtonJumpToLevelClicked()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            if (!activeScene.Equals(Constant.GAMEPLAY_SCENE)) return;

            int target = (int.Parse(inputLevelIndex.text) - 1).Max(0);
            var prefab = await loadLevelEvent.Raise(target);
            if (prefab == null) return;

            reCreateLevelLoadedEvent.Raise();
        }

        private async void OnButtonPreviousLevelClicked()
        {
            currentLevelIndex.Value = (currentLevelIndex.Value - 1).Max(0);
            var prefab = await loadLevelEvent.Raise(currentLevelIndex.Value);
            if (prefab == null) return;

            reCreateLevelLoadedEvent.Raise();
        }

        private async void OnButtonNextLevelClicked()
        {
            currentLevelIndex.Value += 1;
            var prefab = await loadLevelEvent.Raise(currentLevelIndex.Value);
            if (prefab == null) return;

            reCreateLevelLoadedEvent.Raise();
        }

        private void Change(bool state)
        {
            foreach (var @lock in locks)
            {
                @lock.SetActive(state);
            }

            inputPassword.gameObject.SetActive(!state);
            buttonJoin.gameObject.SetActive(!state);
        }

        private void OnButtonJoinClicked()
        {
            string txt = inputPassword.text.ToLower(CultureInfo.InvariantCulture);
            if (password.Equals(txt))
            {
                message.text = "Wellcome back master!";
                message.gameObject.SetActive(true);
                message.color = new Color(0f, 0.65f, 0.89f);
                App.Delay(1.5f, () => message.gameObject.SetActive(false));
                Data.Save($"{Application.identifier}_admin_state", true);
                Refresh();
            }
            else
            {
                message.text = "Wrong password!";
                message.gameObject.SetActive(true);
                message.color = new Color(1f, 0.21f, 0.2f);
                App.Delay(1.5f, () => message.gameObject.SetActive(false));
            }
        }

        private void OnButtonSlideClicked()
        {
            if (_stateSlide)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void Show()
        {
            _stateSlide = true;
            Tween.UIAnchoredPositionX(holder, 500, 0.5f);
            Tween.UIAnchoredPositionX(virtualHolder, 500, 0.5f);
            Refresh();
        }

        private void Hide()
        {
            _stateSlide = false;
            Tween.UIAnchoredPositionX(holder, 0, 0.5f);
            Tween.UIAnchoredPositionX(virtualHolder, 0, 0.5f);
        }
    }
}