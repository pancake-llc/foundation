using Pancake.Tracking;
using UnityEditor;

namespace Pancake.TrackingEditor
{
    [CustomEditor(typeof(ScriptableTrackingFirebaseNoParam))]
    public class FirebaseNoParamDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI() { DrawDefaultInspector(); }
    }
}