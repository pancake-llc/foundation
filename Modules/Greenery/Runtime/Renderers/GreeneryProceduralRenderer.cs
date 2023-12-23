namespace Pancake.Greenery
{
    using Random = UnityEngine.Random;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;
    using System;
    using UnityEngine.Rendering;

    [Serializable, ExecuteAlways]
    public class GreeneryProceduralRenderer : GreeneryRenderer
    {
        public GreeneryProceduralItem greeneryProceduralItem;

        [SerializeField] private ComputeShader renderingCS;
        [SerializeField] private ComputeBuffer spawnDataBuffer;
        [SerializeField] private ComputeBuffer drawTrianglesBuffer;
        [SerializeField] private ComputeBuffer indirectArgsBuffer;

        [SerializeField] private Material RenderingMaterial => greeneryProceduralItem.renderingMaterial;

        [SerializeField] public Bounds currentBounds;

        private int _kernelID;
        private int _threadGroupSize;

        private const int SPAWNDATA_STRIDE = sizeof(float) * (3 + 3 + 4 + 1 + 4);
        private const int DRAW_STRIDE = sizeof(float) * ((3 + 3 + 3 + 2 + 4 + 4) * 3);
        private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;

        public int[] indirectArgs = {0, 1, 0, 0};

        private bool _initialized;
        private Camera _currentCamera;

        private MaterialPropertyBlock _propertyBlock;

        [SerializeField] private float3 minPos;
        [SerializeField] private float3 maxPos;

        public GreeneryProceduralRenderer(GreeneryProceduralItem greeneryProceduralItem, GreeneryManager greeneryManager)
        {
            this.greeneryProceduralItem = greeneryProceduralItem;
            this.greeneryManager = greeneryManager;
            currentBounds = new Bounds();

            minPos = new float3(float.MaxValue);
            maxPos = new float3(float.MinValue);
        }

        public override void Initialize()
        {
            if (_initialized) Release();

            if (greeneryProceduralItem.renderingCS == null || greeneryProceduralItem.shader == null) return;

            if (spawnDataList.Count == 0) return;

            _currentCamera = GetCamera();
            _propertyBlock = new MaterialPropertyBlock();

            _initialized = true;

            renderingCS = UnityEngine.Object.Instantiate(greeneryProceduralItem.renderingCS);

            _kernelID = renderingCS.FindKernel(greeneryProceduralItem.KernelName);

            renderingCS.GetKernelThreadGroupSizes(_kernelID, out uint threadGroupSizeX, out _, out _);
            _threadGroupSize = (int) math.ceil((float) spawnDataList.Count / threadGroupSizeX);


            SetupBuffers();
            greeneryProceduralItem.Setup();

            renderingCS.SetFloat(MaxDistance, greeneryProceduralItem.maxDistance);
            renderingCS.SetFloat(MinFadeDistance, greeneryProceduralItem.minFadeDistance);

            renderingCS.SetBuffer(_kernelID, SpawnDataBuffer, spawnDataBuffer);
            renderingCS.SetBuffer(_kernelID, DrawTrianglesBuffer, drawTrianglesBuffer);
            renderingCS.SetBuffer(_kernelID, IndirectArgsBuffer, indirectArgsBuffer);
            renderingCS.SetMatrix(LocalToWorld, greeneryManager.transform.localToWorldMatrix);
            renderingCS.SetFloat(ApplyCulling, greeneryProceduralItem.applyCulling ? 1 : 0);
            greeneryProceduralItem.SetData(renderingCS);

            _propertyBlock.SetBuffer(DrawTrianglesBuffer, drawTrianglesBuffer);

            UpdateMinMax();
            currentBounds.SetMinMax(minPos, maxPos + greeneryProceduralItem.MaxHeight);

            currentBounds = TransformBounds(currentBounds);

            spawnDataBuffer.SetData(spawnDataList);
            renderingCS.SetInt(NumSpawnData, spawnDataList.Count);

            if (!greeneryProceduralItem.applyCulling)
            {
                GeometryGeneration();
            }
        }

        public override void Release()
        {
            if (_initialized)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(renderingCS);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(renderingCS);
                }

                DiscardBuffers();
                greeneryProceduralItem.Cleanup();
            }

            _initialized = false;

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= OnAssemblyReload;
#endif
        }

        public void SetupBuffers()
        {
            spawnDataBuffer = new ComputeBuffer(spawnDataList.Count, SPAWNDATA_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            drawTrianglesBuffer = new ComputeBuffer(spawnDataList.Count * greeneryProceduralItem.MaxTrianglesPerInstance, DRAW_STRIDE, ComputeBufferType.Append);
            indirectArgsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
#endif
        }


        public void DiscardBuffers()
        {
            DiscardBuffer(spawnDataBuffer);
            DiscardBuffer(drawTrianglesBuffer);
            DiscardBuffer(indirectArgsBuffer);
        }


        public void DiscardBuffer(ComputeBuffer computeBuffer)
        {
            if (computeBuffer != null)
            {
                computeBuffer.Dispose();
                computeBuffer.Release();
                computeBuffer = null;
            }
        }

        public override int AddSpawnPoint(SpawnData spawnData)
        {
            spawnDataList.Add(spawnData);
            currentBounds.Encapsulate(spawnData.position);
            minPos = math.min(minPos, spawnData.position);
            maxPos = math.max(maxPos, spawnData.position);
            return spawnDataList.Count - 1;
        }

        public override void RemoveSpawnPoint(int index) { spawnDataList.RemoveAt(index); }

        public override void ClearSpawnPoints()
        {
            spawnDataList.Clear();
            Release();
        }

        public override void ReprojectSpawnData(int index, float3 position, float3 normal, Color surfaceColor)
        {
            SpawnData data = spawnDataList[index];
            data.position = position;
            data.normal = normal;
            data.surfaceColor = surfaceColor;
            spawnDataList[index] = data;
        }

        public override void UpdateSpawnData(int index, float sizeFactor, Color color)
        {
            SpawnData data = spawnDataList[index];
            data.sizeFactor = sizeFactor;
            data.color = color;
            spawnDataList[index] = data;
            Render();
        }

        public override void UpdateAllSpawnData(bool sizeFactorLock, bool colorLock, float sizeFactor, Gradient colorGradient)
        {
            for (int i = 0; i < spawnDataList.Count; i++)
            {
                SpawnData data = spawnDataList[i];
                if (!sizeFactorLock) data.sizeFactor = sizeFactor;
                if (!colorLock) data.color = colorGradient.Evaluate(Random.value);
                spawnDataList[i] = data;
            }
        }

        public override void MassUpdateSpawnData(List<int> indices, bool sizeFactorLock, bool colorLock, float sizeFactor, Gradient colorGradient)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                SpawnData data = spawnDataList[indices[i]];
                if (!sizeFactorLock) data.sizeFactor = sizeFactor;
                if (!colorLock) data.color = colorGradient.Evaluate(Random.value);
                spawnDataList[indices[i]] = data;
            }
        }

        public override void Render()
        {
            if (!active) return;

            if (!Application.isPlaying)
            {
                Release();
                Initialize();
            }

            if (!_initialized) return;

            if (greeneryProceduralItem.applyCulling)
            {
                GeometryGeneration();
            }

            Graphics.DrawProceduralIndirect(RenderingMaterial,
                currentBounds,
                MeshTopology.Triangles,
                indirectArgsBuffer,
                0,
                null,
                _propertyBlock,
                greeneryProceduralItem.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
                true,
                greeneryManager.gameObject.layer);
        }

        public override void Update() { GeometryGeneration(); }

        public void UpdateMinMax()
        {
            for (int i = 0; i < spawnDataList.Count; i++)
            {
                float3 position = spawnDataList[i].position;
                minPos = math.min(position, minPos);
                maxPos = math.max(position, maxPos);
            }
        }

        public void GeometryGeneration()
        {
            if (renderingCS != null && drawTrianglesBuffer != null)
            {
                drawTrianglesBuffer.SetCounterValue(0);
                indirectArgsBuffer.SetData(indirectArgs);

                Matrix4x4 v = _currentCamera.worldToCameraMatrix;
                Matrix4x4 p = _currentCamera.projectionMatrix;
                Matrix4x4 vp = p * v;
                renderingCS.SetMatrix(VpMatrix, vp);
                renderingCS.SetVector(Time1, Shader.GetGlobalVector(Time1));

                renderingCS.Dispatch(_kernelID, _threadGroupSize, 1, 1);
            }
        }

        private void OnAssemblyReload() { Release(); }
    }
}