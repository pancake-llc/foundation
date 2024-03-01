using UnityEngine;

namespace Pancake
{
    public static partial class PoolHelper
    {
        public static void Populate<T>(this T scriptable, int count) where T : ScriptableObject, IPoolable
        {
            ScriptablePool<T>.GetPoolByOriginal(scriptable).Populate(count);
        }

        public static void Clear<T>(this T scriptable) where T : ScriptableObject, IPoolable { ScriptablePool<T>.GetPoolByOriginal(scriptable, false).Clear(); }

        public static T Request<T>(this T scriptable) where T : ScriptableObject, IPoolable { return ScriptablePool<T>.GetPoolByOriginal(scriptable).Request(); }

        public static void Return<T>(this T scriptable) where T : ScriptableObject, IPoolable
        {
            bool isPooled = ScriptablePool<T>.GetPoolByInstance(scriptable, out var pool);
            if (isPooled) pool.Return(scriptable);
            else Object.Destroy(scriptable);
        }
    }
}