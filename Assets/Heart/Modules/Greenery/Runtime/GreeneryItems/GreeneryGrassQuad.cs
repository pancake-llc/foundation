namespace Pancake.Greenery
{
    using Unity.Mathematics;
    using UnityEngine;

    [CreateAssetMenu(fileName = "GreeneryGrassQuad", menuName = "Pancake/Greenery/Greenery Grass Quad", order = 0)]
    public class GreeneryGrassQuad : GreeneryProceduralItem
    {
        [Header("Geometry settings")] public float2 heightRange = new float2(0.5f, 1);
        public float2 widthRange = new float2(1, 2);
        public float spacing = 0;

        public override float MaxHeight => heightRange.y;
        public override string KernelName => "GrassQuadsMain";
        public override int MaxTrianglesPerInstance => 6;

        public override void SetData(ComputeShader renderingCS)
        {
            renderingCS.SetVector("_HeightWidthRanges", new float4(heightRange, widthRange));
            renderingCS.SetFloat("_Spacing", spacing);
        }
    }
}