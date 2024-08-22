using UnityEngine;

namespace Pancake
{
    [EditorIcon("so_blue_setting")]
    public class HeartSettings : ScriptableSettings<HeartSettings>
    {
        [SerializeField] private bool debugView;
        [SerializeField] private bool enablePrivacyFirstOpen;
        [SerializeField] private bool enableMultipleTouch;
        [SerializeField] private bool requireInternet;
        [SerializeField] private ETargetFrameRate targetFrameRate = ETargetFrameRate.Frame60;
        [SerializeField] private string termOfServiceUrl;
        [SerializeField] private string privacyUrl;
        [SerializeField] private string privacyTitle;
        [TextArea(3, 6)] [SerializeField] private string privacyMessage;
#if UNITY_IOS
        [Header("IOS")] [SerializeField] private string appstoreAppId;
        [SerializeField, Range(0, 63)] private int skAdConversionValue = 63;
#endif

        public static bool DebugView => Instance.debugView;
        public static bool EnablePrivacyFirstOpen => Instance.enablePrivacyFirstOpen;
        public static string TermOfServiceURL => Instance.termOfServiceUrl;
        public static string PrivacyURL => Instance.privacyUrl;
        public static string PrivacyTitle => Instance.privacyTitle;
        public static string PrivacyMessage => Instance.privacyMessage;
#if UNITY_IOS
        public static string AppstoreAppId => Instance.appstoreAppId;
        public static int SkAdConversionValue => Instance.skAdConversionValue;
#endif
        public static bool EnableMultipleTouch => Instance.enableMultipleTouch;
        public static bool RequireInternet => Instance.requireInternet;
        public static ETargetFrameRate TargetFrameRate => Instance.targetFrameRate;
    }
}