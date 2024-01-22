using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class BackupDataView : View
    {
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonBackup;
        [SerializeField, PopupPickup] private string popupChoosePlatform;

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonBackup.onClick.AddListener(OnButtonBackupPressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonBackupPressed()
        {
            var choosePlatform = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER).Push<ChoosePlatformLoginPopup>(popupChoosePlatform, true);
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }
    }
}