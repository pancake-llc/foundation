namespace Pancake.Greenery
{
    using Unity.Mathematics;
    using UnityEngine;

    [CreateAssetMenu(fileName = "GreeneryGrassQuad", menuName = "Pancake/Greenery/Greenery Grass Quad", order = 0)]
    public class GreeneryGrassQuad : GreeneryProceduralItem
    {
        [Header("Geometry settings")] public float2 heightRange = new(0.5f, 1);
        public float2 widthRange = new(1, 2);
        public float spacing;
        private static readonly int Spacing = Shader.PropertyToID("_Spacing");

        public override float MaxHeight => heightRange.y;
        public override string KernelName => "GrassQuadsMain";
        public override int MaxTrianglesPerInstance => 6;

        public override void SetData(ComputeShader renderingCs)
        {
            renderingCs.SetVector(HeightWidthRanges, new float4(heightRange, widthRange));
            renderingCs.SetFloat(Spacing, spacing);
        }
    }
}