using UnityEngine;

namespace Pancake.Monetization
{
    //[HideMonoScript]
    [EditorIcon("scriptable_ad")]
    public class AdSettings : ScriptableSettings<AdSettings>
    {
        //[LabelText("Settings")]
        [SerializeField] private AdCommonSettings adCommonSettings = new AdCommonSettings();
        //[LabelText("AdMob")]
        [SerializeField] private AdmobSettings admobSettings = new AdmobSettings();
        //[LabelText("AppLovin")]
        [SerializeField] private ApplovinSettings maxSettings = new ApplovinSettings();

        public static AdCommonSettings AdCommonSettings => Instance.adCommonSettings;
        public static AdmobSettings AdmobSettings => Instance.admobSettings;
        public static ApplovinSettings MaxSettings => Instance.maxSettings;
        public static EAdNetwork CurrentNetwork => Instance.adCommonSettings.CurrentNetwork;
    }
}