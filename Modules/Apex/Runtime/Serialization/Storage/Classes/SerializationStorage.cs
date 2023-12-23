using UnityEngine;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    /// <summary>
    /// Serialization storage wrapper of generic type.
    /// </summary>
    /// <typeparam name="T">Type of storage.</typeparam>
    public class SerializationStorage<T> : SerializationStorageBase, ISerializationStorage<T>
    {
        [SerializeField] protected T storageData;

        /// <summary>
        /// Get storage data.
        /// </summary>
        /// <returns>Storage data type of (T)</returns>
        public T GetStorageData() { return storageData; }

        /// <summary>
        /// Set storage data.
        /// </summary>
        /// <param name="value">Storage data type of (T)</param>
        public void SetStorageData(T value) { storageData = value; }
    }
}