using Pancake.Apex;
using UnityEngine;

namespace Pancake.Tracking
{
    [Searchable]
    [CreateAssetMenu(fileName = "scriptable_tracking_noparam", menuName = "Pancake/Tracking/No Param", order = 1)]
    [EditorIcon("scriptable_tracking")]
    public class ScriptableTrackingNoParam : ScriptableObject
    {
        [SerializeField, TextArea(3, 6)] private string developerDescription;


        [SerializeField, Label("Firebase")] private ScriptableTrackingFirebaseNoParam scriptableTrackingFirebaseNoParamEvent;
        [SerializeField, Label("Adjust")] private ScriptableTrackingAdjustNoParam scriptableTrackingAdjustNoParamEvent;


        public void Track()
        {
            if (scriptableTrackingFirebaseNoParamEvent != null) scriptableTrackingFirebaseNoParamEvent.Track();
            if (scriptableTrackingAdjustNoParamEvent != null) scriptableTrackingAdjustNoParamEvent.Track();
        }
    }
}