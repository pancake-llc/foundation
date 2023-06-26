using UnityEngine;
using System.Collections;

namespace Pancake.Sensor
{
    /*
     * Add this component to a gameobject which has a FOVCollider2D and it will add a mesh renderer and assign the fovs mesh to it.
     * From here you may add your own material to the mesh renderer.
     */
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(FOVCollider2D))]
    [ExecuteInEditMode]
    public class RenderFOVCollider2D : MonoBehaviour
    {
        MeshFilter mf;
        FOVCollider2D fov;

        void Awake()
        {
            mf = GetComponent<MeshFilter>();
            fov = GetComponent<FOVCollider2D>();
        }

        void Update()
        {
            if (mf.sharedMesh != fov.FOVMesh)
            {
                mf.sharedMesh = fov.FOVMesh;
            }
        }
    }
}