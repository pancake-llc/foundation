using UnityEditor;

namespace Pancake.Greenery
{
    using System;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.Rendering;

    [Serializable, ExecuteAlways]
    public class GreeneryInstanceRenderer : GreeneryRenderer
    {
        public GreeneryInstance greeneryInstance;

        [SerializeField] private ComputeBuffer transformBuffer;
        [SerializeField] private ComputeBuffer normalBuffer;
        [SerializeField] private ComputeBuffer surfaceColorBuffer;
        [SerializeField] private ComputeBuffer colorBuffer;
        [SerializeField] private ComputeBuffer argsBuffer;

        [SerializeField] private MaterialPropertyBlock materialRuntimeProperties;

        private uint[] _args = {0, 0, 0, 0, 0};

        [SerializeField] private ComputeShader instanceCullingCS;
        [SerializeField] private ComputeBuffer visibleInstanceIDBuffer;

        private Camera _currentCamera;

        [SerializeField] public Bounds currentBounds;
        
        public GreeneryInstanceRenderer(GreeneryInstance greeneryInstance, GreeneryManager greeneryManager)
        {
            this.greeneryInstance = greeneryInstance;
            this.greeneryManager = greeneryManager;
            InstantiateCullingCS();
        }

        public override void Initialize()
        {
            InstantiateCullingCS();
            UpdateBuffersAndData();
        }

        public void InstantiateCullingCS()
        {
            if (greeneryInstance.instanceCullingCS != null)
            {
                instanceCullingCS = UnityEngine.Object.Instantiate(greeneryInstance.instanceCullingCS);
            }
        }

        private void SetupBuffers()
        {
            if (spawnDataList.Count == 0)
            {
                return;
            }

            _args = new uint[5] {0, 0, 0, 0, 0};
            if (materialRuntimeProperties == null)
            {
                materialRuntimeProperties = new MaterialPropertyBlock();
            }

            transformBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(float) * 4 * 4, ComputeBufferType.Default, ComputeBufferMode.Immutable);
            normalBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(float) * 3, ComputeBufferType.Default, ComputeBufferMode.Immutable);
            surfaceColorBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(float) * 4, ComputeBufferType.Default, ComputeBufferMode.Immutable);
            colorBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(float) * 3, ComputeBufferType.Default, ComputeBufferMode.Immutable);

            visibleInstanceIDBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(uint), ComputeBufferType.Append);

            argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);


#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
#endif
        }

        public override void Release()
        {
            DiscardBuffer(transformBuffer);
            DiscardBuffer(normalBuffer);
            DiscardBuffer(argsBuffer);
            DiscardBuffer(surfaceColorBuffer);
            DiscardBuffer(colorBuffer);
            DiscardBuffer(visibleInstanceIDBuffer);
            materialRuntimeProperties = null;

#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload -= OnAssemblyReload;
#endif
        }

        private void DiscardBuffer(ComputeBuffer buffer)
        {
            if (buffer != null)
            {
                buffer.Dispose();
                buffer.Release();
            }

            buffer = null;
        }

        public void UpdateBuffersAndData()
        {
            UpdateBuffers();
            UpdateData();
        }

        public override int AddSpawnPoint(SpawnData spawnData)
        {
            spawnDataList.Add(spawnData);
            UpdateBuffersAndData();
            return spawnDataList.Count - 1;
        }

        public override void RemoveSpawnPoint(int index)
        {
            spawnDataList.RemoveAt(index);
            UpdateBuffersAndData();
        }

        public override void ReprojectSpawnData(int index, float3 position, float3 normal, Color surfaceColor)
        {
            SpawnData data = spawnDataList[index];
            data.position = position;
            data.normal = normal;
            data.surfaceColor = surfaceColor;
            spawnDataList[index] = data;
            UpdateBuffersAndData();
        }

        public override void UpdateSpawnData(int index, float sizeFactor, Color color)
        {
            SpawnData data = spawnDataList[index];
            data.sizeFactor = sizeFactor;
            data.color = color;
            spawnDataList[index] = data;
            UpdateBuffersAndData();
        }

        public override void UpdateAllSpawnData(bool sizeFactorLock, bool colorLock, float sizeFactor, Gradient colorGradient)
        {
            for (int i = 0; i < spawnDataList.Count; i++)
            {
                SpawnData data = spawnDataList[i];
                if (!sizeFactorLock) data.sizeFactor = sizeFactor;
                if (!colorLock) data.color = colorGradient.Evaluate(UnityEngine.Random.value);
                spawnDataList[i] = data;
            }

            UpdateBuffersAndData();
        }


        public override void MassUpdateSpawnData(List<int> indices, bool sizeFactorLock, bool colorLock, float sizeFactor, Gradient colorGradient)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                SpawnData data = spawnDataList[indices[i]];
                if (!sizeFactorLock) data.sizeFactor = sizeFactor;
                if (!colorLock) data.color = colorGradient.Evaluate(UnityEngine.Random.value);
                spawnDataList[indices[i]] = data;
            }

            UpdateBuffersAndData();
        }


        public override void ClearSpawnPoints()
        {
            spawnDataList.Clear();
            Release();
        }

        public void UpdateBuffers()
        {
            if (!CheckBufferData())
            {
                Release();
                SetupBuffers();
            }
        }

        public void UpdateData()
        {
            if (spawnDataList.Count == 0)
            {
                return;
            }

            if (!CheckBufferData())
            {
                return;
            }

            currentBounds = new Bounds();
            Bounds instanceBounds = new Bounds();

            Matrix4x4[] transforms = new Matrix4x4[spawnDataList.Count];
            Vector3[] normals = new Vector3[spawnDataList.Count];
            Vector4[] surfaceColors = new Vector4[spawnDataList.Count];
            Vector3[] colors = new Vector3[spawnDataList.Count];


            for (int i = 0; i < spawnDataList.Count; i++)
            {
                SpawnData data = spawnDataList[i];
                float random = Mathf.PerlinNoise(data.position.x * 20.0f, data.position.z * 20.0f);
                float sizeFactor = data.sizeFactor * Mathf.Lerp(greeneryInstance.sizeRange.x, greeneryInstance.sizeRange.y, random);
                float3 position = data.position + data.normal * greeneryInstance.pivotOffset * sizeFactor;
                Quaternion randomRot = Quaternion.Euler(Mathf.Lerp(greeneryInstance.xRotation.x, greeneryInstance.xRotation.y, random),
                    Mathf.Lerp(greeneryInstance.yRotation.x, greeneryInstance.yRotation.y, random),
                    Mathf.Lerp(greeneryInstance.zRotation.x, greeneryInstance.zRotation.y, random));
                Quaternion rot = greeneryInstance.alignToSurface ? Quaternion.FromToRotation(Vector3.up, data.normal).normalized : Quaternion.identity;
                Matrix4x4 transform = Matrix4x4.TRS(position, rot * randomRot, Vector3.one * sizeFactor);
                instanceBounds.center = position;
                instanceBounds.size = greeneryInstance.instancedMesh.bounds.size * sizeFactor;

                transforms[i] = transform;
                normals[i] = (transform.inverse * new Vector4(data.normal.x, data.normal.y, data.normal.z, 0.0f)).normalized;
                surfaceColors[i] = data.surfaceColor;
                colors[i] = new Vector3(data.color.r, data.color.g, data.color.b);

                currentBounds.Encapsulate(instanceBounds);
            }

            transformBuffer.SetData(transforms);
            normalBuffer.SetData(normals);
            surfaceColorBuffer.SetData(surfaceColors);
            colorBuffer.SetData(colors);

            _args[0] = greeneryInstance.instancedMesh.GetIndexCount(0);
            _args[1] = (uint) spawnDataList.Count;
            _args[2] = greeneryInstance.instancedMesh.GetIndexStart(0);
            _args[3] = greeneryInstance.instancedMesh.GetBaseVertex(0);

            argsBuffer.SetData(_args);

            currentBounds = TransformBounds(currentBounds);

            materialRuntimeProperties.SetBuffer(TransformBuffer, transformBuffer);
            materialRuntimeProperties.SetBuffer(NormalBuffer, normalBuffer);
            materialRuntimeProperties.SetBuffer(SurfaceColorBuffer, surfaceColorBuffer);
            materialRuntimeProperties.SetBuffer(ColorBuffer, colorBuffer);
            materialRuntimeProperties.SetMatrix(LocalToWorld, greeneryManager.transform.localToWorldMatrix);
            materialRuntimeProperties.SetFloat(UseSurfaceNormals, greeneryInstance.useSurfaceNormals ? 1.0f : 0.0f);

            if (instanceCullingCS != null)
            {
                _currentCamera = GetCamera();
                instanceCullingCS.SetFloat(MaxDrawDistance, greeneryInstance.maxDrawDistance);
                instanceCullingCS.SetInt(InstanceCount, spawnDataList.Count);
                instanceCullingCS.SetMatrix(LocalToWorld, greeneryManager.transform.localToWorldMatrix);
            }

            materialRuntimeProperties.SetBuffer(VisibleInstanceIDBuffer, visibleInstanceIDBuffer);
        }

        public override void Render()
        {
            if (!CheckBufferData() || !active)
            {
                return;
            }

            FrustumCulling();

            if (greeneryInstance.instancedMesh != null && greeneryInstance.instanceMaterial != null)
            {
                Graphics.DrawMeshInstancedIndirect(greeneryInstance.instancedMesh,
                    0,
                    greeneryInstance.instanceMaterial,
                    currentBounds,
                    argsBuffer,
                    0,
                    materialRuntimeProperties,
                    greeneryInstance.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
            }
        }

        public override void Update() { UpdateBuffersAndData(); }

        private void FrustumCulling()
        {
            if (instanceCullingCS != null)
            {
                int kernelID = instanceCullingCS.FindKernel("InstanceFrustumCulling");
                instanceCullingCS.SetBuffer(kernelID, TransformBuffer, transformBuffer);
                instanceCullingCS.SetVector(MaxWidthHeight,
                    new Vector2(greeneryInstance.instancedMesh.bounds.size.x, greeneryInstance.instancedMesh.bounds.size.y) * greeneryInstance.sizeRange.y);

                Matrix4x4 v = _currentCamera.worldToCameraMatrix;
                Matrix4x4 p = _currentCamera.projectionMatrix;
                Matrix4x4 vp = p * v;
                instanceCullingCS.SetMatrix(VpMatrix, vp);

                visibleInstanceIDBuffer.SetCounterValue(0);

                instanceCullingCS.SetBuffer(kernelID, VisibleInstanceIDBuffer, visibleInstanceIDBuffer);

                instanceCullingCS.GetKernelThreadGroupSizes(kernelID, out uint threadGroupSizeX, out _, out _);

                instanceCullingCS.Dispatch(kernelID, Mathf.CeilToInt((float) spawnDataList.Count / threadGroupSizeX), 1, 1);

                ComputeBuffer.CopyCount(visibleInstanceIDBuffer, argsBuffer, 4);
            }
        }

        private bool CheckBufferData()
        {
            return argsBuffer != null && materialRuntimeProperties != null && transformBuffer != null && normalBuffer != null && surfaceColorBuffer != null &&
                   colorBuffer != null && transformBuffer.count == spawnDataList.Count && normalBuffer.count == spawnDataList.Count &&
                   surfaceColorBuffer.count == spawnDataList.Count && colorBuffer.count == spawnDataList.Count;
        }


        private void OnAssemblyReload() { Release(); }
    }
}