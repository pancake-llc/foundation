using UnityEngine;

namespace Pancake.Monetization
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    [Searchable]
    [CreateAssetMenu(fileName = "ad_reward_inter_unit_wrapper.asset", menuName = "Pancake/Misc/Advertising/Reward Inter Variable Wrapper", order = 4)]
    [EditorIcon("scriptable_bind")]
    public class RewardInterVariable : ScriptableObject
    {
        [SerializeField] private AdmobRewardInterVariable admobRewardInter;
        [SerializeField] private ApplovinRewardInterVariable applovinRewardInter;

        public AdUnitVariable Context()
        {
            return AdStatic.currentNetworkShared switch
            {
                EAdNetwork.Applovin => applovinRewardInter,
                EAdNetwork.Admob => admobRewardInter,
                _ => admobRewardInter
            };
        }
    }
}