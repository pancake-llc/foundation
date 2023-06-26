using UnityEngine;
using System.Collections;

namespace Pancake.Sensor
{
    /*
     * A paramemtric shape for creating field of view cones that work with the trigger sensor. Requires a MeshCollider
     * component on the same gameobject. When the script starts it will dynamically create a mesh for the fov cone and
     * assign it to this MeshCollider component.
     */
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public class FOVCollider : MonoBehaviour
    {
        [Tooltip("The length of the field of view cone in world units.")]
        public float Length = 5f;

        [Tooltip("The distance to the near plane of the frustum.")] public float NearDistance = 0.1f;

        [Range(1f, 180f), Tooltip("The arc angle of the fov cone.")] public float FOVAngle = 90f;

        [Range(1f, 180f), Tooltip("The elevation angle of the cone.")]
        public float ElevationAngle = 90f;

        [Range(0, 8), Tooltip("The number of vertices used to approximate the arc of the fov cone. Ideally this should be as low as possible.")]
        public int Resolution = 0;

        // Returns the generated collider mesh so that it can be rendered.
        public Mesh FOVMesh => mesh;

        Mesh mesh;
        MeshCollider mc;
        Vector3[] pts;
        int[] triangles;

        void Awake()
        {
            mc = GetComponent<MeshCollider>();
            CreateCollider();
        }

        void OnValidate()
        {
            Length = Mathf.Max(0f, Length);
            NearDistance = Mathf.Clamp(NearDistance, 0f, Length);
            if (mc != null)
            {
                CreateCollider();
            }
        }

        public void CreateCollider()
        {
            pts = new Vector3[4 + (2 + Resolution) * (2 + Resolution)];
            // There are 2 triangles on the base
            var baseTriangleIndices = 2 * 3;
            // The arc is (Resolution+2) vertices to each side, making (Resolution+1)*(Resolution+1) boxes of 2 tris each
            var arcTriangleIndices = (Resolution + 1) * (Resolution + 1) * 2 * 3;
            // There are 4 sides to the cone, and each side has Resolution+2 triangles
            var sideTriangleIndices = (Resolution + 2) * 3;
            triangles = new int[baseTriangleIndices + arcTriangleIndices + sideTriangleIndices * 4];

            // Base points
            pts[0] = Quaternion.Euler(-ElevationAngle / 2f, -FOVAngle / 2f, 0f) * Vector3.forward * NearDistance; // Bottom Left
            pts[1] = Quaternion.Euler(ElevationAngle / 2f, -FOVAngle / 2f, 0f) * Vector3.forward * NearDistance; // Bottom Right
            pts[2] = Quaternion.Euler(ElevationAngle / 2f, FOVAngle / 2f, 0f) * Vector3.forward * NearDistance; // Top Right
            pts[3] = Quaternion.Euler(-ElevationAngle / 2f, FOVAngle / 2f, 0f) * Vector3.forward * NearDistance; // Top Left
            triangles[0] = 2;
            triangles[1] = 1;
            triangles[2] = 0;
            triangles[3] = 3;
            triangles[4] = 2;
            triangles[5] = 0;

            for (int y = 0; y < 2 + Resolution; y++)
            {
                for (int x = 0; x < 2 + Resolution; x++)
                {
                    int i = 4 + y * (2 + Resolution) + x;
                    float ay = Mathf.Lerp(-FOVAngle / 2f, FOVAngle / 2f, (float) x / (float) (Resolution + 1));
                    float ax = Mathf.Lerp(-ElevationAngle / 2f, ElevationAngle / 2f, (float) y / (float) (Resolution + 1));
                    Vector3 p = Quaternion.Euler(ax, ay, 0f) * Vector3.forward * Length;
                    pts[i] = p;

                    if (x < (1 + Resolution) && y < (1 + Resolution))
                    {
                        var ti = baseTriangleIndices + (y * (Resolution + 1) + x) * 3 * 2;
                        triangles[ti] = i + 1 + (2 + Resolution); // top right
                        triangles[ti + 1] = i + 1; // bottom right
                        triangles[ti + 2] = i; // bottom left
                        triangles[ti + 3] = i + (2 + Resolution); // top left
                        triangles[ti + 4] = i + (2 + Resolution) + 1; // top right
                        triangles[ti + 5] = i; // bottom left
                    }
                }
            }

            // Top and bottom side triangles
            for (int x = 0; x < 2 + Resolution; x++)
            {
                var iTop = 4 + x;
                var iBottom = 4 + (1 + Resolution) * (2 + Resolution) + x;

                var tiTop = baseTriangleIndices + arcTriangleIndices + x * 3;
                var tiBottom = tiTop + sideTriangleIndices;
                if (x == 0)
                {
                    triangles[tiTop] = 2;
                    triangles[tiTop + 1] = 3;
                    triangles[tiTop + 2] = iTop;

                    triangles[tiBottom] = 0;
                    triangles[tiBottom + 1] = 1;
                    triangles[tiBottom + 2] = iBottom;
                }
                else
                {
                    triangles[tiTop] = iTop;
                    triangles[tiTop + 1] = 2;
                    triangles[tiTop + 2] = iTop - 1;

                    triangles[tiBottom] = 1;
                    triangles[tiBottom + 1] = iBottom;
                    triangles[tiBottom + 2] = iBottom - 1;
                }
            }

            // Left and right side triangles
            var yIncr = 2 + Resolution;
            for (int y = 0; y < 2 + Resolution; y++)
            {
                var iLeft = 4 + y * (2 + Resolution);
                var iRight = iLeft + (1 + Resolution);

                var tiLeft = baseTriangleIndices + arcTriangleIndices + sideTriangleIndices * 2 + y * 3;
                var tiRight = tiLeft + sideTriangleIndices;
                if (y == 0)
                {
                    triangles[tiLeft] = 3;
                    triangles[tiLeft + 1] = 0;
                    triangles[tiLeft + 2] = iLeft;

                    triangles[tiRight] = 1;
                    triangles[tiRight + 1] = 2;
                    triangles[tiRight + 2] = iRight;
                }
                else
                {
                    triangles[tiLeft] = 0;
                    triangles[tiLeft + 1] = iLeft;
                    triangles[tiLeft + 2] = iLeft - yIncr;

                    triangles[tiRight] = iRight;
                    triangles[tiRight + 1] = 1;
                    triangles[tiRight + 2] = iRight - yIncr;
                }
            }

            releaseMesh();
            mesh = new Mesh();
            mesh.vertices = pts;
            mesh.triangles = triangles;
            mesh.name = "FOVColliderPoints";
            mc.sharedMesh = mesh;
            mc.convex = true;
            mc.isTrigger = true;
        }

        void releaseMesh()
        {
            if (mc.sharedMesh != null && mc.sharedMesh == mesh)
            {
                DestroyImmediate(mc.sharedMesh, true);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            foreach (Vector3 p in pts)
            {
                Gizmos.DrawSphere(transform.TransformPoint(p), 0.02f);
            }
        }
    }
}