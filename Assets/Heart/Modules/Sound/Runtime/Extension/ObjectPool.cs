using System.Collections.Generic;

namespace Pancake.Sound
{
    internal abstract class ObjectPool<T> where T : class
    {
        protected readonly int maxPoolSize;
        protected readonly T baseObject;
        protected readonly List<T> pool = new();

        protected abstract T CreateObject();
        protected abstract void DestroyObject(T instance);

        protected ObjectPool(T baseObject, int maxInternalPoolSize)
        {
            this.baseObject = baseObject;
            maxPoolSize = maxInternalPoolSize;
        }

        public virtual void Prewarm(int initialCount)
        {
            for (int i = 0; i < initialCount; i++)
            {
                T obj = CreateObject();
                pool.Add(obj);
            }
        }

        public virtual T Extract()
        {
            T obj;
            if (pool.Count == 0) obj = CreateObject();
            else
            {
                int lastIndex = pool.Count - 1;
                obj = pool[lastIndex];
                pool.RemoveAt(lastIndex);
            }

            return obj;
        }

        public virtual void Recycle(T obj)
        {
            if (pool.Count == maxPoolSize) DestroyObject(obj);
            else pool.Add(obj);
        }
    }
}