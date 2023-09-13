using UnityEngine;

namespace Pancake.Tracking
{
    [Searchable]
    //[HideMonoScript]
    [CreateAssetMenu(fileName = "firebase_tracking_name", menuName = "Pancake/Tracking/Firebase No Param")]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableFirebaseNoParamTracking : ScriptableTracking
    {
        [Space] [SerializeField] private string eventName;

        public override void Track()
        {
            Debug.Log("Application.isMobilePlatform: "+ Application.isMobilePlatform);
            Debug.Log("Tracking");
#if PANCAKE_FIREBASE_ANALYTIC
            if (!Application.isMobilePlatform) return;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
#endif
        }
    }
}