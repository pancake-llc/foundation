using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Monetization
{
    public class AdSettings : ScriptableSettings<AdSettings>
    {
        [SerializeField, LabelText("Settings")] private AdCommonSettings adCommonSettings = new AdCommonSettings();
        [SerializeField, LabelText("AdMob")] private AdmobSettings admobSettings = new AdmobSettings();
        [SerializeField, LabelText("AppLovin")] private ApplovinSettings maxSettings = new ApplovinSettings();
        
        public static AdCommonSettings AdCommonSettings => Instance.adCommonSettings;
        public static AdmobSettings AdmobSettings => Instance.admobSettings;
        public static ApplovinSettings MaxSettings => Instance.maxSettings;
        public static EAdNetwork CurrentNetwork => Instance.adCommonSettings.CurrentNetwork;
    }
}