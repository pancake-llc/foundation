using Pancake.Sensor;
using UnityEditor;

namespace Pancake.SensorEditor
{
    [CustomEditor(typeof(UserSignals))]
    [CanEditMultipleObjects]
    public class UserSignalsEditor : BaseSensorEditor<UserSignals>
    {
        SerializedProperty inputSignals;

        protected override bool canTest => false;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (serializedObject == null)
            {
                return;
            }

            inputSignals = serializedObject.FindProperty("inputSignals");
        }

        protected override void InspectorParameters() { EditorGUILayout.PropertyField(inputSignals, true); }
    }
}