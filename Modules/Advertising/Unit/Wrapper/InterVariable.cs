using Pancake.Scriptable;
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
        [SerializeField] private StringPairVariable remoteConfigUsingAdmob;
        public AdUnitVariable admobInter;
        public AdUnitVariable applovinInter;

        public AdUnitVariable Context()
        {
            bool.TryParse(remoteConfigUsingAdmob.Value.value, out bool usingAdmob);
            return usingAdmob ? admobInter : applovinInter;
        }
    }
}