using Pancake.Scriptable;
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
        [SerializeField] private StringVariable remoteConfigFlagUseAdmob;
        public AdUnitVariable admobRewardInter;
        public AdUnitVariable applovinRewardInter;

        public AdUnitVariable Context()
        {
            bool.TryParse(remoteConfigFlagUseAdmob.Value, out bool status);
            return status ? admobRewardInter : applovinRewardInter;
        }
    }
}