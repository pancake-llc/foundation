using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Monetization
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    [Searchable]
    [CreateAssetMenu(fileName = "ad_reward_inter_unit_wrapper.asset", menuName = "Pancake/Misc/Advertising/Reward Inter Variable Wrapper")]
    [EditorIcon("scriptable_bind")]
    public class RewardInterVariable : ScriptableObject
    {
        [SerializeField] private StringPairVariable remoteConfigUsingAdmob;
        public AdUnitVariable admobRewardInter;
        public AdUnitVariable applovinRewardInter;

        public AdUnitVariable Context()
        {
            bool.TryParse(remoteConfigUsingAdmob.Value.value, out bool usingAdmob);
            return usingAdmob ? admobRewardInter : applovinRewardInter;
        }
    }
}