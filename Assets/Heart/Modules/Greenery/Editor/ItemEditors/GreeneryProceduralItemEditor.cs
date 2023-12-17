using Pancake.Greenery;
using UnityEditor;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryProceduralItem), true)]
    public class GreeneryProceduralItemEditor : UnityEditor.Editor
    {
        private MaterialEditor _materialEditor;
        private SerializedProperty _castShadowsProperty;
        private SerializedProperty _renderingCsProperty;
        private SerializedProperty _applyCullingProperty;
        private SerializedProperty _maxDistanceProperty;
        private SerializedProperty _minFadeDistanceProperty;

        private void OnEnable()
        {
            var item = target as GreeneryProceduralItem;
            if (item != null && item.renderingMaterial != null)
            {
                if (_materialEditor == null) _materialEditor = (MaterialEditor) CreateEditor(item.renderingMaterial);
                else
                {
                    DestroyImmediate(_materialEditor);
                    _materialEditor = (MaterialEditor) CreateEditor(item.renderingMaterial);
                }
            }
            
            Init();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Draw();
            serializedObject.ApplyModifiedProperties();

            if (_materialEditor != null)
            {
                _materialEditor.DrawHeader();
                _materialEditor.OnInspectorGUI();
            }
        }

        protected virtual void Init()
        {
            _castShadowsProperty = serializedObject.FindProperty("castShadows");
            _renderingCsProperty = serializedObject.FindProperty("renderingCS");
            _applyCullingProperty = serializedObject.FindProperty("applyCulling");
            _maxDistanceProperty = serializedObject.FindProperty("maxDistance");
            _minFadeDistanceProperty = serializedObject.FindProperty("minFadeDistance");
        }

        protected virtual void Draw()
        {
            EditorGUILayout.PropertyField(_castShadowsProperty);
            EditorGUILayout.PropertyField(_renderingCsProperty);
            EditorGUILayout.PropertyField(_applyCullingProperty);
            EditorGUILayout.PropertyField(_maxDistanceProperty);
            EditorGUILayout.PropertyField(_minFadeDistanceProperty);
        }


        private void OnDisable() { DestroyImmediate(_materialEditor); }
    }
}