using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pancake
{
    public sealed class MagicLinkedList<T> : ICollection<T>, ICollection
    {
        private readonly LinkedList<T> _linkedList;
        private readonly Queue<LinkedListNode<T>> _cacheNodes;

        public MagicLinkedList()
        {
            _linkedList = new LinkedList<T>();
            _cacheNodes = new Queue<LinkedListNode<T>>();
        }

        /// <summary>
        /// Get the number of nodes actually contained in the linked list.
        /// </summary>
        public int Count => _linkedList.Count;

        /// <summary>
        /// Get the number of linked list node caches.
        /// </summary>
        public int CacheNodeCount => _cacheNodes.Count;

        /// <summary>
        /// Get the first node of the linked list.
        /// </summary>
        public LinkedListNode<T> First => _linkedList.First;

        /// <summary>
        /// Get the last node of the linked list.
        /// </summary>
        public LinkedListNode<T> Last => _linkedList.Last;

        public bool IsSynchronized => ((ICollection) _linkedList).IsSynchronized;
        public object SyncRoot => ((ICollection) _linkedList).SyncRoot;
        public bool IsReadOnly => ((ICollection<T>) _linkedList).IsReadOnly;

        /// <summary>
        /// Adds a new node containing the specified value after the specified existing node in the linked list.
        /// </summary>
        /// <param name="node">The specified existing node.</param>
        /// <param name="value">Specify a value.</param>
        /// <returns>A new node containing the specified value.</returns>
        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            var newNode = AcquireNode(value);
            _linkedList.AddAfter(node, newNode);
            return newNode;
        }

        /// <summary>
        /// Adds the specified new node after the specified existing node in the linked list.
        /// </summary>
        /// <param name="node">Specified existing node.</param>
        /// <param name="newNode">Specified new node.</param>
        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode) { _linkedList.AddAfter(node, newNode); }


        /// <summary>
        /// Add a new node containing the specified value before the specified existing node in the linked list.
        /// </summary>
        /// <param name="node">Specified existing node.</param>
        /// <param name="value">Specify the value.</param>
        /// <returns>A new node containing the specified value.</returns>
        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            var newNode = AcquireNode(value);
            _linkedList.AddBefore(node, newNode);
            return newNode;
        }

        /// <summary>
        /// Add the specified new node before the specified existing node in the linked list.
        /// </summary>
        /// <param name="node">The specified existing node.</param>
        /// <param name="newNode">Specifies the new node.</param>
        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode) { _linkedList.AddBefore(node, newNode); }

        /// <summary>
        /// Add a new node containing the specified value at the beginning of the linked list.
        /// </summary>
        /// <param name="value">Specify the value.</param>
        /// <returns>A new node containing the specified value.</returns>
        public LinkedListNode<T> AddFirst(T value)
        {
            var node = AcquireNode(value);
            _linkedList.AddFirst(node);
            return node;
        }

        /// <summary>
        /// Adds the specified new node at the beginning of the linked list.
        /// </summary>
        /// <param name="node">The specified new node.</param>
        public void AddFirst(LinkedListNode<T> node) { _linkedList.AddFirst(node); }

        /// <summary>
        /// Add a new node containing the specified value at the end of the linked list.
        /// </summary>
        /// <param name="value">Specify the value.</param>
        /// <returns>A new node containing the specified value.</returns>
        public LinkedListNode<T> AddLast(T value)
        {
            var node = AcquireNode(value);
            _linkedList.AddLast(node);
            return node;
        }

        /// <summary>
        /// Add the specified new node at the end of the linked list.
        /// </summary>
        /// <param name="node">The specified new node.</param>
        public void AddLast(LinkedListNode<T> node) { _linkedList.AddLast(node); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        /// <summary>
        /// Returns the enumerator for iterating through the collection.
        /// </summary>
        /// <returns>Iterate through the enumerator of the collection. </returns>
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /// <summary>
        /// Returns the enumerator for iterating through the collection.
        /// </summary>
        /// <returns>Iterate through the enumerator of the collection. </returns>
        public Enumerator GetEnumerator() { return new Enumerator(_linkedList); }


        /// <summary>
        /// Add values to the end of ICollection`1.
        /// </summary>
        /// <param name="value">The value to be added. </param>
        public void Add(T value) { AddLast(value); }

        /// <summary>
        /// Remove all nodes from the linked list.
        /// </summary>
        public void Clear()
        {
            var current = _linkedList.First;
            while (current != null)
            {
                ReleaseNode(current);
                current = current.Next;
            }

            _linkedList.Clear();
        }

        /// <summary>
        /// Clear the linked list node cache.
        /// </summary>
        public void ClearCacheNodes() { _cacheNodes.Clear(); }

        /// <summary>
        /// Determine whether a value is in the linked list.
        /// </summary>
        /// <param name="value">Specify the value.</param>
        /// <returns>Whether a value is in the linked list.</returns>
        public bool Contains(T value) { return _linkedList.Contains(value); }

        /// <summary>
        /// Copy the entire linked list to a compatible one-dimensional array starting from the specified index of the target array.
        /// </summary>
        /// <param name="array">One-dimensional array, which is the target of elements copied from the linked list. Arrays must have zero-based indices.</param>
        /// <param name="index">The zero-based index in the array from which to start copying.</param>
        public void CopyTo(T[] array, int index) { _linkedList.CopyTo(array, index); }

        /// <summary>
        /// Starting from a specific ICollection index, copy the elements of the array into an array.
        /// </summary>
        /// <param name="array">One-dimensional array, which is the target of elements copied from ICollection. Arrays must have zero-based indices.</param>
        /// <param name="index">The zero-based index in the array from which to start copying.</param>
        public void CopyTo(Array array, int index) { ((ICollection) _linkedList).CopyTo(array, index); }

        /// <summary>
        /// Remove the first occurrence of the specified value from the linked list.
        /// </summary>
        /// <param name="value">Specify the value.</param>
        /// <returns>Whether the removal is successful.</returns>
        public bool Remove(T value)
        {
            var node = _linkedList.Find(value);
            while (node != null)
            {
                _linkedList.Remove(node);
                ReleaseNode(node);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove the specified node from the linked list.
        /// </summary>
        /// <param name="node">The specified node.</param>
        public void Remove(LinkedListNode<T> node)
        {
            _linkedList.Remove(node);
            ReleaseNode(node);
        }

        /// <summary>
        /// Remove the node at the beginning of the linked list.
        /// </summary>
        public void RemoveFirst()
        {
            var first = _linkedList.First;
            if (first == null) throw new Exception("First is invalid.");

            _linkedList.RemoveFirst();
            ReleaseNode(first);
        }

        /// <summary>
        /// Remove the node at the end of the linked list.
        /// </summary>
        public void RemoveLast()
        {
            var last = _linkedList.Last;
            if (last == null) throw new Exception("Last is invalid.");

            _linkedList.RemoveLast();
            ReleaseNode(last);
        }

        /// <summary>
        /// Find the first node that contains the specified value.
        /// </summary>
        /// <param name="value">The specified value to find.</param>
        /// <returns>The first node containing the specified value.</returns>
        public LinkedListNode<T> Find(T value) { return _linkedList.Find(value); }

        /// <summary>
        /// Find the last node that contains the specified value.
        /// </summary>
        /// <param name="value">The specified value to find.</param>
        /// <returns>The last node containing the specified value.</returns>
        public LinkedListNode<T> FindLast(T value) { return _linkedList.FindLast(value); }


        private LinkedListNode<T> AcquireNode(T value)
        {
            LinkedListNode<T> node;
            if (_cacheNodes.Count > 0)
            {
                node = _cacheNodes.Dequeue();
                node.Value = value;
            }
            else
            {
                node = new LinkedListNode<T>(value);
            }

            return node;
        }

        private void ReleaseNode(LinkedListNode<T> node)
        {
            node.Value = default;
            _cacheNodes.Enqueue(node);
        }


        /// <summary>
        /// An enumerator to iterate over the collection.
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
            private LinkedList<T>.Enumerator _enumerator;

            internal Enumerator(LinkedList<T> linkedList)
            {
                if (linkedList == null) throw new Exception("Linked list is invalid.");

                _enumerator = linkedList.GetEnumerator();
            }

            /// <summary>
            /// Get the current node.
            /// </summary>
            public T Current => _enumerator.Current;

            /// <summary>
            /// Get the current enumerator.
            /// </summary>
            object IEnumerator.Current => _enumerator.Current;

            /// <summary>
            /// Clean up enumerators.
            /// </summary>
            public void Dispose() { _enumerator.Dispose(); }

            /// <summary>
            /// Get the next node.
            /// </summary>
            /// <returns>Return the next node.</returns>
            public bool MoveNext() { return _enumerator.MoveNext(); }

            /// <summary>
            /// Reset enumerator.
            /// </summary>
            void IEnumerator.Reset() { ((IEnumerator<T>) _enumerator).Reset(); }
        }
    }
}