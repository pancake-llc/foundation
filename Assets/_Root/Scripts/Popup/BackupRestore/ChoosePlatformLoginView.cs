using Pancake.Threading.Tasks;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public sealed class ChoosePlatformLoginView : View
    {
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonGpgs;

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonGpgs.onClick.AddListener(OnButtonGpgsPressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonGpgsPressed()
        {
            
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }
    }
}