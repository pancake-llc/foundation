using UnityEngine;

namespace Pancake.Tracking
{
    [Searchable]
    [CreateAssetMenu(fileName = "firebase_tracking_name", menuName = "Pancake/Tracking/Firebase No Param", order = 3)]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableTrackingFirebaseNoParam : ScriptableTracking
    {
        [Space] [SerializeField] private string eventName;

        public override void Track()
        {
#if PANCAKE_FIREBASE_ANALYTIC
            if (!Application.isMobilePlatform) return;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
#endif
        }
    }
}