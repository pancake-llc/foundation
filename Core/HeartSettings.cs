using Pancake.Attribute;
using UnityEngine;

namespace Pancake
{
    [HideMono]
    [EditorIcon("scriptable_setting")]
    public class HeartSettings : ScriptableSettings<HeartSettings>
    {
        [SerializeField, LabelText("Privacy on FirstOpen")] private bool enablePrivacyFirstOpen;

        [SerializeField, Indent, ShowIf(nameof(enablePrivacyFirstOpen))]
        private string privacyUrl;

        [SerializeField, Indent, LabelText("Title"), ShowIf(nameof(enablePrivacyFirstOpen))] private string privacyTitle;
        [SerializeField, Indent, LabelText("Message"), TextArea(3, 6), ShowIf(nameof(enablePrivacyFirstOpen))] private string privacyMessage;

        public static bool EnablePrivacyFirstOpen => Instance.enablePrivacyFirstOpen;
        public static string PrivacyUrl => Instance.privacyUrl;
        public static string PrivacyTitle => Instance.privacyTitle;
        public static string PrivacyMessage => Instance.privacyMessage;
    }
}