namespace Pancake.Greenery
{
    using System;
    using UnityEngine;

    [Serializable]
    public abstract class GreeneryProceduralItem : GreeneryItem
    {
        protected static readonly int HeightWidthRanges = Shader.PropertyToID("_HeightWidthRanges");

        public bool castShadows;

        [HideInInspector] public Shader shader;

        // ReSharper disable once InconsistentNaming
        [Header("Item settings")] public ComputeShader renderingCS;
        public bool applyCulling = true;
        public float maxDistance = 100;
        public float minFadeDistance = 20;

        public virtual float MaxHeight => 0;
        public virtual string KernelName => "CSMain";
        public virtual int MaxTrianglesPerInstance => 1;

        public virtual void Setup() { }
        public virtual void Cleanup() { }
        public abstract void SetData(ComputeShader renderingCs);

        [HideInInspector] public Material renderingMaterial;

        private void OnEnable()
        {
            if (renderingMaterial != null) return;

            renderingMaterial = new Material(shader);
        }
    }
}