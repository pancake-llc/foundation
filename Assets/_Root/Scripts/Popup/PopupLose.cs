using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.Scriptable;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class PopupLose : GameComponent
    {
        [SerializeField] private ScriptableEventGetGameObject canvasMaster;
        [SerializeField] private ScriptableEventGetGameObject gameplayCanvasUI;
        [SerializeField] private Button buttonHome;
        [SerializeField] private Button buttonReplay;
        [SerializeField] private Button buttonShop;
        [SerializeField] private Button buttonSkip;

        // [Header("POPUP")] [SerializeField] private PopupShowEvent popupShowEvent;
        // [SerializeField, PopupPickup] private string popupShop;
        // [SerializeField, PopupPickup] private string popupLose;

        [Header("EVENT")] [SerializeField] private ScriptableEventString changeSceneEvent;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private ScriptableEventLoadLevel loadLevelEvent;
        [SerializeField] private ScriptableEventNoParam showUiGameplayEvent;
        [SerializeField] private IntVariable currentLevelIndex;
        [SerializeField] private RewardVariable rewardVariable;

        // private bool _initialized;
        //
        // public override void Init()
        // {
        //     base.Init();
        //     buttonReplay.gameObject.SetActive(true);
        //     if (!_initialized)
        //     {
        //         _initialized = true;
        //         buttonHome.onClick.AddListener(OnButtonHomeClicked);
        //         buttonReplay.onClick.AddListener(OnButtonReplayClicked);
        //         buttonShop.onClick.AddListener(OnButtonShopClicked);
        //         buttonSkip.onClick.AddListener(OnButtonSkipClicked);
        //     }
        // }
        //
        // private void OnButtonSkipClicked()
        // {
        //     if (Application.isMobilePlatform)
        //     {
        //         rewardVariable.Context().OnCompleted(SkipLevel).Show();
        //     }
        //     else
        //     {
        //         SkipLevel();
        //     }
        // }
        //
        // private async void SkipLevel()
        // {
        //     currentLevelIndex.Value++;
        //     await loadLevelEvent.Raise(currentLevelIndex);
        //     reCreateLevelLoadedEvent.Raise();
        //     closePopupEvent.Raise();
        //     showUiGameplayEvent.Raise();
        // }
        //
        // private void OnButtonShopClicked()
        // {
        //     closePopupEvent.Raise();// close popup lose
        //     var popup = popupShowEvent.Raise(popupShop, canvasMaster.Raise().transform);
        //     popup.OnAfterClose(() =>
        //     {
        //         popupShowEvent.Raise(popupLose, gameplayCanvasUI.Raise().transform, false); // show popup lose again with out init avoid increase level
        //     });
        // }
        //
        // private void OnButtonReplayClicked()
        // {
        //     reCreateLevelLoadedEvent.Raise();
        //     closePopupEvent.Raise();
        //     showUiGameplayEvent.Raise();
        // }
        //
        // private void OnButtonHomeClicked()
        // {
        //     closePopupEvent.Raise();
        //     changeSceneEvent.Raise(Constant.MENU_SCENE);
        // }
    }
}