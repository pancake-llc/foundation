using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    /// <summary>
    /// Represents of serialized Dictionary.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    [System.Serializable]
    public abstract class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IDictionary<TKey, TValue>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
        IReadOnlyDictionary<TKey, TValue>,
        ICollection,
        IDictionary,
        IDeserializationCallback,
        ISerializable
    {
        protected Dictionary<TKey, TValue> dictionary;

        #region [Abstract Methods]

        /// <summary>
        /// Serialized Dictionary reference keys.
        /// </summary>
        /// <returns>Array of the (TKey).</returns>
        protected abstract List<TKey> GetKeys();

        /// <summary>
        /// Serialized Dictionary reference values.
        /// </summary>
        /// <returns>Array of the (TValue).</returns>
        protected abstract List<TValue> GetValues();

        /// <summary>
        /// Set serialized Dictionary reference keys.
        /// </summary>
        protected abstract void SetKeys(List<TKey> keys);

        /// <summary>
        /// Set serialized Dictionary reference values.
        /// </summary>
        protected abstract void SetValues(List<TValue> values);

        #endregion

        #region [Serialization]

        /// <summary>
        /// Called before engine serializes this object.
        /// <br>Implement this method to receive a callback before engine serializes this object.</br>
        /// </summary>
        public override void OnBeforeSerialize() { }

        /// <summary>
        /// Called after engine deserializes this object.
        /// <br>Implement this method to receive a callback after engine deserializes this object.</br>
        /// </summary>
        public override void OnAfterDeserialize()
        {
            if (GetKeys() != null && GetValues() != null && GetKeyLength() == GetValueLength())
            {
                dictionary.Clear();
                for (int i = 0; i < GetKeyLength(); i++)
                {
                    dictionary[GetKey(i)] = GetValue(i);
                }
            }
        }

        #endregion

        #region [Base Dictionary<TKey, TValue> Methods Implementation]

        /// <summary>
        /// Initializes a new instance of the Dictionary(TKey,TValue) class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public SerializableDictionary() { dictionary = new Dictionary<TKey, TValue>(); }

        /// <summary>
        /// Initializes a new instance of the Dictionary(TKey,TValue) class that contains elements copied from the specified IDictionary(TKey,TValue) and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The IDictionary(TKey,TValue) whose elements are copied to the new Dictionary(TKey,TValue).</param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) { dictionary = new Dictionary<TKey, TValue>(dictionary); }

        /// <summary>
        /// Initializes a new instance of the Dictionary(TKey,TValue) class that contains elements copied from the specified IDictionary(TKey,TValue) and uses the specified IEqualityComparer(T).
        /// </summary>
        /// <param name="comparer">The IEqualityComparer(T) implementation to use when comparing keys, or null to use the default EqualityComparer(T) for the type of the key.</param>
        public SerializableDictionary(IEqualityComparer<TKey> comparer) { dictionary = new Dictionary<TKey, TValue>(comparer); }

        /// <summary>
        /// Initializes a new instance of the Dictionary(TKey,TValue) class that is empty, has the default initial capacity, and uses the specified IEqualityComparer(T).
        /// </summary>
        /// <param name="capacity">The initial number of elements that the Dictionary(TKey,TValue) can contain.</param>
        public SerializableDictionary(int capacity) { dictionary = new Dictionary<TKey, TValue>(capacity); }

        /// <summary>
        /// Initializes a new instance of the Dictionary(TKey,TValue) class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The initial number of elements that the Dictionary(TKey,TValue) can contain.</param>
        /// <param name="comparer">The IEqualityComparer(T) implementation to use when comparing keys, or null to use the default EqualityComparer(T) for the type of the key.</param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the Dictionary(TKey,TValue) class that is empty, has the specified initial capacity, and uses the specified IEqualityComparer(T).
        /// </summary>
        /// <param name="capacity">The initial number of elements that the Dictionary(TKey,TValue) can contain.</param>
        /// <param name="comparer">The IEqualityComparer(T) implementation to use when comparing keys, or null to use the default EqualityComparer(T) for the type of the key.</param>
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) { dictionary = new Dictionary<TKey, TValue>(capacity, comparer); }

        /// <summary>
        /// Adds the specified value to the ICollection(T) with the specified key.
        /// </summary>
        /// <param name="item">The KeyValuePair(TKey,TValue) structure representing the key and value to add to the Dictionary(TKey,TValue).</param>
        public void Add(KeyValuePair<TKey, TValue> item) { ((ICollection<KeyValuePair<TKey, TValue>>) dictionary).Add(item); }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        public void Add(TKey key, TValue value) { dictionary.Add(key, value); }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key.</param>
        /// <param name="value">The object to use as the value.</param>
        public void Add(object key, object value) { ((IDictionary) dictionary).Add(key, value); }

        /// <summary>
        /// Removes all keys and values from the Dictionary(TKey,TValue).
        /// </summary>
        public void Clear() { dictionary.Clear(); }

        /// <summary>
        /// Determines whether the ICollection(T) contains a specific key and value.
        /// </summary>
        /// <param name="item">The KeyValuePair(TKey,TValue) structure to locate in the ICollection(T).</param>
        /// <returns>
        /// True if keyValuePair is found in the ICollection(T). 
        /// Otherwise, false.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item) { return ((ICollection<KeyValuePair<TKey, TValue>>) dictionary).Contains(item); }

        /// <summary>
        /// Determines whether the IDictionary contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the IDictionary.</param>
        /// <returns>True if the IDictionary contains an element with the specified key.
        /// Otherwise, false.</returns>
        public bool Contains(object key) { return ((IDictionary) dictionary).Contains(key); }

        /// <summary>
        /// Determines whether the Dictionary(TKey,TValue) contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the Dictionary(TKey,TValue).</param>
        /// <returns>
        /// True if the Dictionary(TKey,TValue) contains an element with the specified key. 
        /// Otherwise, false.
        /// </returns>
        public bool ContainsKey(TKey key) { return dictionary.ContainsKey(key); }

        /// <summary>
        /// Determines whether the Dictionary(TKey,TValue) contains a specific value.
        /// </summary>
        /// <param name="key">The value to locate in the Dictionary(TKey,TValue). The value can be null for reference types.</param>
        /// <returns>
        /// True if the Dictionary(TKey,TValue) contains an element with the specified value.
        /// Otherwise, false.
        /// </returns>
        public bool ContainsValue(TValue value) { return dictionary.ContainsValue(value); }

        /// <summary>
        /// Copies the elements of the ICollection(T) to an array of type KeyValuePair(TKey,TValue), starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array of type KeyValuePair(TKey,TValue) that is the destination of the KeyValuePair(TKey,TValue) elements copied from the ICollection(T). The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { ((ICollection<KeyValuePair<TKey, TValue>>) dictionary).CopyTo(array, arrayIndex); }

        /// <summary>
        /// Removes a key and value from the dictionary.
        /// </summary>
        /// <param name="item">The KeyValuePair(TKey,TValue) structure representing the key and value to remove from the Dictionary(TKey,TValue).</param>
        /// <returns>
        /// True if the key and value represented by keyValuePair is successfully found and removed
        /// Otherwise, false. 
        /// This method returns false if keyValuePair is not found in the ICollection(T).
        /// </returns>
        public bool Remove(KeyValuePair<TKey, TValue> item) { return ((ICollection<KeyValuePair<TKey, TValue>>) dictionary).Remove(item); }

        /// <summary>
        /// Removes the value with the specified key from the Dictionary(TKey,TValue).
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// True if the element is successfully found and removed.
        /// Otherwise, false.
        /// This method returns false if key is not found in the Dictionary(TKey,TValue).
        /// </returns>
        public bool Remove(TKey key) { return dictionary.Remove(key); }

        /// <summary>
        /// Removes the element with the specified key from the IDictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public void Remove(object key) { ((IDictionary) dictionary).Remove(key); }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found.
        /// Otherwise, the default value for the type of the value parameter. 
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// True if the Dictionary(TKey,TValue) contains an element with the specified key.
        /// Otherwise, false.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value) { return dictionary.TryGetValue(key, out value); }

        #endregion

        #region [IReadOnlyDictionary<TKey, TValue> Implementation]

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <value>
        /// The value associated with the specified key.
        /// If the specified key is not found, a get operation throws a KeyNotFoundException,
        /// and a set operation creates a new element with the specified key.
        /// </value>
        public TValue this[TKey key] { get { return ((IDictionary<TKey, TValue>) dictionary)[key]; } set { ((IDictionary<TKey, TValue>) dictionary)[key] = value; } }

        #endregion

        #region [IDictionary Implementation]

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <value>
        /// The value associated with the specified key,
        /// or null if key is not in the dictionary
        /// or key is of a type that is not assignable to the key type TKey of the Dictionary(TKey,TValue).
        /// </value>
        public object this[object key] { get { return ((IDictionary) dictionary)[key]; } set { ((IDictionary) dictionary)[key] = value; } }

        /// <summary>
        /// Gets a value that indicates whether the IDictionary is read-only.
        /// </summary>
        /// <value>
        /// True if the IDictionary is read-only.
        /// Otherwise, false. 
        /// In the default implementation of Dictionary(TKey,TValue), this property always returns false.
        /// </value>
        public bool IsReadOnly { get { return ((ICollection<KeyValuePair<TKey, TValue>>) dictionary).IsReadOnly; } }

        /// <summary>
        /// Gets a value that indicates whether the IDictionary has a fixed size.
        /// </summary>
        /// <value>
        /// True if the IDictionary has a fixed size.
        /// Otherwise, false. 
        /// In the default implementation of Dictionary(TKey,TValue), this property always returns false.
        /// </value>
        public bool IsFixedSize { get { return ((IDictionary) dictionary).IsFixedSize; } }

        /// <summary>
        /// Gets a collection containing the keys in the Dictionary(TKey,TValue).
        /// </summary>
        /// <value>
        /// A Dictionary(TKey,TValue).KeyCollection containing the keys in the Dictionary(TKey,TValue).
        /// </value>
        public ICollection<TKey> Keys { get { return ((IDictionary<TKey, TValue>) dictionary).Keys; } }

        /// <summary>
        /// Gets a collection containing the values in the Dictionary(TKey,TValue).
        /// </summary>
        /// <value>
        /// A Dictionary(TKey,TValue).ValueCollection containing the values in the Dictionary(TKey,TValue).
        /// </value>
        public ICollection<TValue> Values { get { return ((IDictionary<TKey, TValue>) dictionary).Values; } }

        /// <summary>
        /// Gets an ICollection containing the keys of the IDictionary.
        /// </summary>
        /// <value>An ICollection containing the keys of the IDictionary.</value>
        ICollection IDictionary.Keys { get { return ((IDictionary) dictionary).Keys; } }

        /// <summary>
        /// Gets an ICollection containing the values in the IDictionary.
        /// </summary>
        /// <value>An ICollection containing the values in the IDictionary.</value>
        ICollection IDictionary.Values { get { return ((IDictionary) dictionary).Values; } }

        /// <summary>
        /// Gets a collection containing the keys of the IReadOnlyDictionary(TKey,TValue).
        /// </summary>
        /// <value>A collection containing the keys of the IReadOnlyDictionary(TKey,TValue).</value>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys { get { return ((IReadOnlyDictionary<TKey, TValue>) dictionary).Keys; } }

        /// <summary>
        /// Gets a collection containing the values of the IReadOnlyDictionary(TKey,TValue).
        /// </summary>
        /// <value>A collection containing the values of the IReadOnlyDictionary(TKey,TValue).</value>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values { get { return ((IReadOnlyDictionary<TKey, TValue>) dictionary).Values; } }

        /// <summary>
        /// Returns an IDictionaryEnumerator for the IDictionary.
        /// </summary>
        /// <returns>An IDictionaryEnumerator for the IDictionary.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator() { return ((IDictionary) dictionary).GetEnumerator(); }

        #endregion

        #region [ICollection / IReadOnlyCollection<KeyValuePair<TKey, TValue>> / ICollection<KeyValuePair<TKey, TValue>> Implementation]

        /// <summary>
        /// Gets the number of key/value pairs contained in the Dictionary(TKey,TValue).
        /// </summary>
        /// <value>The number of key/value pairs contained in the Dictionary(TKey,TValue).</value>
        public int Count { get { return dictionary.Count; } }

        /// <summary>
        /// Gets a value that indicates whether access to the ICollection is synchronized (thread safe).
        /// </summary>
        /// <value>
        /// true if access to the ICollection is synchronized (thread safe).
        /// Otherwise, false. 
        /// In the default implementation of Dictionary(TKey,TValue), this property always returns false.
        /// </value>
        public bool IsSynchronized { get { return ((ICollection) dictionary).IsSynchronized; } }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the ICollection.
        /// </summary>
        /// <value>An object that can be used to synchronize access to the ICollection.</value>
        public object SyncRoot { get { return ((ICollection) dictionary).SyncRoot; } }

        /// <summary>
        /// Copies the elements of the ICollection(T) to an array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from ICollection(T). The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index) { ((ICollection) dictionary).CopyTo(array, index); }

        #endregion

        #region [IEnumerable<KeyValuePair<TKey, TValue>> / IEnumerable Implementation]

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return ((ICollection<KeyValuePair<TKey, TValue>>) dictionary).GetEnumerator(); }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return ((ICollection<KeyValuePair<TKey, TValue>>) dictionary).GetEnumerator(); }

        #endregion

        #region [ISerializable Implementation]

        /// <summary>
        /// Implements the ISerializable interface and returns the data needed to serialize the Dictionary(TKey,TValue) instance.
        /// </summary>
        /// <param name="info">A SerializationInfo object that contains the information required to serialize the Dictionary(TKey,TValue) instance.</param>
        /// <param name="context">A StreamingContext structure that contains the source and destination of the serialized stream associated with the Dictionary(TKey,TValue) instance.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context) { ((ISerializable) dictionary).GetObjectData(info, context); }

        #endregion

        #region [IDeserializationCallback Implementation]

        /// <summary>
        /// Implements the ISerializable interface and raises the deserialization event when the deserialization is complete.
        /// </summary>
        /// <param name="sender">The source of the deserialization event.</param>
        public void OnDeserialization(object sender) { ((IDeserializationCallback) dictionary).OnDeserialization(sender); }

        #endregion

        #region [Getter / Setter]

        /// <summary>
        /// Serialized Dictionary reference key.
        /// </summary>
        protected TKey GetKey(int index) { return GetKeys()[index]; }

        /// <summary>
        /// Serialized Dictionary reference value.
        /// </summary>
        protected TValue GetValue(int index) { return GetValues()[index]; }

        /// <summary>
        /// Set serialized Dictionary reference key.
        /// </summary>
        protected void SetKey(int index, TKey key) { GetKeys()[index] = key; }

        /// <summary>
        /// Set serialized Dictionary reference value.
        /// </summary>
        protected void SetValue(int index, TValue value) { GetValues()[index] = value; }

        /// <summary>
        /// Serialized Dictionary reference key length.
        /// </summary>
        protected int GetKeyLength() { return GetKeys().Count; }

        /// <summary>
        /// Serialized Dictionary reference value length.
        /// </summary>
        /// <returns></returns>
        protected int GetValueLength() { return GetValues().Count; }

        #endregion
    }
}