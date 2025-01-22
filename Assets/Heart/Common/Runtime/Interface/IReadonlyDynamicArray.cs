using System.Collections.Generic;

namespace Pancake.Common
{
    public interface IReadonlyDynamicArray<T> : IEnumerable<T>
    {
        public int Count { get; }
        public int Length { get; set; }
        public int Capacity { get; }
        public T this[int index] { get; }

        public new DynamicArrayEnumerator<T> GetEnumerator();
    }
}