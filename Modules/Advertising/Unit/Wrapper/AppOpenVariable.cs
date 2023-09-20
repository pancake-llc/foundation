using Pancake.Scriptable;
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
        [SerializeField] private StringVariable remoteConfigFlagUseAdmob;
        public AdUnitVariable admobAppOpen;
        public AdUnitVariable applovinAppOpen;

        public AdUnitVariable Context()
        {
            bool.TryParse(remoteConfigFlagUseAdmob.Value, out bool status);
            return status ? admobAppOpen : applovinAppOpen;
        }
    }
}