using Pancake.Apex;
using Pancake.Component;
using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using Pancake.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class PopupWin : GameComponent
    {
        [SerializeField] private Button buttonHome;
        [SerializeField] private Button buttonVideoX5;
        [SerializeField] private Button buttonContinue;
        [SerializeField] private Button buttonShop;
        [SerializeField] private int numberCoinReceive = 100;

        // [Header("POPUP")] [SerializeField] private PopupShowEvent popupShowEvent;
        // [SerializeField, PopupPickup] private string popupShop;
        // [SerializeField, PopupPickup] private string popupWin;

        [Header("EVENT")] [SerializeField] private ScriptableEventGetGameObject canvasMaster;
        [SerializeField] private ScriptableEventGetGameObject gameplayCanvasUI;
        [SerializeField] private ScriptableEventString changeSceneEvent;
        [SerializeField] private ScriptableEventVfxMagnet fxCoinSpawnEvent;
        [SerializeField] private ScriptableEventLoadLevel eventLoadLevel;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private ScriptableEventNoParam showUiGameplayEvent;
        [SerializeField] private IntVariable currentLevelIndex;
        [SerializeField] private RewardVariable rewardVariable;
        [SerializeField] private IntVariable winGifProgresValue;
        [SerializeField] private Vector2Int rangeGiftValueIncrease;

        [Header("SOUND"), SerializeField] private bool enabledSound;
        [SerializeField, ShowIf(nameof(enabledSound))] private Audio audioWin;
        [SerializeField, ShowIf(nameof(enabledSound))] private ScriptableEventAudio playAudioEvent;
        [SerializeField] private bool overrideBGM;
        [SerializeField, ShowIf(nameof(overrideBGM))] private Audio bgmWin;
        [SerializeField, ShowIf(nameof(overrideBGM))] private ScriptableEventAudio playBgmEvent;


        [Header("RATE")] [SerializeField] private BoolVariable canShowRate;
        [SerializeField] private ScriptableEventNoParam reviewEvent;
        
//         protected override bool EnableTrackBackButton() { return false; }
//
//         private bool _initialized;
//         private LevelComponent _prewarmNextLevel;
//
//         public override async void Init()
//         {
//             base.Init();
//             _prewarmNextLevel = null;
//             buttonContinue.gameObject.SetActive(true);
//             if (enabledSound) playAudioEvent.Raise(audioWin);
//             if (overrideBGM) playBgmEvent.Raise(bgmWin);
//
//             if (!_initialized)
//             {
//                 _initialized = true;
//                 buttonVideoX5.onClick.AddListener(OnButtonVideoX5Clicked);
//                 buttonHome.onClick.AddListener(OnButtonHomeClicked);
//                 buttonContinue.onClick.AddListener(OnButtonContinueClicked);
//                 buttonShop.onClick.AddListener(OnButtonShopClicked);
//             }
//
//             int addedValue = UnityEngine.Random.Range(rangeGiftValueIncrease.x, rangeGiftValueIncrease.y);
//             int startValue = winGifProgresValue.Value;
// #pragma warning disable 4014
//             Tween.Custom(startValue,
//                 startValue + addedValue,
//                 0.35f,
//                 v =>
//                 {
//                     if (v >= 100)
//                     {
//                         if (winGifProgresValue.Value < 100) winGifProgresValue.Value = 100;
//                     }
//                     else
//                     {
//                         winGifProgresValue.Value = (int) v;
//                     }
//                 });
// #pragma warning restore 4014
//             // load next level
//             currentLevelIndex.Value++;
//             _prewarmNextLevel = await eventLoadLevel.Raise(currentLevelIndex);
//         }
//
//         private void CollectReward(int number)
//         {
//             // spawn coin
//             Data.Save(Constant.USER_CURRENT_COIN, Data.Load(Constant.USER_CURRENT_COIN, 0) + number);
//             fxCoinSpawnEvent.Raise(Vector2.zero, number);
//         }
//
//         private async void Continute()
//         {
//             await UniTask.WaitUntil(() => _prewarmNextLevel != null);
//             reCreateLevelLoadedEvent.Raise();
//             closePopupEvent.Raise();
//             showUiGameplayEvent.Raise();
//         }
//
//         private void OnButtonShopClicked()
//         {
//             closePopupEvent.Raise(); // close popup win
//             var popup = popupShowEvent.Raise(popupShop, canvasMaster.Raise().transform);
//             popup.OnAfterClose(() =>
//             {
//                 popupShowEvent.Raise(popupWin, gameplayCanvasUI.Raise().transform, false); // show popup win again with out init avoid increase level
//             });
//         }
//
//         private void OnButtonContinueClicked() { InternalContinue(numberCoinReceive); }
//
//         private void InternalContinue(int reward)
//         {
//             CollectReward(reward);
//             buttonVideoX5.interactable = false;
//             buttonContinue.gameObject.SetActive(false);
//             buttonHome.interactable = false;
//             App.Delay(2f, Continute);
//         }
//
//         private void OnButtonHomeClicked()
//         {
//             closePopupEvent.Raise();
//             changeSceneEvent.Raise(Constant.MENU_SCENE);
//         }
//
//         private void OnButtonVideoX5Clicked()
//         {
//             if (Application.isMobilePlatform)
//             {
//                 rewardVariable.Context().OnCompleted(() => { InternalContinue(numberCoinReceive * 5); }).Show();
//             }
//             else
//             {
//                 InternalContinue(numberCoinReceive * 5);
//             }
//         }
    }
}