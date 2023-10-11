namespace Pancake.Tracking
{
    using UnityEngine;

    [Searchable]
    [CreateAssetMenu(fileName = "firebase_tracking_name", menuName = "Pancake/Tracking/Firebase Two Param", order = 5)]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableTrackingFirebaseTwoParam : ScriptableTracking
    {
        [Space] [SerializeField] private string eventName;
        [Space, Header("Params")] [SerializeField] private string paramName1;
        [SerializeField] private string paramName2;

        public override void Track() { throw new System.NotImplementedException(); }

        public void Track(string param1Value, string param2Value)
        {
#if PANCAKE_FIREBASE_ANALYTIC
            if (!Application.isMobilePlatform) return;
            Firebase.Analytics.Parameter[] parameters = {new(paramName1, param1Value), new(paramName2, param2Value)};
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameters);
#endif
        }
    }
}