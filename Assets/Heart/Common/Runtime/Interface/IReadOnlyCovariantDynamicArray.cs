using System.Collections.Generic;

namespace Pancake.Common
{
    public interface IReadOnlyCovariantDynamicArray<out T> : IEnumerable<T>
    {
        public int Count { get; }
        public int Length { get; set; }
        public int Capacity { get; }
        public T this[int index] { get; }
    }
}