using System;
using Pancake.ExLibEditor;
using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryLODInstance))]
    public class GreeneryLODInstanceEditor : UnityEditor.Editor
    {
        private MaterialEditor _materialEditor;
        
        private SerializedProperty _lodCullingCsProperty;
        private SerializedProperty _maxDrawDistanceProperty;
        private SerializedProperty _sizeRangeProperty;
        private SerializedProperty _pivotOffsetProperty;
        private SerializedProperty _alignToSurfaceProperty;
        private SerializedProperty _useSurfaceNormalsProperty;
        private SerializedProperty _xRotationProperty;
        private SerializedProperty _yRotationProperty;
        private SerializedProperty _zRotationProperty;
        private SerializedProperty _instanceLODsProperty;

        private void OnEnable()
        {
            _lodCullingCsProperty = serializedObject.FindProperty("lodCullingCS");
            _maxDrawDistanceProperty = serializedObject.FindProperty("maxDrawDistance");
            _sizeRangeProperty = serializedObject.FindProperty("sizeRange");
            _pivotOffsetProperty = serializedObject.FindProperty("pivotOffset");
            _alignToSurfaceProperty = serializedObject.FindProperty("alignToSurface");
            _useSurfaceNormalsProperty = serializedObject.FindProperty("useSurfaceNormals");
            _xRotationProperty = serializedObject.FindProperty("xRotation");
            _yRotationProperty = serializedObject.FindProperty("yRotation");
            _zRotationProperty = serializedObject.FindProperty("zRotation");
            _instanceLODsProperty = serializedObject.FindProperty("instanceLODs");
        }


        public override void OnInspectorGUI()
        {
            var greeneryLOD = target as GreeneryLODInstance;
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(_lodCullingCsProperty);
            EditorGUILayout.PropertyField(_maxDrawDistanceProperty);
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
            
            EditorGUILayout.PropertyField(_instanceLODsProperty, true);
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            int previewIndex = EditorGUILayout.IntSlider("LOD Preview", greeneryLOD.previewIndex, 0, Math.Max(greeneryLOD.instanceLODs.Count - 1, 0));
            
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < greeneryLOD.instanceLODs.Count; i++)
            {
                var instanceLOD = greeneryLOD.instanceLODs[i];
                if (i == 0)
                {
                    if (GUILayout.Button(i.ToString(), GUILayout.Width((Screen.width - 50) * instanceLOD.LODFactor), GUILayout.Height(30)))
                    {
                        previewIndex = i;
                    }
                }
                else
                {
                    if (GUILayout.Button(i.ToString(),
                            GUILayout.Width((Screen.width - 50) * (instanceLOD.LODFactor - greeneryLOD.instanceLODs[i - 1].LODFactor)),
                            GUILayout.Height(30)))
                    {
                        previewIndex = i;
                    }
                }
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(greeneryLOD, "Changed preview index");
                greeneryLOD.previewIndex = previewIndex;

                if (_materialEditor != null) DestroyImmediate(_materialEditor);
                if (greeneryLOD.instanceLODs.Count == 0) return;
                if (greeneryLOD.instanceLODs[previewIndex].instanceMaterial != null)
                {
                    _materialEditor = (MaterialEditor) CreateEditor(greeneryLOD.instanceLODs[previewIndex].instanceMaterial);
                }
            }

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();


            if (_materialEditor != null)
            {
                _materialEditor.DrawHeader();
                _materialEditor.OnInspectorGUI();
            }
        }

        private void OnDisable() { DestroyImmediate(_materialEditor); }
    }
}