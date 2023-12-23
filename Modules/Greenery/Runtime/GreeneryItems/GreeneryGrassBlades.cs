namespace Pancake.Greenery
{
    using Unity.Mathematics;
    using UnityEngine;

    [CreateAssetMenu(fileName = "GreeneryGrassBlades", menuName = "Pancake/Greenery/Greenery Grass Blades", order = 0)]
    public class GreeneryGrassBlades : GreeneryProceduralItem
    {
        [Header("Geometry settings")] public float2 widthRange = new(0.2f, 0.3f);
        public float2 heightRange = new(0.5f, 1f);
        public float positioningRadius = 0.5f;
        public float forwardRotation = 0.3f;
        public float bladeCurvature = 2;
        [Range(0, 1)] public float normalCurving = 0.5f;
        [Range(1, 8)] public int bladesPerPoint = 2;
        [Range(1, 5)] public int segmentsPerBlade = 3;
        [Range(0.0f, 1.0f)] public float rotationVariation;
        [Range(0.0f, 1.0f)] public float curvatureVariation;
        
        private static readonly int PositioningRadius = Shader.PropertyToID("_PositioningRadius");
        private static readonly int ForwardRotation = Shader.PropertyToID("_ForwardRotation");
        private static readonly int BladeCurvature = Shader.PropertyToID("_BladeCurvature");
        private static readonly int BladesPerPoint = Shader.PropertyToID("_BladesPerPoint");
        private static readonly int SegmentsPerBlade = Shader.PropertyToID("_SegmentsPerBlade");
        private static readonly int RotationVariation = Shader.PropertyToID("_RotationVariation");
        private static readonly int CurvatureVariation = Shader.PropertyToID("_CurvatureVariation");
        private static readonly int NormalCurving = Shader.PropertyToID("_NormalCurving");

        public override string KernelName => "GreeneryGrassBladesMain";
        public override float MaxHeight => heightRange.y;
        public override int MaxTrianglesPerInstance => bladesPerPoint * ((segmentsPerBlade - 1) * 2 + 1);

        public override void SetData(ComputeShader renderingCs)
        {
            renderingCs.SetVector(HeightWidthRanges, new float4(heightRange, widthRange));
            renderingCs.SetFloat(PositioningRadius, positioningRadius);
            renderingCs.SetFloat(ForwardRotation, forwardRotation);
            renderingCs.SetFloat(BladeCurvature, bladeCurvature);
            renderingCs.SetInt(BladesPerPoint, bladesPerPoint);
            renderingCs.SetInt(SegmentsPerBlade, segmentsPerBlade);
            renderingCs.SetFloat(RotationVariation, rotationVariation);
            renderingCs.SetFloat(CurvatureVariation, curvatureVariation);
            renderingCs.SetFloat(NormalCurving, normalCurving);
        }
    }
}