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
        [SerializeField] public List<SpawnData> spawnDataList = new List<SpawnData>();
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