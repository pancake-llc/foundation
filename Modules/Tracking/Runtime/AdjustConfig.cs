#if PANCAKE_ADJUST
using com.adjust.sdk;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Pancake.Tracking
{
    [EditorIcon("scriptable_adjust")]
    public class AdjustConfig : ScriptableSettings<AdjustConfig>
    {
        [SerializeField] private string appToken;

        public static string AppToken => Instance.appToken;

#if PANCAKE_ADJUST
        [SerializeField] private AdjustEnvironment environment = AdjustEnvironment.Production;
        [SerializeField] private AdjustLogLevel logLevel = AdjustLogLevel.Error;

        public static AdjustEnvironment Environment => Instance.environment;
        public static AdjustLogLevel LogLevel => Instance.logLevel;
        public static bool IsProductEnvironment => Environment == AdjustEnvironment.Production;
        public static bool IsErrorLogLevel => LogLevel == AdjustLogLevel.Error;
#endif
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AdjustConfig))]
    public class AdjustConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _appToken;
#if PANCAKE_ADJUST
        private SerializedProperty _environment;
        private SerializedProperty _logLevel;
#endif

        private void OnEnable()
        {
            _appToken = serializedObject.FindProperty("appToken");
#if PANCAKE_ADJUST
            _environment = serializedObject.FindProperty("environment");
            _logLevel = serializedObject.FindProperty("logLevel");
#endif
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_appToken);
#if PANCAKE_ADJUST
            EditorGUILayout.PropertyField(_environment);
            EditorGUILayout.PropertyField(_logLevel);
#endif
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}