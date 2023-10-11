#if NET_4_6 || NET_STANDARD_2_0


using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    /// <summary>
    /// Represents serialized HashSet collection.
    /// 
    /// Implement this class to create new serialized HashSet(T) collection.
    /// </summary>
    /// <typeparam name="T">Type of HashSet values.</typeparam>
    public abstract class SerializableHashSet<T> : SerializableHashSetBase,
        ICollection<T>,
        IEnumerable<T>,
        IEnumerable,
        IReadOnlyCollection<T>,
        ISet<T>,
        IDeserializationCallback,
        ISerializable
    {
        // Dictionary reference.
        protected HashSet<T> hashSet;

        #region [Abstract Methods]

        protected abstract List<T> GetKeys();

        protected abstract void SetKeys(List<T> keys);

        #endregion

        #region [Serialization]

        /// <summary>
        /// Called before engine serializes this object.
        /// 
        /// Implement this method to receive a callback before engine serializes this object.
        /// </summary>
        public override void OnBeforeSerialize()
        {
            GetKeys().Clear();
            List<T> temp = new List<T>();
            foreach (T item in hashSet)
            {
                temp.Add(item);
            }

            SetKeys(temp);
        }

        /// <summary>
        /// Called after engine deserializes this object.
        /// 
        /// Implement this method to receive a callback after engine deserializes this object.
        /// </summary>
        public override void OnAfterDeserialize()
        {
            hashSet.Clear();

            List<T> keys = GetKeys();
            for (int i = 0; i < keys.Count; i++)
            {
                hashSet.Add(keys[i]);
            }
        }

        #endregion

        #region [Base HashSet<T> Methods Implementation]

        /// <summary>
        /// Initializes a new instance of HashSet(T) class that is empty and uses the default equality comparer for the set type.
        /// </summary>
        public SerializableHashSet() { hashSet = new HashSet<T>(); }

        /// <summary>
        /// Initializes a new instance of HashSet(T) class that uses the default equality comparer for the set type,
        /// contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        public SerializableHashSet(IEnumerable<T> collection) { hashSet = new HashSet<T>(collection); }

        /// <summary>
        /// Initializes a new instance of HashSet(T) class that is empty and uses the specified equality comparer for the set type.
        /// </summary>
        /// <param name="comparer">The IEqualityComparer(T) implementation to use when comparing values in the set,
        /// or null to use the default EqualityComparer(T) implementation for the set type.</param>
        public SerializableHashSet(IEqualityComparer<T> comparer) { hashSet = new HashSet<T>(comparer); }

        /// <summary>
        /// Initializes a new instance of HashSet(T) class that uses the specified equality comparer for the set type,
        /// contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <param name="comparer">The IEqualityComparer(T) implementation to use when comparing values in the set,
        /// or null to use the default EqualityComparer(T) implementation for the set type.</param>
        public SerializableHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) { hashSet = new HashSet<T>(collection, comparer); }

        /// <summary>
        /// Gets the number of elements that are contained in a set.
        /// </summary>
        /// <value>The number of elements that are contained in the set.</value>
        public int Count { get { return hashSet.Count; } }

        /// <summary>
        /// Gets the IEqualityComparer(T) object that is used to determine equality for the values in the set.
        /// </summary>
        /// <value>The IEqualityComparer(T) object that is used to determine equality for the values in the set.</value>
        public IEqualityComparer<T> Comparer { get { return hashSet.Comparer; } }

        public bool IsReadOnly => ((ICollection<T>) hashSet).IsReadOnly;

        /// <summary>
        /// Adds the specified element to a set.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>True if the element is added to HashSet(T) object; false if the element is already present.</returns>
        public bool Add(T item) { return hashSet.Add(item); }

        /// <summary>
        /// Removes all elements from a HashSet(T) object.
        /// </summary>
        public void Clear() { hashSet.Clear(); }

        /// <summary>
        /// Determines whether a HashSet(T) object contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in HashSet(T) object.</param>
        /// <returns>True if HashSet(T) object contains the specified element; otherwise, false.</returns>
        public bool Contains(T item) { return hashSet.Contains(item); }

        /// <summary>
        /// Copies the elements of a HashSet(T) object to an array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from HashSet(T) object. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) { hashSet.CopyTo(array, arrayIndex); }

        /// <summary>
        /// Copies the specified number of elements of a HashSet(T) object to an array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from HashSet(T) object. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <param name="count">The number of elements to copy to array.</param>
        public void CopyTo(T[] array, int arrayIndex, int count) { hashSet.CopyTo(array, arrayIndex, count); }

        /// <summary>
        /// Copies the elements of a HashSet(T) object to an array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from HashSet(T) object. The array must have zero-based indexing.</param>
        public void CopyTo(T[] array) { hashSet.CopyTo(array); }

        /// <summary>
        /// Removes all elements in the specified collection from the current HashSet(T) object.
        /// </summary>
        /// <param name="other">The collection of items to remove from HashSet(T) object.</param>
        public void ExceptWith(IEnumerable<T> other) { hashSet.ExceptWith(other); }


        /// <summary>
        /// Modifies the current HashSet(T) object to contain only elements that are present in that object and in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        public void IntersectWith(IEnumerable<T> other) { hashSet.IntersectWith(other); }

        /// <summary>
        /// Determines whether a HashSet(T) object is a proper subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        /// <returns>True if HashSet(T) object is a proper subset of other; otherwise, false.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other) { return hashSet.IsProperSubsetOf(other); }

        /// <summary>
        /// Determines whether a HashSet(T) object is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        /// <returns>True if HashSet(T) object is a proper superset of other; otherwise, false.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other) { return hashSet.IsProperSupersetOf(other); }

        /// <summary>
        /// Determines whether a HashSet(T) object is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        /// <returns>True if HashSet(T) object is a subset of other; otherwise, false.</returns>
        public bool IsSubsetOf(IEnumerable<T> other) { return hashSet.IsSubsetOf(other); }

        /// <summary>
        /// Determines whether a HashSet(T) object is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        /// <returns>True if HashSet(T) object is a superset of other; otherwise, false.</returns>
        public bool IsSupersetOf(IEnumerable<T> other) { return hashSet.IsSupersetOf(other); }


        /// <summary>
        /// Determines whether the current HashSet(T) object and a specified collection share common elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        /// <returns>True if HashSet(T) object and other share at least one common element; otherwise, false.</returns>
        public bool Overlaps(IEnumerable<T> other) { return hashSet.Overlaps(other); }

        /// <summary>
        /// Removes the specified element from a HashSet(T) object.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>True if the element is successfully found and removed.
        /// Otherwise, false.
        /// This method returns false if item is not found in HashSet(T) object.</returns>
        public bool Remove(T item) { return hashSet.Remove(item); }

        /// <summary>
        /// Removes all elements that match the conditions defined by the specified predicate from a HashSet(T) collection.
        /// </summary>
        /// <param name="match">The Predicate(T) delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements that were removed from HashSet(T) collection.</returns>
        public int RemoveWhere(Predicate<T> match) { return hashSet.RemoveWhere(match); }

        /// <summary>
        /// Determines whether a HashSet(T) object and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        /// <returns>True if HashSet(T) object is equal to other; otherwise, false.</returns>
        public bool SetEquals(IEnumerable<T> other) { return hashSet.SetEquals(other); }

        /// <summary>
        /// Modifies the current HashSet(T) object to contain only elements that are present either in that object or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        public void SymmetricExceptWith(IEnumerable<T> other) { hashSet.SymmetricExceptWith(other); }

        /// <summary>
        /// Sets the capacity of a HashSet(T) object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.
        /// </summary>
        public void TrimExcess() { hashSet.TrimExcess(); }

        /// <summary>
        /// Modifies the current HashSet(T) object to contain all elements that are present in itself, the specified collection, or both.
        /// </summary>
        /// <param name="other">The collection to compare to the current HashSet(T) object.</param>
        public void UnionWith(IEnumerable<T> other) { hashSet.UnionWith(other); }

        #endregion

        #region [ICollection Implementation]

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return ((ICollection<T>) hashSet).GetEnumerator(); }

        /// <summary>
        /// Adds an item to an ICollection<T> object.
        /// </summary>
        /// <param name="item">The object to add to the ICollection<T> object.</param>
        void ICollection<T>.Add(T item) { ((ICollection<T>) hashSet).Add(item); }

        #endregion

        #region [IEnumerable<T> Implementation]

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator(T) object that can be used to iterate through the collection.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return ((IEnumerable<T>) hashSet).GetEnumerator(); }

        #endregion

        #region [ISerializable Implementation]

        /// <summary>
        /// Implements the ISerializable interface and returns the data needed to serialize a HashSet(T) object.
        /// </summary>
        /// <param name="info">A SerializationInfo object that contains the information required to serialize HashSet(T) object.</param>
        /// <param name="context">A StreamingContext structure that contains the source and destination of the serialized stream associated with HashSet(T) object.</param>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { hashSet.GetObjectData(info, context); }

        #endregion

        #region [IDeserializationCallback Implementation]

        /// <summary>
        /// Implements the ISerializable interface and raises the deserialization event when the deserialization is complete.
        /// </summary>
        /// <param name="sender">The source of the deserialization event.</param>
        public virtual void OnDeserialization(object sender) { hashSet.OnDeserialization(sender); }

        #endregion
    }
}
#endif