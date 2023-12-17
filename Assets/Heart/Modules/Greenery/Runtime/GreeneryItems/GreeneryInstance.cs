using Pancake.Apex;
using UnityEngine;

namespace Pancake.Greenery
{
    [CreateAssetMenu(fileName = "GreeneryInstance", menuName = "Pancake/Greenery/Greenery Instance", order = 0)]
    public class GreeneryInstance : GreeneryItem
    {
        public bool castShadows;

        [Space]
        // ReSharper disable once InconsistentNaming
        public ComputeShader instanceCullingCS;
        public float maxDrawDistance = 100.0f;
        public Mesh instancedMesh;
        public Material instanceMaterial;
        [Space]
        public Vector2 sizeRange = new(1, 1);
        public float pivotOffset;
        public bool alignToSurface = true;
        public bool useSurfaceNormals;
        [MinMaxSlider(0, 360)] public Vector2Int xRotation;
        [MinMaxSlider(0, 360)] public Vector2Int yRotation;
        [MinMaxSlider(0, 360)] public Vector2Int zRotation;
    }
}