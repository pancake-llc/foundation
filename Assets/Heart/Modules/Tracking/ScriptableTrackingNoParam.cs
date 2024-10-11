using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.Tracking
{
    [Searchable]
    [CreateAssetMenu(fileName = "scriptable_tracking_noparam", menuName = "Pancake/Tracking/WrapperNo Param", order = 1)]
    [EditorIcon("so_blue_tracking")]
    public class ScriptableTrackingNoParam : ScriptableObject
    {
        [SerializeField, TextArea(3, 6)] private string developerDescription;


        [SerializeField, LabelText("Firebase")] private ScriptableTrackingFirebaseNoParam scriptableTrackingFirebaseNoParamEvent;
        [SerializeField, LabelText("Adjust")] private ScriptableTrackingAdjustNoParam scriptableTrackingAdjustNoParamEvent;


        public void Track()
        {
            if (scriptableTrackingFirebaseNoParamEvent != null) scriptableTrackingFirebaseNoParamEvent.Track();
            if (scriptableTrackingAdjustNoParamEvent != null) scriptableTrackingAdjustNoParamEvent.Track();
        }
    }
}