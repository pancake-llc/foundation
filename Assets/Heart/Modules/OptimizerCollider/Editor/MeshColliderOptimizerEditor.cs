using Pancake.OptimizerCollider;
using Pancake.ComputationalGeometry;

namespace Pancake.OptimizerColliderEditor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(MeshColliderOptimizer))]
    public class MeshColliderOptimizerEditor : Editor
    {
        private MeshColliderOptimizer _meshOptimizer;
        private void OnEnable() { _meshOptimizer = (MeshColliderOptimizer) target; }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Optimize Collider", EditorStyles.boldLabel);

            _meshOptimizer.meshProperties.connectingMode =
                (HalfEdgeData3.ConnectOppositeEdges) EditorGUILayout.EnumPopup("Connecting Mode", _meshOptimizer.meshProperties.connectingMode);
            _meshOptimizer.meshProperties.optimizationFactor =
                Mathf.Max(0f, EditorGUILayout.FloatField("Optimization Factor", _meshOptimizer.meshProperties.optimizationFactor));
            _meshOptimizer.meshProperties.meshStyle = (MyMesh.MeshStyle) EditorGUILayout.EnumPopup("Mesh Style", _meshOptimizer.meshProperties.meshStyle);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Optimize Mesh Collider")) _meshOptimizer.SimplifyMeshCollider(_meshOptimizer.meshProperties);

            if (GUILayout.Button("Reset")) _meshOptimizer.Reset();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Save Optimized Collider")) _meshOptimizer.SaveOptimizedCollider();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Load Saved Collider Data", EditorStyles.boldLabel);

            _meshOptimizer.savedMesh = (Mesh) EditorGUILayout.ObjectField("Saved Mesh", _meshOptimizer.savedMesh, typeof(Mesh), false);

            if (GUILayout.Button("Load Saved Mesh")) _meshOptimizer.LoadSavedMesh(_meshOptimizer.savedMesh);
        }
    }
}