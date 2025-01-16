using System.Collections;
using System.Collections.Generic;

namespace Pancake.Common
{
    public struct DynamicArrayEnumerator<T> : IEnumerator<T>
    {
        private readonly DynamicArray<T> _array;

        private int _index;

        public DynamicArrayEnumerator(DynamicArray<T> array)
        {
            _array = array;
            _index = -1;
        }

        public bool MoveNext()
        {
            _index += 1;
            return _index < _array.Length;
        }

        public void Reset() { _index = -1; }

        public T Current => _array[_index];

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}