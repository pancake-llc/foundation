using Pancake.Apex;
using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class UpdateView : View
    {
        [SerializeField] private TextMeshProUGUI textVersion;
        [SerializeField] private TextMeshProUGUI textDescription;
        [SerializeField] private Button buttonOk;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Toggle toggleDontShowAgain;
        [SerializeField] private BoolVariable dontShowUpdateAgain;
        [SerializeField, Label("Change Log")] private StringVariable remoteConfigChangelog;
        [SerializeField, Label("Version")] private StringVariable remoteConfigNewVersion;


        protected override UniTask Initialize()
        {
            buttonOk.onClick.AddListener(OnButtonOkClicked);
            buttonClose.onClick.AddListener(OnButtonCloseClicked);
            return UniTask.CompletedTask;
        }

        private void OnButtonCloseClicked() { PopupHelper.Close(transform); }

        public override void Refresh()
        {
            textDescription.SetText(remoteConfigChangelog.Value);
            textVersion.SetText($"Version {remoteConfigNewVersion.Value}");
        }

        private void OnButtonOkClicked()
        {
            C.GotoStore();
            MarkRemember();
        }

        private void MarkRemember()
        {
            if (toggleDontShowAgain.isOn) dontShowUpdateAgain.Value = true;
        }
    }
}