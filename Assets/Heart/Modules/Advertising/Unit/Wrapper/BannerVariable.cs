using UnityEngine;

namespace Pancake.Monetization
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    [Searchable]
    [CreateAssetMenu(fileName = "ad_banner_unit_wrapper.asset", menuName = "Pancake/Misc/Advertising/Banner Variable Wrapper", order = 1)]
    [EditorIcon("so_blue_bind")]
    public class BannerVariable : ScriptableObject
    {
        [SerializeField] private AdmobBannerVariable admobBanner;
        [SerializeField] private ApplovinBannerVariable applovinBanner;

        public AdUnitVariable Context()
        {
            return AdStatic.currentNetworkShared switch
            {
                EAdNetwork.Applovin => applovinBanner,
                EAdNetwork.Admob => admobBanner,
                _ => admobBanner
            };
        }
    }
}