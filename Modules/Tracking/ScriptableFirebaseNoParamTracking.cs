using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Tracking
{
    [Searchable]
    [HideMono]
    [CreateAssetMenu(fileName = "firebase_tracking_name", menuName = "Pancake/Tracking/Firebase No Param")]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableFirebaseNoParamTracking : ScriptableTracking
    {
        [Space] [SerializeField] private string eventName;

        public override void Track()
        {
            if (Application.isEditor) return;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
        }
    }
}