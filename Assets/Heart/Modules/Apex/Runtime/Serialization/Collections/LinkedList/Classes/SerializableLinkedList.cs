using System.Collections.Generic;
using System.Collections;
using System;
using System.Runtime.Serialization;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    /// <summary>
    /// Represents serialized doubly linked list.
    /// 
    /// Implement this class to create new serialized LinkedList(T) collection.
    /// </summary>
    /// <typeparam name="T">Type of LinkedList values.</typeparam>
    public abstract class SerializableLinkedList<T> : SerializableLinkedListBase,
        IEnumerable<T>,
        IEnumerable,
        IReadOnlyCollection<T>,
        ICollection,
        IDeserializationCallback,
        ISerializable
    {
        // LinkedList reference.
        protected LinkedList<T> linkedList;

        #region [Abstract Methods]

        protected abstract List<T> GetValues();

        protected abstract void SetValues(List<T> keys);

        #endregion

        #region [Serialization]

        /// <summary>
        /// Called after engine deserializes this object.
        /// 
        /// Implement this method to receive a callback after engine deserializes this object.
        /// </summary>
        public override void OnAfterDeserialize()
        {
            if (GetValues() != null)
            {
                linkedList = new LinkedList<T>(GetValues());
                SetValues(null);
            }
        }

        /// <summary>
        /// Called before engine serializes this object.
        /// 
        /// Implement this method to receive a callback before engine serializes this object.
        /// </summary>
        public override void OnBeforeSerialize() { SetValues(new List<T>(linkedList)); }

        #endregion

        #region [Base LinkedList<T> Methods Implementation]

        /// <summary>
        /// Initializes a new instance of the serializable LinkedList(T) class that is empty and has the default initial capacity.
        /// </summary>
        public SerializableLinkedList() { linkedList = new LinkedList<T>(); }

        /// <summary>
        /// Initializes a new instance of the serializable LinkedList(T) class that contains elements copied from the specified collection
        /// and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">Specified collection will copied to linkedList.</param>
        public SerializableLinkedList(IEnumerable<T> collection) { linkedList = new LinkedList<T>(collection); }

        /// <summary>
        /// Adds the specified new node after the specified existing node in the LinkedList(T).
        /// </summary>
        /// <param name="node">The LinkedListNode(T) after which to insert newNode.</param>
        /// <param name="newNode">The new LinkedListNode(T) to add to the LinkedList(T).</param>
        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode) { linkedList.AddAfter(node, newNode); }

        /// <summary>
        /// Adds a new node containing the specified value after the specified existing node in the LinkedList(T).
        /// </summary>
        /// <param name="node">The LinkedListNode(T) after which to insert a new LinkedListNode(T) containing value.</param>
        /// <param name="value">The value to add to the LinkedList(T).</param>
        /// <returns>The new LinkedListNode(T) containing value.</returns>
        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value) { return linkedList.AddAfter(node, value); }

        /// <summary>
        /// Adds the specified new node before the specified existing node in the LinkedList(T).
        /// </summary>
        /// <param name="node">The LinkedListNode(T) before which to insert newNode.</param>
        /// <param name="newNode">The new LinkedListNode(T) to add to the LinkedList(T).</param>
        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode) { linkedList.AddBefore(node, newNode); }

        /// <summary>
        /// Adds a new node containing the specified value before the specified existing node in the LinkedList(T).
        /// </summary>
        /// <param name="node">The LinkedListNode(T) before which to insert a new LinkedListNode(T) containing value.</param>
        /// <param name="value">The value to add to the LinkedList(T).</param>
        /// <returns>The new LinkedListNode(T) containing value.</returns>
        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value) { return linkedList.AddBefore(node, value); }

        /// <summary>
        /// Adds the specified new node at the start of the LinkedList(T).
        /// </summary>
        /// <param name="node">The new LinkedListNode(T) to add at the start of the LinkedList(T).</param>
        public void AddFirst(LinkedListNode<T> node) { linkedList.AddFirst(node); }

        /// <summary>
        /// Adds a new node containing the specified value at the start of the LinkedList(T).
        /// </summary>
        /// <param name="value">The value to add at the start of the LinkedList(T).</param>
        /// <returns>The new LinkedListNode(T) containing value.</returns>
        public LinkedListNode<T> AddFirst(T value) { return linkedList.AddFirst(value); }

        /// <summary>
        /// Adds the specified new node at the end of the LinkedList(T).
        /// </summary>
        /// <param name="node">The new LinkedListNode(T) to add at the end of the LinkedList(T).</param>
        public void AddLast(LinkedListNode<T> node) { linkedList.AddLast(node); }

        /// <summary>
        /// Adds a new node containing the specified value at the end of the LinkedList(T).
        /// </summary>
        /// <param name="value">The value to add at the end of the LinkedList(T).</param>
        /// <returns>The new LinkedListNode(T) containing value.</returns>
        public LinkedListNode<T> AddLast(T value) { return linkedList.AddLast(value); }

        /// <summary>
        /// Removes all nodes from the LinkedList(T).
        /// </summary>
        public void Clear() { linkedList.Clear(); }

        /// <summary>
        /// Determines whether a value is in the LinkedList(T).
        /// </summary>
        /// <param name="value">The value to locate in the LinkedList(T). The value can be null for reference types.</param>
        /// <returns>
        /// True if value is found in the LinkedList(T)
        /// Otherwise, false.
        /// </returns>
        public bool Contains(T value) { return linkedList.Contains(value); }

        /// <summary>
        /// Copies the entire LinkedList(T) to a compatible one-dimensional Array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from LinkedList(T). The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int index) { linkedList.CopyTo(array, index); }

        /// <summary>
        /// Finds the first node that contains the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the LinkedList(T).</param>
        /// <returns>
        /// The first LinkedListNode(T) that contains the specified value, if found
        /// Otherwise, null.
        /// </returns>
        public LinkedListNode<T> Find(T value) { return linkedList.Find(value); }

        /// <summary>
        /// Finds the last node that contains the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the LinkedList(T).</param>
        /// <returns>
        /// The last LinkedListNode(T) that contains the specified value, if found
        /// Otherwise, null.
        /// </returns>
        public LinkedListNode<T> FindLast(T value) { return linkedList.FindLast(value); }

        /// <summary>
        /// Removes the specified node from the LinkedList(T).
        /// </summary>
        /// <param name="node">The LinkedListNode(T) to remove from the LinkedList(T).</param>
        public void Remove(LinkedListNode<T> node) { linkedList.Remove(node); }

        /// <summary>
        /// Removes the first occurrence of the specified value from the LinkedList(T).
        /// </summary>
        /// <param name="value">The value to remove from the LinkedList(T).</param>
        /// <returns>
        /// True if the element containing value is successfully removed
        /// Otherwise, false. 
        /// This method also returns false if value was not found in the original LinkedList(T).
        /// </returns>
        public bool Remove(T value) { return linkedList.Remove(value); }

        /// <summary>
        /// Removes the node at the start of the LinkedList(T).
        /// </summary>
        public void RemoveFirst() { linkedList.RemoveFirst(); }

        /// <summary>
        /// Removes the node at the end of the LinkedList(T).
        /// </summary>
        public void RemoveLast() { linkedList.RemoveLast(); }

        #endregion

        #region [IReadOnlyCollection<T> / ICollection Implementation]

        /// <summary>
        /// Gets the number of nodes actually contained in the LinkedList(T).
        /// </summary>
        /// <value>The number of nodes actually contained in the LinkedList(T).</value>
        public int Count { get { return linkedList.Count; } }

        /// <summary>
        /// Gets a value indicating whether access to the ICollection is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// True if access to the ICollection is synchronized (thread safe), otherwise, false. 
        /// In the default implementation of LinkedList(T), this property always returns false.
        /// </returns>
        public bool IsSynchronized { get { return ((ICollection) linkedList).IsSynchronized; } }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the ICollection.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the ICollection. 
        /// In the default implementation of LinkedList(T), this property always returns the current instance.
        /// </returns>
        public object SyncRoot { get { return ((ICollection) linkedList).SyncRoot; } }

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index) { ((ICollection) linkedList).CopyTo(array, index); }

        #endregion

        #region [IEnumerable<T> / IEnumerable Implementation]

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerator<T> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator() { return linkedList.GetEnumerator(); }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return linkedList.GetEnumerator(); }

        #endregion

        #region [IDeserializationCallback Implementation]

        /// <summary>
        /// Implements the ISerializable interface and raises the deserialization event when the deserialization is complete.
        /// </summary>
        /// <param name="sender">The source of the deserialization event.</param>
        public virtual void OnDeserialization(object sender) { linkedList.OnDeserialization(sender); }

        #endregion

        #region [ISerializable Implementation]

        /// <summary>
        /// Implements the ISerializable interface and returns the data needed to serialize the LinkedList(T) instance.
        /// </summary>
        /// <param name="info">A SerializationInfo object that contains the information required to serialize the LinkedList(T) instance.</param>
        /// <param name="context">A StreamingContext object that contains the source and destination of the serialized stream associated with the LinkedList(T) instance.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { linkedList.GetObjectData(info, context); }

        #endregion
    }
}