using Pancake.ExLibEditor;
using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryInstance))]
    public class GreeneryInstanceEditor : UnityEditor.Editor
    {
        private MaterialEditor _materialEditor;

        private SerializedProperty _castShadowsProperty;
        private SerializedProperty _instanceCullingCsProperty;
        private SerializedProperty _maxDrawDistanceProperty;
        private SerializedProperty _instancedMeshProperty;
        private SerializedProperty _instanceMaterialProperty;
        private SerializedProperty _sizeRangeProperty;
        private SerializedProperty _pivotOffsetProperty;
        private SerializedProperty _alignToSurfaceProperty;
        private SerializedProperty _useSurfaceNormalsProperty;
        private SerializedProperty _xRotationProperty;
        private SerializedProperty _yRotationProperty;
        private SerializedProperty _zRotationProperty;

        private void OnEnable()
        {
            var item = target as GreeneryInstance;
            if (item != null && item.instanceMaterial != null)
            {
                if (_materialEditor == null) _materialEditor = (MaterialEditor) CreateEditor(item.instanceMaterial);
                else
                {
                    DestroyImmediate(_materialEditor);
                    _materialEditor = (MaterialEditor) CreateEditor(item.instanceMaterial);
                }
            }

            _castShadowsProperty = serializedObject.FindProperty("castShadows");
            _instanceCullingCsProperty = serializedObject.FindProperty("instanceCullingCS");
            _maxDrawDistanceProperty = serializedObject.FindProperty("maxDrawDistance");
            _instancedMeshProperty = serializedObject.FindProperty("instancedMesh");
            _instanceMaterialProperty = serializedObject.FindProperty("instanceMaterial");
            _sizeRangeProperty = serializedObject.FindProperty("sizeRange");
            _pivotOffsetProperty = serializedObject.FindProperty("pivotOffset");
            _alignToSurfaceProperty = serializedObject.FindProperty("alignToSurface");
            _useSurfaceNormalsProperty = serializedObject.FindProperty("useSurfaceNormals");
            _xRotationProperty = serializedObject.FindProperty("xRotation");
            _yRotationProperty = serializedObject.FindProperty("yRotation");
            _zRotationProperty = serializedObject.FindProperty("zRotation");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_castShadowsProperty);
            EditorGUILayout.PropertyField(_instanceCullingCsProperty);
            EditorGUILayout.PropertyField(_maxDrawDistanceProperty);
            EditorGUILayout.PropertyField(_instancedMeshProperty);
            EditorGUILayout.PropertyField(_instanceMaterialProperty);
            EditorGUILayout.PropertyField(_sizeRangeProperty);
            EditorGUILayout.PropertyField(_pivotOffsetProperty);
            EditorGUILayout.PropertyField(_alignToSurfaceProperty);
            EditorGUILayout.PropertyField(_useSurfaceNormalsProperty);
            var rect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(60);
            var xRotationRect = new Rect(rect.x, rect.y + 20, rect.width, rect.height);
            xRotationRect = EditorGUI.PrefixLabel(xRotationRect, new GUIContent("X Rotation"));
            Uniform.DrawSilderVector2Int(ref xRotationRect, _xRotationProperty, 0, 360);
            var yRotationRect = new Rect(rect.x, rect.y + 40, rect.width, rect.height);
            yRotationRect = EditorGUI.PrefixLabel(yRotationRect, new GUIContent("Y Rotation"));
            Uniform.DrawSilderVector2Int(ref yRotationRect, _yRotationProperty, 0, 360);
            var zRotationRect = new Rect(rect.x, rect.y + 60, rect.width, rect.height);
            zRotationRect = EditorGUI.PrefixLabel(zRotationRect, new GUIContent("Z Rotation"));
            Uniform.DrawSilderVector2Int(ref zRotationRect, _zRotationProperty, 0, 360);
            serializedObject.ApplyModifiedProperties();

            if (_materialEditor != null)
            {
                _materialEditor.DrawHeader();
                _materialEditor.OnInspectorGUI();
            }
        }


        private void OnDisable() { DestroyImmediate(_materialEditor); }
    }
}