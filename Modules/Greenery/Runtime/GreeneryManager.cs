using UnityEditor;

namespace Pancake.Greenery
{
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Mathematics;

    [ExecuteAlways]
    public class GreeneryManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        [HideInInspector] public Dictionary<GreeneryItem, GreeneryRenderer> rendererDictionary = new();
        [HideInInspector, SerializeReference] public List<GreeneryItem> greeneryItems = new();
        [HideInInspector, SerializeReference] public List<GreeneryRenderer> greeneryRenderers = new();

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += UpdateRenderers;
#endif
            foreach (GreeneryRenderer r in greeneryRenderers)
            {
                r.Initialize();
            }

            foreach (var kvp in rendererDictionary)
            {
                if (kvp.Key == null)
                {
                    rendererDictionary[kvp.Key].Release();
                    rendererDictionary.Remove(kvp.Key);
                }
            }
        }

        private void OnDisable()
        {
            foreach (GreeneryRenderer renderer in greeneryRenderers)
            {
                renderer.Release();
            }

#if UNITY_EDITOR
            EditorApplication.update -= UpdateRenderers;
#endif
        }

        private void LateUpdate() { Render(); }

        private void Render()
        {
            for (int i = 0; i < greeneryRenderers.Count; i++)
            {
                GreeneryRenderer r = greeneryRenderers[i];
                r.Render();
            }
        }

        private void UpdateRenderers()
        {
            if (!Application.isPlaying)
            {
                foreach (GreeneryRenderer r in greeneryRenderers)
                {
                    r?.Update();
                }
            }
        }


        public int AddSpawnPoint(GreeneryItem item, SpawnData spawnData)
        {
            spawnData.position = transform.InverseTransformPoint(spawnData.position);
            spawnData.normal = transform.InverseTransformDirection(spawnData.normal);

            if (!rendererDictionary.ContainsKey(item))
            {
                if (item is GreeneryInstance instanceItem)
                {
                    if (instanceItem.instancedMesh != null && instanceItem.instanceMaterial != null)
                    {
                        rendererDictionary.Add(instanceItem, new GreeneryInstanceRenderer(instanceItem, this));
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (item is GreeneryProceduralItem greeneryProceduralItem)
                {
                    if (greeneryProceduralItem.renderingCS != null && greeneryProceduralItem.shader != null)
                    {
                        rendererDictionary.Add(greeneryProceduralItem, new GreeneryProceduralRenderer(greeneryProceduralItem, this));
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (item is GreeneryLODInstance lodInstanceItem)
                {
                    if (lodInstanceItem.instancesLOD.Count > 0)
                    {
                        rendererDictionary.Add(lodInstanceItem, new GreeneryLODInstanceRenderer(lodInstanceItem, this));
                    }
                    else
                    {
                        return -1;
                    }
                }

                Render();
            }

            return rendererDictionary[item].AddSpawnPoint(spawnData);
        }


        public void RemoveSpawnPoint(GreeneryItem item, int index)
        {
            if (rendererDictionary.ContainsKey(item))
            {
                rendererDictionary[item].RemoveSpawnPoint(index);
                if (rendererDictionary[item].spawnDataList.Count <= 0)
                {
                    rendererDictionary[item].Release();
                    rendererDictionary.Remove(item);
                }
            }
        }

        public void ClearSpawnPoints(GreeneryItem item)
        {
            if (rendererDictionary.ContainsKey(item))
            {
                rendererDictionary[item].ClearSpawnPoints();
                rendererDictionary.Remove(item);
            }
        }

        public void UpdateAllSpawnData(GreeneryItem item, bool sizeFactorLock, bool colorLock, float sizeFactor, Gradient colorGradient)
        {
            if (rendererDictionary.ContainsKey(item))
            {
                rendererDictionary[item].UpdateAllSpawnData(sizeFactorLock, colorLock, sizeFactor, colorGradient);
            }
        }

        public void MassUpdateSpawnData(GreeneryItem item, List<int> indices, bool sizeFactorLock, bool colorLock, float sizeFactor, Gradient colorGradient)
        {
            if (rendererDictionary.ContainsKey(item))
            {
                rendererDictionary[item]
                .MassUpdateSpawnData(indices,
                    sizeFactorLock,
                    colorLock,
                    sizeFactor,
                    colorGradient);
            }
        }

        //Not really used anymore
        public void UpdateSpawnData(GreeneryItem item, int index, float sizeFactor, Color color)
        {
            if (rendererDictionary.ContainsKey(item))
            {
                rendererDictionary[item].UpdateSpawnData(index, sizeFactor, color);
            }
        }

        public void ReprojectSpawnData(GreeneryItem item, int index, float3 position, float3 normal, Color surfaceColor)
        {
            if (rendererDictionary.ContainsKey(item))
            {
                rendererDictionary[item].ReprojectSpawnData(index, transform.InverseTransformPoint(position), transform.InverseTransformDirection(normal), surfaceColor);
            }
        }

        public void ReplaceSpawnData(GreeneryItem sourceItem, GreeneryItem newItem)
        {
            List<SpawnData> sourceSpawnDataList = GetSpawnDataList(sourceItem);
            foreach (var spawnData in sourceSpawnDataList)
            {
                AddSpawnPoint(newItem, spawnData);
            }
        }

        public List<SpawnData> GetSpawnDataList(GreeneryItem item)
        {
            if (rendererDictionary.ContainsKey(item))
            {
                return rendererDictionary[item].GetSpawnDataList();
            }
            else
            {
                return null;
            }
        }

        public void OnBeforeSerialize()
        {
            greeneryItems.Clear();
            greeneryRenderers.Clear();

            foreach (var kvp in rendererDictionary)
            {
                greeneryItems.Add(kvp.Key);
                greeneryRenderers.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            rendererDictionary = new Dictionary<GreeneryItem, GreeneryRenderer>();
            for (int i = 0; i < Mathf.Min(greeneryItems.Count, greeneryRenderers.Count); i++)
            {
                rendererDictionary.Add(greeneryItems[i], greeneryRenderers[i]);
            }
        }
    }
}