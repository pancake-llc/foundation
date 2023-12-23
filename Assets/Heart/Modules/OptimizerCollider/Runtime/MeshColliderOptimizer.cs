namespace Pancake.OptimizerCollider
{
    using UnityEngine;
    using ComputationalGeometry;
    using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class MeshColliderOptimizer : MonoBehaviour
    {
        private Mesh _initMesh;
        private MeshCollider _collider;
        public MeshProperties meshProperties;
        [HideInInspector] public Mesh savedMesh;

        public struct MeshProperties
        {
            public HalfEdgeData3.ConnectOppositeEdges connectingMode;
            public float optimizationFactor;
            public MyMesh.MeshStyle meshStyle;
        }

        private void OnEnable() { GetInit(); }

        private void GetInit()
        {
            _initMesh = GetComponent<MeshFilter>().sharedMesh;
            _collider = GetComponent<MeshCollider>();
        }

        public void SimplifyMeshCollider(MeshProperties mp)
        {
            int edgesToContract = (int) (mp.optimizationFactor * 25f);

            if (_collider == null)
                _collider = GetComponent<MeshCollider>();

            if (edgesToContract <= 0)
            {
                _collider.sharedMesh = _initMesh;
                return;
            }

            Mesh meshToSimplify = _initMesh;
            MyMesh myMeshToSimplify = new MyMesh(meshToSimplify);

            Normalizer3 normalizer = new Normalizer3(myMeshToSimplify.vertices);
            myMeshToSimplify.vertices = normalizer.Normalize(myMeshToSimplify.vertices);

            HalfEdgeData3 myMeshToSimplify_HalfEdge = new HalfEdgeData3(myMeshToSimplify, mp.connectingMode);
            HalfEdgeData3 mySimplifiedMesh_HalfEdge = MeshSimplification_QEM.Simplify(myMeshToSimplify_HalfEdge,
                maxEdgesToContract: edgesToContract,
                maxError: Mathf.Infinity,
                normalizeTriangles: true);

            MyMesh mySimplifiedMesh = mySimplifiedMesh_HalfEdge.ConvertToMyMesh("Simplified Mesh", mp.meshStyle);
            mySimplifiedMesh.vertices = normalizer.UnNormalize(mySimplifiedMesh.vertices);
            mySimplifiedMesh.vertices = mySimplifiedMesh.vertices.Select(x => x.ToVector3().ToMyVector3()).ToList();

            Mesh unitySimplifiedMesh = mySimplifiedMesh.ConvertToUnityMesh(generateNormals: true, meshName: "Simplified Collider");

            _collider.sharedMesh = unitySimplifiedMesh;
        }

        public void Reset()
        {
            if (_initMesh == null) _initMesh = GetComponent<MeshCollider>().sharedMesh;

            if (_collider == null) _collider = GetComponent<MeshCollider>();

            _collider.sharedMesh = _initMesh;
        }

#if UNITY_EDITOR
        public void SaveOptimizedCollider()
        {
            if (_collider.sharedMesh != null)
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Optimized Collider", "OptimizedCollider", "asset", "Enter the filename for Optimized Collider:");
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.CreateAsset(_collider.sharedMesh, path);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                Debug.LogWarning("Need an Optimized Collider bauss :3");
            }
        }

        public void LoadSavedMesh(Mesh mesh)
        {
            if (mesh == null || _collider == null)
            {
                Debug.LogWarning("Add a path first bauss :3");
                return;
            }

            _collider.sharedMesh = mesh;
        }
#endif
    }
}