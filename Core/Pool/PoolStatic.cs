using UnityEngine;

namespace Pancake
{
    public static class PoolStatic
    {
        public static T Spawn<T>(this Pool pool, Vector3 position = default, Quaternion rotation = default) where T : Component
        {
            return DefaultSpawn(pool,
                    position,
                    rotation,
                    null,
                    false)
                .GetComponent<T>();
        }

        public static T Spawn<T>(this Pool pool, Transform parent, Quaternion rotation = default, bool worldStaysPosition = false) where T : Component
        {
            var position = parent != null ? parent.position : Vector3.zero;

            return DefaultSpawn(pool,
                    position,
                    rotation,
                    parent,
                    worldStaysPosition)
                .GetComponent<T>();
        }

        public static GameObject Spawn(this Pool pool, Vector3 position = default, Quaternion rotation = default)
        {
            return DefaultSpawn(pool,
                position,
                rotation,
                null,
                false);
        }

        public static GameObject Spawn(this Pool pool, Transform parent, Quaternion rotation = default, bool worldPositionStays = false)
        {
            var position = parent != null ? parent.position : Vector3.zero;

            return DefaultSpawn(pool,
                position,
                rotation,
                parent,
                worldPositionStays);
        }

        public static void ReturnPool(this Component component, float delay = 0f) { component.gameObject.ReturnPool(delay); }

        public static void ReturnPool(this GameObject gameObject, float delay = 0f)
        {
            if (gameObject.TryGetComponent(out PoolMember member))
            {
                member.Return(delay);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"{gameObject.name} was not created by pool so it will be destroyed!");
#endif
                UnityEngine.Object.Destroy(gameObject, delay);
            }
        }

        /// <summary>
        /// Default spawn method
        /// </summary>
        /// <returns> Spawned GameObject </returns>
        private static GameObject DefaultSpawn(this Pool pool, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            if (!Application.isPlaying) return default;

            var member = pool.Get();
            var gameObject = member.gameObject;

            gameObject.SetActive(true);

            member.transform.SetParent(parent, worldPositionStays);
            member.transform.SetPositionAndRotation(position, rotation);

            return gameObject;
        }
    }
}