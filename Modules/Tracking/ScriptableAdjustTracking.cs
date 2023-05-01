#if PANCAKE_ADJUST
using com.adjust.sdk;
#endif
using UnityEngine;

namespace Pancake.Tracking
{
    [Searchable]
    //[HideMonoScript]
    [CreateAssetMenu(fileName = "adjust_tracking_name", menuName = "Pancake/Tracking/Adjust")]
    [EditorIcon("scriptable_adjust")]
    public class ScriptableAdjustTracking : ScriptableTracking
    {
        [Space] [SerializeField] private string eventToken;

        public override void Track()
        {
            if (Application.isEditor) return;
#if PANCAKE_ADJUST
            Adjust.trackEvent(new AdjustEvent(eventToken));
#endif
        }
    }
}