using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class NotificationView : View
    {
        [SerializeField] private TextMeshProUGUI textMessage;
        [SerializeField] private Button buttonClose;

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        public void SetMessage(string message) { textMessage.SetText(message); }
    }
}