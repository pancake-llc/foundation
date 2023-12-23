using System.Collections.Generic;
using System.Collections;
using System;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    /// <summary>
    /// Represents a first-in, first-out serialized collection of objects.
    /// 
    /// Implement this class to create new serialized Queue(T) collection.
    /// </summary>
    /// <typeparam name="T">Type of Queue values.</typeparam>
    public abstract class SerializableQueue<T> : SerializableQueueBase, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
    {
        // Queue reference.
        protected Queue<T> queue;

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
                queue = new Queue<T>(GetValues());
                SetValues(null);
            }
        }

        /// <summary>
        /// Called before engine serializes this object.
        /// 
        /// Implement this method to receive a callback before engine serializes this object.
        /// </summary>
        public override void OnBeforeSerialize() { SetValues(new List<T>(queue)); }

        #endregion

        #region [Base Queue<T> Methods Implementation]

        /// <summary>
        /// Initializes a new instance of the serializable Queue(T) class that is empty and has the default initial capacity.
        /// </summary>
        public SerializableQueue() { queue = new Queue<T>(); }

        /// <summary>
        /// Initializes a new instance of the serializable Queue(T) class that contains elements copied from the specified collection
        /// and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">Specified collection will copied to queue.</param>
        public SerializableQueue(IEnumerable<T> collection) { queue = new Queue<T>(collection); }

        /// <summary>
        /// Initializes a new instance of the serializable Queue(T) class that is empty and has the specified initial capacity
        /// or the default initial capacity, whichever is greater.
        /// </summary>
        /// <param name="capacity">Initial queue capacity.</param>
        public SerializableQueue(int capacity) { queue = new Queue<T>(capacity); }

        /// <summary>
        /// Removes all objects from the Queue(T).
        /// </summary>
        public void Clear() { queue.Clear(); }

        /// <summary>
        /// Determines whether an element is in the Queue(T).
        /// </summary>
        /// <param name="item">Object type of (T)</param>
        /// <returns>True if item is found in the Queue(T); otherwise, false.</returns>
        public bool Contains(T item) { return queue.Contains(item); }

        /// <summary>
        /// Returns the object at the beginning of the Queue(T) without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the Queue(T).</returns>
        public T Peek() { return queue.Peek(); }

        /// <summary>
        /// Removes and returns the object at the beginning of the Queue(T).
        /// </summary>
        /// <returns>The object that is removed from the beginning of the Queue(T).</returns>
        public T Dequeue() { return queue.Dequeue(); }

        /// <summary>
        /// Adds an object to the end of the Queue(T).
        /// </summary>
        /// <param name="item">The object to add to the Queue(T). The value can be null for reference types.</param>
        public void Enqueue(T item) { queue.Enqueue(item); }

        /// <summary>
        /// Copies the Queue(T) to a new array.
        /// </summary>
        /// <returns>A new array containing copies of the elements of the Queue(T).</returns>
        public T[] ToArray() { return queue.ToArray(); }

        /// <summary>
        /// Copies the Queue(T) to an existing one-dimensional Array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from Queue(T). The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) { queue.CopyTo(array, arrayIndex); }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the Queue(T), if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess() { queue.TrimExcess(); }

        #endregion

        #region [IReadOnlyCollection<T> / ICollection Implementation]

        /// <summary>
        /// Gets the number of elements contained in the Queue(T).
        /// </summary>
        /// <value>The number of elements contained in the Queue(T).</value>
        public int Count { get { return queue.Count; } }

        /// <summary>
        /// Gets a value indicating whether access to the ICollection is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// True if access to the ICollection is synchronized (thread safe), otherwise, false. 
        /// In the default implementation of Queue(T), this property always returns false.
        /// </returns>
        public bool IsSynchronized { get { return ((ICollection) queue).IsSynchronized; } }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the ICollection.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the ICollection. 
        /// In the default implementation of Queue(T), this property always returns the current instance.
        /// </returns>
        public object SyncRoot { get { return ((ICollection) queue).SyncRoot; } }

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index) { ((ICollection) queue).CopyTo(array, index); }

        #endregion

        #region [IEnumerable<T> / IEnumerable Implementation]

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerator(T) that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator() { return queue.GetEnumerator(); }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return queue.GetEnumerator(); }

        #endregion
    }
}