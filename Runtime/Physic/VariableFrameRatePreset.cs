#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "VariableFrameRatePreset", menuName = "Pancake/Create Frame Rate Preset", order = 0)]
    public class VariableFrameRatePreset : ScriptableObject
    {
        [SerializeField] private VariableFrameRatePhysicsSystem.FixedDeltaTimeType fixedDeltaTimeType;

        public static VariableFrameRatePhysicsSystem.FixedDeltaTimeType GetFixedDeltaTimeType()
        {
            var fixedDeltaTimeType = VariableFrameRatePhysicsSystem.FixedDeltaTimeType.Fixed;

            var instance = Resources.Load<VariableFrameRatePreset>("VariableFrameRatePreset");
            if (instance != null)
            {
                fixedDeltaTimeType = instance.fixedDeltaTimeType;
#if UNITY_EDITOR
                // In the case of Editor, leave the Unload timing to the editor (also save)
#else
            Resources.UnloadAsset(instance);
#endif
            }

            return fixedDeltaTimeType;
        }

        public static float GetDefaultFixedDeltaTimeStep()
        {
#if UNITY_EDITOR
            var timeManager = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TimeManager.asset");
            var timeManagerSo = new SerializedObject(timeManager);
            var fixedTimestep = timeManagerSo.FindProperty("Fixed Timestep").floatValue;
            timeManagerSo.Dispose();
            return fixedTimestep;
#else
        return Time.fixedDeltaTime;
#endif
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(VariableFrameRatePreset))]
    public class VariableFrameRatePresetEditor : Editor
    {
        private const string DESCRIPTION = "Description of each: \n" + "\n" + "Fixed:\n" +
                                           " Unity's original method. fixedDeltaTime is fixed and updated at 0.02(s) \n" +
                                           " FixedUpdate 0~ times in 1 frame, physics is stable.\n" + "\n" + "Variable:\n" + " fixedDeltaTime = deltaTime. \n" +
                                           " FixedUpdate exactly once per frame.\n" + " Works well with the rest of the game.\n" + "\n" + "VariableWithSubStep:\n" +
                                           " Divide deltaTime by the original fixedDeltaTime value,\n" + " Set to fixedDeltaTime. (Example: 0.033 = 0.02 + 0.013)\n" +
                                           " FixedUpdate 1~ times per frame.\n" + " A compromise between Fixed and Variable (stability & compatibility)\n";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(DESCRIPTION);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif
}