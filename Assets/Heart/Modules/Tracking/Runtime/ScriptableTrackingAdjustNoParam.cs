#if PANCAKE_ADJUST
using com.adjust.sdk;
#endif
using UnityEngine;

namespace Pancake.Tracking
{
    [Searchable]
    [CreateAssetMenu(fileName = "adjust_tracking_name", menuName = "Pancake/Tracking/Adjust No Param", order = 2)]
    [EditorIcon("scriptable_adjust")]
    public class ScriptableTrackingAdjustNoParam : ScriptableTracking
    {
        [Space] [SerializeField] private string eventToken;

        public override void Track()
        {
#if PANCAKE_ADJUST
            if (!Application.isMobilePlatform) return;
            Adjust.trackEvent(new AdjustEvent(eventToken));
#endif
        }
    }
}