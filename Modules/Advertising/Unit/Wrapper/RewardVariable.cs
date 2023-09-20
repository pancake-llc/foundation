using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Monetization
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    [Searchable]
    [CreateAssetMenu(fileName = "ad_reward_unit_wrapper.asset", menuName = "Pancake/Misc/Advertising/Reward Variable Wrapper", order = 3)]
    [EditorIcon("scriptable_bind")]
    public class RewardVariable : ScriptableObject
    {
        [SerializeField] private StringVariable remoteConfigFlagUseAdmob;
        [SerializeField] private AdUnitVariable admobReward;
        [SerializeField] private AdUnitVariable applovinReward;

        public AdUnitVariable Context()
        {
            bool.TryParse(remoteConfigFlagUseAdmob.Value, out bool status);
            return status ? admobReward : applovinReward;
        }
    }
}