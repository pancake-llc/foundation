// using Pancake.SceneFlow;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Pancake.UI
// {
//     public sealed class RestoreDataView : View
//     {
//         [SerializeField] private Button buttonClose;
//         [SerializeField] private Button buttonBackup;
//         [SerializeField, PopupPickup] private string popupChoosePlatform;
//
//         protected override UniTask Initialize()
//         {
//             buttonClose.onClick.AddListener(OnButtonClosePressed);
//             buttonBackup.onClick.AddListener(OnButtonBackupPressed);
//             return UniTask.CompletedTask;
//         }
//         
//         private async void OnButtonBackupPressed()
//         {
//             PlaySoundClose();
//             await PopupHelper.Close(transform);
//             await PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER).Push<ChoosePlatformLoginPopup>(popupChoosePlatform, true, onLoad: tuple => tuple.popup.view.Setup(false));
//         }
//
//         private void OnButtonClosePressed()
//         {
//             PlaySoundClose();
//             PopupHelper.Close(transform);
//         }
//     }
// }