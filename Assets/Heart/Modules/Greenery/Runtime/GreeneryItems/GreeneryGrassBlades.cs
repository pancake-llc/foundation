namespace Pancake.Greenery
{
    using Unity.Mathematics;
    using UnityEngine;

    [CreateAssetMenu(fileName = "GreeneryGrassBlades", menuName = "Greenery/Greenery Grass Blades", order = 0)]
    public class GreeneryGrassBlades : GreeneryProceduralItem
    {
        [Header("Geometry settings")] public float2 widthRange = new float2(0.2f, 0.3f);
        public float2 heightRange = new float2(0.5f, 1f);
        public float positioningRadius = 0.5f;
        public float forwardRotation = 0.3f;
        public float bladeCurvature = 2;
        [Range(0, 1)] public float normalCurving = 0.5f;
        [Range(1, 8)] public int bladesPerPoint = 2;
        [Range(1, 5)] public int segmentsPerBlade = 3;
        [Range(0.0f, 1.0f)] public float rotationVariation = 0;
        [Range(0.0f, 1.0f)] public float curvatureVariation = 0;

        public override string KernelName => "GreeneryGrassBladesMain";
        public override float MaxHeight => heightRange.y;
        public override int MaxTrianglesPerInstance => bladesPerPoint * ((segmentsPerBlade - 1) * 2 + 1);

        public override void SetData(ComputeShader renderingCS)
        {
            renderingCS.SetVector("_HeightWidthRanges", new float4(heightRange, widthRange));
            renderingCS.SetFloat("_PositioningRadius", positioningRadius);
            renderingCS.SetFloat("_ForwardRotation", forwardRotation);
            renderingCS.SetFloat("_BladeCurvature", bladeCurvature);
            renderingCS.SetInt("_BladesPerPoint", bladesPerPoint);
            renderingCS.SetInt("_SegmentsPerBlade", segmentsPerBlade);
            renderingCS.SetFloat("_RotationVariation", rotationVariation);
            renderingCS.SetFloat("_CurvatureVariation", curvatureVariation);
            renderingCS.SetFloat("_NormalCurving", normalCurving);
        }
    }
}