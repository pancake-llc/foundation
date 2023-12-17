namespace Pancake.Greenery
{
    using System;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.Rendering;


    [Serializable, ExecuteAlways]
    public class GreeneryLODInstanceRenderer : GreeneryRenderer
    {
        [Serializable]
        public class RendererData
        {
            public ComputeBuffer argsBuffer;
            public MaterialPropertyBlock materialRuntimeProperties;
            public ComputeBuffer visibleInstanceIDBuffer;
        }

        public GreeneryLODInstance greeneryLODInstance;

        [SerializeField] private ComputeBuffer transformBuffer;
        [SerializeField] private ComputeBuffer normalBuffer;
        [SerializeField] private ComputeBuffer surfaceColorBuffer;
        [SerializeField] private ComputeBuffer colorBuffer;
        [SerializeField] private ComputeShader lodCullingCS;

        [SerializeReference] private RendererData[] rendererDataArray;

        private uint[] _args = {0, 0, 0, 0, 0};

        private Camera _currentCamera;

        [SerializeField] public Bounds currentBounds;

        public GreeneryLODInstanceRenderer(GreeneryLODInstance greeneryLODInstance, GreeneryManager greeneryManager)
        {
            this.greeneryLODInstance = greeneryLODInstance;
            this.greeneryManager = greeneryManager;
            InstantiateCullingCs();
        }

        public override void Initialize()
        {
            InstantiateCullingCs();
            UpdateBuffersAndData();
        }

        public void InstantiateCullingCs()
        {
            if (greeneryLODInstance.LODCullingCS != null)
            {
                lodCullingCS = UnityEngine.Object.Instantiate(greeneryLODInstance.LODCullingCS);
            }
        }

        private void SetupBuffers()
        {
            if (spawnDataList.Count == 0)
            {
                return;
            }

            _args = new uint[5] {0, 0, 0, 0, 0};

            transformBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(float) * 4 * 4, ComputeBufferType.Default, ComputeBufferMode.Immutable);
            normalBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(float) * 3, ComputeBufferType.Default, ComputeBufferMode.Immutable);
            surfaceColorBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(float) * 4, ComputeBufferType.Default, ComputeBufferMode.Immutable);
            colorBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(float) * 3, ComputeBufferType.Default, ComputeBufferMode.Immutable);

            rendererDataArray = new RendererData[greeneryLODInstance.instancesLOD.Count];
            for (int i = 0; i < rendererDataArray.Length; i++)
            {
                rendererDataArray[i] = new RendererData();
            }

            foreach (var rendererData in rendererDataArray)
            {
                if (rendererData.materialRuntimeProperties == null)
                {
                    rendererData.materialRuntimeProperties = new MaterialPropertyBlock();
                }


                rendererData.argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
                rendererData.visibleInstanceIDBuffer = new ComputeBuffer(spawnDataList.Count, sizeof(uint), ComputeBufferType.Append);
            }


#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
#endif
        }

        public override void Release()
        {
            DiscardBuffer(transformBuffer);
            DiscardBuffer(normalBuffer);
            DiscardBuffer(surfaceColorBuffer);
            DiscardBuffer(colorBuffer);
            if (rendererDataArray != null)
            {
                foreach (var rendererData in rendererDataArray)
                {
                    DiscardBuffer(rendererData.visibleInstanceIDBuffer);
                    DiscardBuffer(rendererData.argsBuffer);
                    rendererData.materialRuntimeProperties = null;
                }

                rendererDataArray = null;
            }

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= OnAssemblyReload;
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

            if (!CheckBufferData() || !CheckMeshData())
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
                float random = Mathf.PerlinNoise(data.position.x, data.position.y);
                float sizeFactor = data.sizeFactor * Mathf.Lerp(greeneryLODInstance.sizeRange.x, greeneryLODInstance.sizeRange.y, random);
                float3 position = data.position + data.normal * greeneryLODInstance.pivotOffset * sizeFactor;
                Quaternion randomRot = Quaternion.Euler(Mathf.Lerp(greeneryLODInstance.xRotation.x, greeneryLODInstance.xRotation.y, random),
                    Mathf.Lerp(greeneryLODInstance.yRotation.x, greeneryLODInstance.yRotation.y, random),
                    Mathf.Lerp(greeneryLODInstance.zRotation.x, greeneryLODInstance.zRotation.y, random));
                Quaternion rot = greeneryLODInstance.alignToSurface ? Quaternion.FromToRotation(Vector3.up, data.normal).normalized : Quaternion.identity;
                Matrix4x4 transform = Matrix4x4.TRS(position, rot * randomRot, Vector3.one * sizeFactor);
                instanceBounds.center = position;
                instanceBounds.size = greeneryLODInstance.instancesLOD[0].instancedMesh.bounds.size * sizeFactor;

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

            _currentCamera = GetCamera();


            for (int i = 0; i < rendererDataArray.Length; i++)
            {
                RendererData rendererData = rendererDataArray[i];
                _args[0] = greeneryLODInstance.instancesLOD[i].instancedMesh.GetIndexCount(0);
                _args[1] = (uint) spawnDataList.Count;
                _args[2] = greeneryLODInstance.instancesLOD[i].instancedMesh.GetIndexStart(0);
                _args[3] = greeneryLODInstance.instancesLOD[i].instancedMesh.GetBaseVertex(0);

                rendererData.argsBuffer.SetData(_args);

                rendererData.materialRuntimeProperties.SetBuffer(TransformBuffer, transformBuffer);
                rendererData.materialRuntimeProperties.SetBuffer(NormalBuffer, normalBuffer);
                rendererData.materialRuntimeProperties.SetBuffer(SurfaceColorBuffer, surfaceColorBuffer);
                rendererData.materialRuntimeProperties.SetBuffer(ColorBuffer, colorBuffer);
                rendererData.materialRuntimeProperties.SetMatrix(LocalToWorld, greeneryManager.transform.localToWorldMatrix);
                rendererData.materialRuntimeProperties.SetFloat(UseSurfaceNormals, greeneryLODInstance.useSurfaceNormals ? 1.0f : 0.0f);

                rendererData.materialRuntimeProperties.SetBuffer(VisibleInstanceIDBuffer, rendererData.visibleInstanceIDBuffer);
            }

            if (lodCullingCS != null)
            {
                lodCullingCS.SetMatrix(LocalToWorld, greeneryManager.transform.localToWorldMatrix);
                lodCullingCS.SetFloat(MaxDistance, greeneryLODInstance.maxDrawDistance);
            }

            currentBounds = TransformBounds(currentBounds);
        }

        public override void Render()
        {
            if (!CheckBufferData() || !active || !CheckMeshData())
            {
                return;
            }

            if (Application.isPlaying)
            {
                for (int i = 0; i < rendererDataArray.Length; i++)
                {
                    RendererData rendererData = rendererDataArray[i];
                    LODCulling(i);
                    GreeneryLODInstance.InstanceLOD instanceLOD = greeneryLODInstance.instancesLOD[i];
                    if (instanceLOD.instancedMesh != null && instanceLOD.instanceMaterial != null)
                    {
                        Graphics.DrawMeshInstancedIndirect(instanceLOD.instancedMesh,
                            0,
                            instanceLOD.instanceMaterial,
                            currentBounds,
                            rendererData.argsBuffer,
                            0,
                            rendererData.materialRuntimeProperties,
                            instanceLOD.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
                    }
                }
            }
            else
            {
                int index = greeneryLODInstance.previewIndex;
                LODCulling(index, true);
                RendererData rendererData = rendererDataArray[index];
                GreeneryLODInstance.InstanceLOD instanceLOD = greeneryLODInstance.instancesLOD[index];
                if (instanceLOD.instancedMesh != null && instanceLOD.instanceMaterial != null)
                {
                    Graphics.DrawMeshInstancedIndirect(instanceLOD.instancedMesh,
                        0,
                        instanceLOD.instanceMaterial,
                        currentBounds,
                        rendererData.argsBuffer,
                        0,
                        rendererData.materialRuntimeProperties,
                        instanceLOD.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
                }
            }
        }

        public override void Update() { UpdateBuffersAndData(); }

        private void LODCulling(int index, bool editorPreview = false)
        {
            RendererData rendererData = rendererDataArray[index];
            if (lodCullingCS != null)
            {
                int kernelID = lodCullingCS.FindKernel("InstanceLODCulling");

                if (lodCullingCS != null)
                {
                    if (index == 0)
                    {
                        lodCullingCS.SetVector(RenderingRange, new Vector2(0, greeneryLODInstance.instancesLOD[index].LODFactor));
                    }
                    else
                    {
                        lodCullingCS.SetVector(RenderingRange,
                            new Vector2(greeneryLODInstance.instancesLOD[index - 1].LODFactor, greeneryLODInstance.instancesLOD[index].LODFactor));
                    }
                }

                lodCullingCS.SetBuffer(kernelID, TransformBuffer, transformBuffer);
                lodCullingCS.SetInt(InstanceCount, spawnDataList.Count);
                lodCullingCS.SetVector(MaxWidthHeight,
                    new Vector2(greeneryLODInstance.instancesLOD[index].instancedMesh.bounds.size.x,
                        greeneryLODInstance.instancesLOD[index].instancedMesh.bounds.size.y) * greeneryLODInstance.sizeRange.y);

                if (editorPreview)
                {
                    lodCullingCS.SetInt(EditorPreview, 1);
                }
                else
                {
                    lodCullingCS.SetInt(EditorPreview, 0);
                }

                Matrix4x4 v = _currentCamera.worldToCameraMatrix;
                Matrix4x4 p = _currentCamera.projectionMatrix;
                Matrix4x4 vp = p * v;
                lodCullingCS.SetMatrix(VpMatrix, vp);

                rendererData.visibleInstanceIDBuffer.SetCounterValue(0);

                lodCullingCS.SetBuffer(kernelID, VisibleInstanceIDBuffer, rendererData.visibleInstanceIDBuffer);

                lodCullingCS.GetKernelThreadGroupSizes(kernelID, out uint threadGroupSizeX, out _, out _);

                lodCullingCS.Dispatch(kernelID, Mathf.CeilToInt((float) spawnDataList.Count / threadGroupSizeX), 1, 1);

                ComputeBuffer.CopyCount(rendererData.visibleInstanceIDBuffer, rendererData.argsBuffer, 4);
            }
        }

        private bool CheckBufferData()
        {
            bool rendererDataCheck = true;
            if (rendererDataArray != null)
            {
                foreach (var rendererData in rendererDataArray)
                {
                    rendererDataCheck = rendererData.argsBuffer != null && rendererData.materialRuntimeProperties != null && rendererData.visibleInstanceIDBuffer != null;
                }
            }
            else return false;

            return rendererDataCheck && rendererDataArray != null && rendererDataArray.Length == greeneryLODInstance.instancesLOD.Count && transformBuffer != null &&
                   normalBuffer != null && surfaceColorBuffer != null && colorBuffer != null && transformBuffer.count == spawnDataList.Count &&
                   normalBuffer.count == spawnDataList.Count && surfaceColorBuffer.count == spawnDataList.Count && colorBuffer.count == spawnDataList.Count;
        }

        private bool CheckMeshData()
        {
            bool instanceCheck = true;
            foreach (var instanceLOD in greeneryLODInstance.instancesLOD)
            {
                instanceCheck = instanceLOD.instancedMesh != null && instanceLOD.instanceMaterial != null;
            }

            return instanceCheck;
        }


        private void OnAssemblyReload() { Release(); }
    }
}