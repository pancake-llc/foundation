using System;
using System.Collections.Generic;
using Pancake.Threading.Tasks;
using UnityEngine;

namespace Pancake
{
    public static class MagicPool
    {
        public static event Action<GameObject> OnSpawned;
        public static event Action<GameObject> OnDespawned;
        public static readonly List<Pool> Pools = new List<Pool>(64);
        private static readonly List<IPoolable> PoolableComponents = new List<IPoolable>(32);

        public static void InstallPoolPreset(PoolPreset preset)
        {
            if (preset == null || !Application.isPlaying) return;

            for (int i = 0; i < preset.PoolObjects.Count; i++)
            {
                var poolObject = preset.PoolObjects[i];
                var pool = GetPoolByPrefab(poolObject.Prefab);
                pool.Populate(poolObject.Size);
            }
        }

        public static T Spawn<T>(T component, Vector3 position = default, Quaternion rotation = default) where T : Component
        {
            return DefaultSpawn(component.gameObject,
                    position,
                    rotation,
                    null,
                    false)
                .GetComponent<T>();
        }

        public static T Spawn<T>(T component, Transform parent, Quaternion rotation = default, bool worldStaysPosition = false) where T : Component
        {
            var position = parent != null ? parent.position : Vector3.zero;

            return DefaultSpawn(component.gameObject,
                    position,
                    rotation,
                    parent,
                    worldStaysPosition)
                .GetComponent<T>();
        }

        public static GameObject Spawn(GameObject gameObject, Vector3 position = default, Quaternion rotation = default)
        {
            return DefaultSpawn(gameObject,
                position,
                rotation,
                null,
                false);
        }

        public static GameObject Spawn(GameObject gameObject, Transform parent, Quaternion rotation = default, bool worldPositionStays = false)
        {
            var position = parent != null ? parent.position : Vector3.zero;

            return DefaultSpawn(gameObject,
                position,
                rotation,
                parent,
                worldPositionStays);
        }

        public static void Despawn(Component toDespawn, float delay = 0f) { DefaultDespawn(toDespawn.gameObject, delay); }

        public static void Despawn(GameObject toDespawn, float delay = 0f) { DefaultDespawn(toDespawn, delay); }

        /// <summary>
        /// Destroys pool by GameObject
        /// </summary>
        public static void DestroyPool(GameObject gameObject)
        {
            if (gameObject == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("GameObject is null!");
#endif
                return;
            }

            if (gameObject.TryGetComponent(out Poolable poolable))
            {
                DestroyPool(poolable.Pool);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"{gameObject.name} was not spawned by NightPool!");
#endif
            }
        }

        public static void DestroyPool(Pool pool)
        {
            if (pool == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Pool is null!");
#endif
                return;
            }

            foreach (var poolable in pool.Poolables)
            {
                UnityEngine.Object.Destroy(poolable.gameObject);
            }

            UnityEngine.Object.Destroy(pool.gameObject);

            Pools.Remove(pool);
        }

        /// <summary>
        /// Destroys all pools
        /// </summary>
        public static void DestroyAllPools()
        {
            if (!Application.isPlaying) return;

            var pools = Pools.ToArray();

            foreach (var pool in pools)
            {
                DestroyPool(pool);
            }

            Pools.Clear();
        }

        /// <summary>
        /// Resets the static components of NightPool
        /// </summary>
        public static void Reset()
        {
            ResetLists();

            ResetActions();
        }

        public static Pool GetPoolByPrefab(GameObject prefab)
        {
            int count = Pools.Count;
            for (int i = 0; i < count; i++)
            {
                if (Pools[i].Prefab == prefab) return Pools[i];
            }

            return CreateNewPool(prefab);
        }

        private static Pool CreateNewPool(GameObject prefab)
        {
            var parent = new GameObject($"[Pool] {prefab.name}");
            var pool = parent.AddComponent<Pool>();
            pool.Init(prefab, parent.transform);
            Pools.Add(pool);
            return pool;
        }

        /// <summary>
        /// Default spawn method
        /// </summary>
        /// <returns> Spawned GameObject </returns>
        private static GameObject DefaultSpawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            if (!Application.isPlaying) return default;

            var pool = GetPoolByPrefab(prefab);
            var freePoolable = pool.Get();
            var gameObject = freePoolable.gameObject;

            gameObject.SetActive(true);

            SetupTransform(freePoolable.transform,
                position,
                rotation,
                parent,
                worldPositionStays);

            CheckForSpawnEvents(gameObject);

            return gameObject;
        }

        /// <summary>
        /// Default despawn method
        /// </summary>
        /// <param name="gameObject"> GameObject to despawn </param>
        /// <param name="delay"> For despawn with a delay </param>
        private static async void DefaultDespawn(GameObject gameObject, float delay = 0f)
        {
            if (!Application.isPlaying) return;

            if (gameObject.TryGetComponent(out Poolable poolable))
            {
                if (delay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                    if (!Application.isPlaying) return;
                    if (gameObject == null) return;
                }

                var pool = poolable.Pool;

                if (pool != null)
                {
                    gameObject.SetActive(false);
                    gameObject.transform.SetParent(pool.Parent);

                    pool.Add(poolable);
                }
                else
                {
                    UnityEngine.Object.Destroy(gameObject);
                }

                CheckForDespawnEvents(gameObject);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"{gameObject.name} was not spawned by MagicPool and will be destroyed!");
#endif
                UnityEngine.Object.Destroy(gameObject, delay);
            }
        }

        private static void CheckForSpawnEvents(GameObject gameObject)
        {
            OnSpawned?.Invoke(gameObject);
            gameObject.GetComponentsInChildren(PoolableComponents);
            for (int i = 0; i < PoolableComponents.Count; i++)
            {
                PoolableComponents[i].OnSpawn();
            }
        }

        private static void CheckForDespawnEvents(GameObject gameObject)
        {
            OnDespawned?.Invoke(gameObject);
            gameObject.GetComponentsInChildren(PoolableComponents);
            for (int i = 0; i < PoolableComponents.Count; i++)
            {
                PoolableComponents[i].OnDespawn();
            }
        }

        /// <summary>
        /// Sets the position and rotation of Transform
        /// </summary>
        private static void SetupTransform(Transform transform, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = false)
        {
            transform.SetParent(parent, worldPositionStays);
            transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// Resets static Lists in NightPool
        /// </summary>
        private static void ResetLists()
        {
            Pools?.Clear();
            PoolableComponents?.Clear();
        }

        /// <summary>
        /// Resets static Actions in NightPool
        /// </summary>
        private static void ResetActions()
        {
            OnSpawned = null;
            OnDespawned = null;
        }
    }
}