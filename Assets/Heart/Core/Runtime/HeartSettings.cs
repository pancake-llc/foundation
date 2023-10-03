using Pancake.Apex;
using UnityEngine;

namespace Pancake
{
    [EditorIcon("scriptable_setting")]
    public class HeartSettings : ScriptableSettings<HeartSettings>
    {
        [Label("Debug")] [SerializeField] private bool enableAdministrator;
        [Label("Privacy on FirstOpen")] [SerializeField] private bool enablePrivacyFirstOpen;

        [Indent, Label("Url"), ShowIf(nameof(enablePrivacyFirstOpen))] [SerializeField]
        private string privacyUrl;

        [Indent, Label("Title"), ShowIf(nameof(enablePrivacyFirstOpen))] [SerializeField]
        private string privacyTitle;

        [Indent, Label("Message"), TextArea(3, 6), ShowIf(nameof(enablePrivacyFirstOpen))] [SerializeField]
        private string privacyMessage;

        [Header("IOS")]
        [SerializeField] private string appstoreAppId;
        [SerializeField, Range(0, 63)] private int skAdConversionValue = 63;

        public static bool EnableAdministrator => Instance.enableAdministrator;
        public static bool EnablePrivacyFirstOpen => Instance.enablePrivacyFirstOpen;
        public static string PrivacyUrl => Instance.privacyUrl;
        public static string PrivacyTitle => Instance.privacyTitle;
        public static string PrivacyMessage => Instance.privacyMessage;
        public static string AppstoreAppId => Instance.appstoreAppId;
        public static int SkAdConversionValue => Instance.skAdConversionValue;
    }
}