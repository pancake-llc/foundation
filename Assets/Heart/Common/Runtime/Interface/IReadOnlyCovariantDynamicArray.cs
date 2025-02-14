using System.Collections.Generic;

namespace Pancake.Common
{
    public interface IReadOnlyCovariantDynamicArray<out T> : IEnumerable<T>
    {
        public int Length { get; }
        public int Capacity { get; }
        public T this[int index] { get; }
    }
}