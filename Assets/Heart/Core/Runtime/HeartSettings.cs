using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake
{
    [EditorIcon("scriptable_setting")]
    public class HeartSettings : ScriptableSettings<HeartSettings>
    {
        [SerializeField] private bool enableAdministrator;
        [SerializeField] private bool enablePrivacyFirstOpen;
        [SerializeField] private bool enableMultipleTouch;
        [SerializeField] private bool requireInternet;
        [SerializeField] private TargetFrameRate targetFrameRate;
        [SerializeField] private string privacyUrl;
        [SerializeField] private string privacyTitle;
        [TextArea(3, 6)] [SerializeField] private string privacyMessage;
        [Header("IOS")] [SerializeField] private string appstoreAppId;
        [SerializeField, Range(0, 63)] private int skAdConversionValue = 63;

#if UNITY_EDITOR
        [Header("Editor")] [Tooltip("Indicates whether you can immediately edit the name asset upon creation?")] [SerializeField]
        private ENameAssetCreationMode nameCreationMode;
#endif

        public static bool EnableAdministrator => Instance.enableAdministrator;
        public static bool EnablePrivacyFirstOpen => Instance.enablePrivacyFirstOpen;
        public static string PrivacyUrl => Instance.privacyUrl;
        public static string PrivacyTitle => Instance.privacyTitle;
        public static string PrivacyMessage => Instance.privacyMessage;
        public static string AppstoreAppId => Instance.appstoreAppId;
        public static int SkAdConversionValue => Instance.skAdConversionValue;
        public static bool EnableMultipleTouch => Instance.enableMultipleTouch;
        public static bool RequireInternet => Instance.requireInternet;
        public static TargetFrameRate TargetFrameRate => Instance.targetFrameRate;
#if UNITY_EDITOR
        /// <summary>
        /// Only use in editor
        /// </summary>
        [Obsolete] public static ENameAssetCreationMode EditorNameCreationMode => Instance.nameCreationMode;
#endif
    }
}