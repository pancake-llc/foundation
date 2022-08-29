using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global


namespace Pancake.SaveData
{
    /// <summary>Represents a cached file which can be saved to and loaded from, and commited to archive when necessary.</summary>
    public class ArchiveFile
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static Dictionary<string, ArchiveFile> cachedFiles = new Dictionary<string, ArchiveFile>();

        public MetaData metadata;
        private Dictionary<string, BinaryData> _cache = new Dictionary<string, BinaryData>();
        private bool _syncWithFile;
        private DateTime _timestamp = DateTime.UtcNow;

        /// <summary>Creates a new ArchiveFile and loads the default file into the ArchiveFile if there is data to load.</summary>
        public ArchiveFile()
            : this(new MetaData())
        {
        }

        /// <summary>Creates a new ArchiveFile and loads the specified file into the ArchiveFile if there is data to load.</summary>
        /// <param name="filePath">The relative or absolute path of the file in archive our ArchiveFile is associated with.</param>
        public ArchiveFile(string filePath)
            : this(new MetaData(filePath), true)
        {
        }

        /// <summary>Creates a new ArchiveFile and loads the specified file into the ArchiveFile if there is data to load.</summary>
        /// <param name="filePath">The relative or absolute path of the file in archive our ArchiveFile is associated with.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public ArchiveFile(string filePath, MetaData metadata)
            : this(new MetaData(filePath, metadata), true)
        {
        }

        /// <summary>Creates a new ArchiveFile and only loads the default file into it if syncWithFile is set to true.</summary>
        /// <param name="syncWithFile">Whether we should sync this ArchiveFile with the one in archive immediately after creating it.</param>
        public ArchiveFile(bool syncWithFile)
            : this(new MetaData(), syncWithFile)
        {
        }

        /// <summary>Creates a new ArchiveFile and only loads the specified file into it if syncWithFile is set to true.</summary>
        /// <param name="filePath">The relative or absolute path of the file in archive our ArchiveFile is associated with.</param>
        /// <param name="syncWithFile">Whether we should sync this ArchiveFile with the one in archive immediately after creating it.</param>
        public ArchiveFile(string filePath, bool syncWithFile)
            : this(new MetaData(filePath), syncWithFile)
        {
        }

        /// <summary>Creates a new ArchiveFile and only loads the specified file into it if syncWithFile is set to true.</summary>
        /// <param name="filePath">The relative or absolute path of the file in archive our ArchiveFile is associated with.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <param name="syncWithFile">Whether we should sync this ArchiveFile with the one in archive immediately after creating it.</param>
        public ArchiveFile(string filePath, MetaData metadata, bool syncWithFile)
            : this(new MetaData(filePath, metadata), syncWithFile)
        {
        }

        /// <summary>Creates a new ArchiveFile and loads the specified file into the ArchiveFile if there is data to load.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <param name="syncWithFile">Whether we should sync this ArchiveFile with the one in archive immediately after creating it.</param>
        public ArchiveFile(MetaData metadata, bool syncWithFile = true)
        {
            this.metadata = metadata;
            _syncWithFile = syncWithFile;
            if (syncWithFile)
            {
                // Type checking must be enabled when syncing.
                var settingsWithTypeChecking = (MetaData) metadata.Clone();
                settingsWithTypeChecking.typeChecking = true;

                using (var reader = Reader.Create(settingsWithTypeChecking))
                {
                    if (reader == null) return;
                    foreach (KeyValuePair<string, BinaryData> kvp in reader.RawEnumerator)
                        _cache[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>Creates a new ArchiveFile and loads the bytes into the ArchiveFile. Note the bytes must represent that of a file.</summary>
        /// <param name="bytes">The bytes representing our file.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public ArchiveFile(byte[] bytes, MetaData metadata = null)
        {
            if (metadata == null) this.metadata = new MetaData();
            else this.metadata = metadata;
            SaveRaw(bytes, metadata);
        }

        /// <summary>Synchronises this ArchiveFile with a file in archive.</summary>
        public void Sync() { Sync(metadata); }

        /// <summary>Synchronises this ArchiveFile with a file in archive.</summary>
        /// <param name="filePath">The relative or absolute path of the file in archive we want to synchronise with.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public void Sync(string filePath, MetaData metadata = null) { Sync(new MetaData(filePath, metadata)); }

        /// <summary>Synchronises this ArchiveFile with a file in archive.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public void Sync(MetaData metadata)
        {
            if (metadata == null) metadata = new MetaData();

            Archive.DeleteFile(metadata);

            if (_cache.Count == 0) return;

            using (var baseWriter = Writer.Create(metadata, true, !_syncWithFile, false))
            {
                foreach (var kvp in _cache)
                {
                    // If we change the name of a type, the type may be null.
                    // In this case, use System.Object as the type.
                    Type type;
                    if (kvp.Value.type == null) type = typeof(object);
                    else type = kvp.Value.type.type;
                    baseWriter.Write(kvp.Key, type, kvp.Value.bytes);
                }

                baseWriter.Save(!_syncWithFile);
            }
        }

        /// <summary>Removes the data stored in this ArchiveFile. The ArchiveFile will be empty after calling this method.</summary>
        public void Clear() { _cache.Clear(); }

        /// <summary>Returns an array of all of the key names in this ArchiveFile.</summary>
        public string[] GetKeys()
        {
            var keyCollection = _cache.Keys;
            var keys = new string[keyCollection.Count];
            keyCollection.CopyTo(keys, 0);
            return keys;
        }

        #region Save Methods

        /// <summary>Saves a value to a key in this ArchiveFile.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        public void Save<T>(string key, T value)
        {
            var unencryptedSettings = (MetaData) metadata.Clone();
            unencryptedSettings.encryptionType = EEncryptionType.None;
            unencryptedSettings.compressionType = ECompressionType.None;

            // If T is object, use the value to get it's type. Otherwise, use T so that it works with inheritence.

            _cache[key] = new BinaryData(TypeManager.GetOrCreateCustomType(typeof(T)), Archive.Serialize(value, unencryptedSettings));
        }

        /// <summary>Merges the data specified by the bytes parameter into this ArchiveFile.</summary>
        /// <param name="bytes">The bytes we want to merge with this ArchiveFile.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public void SaveRaw(byte[] bytes, MetaData metadata = null)
        {
            if (metadata == null) metadata = new MetaData();

            // Type checking must be enabled when syncing.
            var settingsWithTypeChecking = (MetaData) metadata.Clone();
            settingsWithTypeChecking.typeChecking = true;

            using (var reader = Reader.Create(bytes, settingsWithTypeChecking))
            {
                if (reader == null) return;
                foreach (KeyValuePair<string, BinaryData> kvp in reader.RawEnumerator)
                    _cache[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>Merges the data specified by the bytes parameter into this ArchiveFile.</summary>
        /// <param name="bytes">The bytes we want to merge with this ArchiveFile.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public void AppendRaw(byte[] bytes, MetaData metadata = null)
        {
            if (metadata == null) metadata = new MetaData();
            // AppendRaw just does the same thing as SaveRaw in ArchiveFile.
            SaveRaw(bytes, metadata);
        }

        #endregion

        #region Load Methods

        /* Standard load methods */

        /// <summary>Loads the value from this ArchiveFile with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        public object Load(string key) { return Load<object>(key); }

        /// <summary>Loads the value from this ArchiveFile with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the key does not exist in this ArchiveFile.</param>
        public object Load(string key, object defaultValue) { return Load<object>(key, defaultValue); }

        /// <summary>Loads the value from this ArchiveFile with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        public T Load<T>(string key)
        {
            if (!_cache.TryGetValue(key, out var data))
                throw new KeyNotFoundException("Key \"" + key +
                                               "\" was not found in this ArchiveFile. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");

            var unencryptedSettings = (MetaData) metadata.Clone();
            unencryptedSettings.encryptionType = EEncryptionType.None;
            unencryptedSettings.compressionType = ECompressionType.None;

            if (typeof(T) == typeof(object)) return (T) Archive.Deserialize(data.type, data.bytes, unencryptedSettings);
            return Archive.Deserialize<T>(data.bytes, unencryptedSettings);
        }

        /// <summary>Loads the value from this ArchiveFile with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the key does not exist in this ArchiveFile.</param>
        public T Load<T>(string key, T defaultValue)
        {
            if (!_cache.TryGetValue(key, out var binaryData)) return defaultValue;
            var unencryptedSettings = (MetaData) metadata.Clone();
            unencryptedSettings.encryptionType = EEncryptionType.None;
            unencryptedSettings.compressionType = ECompressionType.None;

            if (typeof(T) == typeof(object)) return (T) Archive.Deserialize(binaryData.type, binaryData.bytes, unencryptedSettings);
            return Archive.Deserialize<T>(binaryData.bytes, unencryptedSettings);
        }

        /// <summary>Loads the value from this ArchiveFile with the given key into an existing object.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public void LoadInto<T>(string key, T obj) where T : class
        {
            if (!_cache.TryGetValue(key, out var binaryData))
                throw new KeyNotFoundException("Key \"" + key +
                                               "\" was not found in this ArchiveFile. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");

            var unencryptedSettings = (MetaData) metadata.Clone();
            unencryptedSettings.encryptionType = EEncryptionType.None;
            unencryptedSettings.compressionType = ECompressionType.None;

            if (typeof(T) == typeof(object)) Archive.DeserializeInto(binaryData.type, binaryData.bytes, obj, unencryptedSettings);
            else Archive.DeserializeInto(binaryData.bytes, obj, unencryptedSettings);
        }

        #endregion

        #region Load Raw Methods

        /// <summary>Loads the ArchiveFile as a raw, unencrypted, uncompressed byte array.</summary>
        public byte[] LoadRawBytes()
        {
            var unencryptedSettings = (MetaData) metadata.Clone();
            unencryptedSettings.encryptionType = EEncryptionType.None;
            unencryptedSettings.compressionType = ECompressionType.None;
            return GetBytes(unencryptedSettings);
        }

        /// <summary>Loads the ArchiveFile as a raw, unencrypted, uncompressed string, using the encoding defined in the ArchiveFile's settings variable.</summary>
        public string LoadRawString()
        {
            if (_cache.Count == 0) return "";
            return metadata.encoding.GetString(LoadRawBytes());
        }

        /*
         * Same as LoadRawString, except it will return an encrypted/compressed file if these are enabled.
         */
        internal byte[] GetBytes(MetaData metaData = null)
        {
            if (_cache.Count == 0) return new byte[0];

            if (metaData == null) metaData = metadata;

            using (var ms = new MemoryStream())
            {
                var dataClone = (MetaData) metaData.Clone();
                dataClone.Location = ELocation.InternalMS;
                // Ensure we return unencrypted bytes.
                dataClone.encryptionType = EEncryptionType.None;
                dataClone.compressionType = ECompressionType.None;

                using (var baseWriter = Writer.Create(ArchiveStream.CreateStream(ms, dataClone, EFileMode.Write), dataClone, true, false))
                {
                    foreach (var kvp in _cache)
                        baseWriter.Write(kvp.Key, kvp.Value.type.type, kvp.Value.bytes);
                    baseWriter.Save(false);
                }

                return ms.ToArray();
            }
        }

        #endregion

        #region Other Archive Methods

        /// <summary>Deletes a key from this ArchiveFile.</summary>
        /// <param name="key">The key we want to delete.</param>
        public void DeleteKey(string key) { _cache.Remove(key); }

        /// <summary>Checks whether a key exists in this ArchiveFile.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <returns>True if the key exists, otherwise False.</returns>
        public bool KeyExists(string key) { return _cache.ContainsKey(key); }

        /// <summary>Gets the size of the cached data in bytes.</summary>
        public int Size()
        {
            int size = 0;
            foreach (var kvp in _cache)
                size += kvp.Value.bytes.Length;
            return size;
        }

        public Type GetKeyType(string key)
        {
            if (!_cache.TryGetValue(key, out var binaryData))
                throw new KeyNotFoundException("Key \"" + key +
                                               "\" was not found in this ArchiveFile. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");
            return binaryData.type.type;
        }

        #endregion

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static ArchiveFile GetOrCreateCachedFile(MetaData metadata)
        {
            if (!cachedFiles.TryGetValue(metadata.path, out var cachedFile))
            {
                cachedFile = new ArchiveFile(metadata, false);
                cachedFiles.Add(metadata.path, cachedFile);
            }

            // Settings might refer to the same file, but might have changed.
            // To account for this, we update the settings of the ArchiveFile each time we access it.
            cachedFile.metadata = metadata;
            return cachedFile;
        }

        internal static void CacheFile(MetaData metadata)
        {
            // If we're still using cached settings, default to file.
            if (metadata.Location == ELocation.Cache)
            {
                metadata = (MetaData) metadata.Clone();
                metadata.Location = ELocation.File;
            }

            if (!Archive.FileExists(metadata)) return;

            // Disable compression and encryption when loading the raw bytes, and the ArchiveFile constructor will expect encrypted/compressed bytes.
            var loadSettings = (MetaData) metadata.Clone();
            loadSettings.compressionType = ECompressionType.None;
            loadSettings.encryptionType = EEncryptionType.None;

            cachedFiles[metadata.path] = new ArchiveFile(Archive.LoadRawBytes(loadSettings), metadata);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static void Store(MetaData metadata = null)
        {
            if (metadata == null) metadata = new MetaData(ELocation.File);
            // If we're still using cached settings, default to file.
            else if (metadata.Location == ELocation.Cache)
            {
                metadata = (MetaData) metadata.Clone();
                metadata.Location = ELocation.File;
            }

            if (!cachedFiles.TryGetValue(metadata.path, out var cachedFile))
                throw new FileNotFoundException("The file '" + metadata.path + "' could not be stored because it could not be found in the cache.");
            cachedFile.Sync(metadata);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static void RemoveCachedFile(MetaData metadata) { cachedFiles.Remove(metadata.path); }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static void CopyCachedFile(MetaData oldData, MetaData newData)
        {
            if (!cachedFiles.TryGetValue(oldData.path, out var cachedFile))
                throw new FileNotFoundException("The file '" + oldData.path + "' could not be copied because it could not be found in the cache.");
            if (cachedFiles.ContainsKey(newData.path))
                throw new InvalidOperationException("Cannot copy file '" + oldData.path + "' to '" + newData.path + "' because '" + newData.path + "' already exists");

            cachedFiles.Add(newData.path, (ArchiveFile) cachedFile.MemberwiseClone());
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static void DeleteKey(string key, MetaData metadata)
        {
            if (cachedFiles.TryGetValue(metadata.path, out var cachedFile))
                cachedFile.DeleteKey(key);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static bool KeyExists(string key, MetaData metadata)
        {
            if (cachedFiles.TryGetValue(metadata.path, out var cachedFile))
                return cachedFile.KeyExists(key);
            return false;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static bool FileExists(MetaData metadata) { return cachedFiles.ContainsKey(metadata.path); }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static string[] GetKeys(MetaData metadata)
        {
            if (!cachedFiles.TryGetValue(metadata.path, out var cachedFile))
                throw new FileNotFoundException("Could not get keys from the file '" + metadata.path + "' because it could not be found in the cache.");
            return cachedFile._cache.Keys.ToArray();
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static string[] GetFiles() { return cachedFiles.Keys.ToArray(); }

        internal static DateTime GetTimestamp(MetaData metadata)
        {
            if (!cachedFiles.TryGetValue(metadata.path, out var cachedFile))
                return new DateTime(1970,
                    1,
                    1,
                    0,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);
            return cachedFile._timestamp;
        }
    }
}