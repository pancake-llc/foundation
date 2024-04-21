using UnityEngine;

namespace Pancake.Tracking
{
    [Searchable]
    [CreateAssetMenu(fileName = "firebase_tracking_name", menuName = "Pancake/Tracking/Firebase Six Param", order = 9)]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableTrackingFirebaseSixParam : ScriptableTracking
    {
        [Space] [SerializeField] private string eventName;
        [Space, Header("Params")] [SerializeField] private string paramName1;
        [SerializeField] private string paramName2;
        [SerializeField] private string paramName3;
        [SerializeField] private string paramName4;
        [SerializeField] private string paramName5;
        [SerializeField] private string paramName6;

        public override void Track() { throw new System.NotImplementedException(); }

        public void Track(string param1Value, string param2Value, string param3Value, string param4Value, string param5Value, string param6Value)
        {
#if PANCAKE_FIREBASE_ANALYTIC
            if (!Application.isMobilePlatform) return;
            Firebase.Analytics.Parameter[] parameters =
            {
                new(paramName1, param1Value), new(paramName2, param2Value), new(paramName3, param3Value), new(paramName4, param4Value), new(paramName5, param5Value),
                new(paramName6, param6Value)
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameters);
#endif
        }
    }
}