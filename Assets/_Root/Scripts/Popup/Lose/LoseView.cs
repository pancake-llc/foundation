using Pancake.Apex;
using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class LoseView : View
    {
        [SerializeField] private Button buttonHome;
        [SerializeField] private Button buttonReplay;
        [SerializeField] private Button buttonShop;
        [SerializeField] private Button buttonSkip;
        [SerializeField, PopupPickup] private string popupShop;
        [Header("EVENT")] [SerializeField] private ScriptableEventString changeSceneEvent;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private ScriptableEventLoadLevel loadLevelEvent;
        [SerializeField] private ScriptableEventNoParam showUiGameplayEvent;
        [SerializeField] private IntVariable currentLevelIndex;
        [SerializeField] private RewardVariable rewardVariable;

        private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);

        protected override UniTask Initialize()
        {
            buttonReplay.gameObject.SetActive(true);
            buttonHome.onClick.AddListener(OnButtonHomePressed);
            buttonReplay.onClick.AddListener(OnButtonReplayPressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            buttonSkip.onClick.AddListener(OnButtonSkipPressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonSkipPressed()
        {
            if (Application.isMobilePlatform)
            {
                rewardVariable.Context().OnCompleted(SkipLevel).Show();
            }
            else
            {
                SkipLevel();
            }
        }

        private async void SkipLevel()
        {
            currentLevelIndex.Value++;
            await loadLevelEvent.Raise(currentLevelIndex);
            reCreateLevelLoadedEvent.Raise();
            PlaySoundClose();
            await PopupHelper.Close(transform);
            showUiGameplayEvent.Raise();
        }

        private void OnButtonShopPressed() { MainPopupContainer.Push<ShopPopup>(popupShop, true); }

        private async void OnButtonReplayPressed()
        {
            reCreateLevelLoadedEvent.Raise();
            PlaySoundClose();
            await PopupHelper.Close(transform);
            showUiGameplayEvent.Raise();
        }

        private async void OnButtonHomePressed()
        {
            PlaySoundClose();
            await PopupHelper.Close(transform);
            changeSceneEvent.Raise(Constant.MENU_SCENE);
        }
    }
}