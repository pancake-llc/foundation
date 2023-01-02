#if PANCAKE_ADS
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

        #endregion

        #region properties

        public static bool RuntimeAutoInitialize => Instance.runtimeAutoInitialize;

        public static AdCommonSettings AdCommonSettings => Instance.adCommonSettings;

        public static AdmobSettings AdmobSettings => Instance.admobSettings;

        public static MaxSettings MaxSettings => Instance.maxSettings;

        public static EAdNetwork CurrentNetwork => Instance.adCommonSettings.CurrentNetwork;

        #endregion
    }
}
#endif