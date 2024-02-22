using UnityEngine;

namespace Pancake.Monetization
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    [Searchable]
    [CreateAssetMenu(fileName = "ad_banner_unit_wrapper.asset", menuName = "Pancake/Misc/Advertising/Banner Variable Wrapper", order = 1)]
    [EditorIcon("scriptable_bind")]
    public class BannerVariable : ScriptableObject
    {
        [SerializeField] private AdUnitVariable admobBanner;
        [SerializeField] private AdUnitVariable applovinBanner;

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