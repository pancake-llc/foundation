using UnityEngine;

namespace Pancake.Monetization
{
    public class AdSettings : ScriptableSettings<AdSettings>
    {
        #region member

        [SerializeField] private bool runtimeAutoInitialize = true;
        [SerializeField] private AdCommonSettings adCommonSettings = new AdCommonSettings();
        [SerializeField] private AdmobSettings admobSettings = new AdmobSettings();
        [SerializeField] private MaxSettings maxSettings = new MaxSettings();
        [SerializeField] private IronSourceSettings ironSourceSettings = new IronSourceSettings();

        #endregion

        #region properties

        public static bool RuntimeAutoInitialize => Instance.runtimeAutoInitialize;

        public static AdCommonSettings AdCommonSettings => Instance.adCommonSettings;

        public static AdmobSettings AdmobSettings => Instance.admobSettings;

        public static MaxSettings MaxSettings => Instance.maxSettings;
        public static IronSourceSettings IronSourceSettings => Instance.ironSourceSettings;

        public static EAdNetwork CurrentNetwork => Instance.adCommonSettings.CurrentNetwork;

        #endregion
    }
}