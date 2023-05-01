using UnityEngine;

namespace Pancake
{
    //[HideMonoScript]
    [EditorIcon("scriptable_setting")]
    public class HeartSettings : ScriptableSettings<HeartSettings>
    {
        //[LabelText("Privacy on FirstOpen")]
        [SerializeField] private bool enablePrivacyFirstOpen;

        //[Indent, ShowIf(nameof(enablePrivacyFirstOpen))]
        [SerializeField]
        private string privacyUrl;

        //[Indent, LabelText("Title"), ShowIf(nameof(enablePrivacyFirstOpen))]
        [SerializeField]
        private string privacyTitle;

        //[Indent, LabelText("Message"), TextArea(3, 6), ShowIf(nameof(enablePrivacyFirstOpen))]
        [SerializeField]
        private string privacyMessage;

        public static bool EnablePrivacyFirstOpen => Instance.enablePrivacyFirstOpen;
        public static string PrivacyUrl => Instance.privacyUrl;
        public static string PrivacyTitle => Instance.privacyTitle;
        public static string PrivacyMessage => Instance.privacyMessage;
    }
}