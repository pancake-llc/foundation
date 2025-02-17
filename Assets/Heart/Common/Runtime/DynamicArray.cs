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
        [SerializeField] internal T[] items;

        /// <summary>
        /// Number of elements in the array.
        /// </summary>
        [field: SerializeField, ReadOnly] public int Length { get; internal set; }

        /// <summary>
        /// Allocated size of the array.
        /// </summary>
        public int Capacity => items.Length;

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
            if (Length + 1 > Capacity) ResizeMaintain(Capacity + CalculatePadding(Capacity));

            this[Length] = item;
            Length++;
        }

        public void AddRange(T[] items)
        {
            int addedSize = items.Length;
            if (Length + items.Length > Capacity) ResizeMaintain(Capacity + CalculatePadding(Capacity) + addedSize);
            for (var i = 0; i < addedSize; i++)
            {
                this[Length++] = items[i];
            }
        }

        public void AddRange(List<T> items)
        {
            int addedSize = items.Count;
            if (Length + items.Count > Capacity) ResizeMaintain(Capacity + CalculatePadding(Capacity) + addedSize);
            for (var i = 0; i < addedSize; i++)
            {
                this[Length++] = items[i];
            }
        }

        public void ResizeNew(int capacity)
        {
            items = new T[capacity];
            Length = 0;
        }

        public void ResizeMaintain(int capacity)
        {
            if (capacity <= Capacity) return;

            var original = items;
            items = new T[capacity];
            Array.Copy(original, items, Length); // Copy only valid elements
        }

        public T this[int index] { get => items[index]; set => items[index] = value; }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        public DynamicArrayEnumerator<T> GetEnumerator() { return new DynamicArrayEnumerator<T>(this); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public void Assign(IEnumerable<T> items, int count)
        {
            if (count > Capacity) ResizeNew(count + CalculatePadding(count));

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

            for (var i = 0; i < Length; i++)
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

        public int CalculatePadding(int currentCapacity, int segment = 4) =>
            (currentCapacity / segment).Max(MINIMUM_PADDING); // 25% of current capacity or minimum MINIMUM_PADDING

        public static DynamicArray<T> Get() { return DynamicArrayPool<T>.Get(); }

        public void Dispose()
        {
            Array.Clear(items, 0, Length);
            DynamicArrayPool<T>.Return(this);
        }
    }

    internal static class DynamicArrayPool<T>
    {
        private static readonly Stack<DynamicArray<T>> Pool = new();

        internal static DynamicArray<T> Get() { return Pool.Count > 0 ? Pool.Pop() : new DynamicArray<T>(); }

        internal static void Return(DynamicArray<T> array)
        {
            array.Length = 0;
            Pool.Push(array);
        }

        internal static void Clear() { Pool.Clear(); }
    }
}