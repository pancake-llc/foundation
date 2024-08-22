using System;

namespace Pancake.Pools
{
    public interface IObjectPool<T> : IDisposable
    {
        T Request();
        void Return(T obj);
    }
}