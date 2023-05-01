namespace Pancake.Tracking
{
    using UnityEngine;

    [Searchable]
    //[HideMonoScript]
    [CreateAssetMenu(fileName = "firebase_tracking_name", menuName = "Pancake/Tracking/Firebase One Param")]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableFirebaseOneParamTracking : ScriptableTracking
    {
        [Space] [SerializeField] private string eventName;
        [Space, Header("Params")] [SerializeField] private string paramName;

        public override void Track() { throw new System.NotImplementedException(); }

        public void Track(string paramValue)
        {
#if PANCAKE_FIREBASE_ANALYTIC
            if (Application.isEditor) return;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
#endif
        }
    }
}