using Pancake.Tracking;
using UnityEditor;

namespace Pancake.TrackingEditor
{
    [CustomEditor(typeof(ScriptableTrackingAdjustNoParam))]
    public class AdjustNoParamDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI() { DrawDefaultInspector(); }
    }
}