using System.Collections.Generic;
using System.Collections;
using System;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    /// <summary>
    /// Represents a variable size last-in-first-out (LIFO) serialized collection of instances of the same specified type.
    /// 
    /// Implement this class to create new serialized Stack(T) collection.
    /// </summary>
    /// <typeparam name="T">Type of Stack values.</typeparam>
    public abstract class SerializableStack<T> : SerializableStackBase, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
    {
        // Stack reference.
        protected Stack<T> stack;

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
                stack = new Stack<T>(GetValues());
                SetValues(null);
            }
        }

        /// <summary>
        /// Called before engine serializes this object.
        /// 
        /// Implement this method to receive a callback before engine serializes this object.
        /// </summary>
        public override void OnBeforeSerialize() { SetValues(new List<T>(stack)); }

        #endregion

        #region [Base Stack<T> Methods Implementation]

        /// <summary>
        /// Initializes a new instance of the serializable Stack(T) class that is empty and has the default initial capacity.
        /// </summary>
        public SerializableStack() { stack = new Stack<T>(); }

        /// <summary>
        /// Initializes a new instance of the serializable Stack(T) class that contains elements copied from the specified collection
        /// and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">Specified collection will copied to stack.</param>
        public SerializableStack(IEnumerable<T> collection) { stack = new Stack<T>(collection); }

        /// <summary>
        /// Initializes a new instance of the serializable Stack(T) class that is empty and has the specified initial capacity
        /// or the default initial capacity, whichever is greater.
        /// </summary>
        /// <param name="capacity">Initial stack capacity.</param>
        public SerializableStack(int capacity) { stack = new Stack<T>(capacity); }

        /// <summary>
        /// Removes all objects from the Stack(T).
        /// </summary>
        public void Clear() { stack.Clear(); }

        /// <summary>
        /// Determines whether an element is in the Stack(T).
        /// </summary>
        /// <param name="item">Object type of (T)</param>
        /// <returns>True if item is found in the Stack(T); otherwise, false.</returns>
        public bool Contains(T item) { return stack.Contains(item); }

        /// <summary>
        /// Returns the object at the top of the Stack(T) without removing it.
        /// </summary>
        /// <returns>The object at the top of the Stack(T).</returns>
        public T Peek() { return stack.Peek(); }

        /// <summary>
        /// Removes and returns the object at the top of the Stack(T).
        /// </summary>
        /// <returns>The object at the top of the Stack(T).</returns>
        public T Pop() { return stack.Pop(); }

        /// <summary>
        /// Inserts an object at the top of the Stack(T).
        /// </summary>
        /// <param name="item">Object type of (T)</param>
        public void Push(T item) { stack.Push(item); }

        /// <summary>
        /// Copies the Stack(T) to a new array.
        /// </summary>
        /// <returns>A new array containing copies of the elements of the Stack(T).</returns>
        public T[] ToArray() { return stack.ToArray(); }

        /// <summary>
        /// Copies the Stack(T) to an existing one-dimensional Array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from Stack(T). The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) { stack.CopyTo(array, arrayIndex); }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the Stack(T), if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess() { stack.TrimExcess(); }

        #endregion

        #region [IReadOnlyCollection<T> / ICollection Implementation]

        /// <summary>
        /// Gets the number of elements contained in the Stack(T).
        /// </summary>
        /// <value>The number of elements contained in the Stack(T).</value>
        public int Count { get { return stack.Count; } }

        /// <summary>
        /// Gets a value indicating whether access to the ICollection is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// True if access to the ICollection is synchronized (thread safe), otherwise, false. 
        /// In the default implementation of Stack(T), this property always returns false.
        /// </returns>
        public bool IsSynchronized { get { return ((ICollection) stack).IsSynchronized; } }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the ICollection.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the ICollection. 
        /// In the default implementation of Stack(T), this property always returns the current instance.
        /// </returns>
        public object SyncRoot { get { return ((ICollection) stack).SyncRoot; } }

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index) { ((ICollection) stack).CopyTo(array, index); }

        #endregion

        #region [IEnumerable<T> / IEnumerable Implementation]

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerator(T) that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator() { return stack.GetEnumerator(); }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return stack.GetEnumerator(); }

        #endregion
    }
}