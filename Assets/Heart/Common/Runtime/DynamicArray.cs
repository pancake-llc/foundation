using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.Common
{
    [Serializable]
    public class DynamicArray<T> : IReadonlyDynamicArray<T>, IReadOnlyCovariantDynamicArray<T>
    {
        [SerializeField] public T[] items;
        public int Count => Length;
        [field: SerializeField, ReadOnly] public int Length { get; set; }
        public int TotalLength => items.Length;
        private const int DEFAULT_CAPACITY = 10;
        private const int MINIMUM_PADDING = 5;

        public DynamicArray(int capacity = DEFAULT_CAPACITY)
        {
            items = new T[capacity];
            Length = 0;
        }

        public void Clear()
        {
            Length = 0;
            Array.Clear(items, 0, items.Length);
        }

        public void Add(T item)
        {
            if (Length + 1 > TotalLength) ResizeMaintain(TotalLength + CalculatePadding(TotalLength));

            this[Length] = item;
            Length++;
        }

        public void ResizeNew(int capacity)
        {
            items = new T[capacity];
            Length = 0;
        }

        public void ResizeMaintain(int capacity)
        {
            if (capacity <= TotalLength) return;

            var original = items;
            items = new T[capacity];
            Array.Copy(original, items, original.Length);
        }

        public T this[int index] { get => items[index]; set => items[index] = value; }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        public DynamicArrayEnumerator<T> GetEnumerator() { return new DynamicArrayEnumerator<T>(this); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public void Assign(IEnumerable<T> items, int count)
        {
            if (count > TotalLength) ResizeNew(count + CalculatePadding(count));

            var i = 0;
            foreach (var item in items)
            {
                this[i] = item;
                i++;
            }

            Length = count;
        }

        public void Filter(Predicate<T> filter)
        {
            var insertIndex = 0;

            for (var i = 0; i < Count; i++)
            {
                var item = this[i];
                if (filter(item))
                {
                    this[insertIndex] = item;
                    insertIndex++;
                }
            }

            Length = insertIndex;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Length) throw new IndexOutOfRangeException();

            for (int i = index; i < Length - 1; i++)
            {
                items[i] = items[i + 1];
            }

            Length--;
            items[Length] = default; // Clear last item
        }

        public int CalculatePadding(int currentCapacity, int segment = 4) => (currentCapacity / segment).Max(MINIMUM_PADDING); // 25% of current capacity or minimum MINIMUM_PADDING
    }
}