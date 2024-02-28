using UnityEngine;

namespace Pancake.Monetization
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    [Searchable]
    [CreateAssetMenu(fileName = "ad_inter_unit_wrapper.asset", menuName = "Pancake/Misc/Advertising/Inter Variable Wrapper", order = 2)]
    [EditorIcon("scriptable_bind")]
    public class InterVariable : ScriptableObject
    {
        [SerializeField] private AdmobInterVariable admobInter;
        [SerializeField] private ApplovinInterVariable applovinInter;

        public AdUnitVariable Context()
        {
            return AdStatic.currentNetworkShared switch
            {
                EAdNetwork.Applovin => applovinInter,
                EAdNetwork.Admob => admobInter,
                _ => admobInter
            };
        }
    }
}