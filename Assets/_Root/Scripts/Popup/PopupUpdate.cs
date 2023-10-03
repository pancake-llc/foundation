using Pancake.Apex;
using Pancake.Scriptable;
using Pancake.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class PopupUpdate : GameComponent
    {
        [SerializeField] private TextMeshProUGUI textVersion;
        [SerializeField] private TextMeshProUGUI textDescription;
        [SerializeField] private Button buttonOk;
        [SerializeField] private Toggle toggleDontShowAgain;
        [SerializeField] private BoolVariable dontShowUpdateAgain;
        [SerializeField, Label("Change Log")] private StringVariable remoteConfigChangelog;
        [SerializeField, Label("Version")] private StringVariable remoteConfigNewVersion;

        private bool _initialized;

        // public override void Init()
        // {
        //     if (_initialized)
        //     {
        //         _initialized = true;
        //         buttonOk.onClick.AddListener(OnButtonOkClicked);
        //     }
        //
        //     textDescription.SetText(remoteConfigChangelog.Value);
        //     textVersion.SetText($"Version {remoteConfigNewVersion.Value}");
        // }
        //
        // private void OnButtonOkClicked()
        // {
        //     C.GotoStore();
        //     MarkRemember();
        // }
        //
        // protected override void OnBeforeClose()
        // {
        //     base.OnBeforeClose();
        //     MarkRemember();
        // }
        //
        // private void MarkRemember()
        // {
        //     if (toggleDontShowAgain.isOn) dontShowUpdateAgain.Value = true;
        // }
    }
}