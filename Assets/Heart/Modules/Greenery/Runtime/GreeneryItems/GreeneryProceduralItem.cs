namespace Pancake.Greenery
{
    using System;
    using System.IO;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Presets;
#endif

    [Serializable]
    public abstract class GreeneryProceduralItem : GreeneryItem
    {
        [Header("Shadow casting")] public bool castShadows;

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
#if UNITY_EDITOR
            Preset materialPreset = null;
            const string umpPath = "Packages/com.pancake.heart/{0}";
            const string normalPath = "Assets/Heart/{0}";
            if (this is GreeneryGrassQuad)
            {
                string u = string.Format(umpPath, "Modules/Greenery/Presets/GrassQuadPreset.preset");
                string n = string.Format(normalPath, "Modules/Greenery/Presets/GrassQuadPreset.preset");
                materialPreset = AssetDatabase.LoadAssetAtPath<Preset>(!File.Exists(Path.GetFullPath(u)) ? n : u);
            }
            else if (this is GreeneryGrassBlades)
            {
                string u = string.Format(umpPath, "Modules/Greenery/Presets/GrassBladesPreset.preset");
                string n = string.Format(normalPath, "Modules/Greenery/Presets/GrassBladesPreset.preset");
                materialPreset = AssetDatabase.LoadAssetAtPath<Preset>(!File.Exists(Path.GetFullPath(u)) ? n : u);
            }

            if (materialPreset != null) materialPreset.ApplyTo(renderingMaterial);
            AssetDatabase.AddObjectToAsset(renderingMaterial, this);
            AssetDatabase.Refresh();
#endif
        }
    }
}