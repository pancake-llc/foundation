#if PANCAKE_ADMOB
using System.IO;
using GoogleMobileAds.Editor;
using Pancake;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class AdmobHelper 
    {
        public static void BridgeCreateGoogleMobileAdsSetting()
        {
            var setting = ScriptableObject.CreateInstance<GoogleMobileAdsSettings>();
            const string path = "Assets/GoogleMobileAds/Resources";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            AssetDatabase.CreateAsset(setting, $"{path}/{nameof(GoogleMobileAdsSettings)}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{nameof(GoogleMobileAdsSettings).TextColor("#52D5F2")} was created ad {path}/{nameof(GoogleMobileAdsSettings)}.asset");
        }

        public static void DrawSetting()
        {
            var googleMobileAdSetting = Resources.Load<GoogleMobileAdsSettings>(nameof(GoogleMobileAdsSettings));
            var editor = UnityEditor.Editor.CreateEditor(googleMobileAdSetting);
            editor.OnInspectorGUI();
        }

        public static bool BridgeCheckGoogleMobileAdsSettingExist()
        {
            var googleMobileAdSetting = Resources.Load<GoogleMobileAdsSettings>(nameof(GoogleMobileAdsSettings));
            return googleMobileAdSetting != null;
        }


        public static bool IsMissingAdmobApplicationId()
        {
            var googleMobileAdSetting = Resources.Load<GoogleMobileAdsSettings>(nameof(GoogleMobileAdsSettings));
#if UNITY_ANDROID
            return string.IsNullOrEmpty(googleMobileAdSetting.GoogleMobileAdsAndroidAppId);
#elif UNITY_IOS
            return string.IsNullOrEmpty(googleMobileAdSetting.GoogleMobileAdsIOSAppId);
#endif
#pragma warning disable CS0162
            return false;
        }
    }

}
#endif