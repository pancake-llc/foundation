using UnityEngine;
using System.Collections;

namespace Pancake.Sensor
{
    /*
     * Add this component to a gameobject which has a FOVCollider and it will add a mesh renderer and assign the fovs mesh to it.
     * From here you may add your own material to the mesh renderer.
     */
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(FOVCollider))]
    [ExecuteInEditMode]
    public class RenderFOVCollider : MonoBehaviour
    {
        MeshFilter mf;
        FOVCollider fov;

        void Awake()
        {
            mf = GetComponent<MeshFilter>();
            fov = GetComponent<FOVCollider>();
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