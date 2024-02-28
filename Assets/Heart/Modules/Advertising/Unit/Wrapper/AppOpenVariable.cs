using UnityEngine;

namespace Pancake.Monetization
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    [Searchable]
    [CreateAssetMenu(fileName = "ad_appopen_unit_wrapper.asset", menuName = "Pancake/Misc/Advertising/App Open Variable Wrapper", order = 0)]
    [EditorIcon("scriptable_bind")]
    public class AppOpenVariable : ScriptableObject
    {
        [SerializeField] private AdmobAppOpenVariable admobAppOpen;
        [SerializeField] private ApplovinAppOpenVariable applovinAppOpen;

        public AdUnitVariable Context()
        {
            return AdStatic.currentNetworkShared switch
            {
                EAdNetwork.Applovin => applovinAppOpen,
                EAdNetwork.Admob => admobAppOpen,
                _ => admobAppOpen
            };
        }
    }
}