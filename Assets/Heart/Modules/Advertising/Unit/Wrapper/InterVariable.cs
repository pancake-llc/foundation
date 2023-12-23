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
        [SerializeField] private StringVariable remoteConfigFlagUseAdmob;
        public AdUnitVariable admobInter;
        public AdUnitVariable applovinInter;

        public AdUnitVariable Context()
        {
            bool.TryParse(remoteConfigFlagUseAdmob.Value, out bool status);
            return status ? admobInter : applovinInter;
        }
    }
}