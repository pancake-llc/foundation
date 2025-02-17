using System;
using System.Collections.Generic;

namespace Pancake.Common
{
    public interface IReadonlyDynamicArray<T> : IEnumerable<T>, IDisposable
    {
        public int Length { get; }
        public int Capacity { get; }
        public T this[int index] { get; }

        public new DynamicArrayEnumerator<T> GetEnumerator();
    }
}