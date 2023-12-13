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

        [SerializeField] private Material RenderingMaterial { get { return greeneryProceduralItem.renderingMaterial; } }

        [SerializeField] public Bounds currentBounds;

        private int kernelID;
        private int threadGroupSize;

        private const int SPAWNDATA_STRIDE = sizeof(float) * (3 + 3 + 4 + 1 + 4);
        private const int DRAW_STRIDE = sizeof(float) * ((3 + 3 + 3 + 2 + 4 + 4) * 3);
        private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;

        public int[] indirectArgs = new int[] {0, 1, 0, 0};

        private bool initialized = false;
        private Camera currentCamera;

        private MaterialPropertyBlock propertyBlock;

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
            if (initialized)
            {
                Release();
            }

            if (greeneryProceduralItem.renderingCS == null || greeneryProceduralItem.shader == null)
            {
                return;
            }

            if (spawnDataList.Count == 0)
            {
                return;
            }

            currentCamera = GetCamera();
            propertyBlock = new MaterialPropertyBlock();

            initialized = true;

            renderingCS = UnityEngine.Object.Instantiate(greeneryProceduralItem.renderingCS);

            kernelID = renderingCS.FindKernel(greeneryProceduralItem.KernelName);

            renderingCS.GetKernelThreadGroupSizes(kernelID, out uint threadGroupSizeX, out _, out _);
            threadGroupSize = (int) math.ceil((float) spawnDataList.Count / threadGroupSizeX);


            SetupBuffers();
            greeneryProceduralItem.Setup();

            renderingCS.SetFloat("_MaxDistance", greeneryProceduralItem.maxDistance);
            renderingCS.SetFloat("_MinFadeDistance", greeneryProceduralItem.minFadeDistance);

            renderingCS.SetBuffer(kernelID, "_SpawnDataBuffer", spawnDataBuffer);
            renderingCS.SetBuffer(kernelID, "_DrawTrianglesBuffer", drawTrianglesBuffer);
            renderingCS.SetBuffer(kernelID, "_IndirectArgsBuffer", indirectArgsBuffer);
            renderingCS.SetMatrix("_LocalToWorld", greeneryManager.transform.localToWorldMatrix);
            renderingCS.SetFloat("_ApplyCulling", greeneryProceduralItem.applyCulling ? 1 : 0);
            greeneryProceduralItem.SetData(renderingCS);

            propertyBlock.SetBuffer("_DrawTrianglesBuffer", drawTrianglesBuffer);

            UpdateMinMax();
            currentBounds.SetMinMax(minPos, maxPos + greeneryProceduralItem.MaxHeight);

            currentBounds = TransformBounds(currentBounds);

            spawnDataBuffer.SetData(spawnDataList);
            renderingCS.SetInt("_NumSpawnData", spawnDataList.Count);

            if (!greeneryProceduralItem.applyCulling)
            {
                GeometryGeneration();
            }
        }

        public override void Release()
        {
            if (initialized)
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

            initialized = false;

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

            if (!initialized)
            {
                return;
            }

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
                propertyBlock,
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

                Matrix4x4 v = currentCamera.worldToCameraMatrix;
                Matrix4x4 p = currentCamera.projectionMatrix;
                Matrix4x4 vp = p * v;
                renderingCS.SetMatrix("_VPMatrix", vp);
                renderingCS.SetVector("_Time", Shader.GetGlobalVector("_Time"));

                renderingCS.Dispatch(kernelID, threadGroupSize, 1, 1);
            }
        }

        private void OnAssemblyReload() { Release(); }
    }
}