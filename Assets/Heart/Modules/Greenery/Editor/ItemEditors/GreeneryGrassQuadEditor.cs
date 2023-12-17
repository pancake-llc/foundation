using Pancake.ExLibEditor;
using Pancake.Greenery;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryGrassQuad), true)]
    public class GreeneryGrassQuadEditor : GreeneryProceduralItemEditor
    {
        private SerializedProperty _widthRangeProperty;
        private SerializedProperty _heightRangeProperty;
        private SerializedProperty _spacingProperty;

        private bool _initialized;

        protected override void Init()
        {
            base.Init();

            _widthRangeProperty = serializedObject.FindProperty("widthRange");
            _heightRangeProperty = serializedObject.FindProperty("heightRange");
            _spacingProperty = serializedObject.FindProperty("spacing");

            var procedural = target as GreeneryProceduralItem;
            if (procedural != null && procedural.renderingMaterial == null)
            {
                procedural.renderingMaterial = new Material(procedural.shader);
                var materialPreset = ProjectDatabase.FindAssetWithPath<Preset>("Modules/Greenery/Presets/GrassQuadPreset.preset");
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
            EditorGUILayout.PropertyField(_spacingProperty);
        }
    }
}