namespace Pancake.Greenery
{
    using Pancake.Apex;
    using UnityEngine;

    [CreateAssetMenu(fileName = "GreeneryInstance", menuName = "Pancake/Greenery/Greenery Instance", order = 0)]
    public class GreeneryInstance : GreeneryItem
    {
        [Header("Shadow casting")] public bool castShadows = false;

        public ComputeShader instanceCullingCS;
        public float maxDrawDistance = 100.0f;
        public Mesh instancedMesh;
        public Material instanceMaterial;
        public Vector2 sizeRange = new Vector2(1, 1);
        public float pivotOffset = 0;
        public bool alignToSurface = true;
        public bool useSurfaceNormals = false;
        [MinMaxSlider(0, 360)] public Vector2Int xRotation;
        [MinMaxSlider(0, 360)] public Vector2Int yRotation;
        [MinMaxSlider(0, 360)] public Vector2Int zRotation;
    }
}