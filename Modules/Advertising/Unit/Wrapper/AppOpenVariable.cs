using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Monetization
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    [Searchable]
    [CreateAssetMenu(fileName = "ad_appopen_unit_wrapper.asset", menuName = "Pancake/Misc/Advertising/App Open Variable Wrapper")]
    [EditorIcon("scriptable_bind")]
    public class AppOpenVariable : ScriptableObject
    {
        [SerializeField] private StringPairVariable remoteConfigUsingAdmob;
        public AdUnitVariable admobAppOpen;
        public AdUnitVariable applovinAppOpen;

        public AdUnitVariable Context()
        {
            bool.TryParse(remoteConfigUsingAdmob.Value.value, out bool usingAdmob);
            return usingAdmob ? admobAppOpen : applovinAppOpen;
        }
    }
}