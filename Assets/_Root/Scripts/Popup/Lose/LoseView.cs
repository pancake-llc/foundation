// using Pancake.LevelSystem;
// using Pancake.Monetization;
// using Pancake.SceneFlow;
// using Pancake.Scriptable;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Pancake.UI
// {
//     public sealed class LoseView : View
//     {
//         [SerializeField] private Button buttonHome;
//         [SerializeField] private Button buttonReplay;
//         [SerializeField] private Button buttonShop;
//         [SerializeField] private Button buttonSkip;
//         [SerializeField, PopupPickup] private string popupShop;
//         [SerializeField] private StringConstant levelType;
//         [Header("EVENT")] [SerializeField] private ScriptableEventString changeSceneEvent;
//         [SerializeField] private ScriptableEventNoParam showUiGameplayEvent;
//
//         private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
//
//         protected override UniTask Initialize()
//         {
//             buttonReplay.gameObject.SetActive(true);
//             buttonHome.onClick.AddListener(OnButtonHomePressed);
//             buttonReplay.onClick.AddListener(OnButtonReplayPressed);
//             buttonShop.onClick.AddListener(OnButtonShopPressed);
//             buttonSkip.onClick.AddListener(OnButtonSkipPressed);
//             return UniTask.CompletedTask;
//         }
//
//         private void OnButtonSkipPressed() { Advertising.Reward?.OnCompleted(SkipLevel).Show(); }
//
//         private async void SkipLevel()
//         {
//             LevelCoordinator.IncreaseLevelIndex(levelType.Value, 1);
//             await LevelCoordinator.LoadLevel(levelType.Value, LevelCoordinator.GetCurrentLevelIndex(levelType.Value));
//             LevelInstantiate.RecreateLevelLoaded(levelType.Value);
//             PlaySoundClose();
//             await PopupHelper.Close(transform, false);
//             showUiGameplayEvent.Raise();
//         }
//
//         private void OnButtonShopPressed() { MainPopupContainer.Push<ShopPopup>(popupShop, true); }
//
//         private async void OnButtonReplayPressed()
//         {
//             LevelInstantiate.RecreateLevelLoaded(levelType.Value);
//             PlaySoundClose();
//             await PopupHelper.Close(transform, false);
//             showUiGameplayEvent.Raise();
//         }
//
//         private async void OnButtonHomePressed()
//         {
//             PlaySoundClose();
//             await PopupHelper.Close(transform);
//             changeSceneEvent.Raise(Constant.MENU_SCENE);
//         }
//     }
// }