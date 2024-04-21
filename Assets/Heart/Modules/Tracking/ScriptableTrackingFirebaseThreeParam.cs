using System;

namespace Pancake.Tracking
{
    using UnityEngine;

    [Searchable]
    [CreateAssetMenu(fileName = "firebase_tracking_name", menuName = "Pancake/Tracking/Firebase Three Param", order = 6)]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableTrackingFirebaseThreeParam : ScriptableTracking
    {
        [Space] [SerializeField] private string eventName;
        [Space, Header("Params")] [SerializeField] private string paramName1;
        [SerializeField] private string paramName2;
        [SerializeField] private string paramName3;

        public override void Track() { throw new NotImplementedException(); }

        public void Track(string param1Value, string param2Value, string param3Value)
        {
#if PANCAKE_FIREBASE_ANALYTIC
            if (!Application.isMobilePlatform) return;
            Firebase.Analytics.Parameter[] parameters = {new(paramName1, param1Value), new(paramName2, param2Value), new(paramName3, param3Value)};
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameters);
#endif
        }
    }
}