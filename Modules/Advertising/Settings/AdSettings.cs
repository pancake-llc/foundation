using UnityEngine;

namespace Pancake.Monetization
{
    public class AdSettings : ScriptableSettings<AdSettings>
    {
        [SerializeField] private bool runtimeAutoInitialize = true;
        [SerializeField] private AdCommonSettings adCommonSettings = new AdCommonSettings();
        [SerializeField] private AdmobSettings admobSettings = new AdmobSettings();
        [SerializeField] private ApplovinSettings maxSettings = new ApplovinSettings();
        
        public static bool RuntimeAutoInitialize => Instance.runtimeAutoInitialize;
        public static AdCommonSettings AdCommonSettings => Instance.adCommonSettings;
        public static AdmobSettings AdmobSettings => Instance.admobSettings;
        public static ApplovinSettings MaxSettings => Instance.maxSettings;
        public static EAdNetwork CurrentNetwork => Instance.adCommonSettings.CurrentNetwork;
    }
}