namespace Pancake.OptimizerCollider
{
    using UnityEngine;
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [RequireComponent(typeof(PolygonCollider2D))]
    [ExecuteInEditMode]
    public class PolygonColliderOptimizer : MonoBehaviour
    {
        [Min(0f)] public float optimizationFactor = 0;
        [HideInInspector] public OptimizedPathData savedPathData;

        private PolygonCollider2D _collider;
        private List<List<Vector2>> _initPaths = new List<List<Vector2>>();
        private void OnEnable() { GetInitPaths(); }

        private void GetInitPaths()
        {
            _collider = GetComponent<PolygonCollider2D>();
            for (int i = 0; i < _collider.pathCount; i++)
            {
                List<Vector2> path = new List<Vector2>(_collider.GetPath(i));
                _initPaths.Add(path);
            }
        }

        public void OptimizePolygonCollider(float tolerance)
        {
            float toleranceNormalized = tolerance / _initPaths.Count;

            if (_collider == null)
                GetInitPaths();

            if (toleranceNormalized == 0)
            {
                for (int i = 0; i < _initPaths.Count; i++)
                {
                    List<Vector2> path = _initPaths[i];
                    _collider.SetPath(i, path.ToArray());
                }

                return;
            }

            for (int i = 0; i < _initPaths.Count; i++)
            {
                List<Vector2> path = _initPaths[i];
                path = DouglasPeuckerReduction.DouglasPeuckerReductionPoints(path, toleranceNormalized);
                _collider.SetPath(i, path.ToArray());
            }
        }

        public void Reset() { OptimizePolygonCollider(0); }
#if UNITY_EDITOR
        public void SaveOptimizedPath()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Optimized Path", "OptimizedPath", "asset", "Enter the filename for Optimized path:");
            if (!string.IsNullOrEmpty(path))
            {
                OptimizedPathData pathData = ScriptableObject.CreateInstance<OptimizedPathData>();
                pathData.paths = new List<List<Vector2>>();

                List<List<Vector2>> currentPaths = new List<List<Vector2>>();
                for (int i = 0; i < _collider.pathCount; i++)
                {
                    List<Vector2> p = new List<Vector2>(_collider.GetPath(i));
                    currentPaths.Add(p);
                }

                pathData.paths = currentPaths;

                AssetDatabase.CreateAsset(pathData, path);
                AssetDatabase.SaveAssets();
            }
        }

        public void LoadSavedPath(OptimizedPathData pathData)
        {
            if (pathData == null || _collider == null)
            {
                Debug.LogWarning("Add a path first bauss :3");
                return;
            }

            _collider.pathCount = pathData.paths.Count;

            for (int i = 0; i < pathData.paths.Count; i++)
                _collider.SetPath(i, pathData.paths[i].ToArray());
        }
#endif
    }
}