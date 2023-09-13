namespace Pancake.Tracking
{
    using UnityEngine;

    [Searchable]
    [CreateAssetMenu(fileName = "firebase_tracking_name", menuName = "Pancake/Tracking/Firebase One Param", order = 4)]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableTrackingFirebaseOneParam : ScriptableTracking
    {
        [Space] [SerializeField] private string eventName;
        [Space, Header("Params")] [SerializeField] private string paramName;

        public override void Track() { throw new System.NotImplementedException(); }

        public void Track(string paramValue)
        {
#if PANCAKE_FIREBASE_ANALYTIC
            if (!Application.isMobilePlatform) return;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
#endif
        }
    }
}