using Pancake.OptimizerCollider;

namespace Pancake.OptimizerColliderEditor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(PolygonColliderOptimizer))]
    public class PolygonColliderOptimizerEditor : Editor
    {
        private PolygonColliderOptimizer _polygonOptimizer;
        private void OnEnable() { _polygonOptimizer = (PolygonColliderOptimizer) target; }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Optimize Collider", EditorStyles.boldLabel);

            base.OnInspectorGUI();

            float tolerance = _polygonOptimizer.optimizationFactor / 50f;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Optimize Polygon Collider")) _polygonOptimizer.OptimizePolygonCollider(tolerance);

            if (GUILayout.Button("Reset")) _polygonOptimizer.Reset();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Save Optimized Path")) _polygonOptimizer.SaveOptimizedPath();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Load Saved Collider Data", EditorStyles.boldLabel);

            _polygonOptimizer.savedPathData =
                (OptimizedPathData) EditorGUILayout.ObjectField("Saved Path", _polygonOptimizer.savedPathData, typeof(OptimizedPathData), false);

            if (GUILayout.Button("Load Saved Path")) _polygonOptimizer.LoadSavedPath(_polygonOptimizer.savedPathData);
        }
    }
}