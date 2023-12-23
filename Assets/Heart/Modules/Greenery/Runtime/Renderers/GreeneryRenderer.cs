using System;
using UnityEditor;

namespace Pancake.Greenery
{
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;

    [Serializable]
    public abstract class GreeneryRenderer
    {
        protected static readonly int TransformBuffer = Shader.PropertyToID("_TransformBuffer");
        protected static readonly int NormalBuffer = Shader.PropertyToID("_NormalBuffer");
        protected static readonly int SurfaceColorBuffer = Shader.PropertyToID("_SurfaceColorBuffer");
        protected static readonly int ColorBuffer = Shader.PropertyToID("_ColorBuffer");
        protected static readonly int LocalToWorld = Shader.PropertyToID("_LocalToWorld");
        protected static readonly int UseSurfaceNormals = Shader.PropertyToID("_UseSurfaceNormals");
        protected static readonly int VisibleInstanceIDBuffer = Shader.PropertyToID("_VisibleInstanceIDBuffer");
        protected static readonly int MaxDistance = Shader.PropertyToID("_MaxDistance");
        protected static readonly int VpMatrix = Shader.PropertyToID("_VPMatrix");
        protected static readonly int MaxDrawDistance = Shader.PropertyToID("_MaxDrawDistance");
        protected static readonly int InstanceCount = Shader.PropertyToID("_InstanceCount");
        protected static readonly int MaxWidthHeight = Shader.PropertyToID("_MaxWidthHeight");
        protected static readonly int RenderingRange = Shader.PropertyToID("_RenderingRange");
        protected static readonly int EditorPreview = Shader.PropertyToID("_EditorPreview");
        protected static readonly int DrawTrianglesBuffer = Shader.PropertyToID("_DrawTrianglesBuffer");
        protected static readonly int MinFadeDistance = Shader.PropertyToID("_MinFadeDistance");
        protected static readonly int SpawnDataBuffer = Shader.PropertyToID("_SpawnDataBuffer");
        protected static readonly int IndirectArgsBuffer = Shader.PropertyToID("_IndirectArgsBuffer");
        protected static readonly int ApplyCulling = Shader.PropertyToID("_ApplyCulling");
        protected static readonly int NumSpawnData = Shader.PropertyToID("_NumSpawnData");
        protected static readonly int Time1 = Shader.PropertyToID("_Time");
        
        [SerializeField] public List<SpawnData> spawnDataList = new();
        public bool active = true;
        public GreeneryManager greeneryManager;

        public abstract void Render();
        public abstract void Initialize();
        public abstract void Release();
        public abstract int AddSpawnPoint(SpawnData spawnData);
        public abstract void RemoveSpawnPoint(int index);
        public abstract void ClearSpawnPoints();
        public abstract void UpdateSpawnData(int index, float sizeFactor, Color color);
        public abstract void UpdateAllSpawnData(bool sizeFactorLock, bool colorLock, float sizeFactor, Gradient colorGradient);
        public abstract void MassUpdateSpawnData(List<int> indices, bool sizeFactorLock, bool colorLock, float sizeFactor, Gradient colorGradient);
        public abstract void ReprojectSpawnData(int index, float3 position, float3 normal, Color surfaceColor);
        public abstract void Update();

        public List<SpawnData> GetSpawnDataList() { return spawnDataList; }

        public void CopyFrom(GreeneryRenderer renderer)
        {
            spawnDataList = renderer.GetSpawnDataList();
            active = renderer.active;
            greeneryManager = renderer.greeneryManager;
        }

        protected Bounds TransformBounds(Bounds boundsOS)
        {
            var center = greeneryManager.transform.TransformPoint(boundsOS.center);

            // transform the local extents' axes
            var extents = boundsOS.extents;
            var axisX = greeneryManager.transform.TransformVector(extents.x, 0, 0);
            var axisY = greeneryManager.transform.TransformVector(0, extents.y, 0);
            var axisZ = greeneryManager.transform.TransformVector(0, 0, extents.z);

            // sum their absolute value to get the world extents
            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds {center = center, extents = extents};
        }

        protected Camera GetCamera()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (SceneView.GetAllSceneCameras().Length > 0)
                {
                    return SceneView.GetAllSceneCameras()[0];
                }
                else
                {
                    return Camera.main;
                }
            }
            else
            {
                return Camera.main;
            }
#else
        return Camera.main;
#endif
        }
    }
}