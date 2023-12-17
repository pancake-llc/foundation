using Pancake.ExLibEditor;
using Pancake.Greenery;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryGrassBlades), true)]
    public class GreeneryGrassBladesEditor : GreeneryProceduralItemEditor
    {
        private SerializedProperty _widthRangeProperty;
        private SerializedProperty _heightRangeProperty;
        private SerializedProperty _positioningRadiusProperty;
        private SerializedProperty _forwardRotationProperty;
        private SerializedProperty _bladeCurvatureProperty;
        private SerializedProperty _normalCurvingProperty;
        private SerializedProperty _bladesPerPointProperty;
        private SerializedProperty _segmentsPerBladeProperty;
        private SerializedProperty _rotationVariationProperty;
        private SerializedProperty _curvatureVariationProperty;

        protected override void Init()
        {
            base.Init();

            _widthRangeProperty = serializedObject.FindProperty("widthRange");
            _heightRangeProperty = serializedObject.FindProperty("heightRange");
            _positioningRadiusProperty = serializedObject.FindProperty("positioningRadius");
            _forwardRotationProperty = serializedObject.FindProperty("forwardRotation");
            _bladeCurvatureProperty = serializedObject.FindProperty("bladeCurvature");
            _normalCurvingProperty = serializedObject.FindProperty("normalCurving");
            _bladesPerPointProperty = serializedObject.FindProperty("bladesPerPoint");
            _segmentsPerBladeProperty = serializedObject.FindProperty("segmentsPerBlade");
            _rotationVariationProperty = serializedObject.FindProperty("rotationVariation");
            _curvatureVariationProperty = serializedObject.FindProperty("curvatureVariation");

            var procedural = target as GreeneryProceduralItem;
            if (procedural != null && procedural.renderingMaterial == null)
            {
                procedural.renderingMaterial = new Material(procedural.shader);
                var materialPreset = ProjectDatabase.FindAssetWithPath<Preset>("Modules/Greenery/Presets/GrassBladesPreset.preset");
                if (materialPreset != null) materialPreset.ApplyTo(procedural.renderingMaterial);
                AssetDatabase.AddObjectToAsset(procedural.renderingMaterial, AssetDatabase.GetAssetPath(target));
                AssetDatabase.Refresh();
            }
        }

        protected override void Draw()
        {
            base.Draw();

            EditorGUILayout.PropertyField(_widthRangeProperty);
            EditorGUILayout.PropertyField(_heightRangeProperty);
            EditorGUILayout.PropertyField(_positioningRadiusProperty);
            EditorGUILayout.PropertyField(_forwardRotationProperty);
            EditorGUILayout.PropertyField(_bladeCurvatureProperty);
            EditorGUILayout.PropertyField(_normalCurvingProperty);
            EditorGUILayout.PropertyField(_bladesPerPointProperty);
            EditorGUILayout.PropertyField(_segmentsPerBladeProperty);
            EditorGUILayout.PropertyField(_rotationVariationProperty);
            EditorGUILayout.PropertyField(_curvatureVariationProperty);
        }
    }
}