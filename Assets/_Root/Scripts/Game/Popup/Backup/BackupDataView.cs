using Cysharp.Threading.Tasks;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    public sealed class BackupDataView : View
    {
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonBackup;
        [SerializeField, PopupPickup] private string choosePlatformPopupKey;

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonBackup.onClick.AddListener(OnButtonBackupPressed);

            return UniTask.CompletedTask;
        }

        private async void OnButtonBackupPressed()
        {
            PlaySoundClose();
            await PopupHelper.Close(transform);
            await MainUIContainer.In.GetMain<PopupContainer>()
                .PushAsync<ChoosePlatformLoginPopup>(choosePlatformPopupKey, true, onLoad: tuple => tuple.popup.view.Setup(true));
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }
    }
}