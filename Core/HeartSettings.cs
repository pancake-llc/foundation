using Pancake.Attribute;
using UnityEngine;

namespace Pancake
{
    [HideMono]
    public class HeartSettings : ScriptableSettings<HeartSettings>
    {
        [SerializeField, LabelText("Privacy on FirstOpen")] private bool enablePrivacyFirstOpen;

        [SerializeField, Indent, ShowIf(nameof(enablePrivacyFirstOpen))]
        private string privacyUrl;

        public static bool EnablePrivacyFirstOpen => Instance.enablePrivacyFirstOpen;
        public static string PrivacyUrl => Instance.privacyUrl;
    }
}