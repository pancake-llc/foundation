using Pancake.Apex;
using Pancake.Component;
using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class WinView : View
    {
        [SerializeField] private Button buttonHome;
        [SerializeField] private Button buttonVideoX5;
        [SerializeField] private Button buttonContinue;
        [SerializeField] private Button buttonShop;
        [SerializeField] private int numberCoinReceive = 100;
        [SerializeField, PopupPickup] private string popupShop;

        [SerializeField] private ScriptableEventString changeSceneEvent;
        [SerializeField] private ScriptableEventVfxMagnet fxCoinSpawnEvent;
        [SerializeField] private ScriptableEventLoadLevel eventLoadLevel;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private ScriptableEventNoParam showUiGameplayEvent;
        [SerializeField] private IntVariable currentLevelIndex;
        [SerializeField] private RewardVariable rewardVariable;
        [SerializeField] private IntVariable winGifProgresValue;
        [SerializeField] private Vector2Int rangeGiftValueIncrease;

        [Header("SOUND")]
        [SerializeField] private bool overrideBGM;
        [SerializeField, ShowIf(nameof(overrideBGM))] private Audio bgmWin;
        [SerializeField, ShowIf(nameof(overrideBGM))] private ScriptableEventAudio playBgmEvent;

        [Header("RATE")] [SerializeField] private BoolVariable canShowRate;
        [SerializeField] private ScriptableEventNoParam reviewEvent;

        private LevelComponent _prewarmNextLevel;
        private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);

        protected override UniTask Initialize()
        {
            buttonVideoX5.onClick.AddListener(OnButtonVideoX5Pressed);
            buttonHome.onClick.AddListener(OnButtonHomePressed);
            buttonContinue.onClick.AddListener(OnButtonContinuePressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            Refresh();
            return UniTask.CompletedTask;
        }


        private void OnButtonShopPressed() { MainPopupContainer.Push<ShopPopup>(popupShop, true); }

        private void OnButtonContinuePressed() { InternalContinue(numberCoinReceive); }

        private async void OnButtonHomePressed()
        {
            PlaySoundClose();
            await PopupHelper.Close(transform);
            changeSceneEvent.Raise(Constant.MENU_SCENE);
        }

        private void OnButtonVideoX5Pressed()
        {
            if (Application.isMobilePlatform)
            {
                rewardVariable.Context().OnCompleted(() => { InternalContinue(numberCoinReceive * 5); }).Show();
            }
            else
            {
                InternalContinue(numberCoinReceive * 5);
            }
        }

        private void CollectReward(int number)
        {
            UserData.AddCoin(number);
            fxCoinSpawnEvent.Raise(Vector2.zero, number);
        }

        private async void Continute()
        {
            await UniTask.WaitUntil(() => _prewarmNextLevel != null);
            reCreateLevelLoadedEvent.Raise();
            PlaySoundClose();
            await PopupHelper.Close(transform);
            showUiGameplayEvent.Raise();
        }

        private void InternalContinue(int reward)
        {
            CollectReward(reward);
            buttonVideoX5.interactable = false;
            buttonContinue.gameObject.SetActive(false);
            buttonHome.interactable = false;
            App.Delay(2f, Continute);
        }

       private async void Refresh()
        {
            _prewarmNextLevel = null;
            buttonContinue.gameObject.SetActive(true);
            if (overrideBGM) playBgmEvent.Raise(bgmWin);

            int addedValue = UnityEngine.Random.Range(rangeGiftValueIncrease.x, rangeGiftValueIncrease.y);
            int startValue = winGifProgresValue.Value;
#pragma warning disable 4014
            Tween.Custom(startValue,
                startValue + addedValue,
                0.35f,
                v =>
                {
                    if (v >= 100)
                    {
                        if (winGifProgresValue.Value < 100) winGifProgresValue.Value = 100;
                    }
                    else
                    {
                        winGifProgresValue.Value = (int) v;
                    }
                });
#pragma warning restore 4014
            // load next level
            currentLevelIndex.Value++;
            _prewarmNextLevel = await eventLoadLevel.Raise(currentLevelIndex);
        }
    }
}