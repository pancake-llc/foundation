using System.Collections.Generic;
using System.IO;
using System;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Pancake.SaveData
{
    public abstract class Writer : IDisposable
    {
        protected HashSet<string> keysToDelete = new HashSet<string>();
        public MetaData metadata;
        internal bool writeHeaderAndFooter;
        internal bool overwriteKeys;

        protected int serializationDepth;

        #region Writer Abstract Methods

        internal abstract void WriteNull();

        internal virtual void StartWriteFile() { serializationDepth++; }

        internal virtual void EndWriteFile() { serializationDepth--; }

        internal virtual void StartWriteObject(string name) { serializationDepth++; }

        internal virtual void EndWriteObject(string name) { serializationDepth--; }

        internal virtual void StartWriteProperty(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            Console.Log("<b>" + name + "</b> (writing property)", null, serializationDepth);
        }

        internal virtual void EndWriteProperty(string name) { }

        internal virtual void StartWriteCollection() { serializationDepth++; }

        internal virtual void EndWriteCollection() { serializationDepth--; }

        internal abstract void StartWriteCollectionItem(int index);
        internal abstract void EndWriteCollectionItem(int index);

        internal abstract void StartWriteDictionary();
        internal abstract void EndWriteDictionary();
        internal abstract void StartWriteDictionaryKey(int index);
        internal abstract void EndWriteDictionaryKey(int index);
        internal abstract void StartWriteDictionaryValue(int index);
        internal abstract void EndWriteDictionaryValue(int index);

        public abstract void Dispose();

        #endregion

        #region Writer Interface abstract methods

        internal abstract void WriteRawProperty(string name, byte[] bytes);

        internal abstract void WritePrimitive(int value);
        internal abstract void WritePrimitive(float value);
        internal abstract void WritePrimitive(bool value);
        internal abstract void WritePrimitive(decimal value);
        internal abstract void WritePrimitive(double value);
        internal abstract void WritePrimitive(long value);
        internal abstract void WritePrimitive(ulong value);
        internal abstract void WritePrimitive(uint value);
        internal abstract void WritePrimitive(byte value);
        internal abstract void WritePrimitive(sbyte value);
        internal abstract void WritePrimitive(short value);
        internal abstract void WritePrimitive(ushort value);
        internal abstract void WritePrimitive(char value);
        internal abstract void WritePrimitive(string value);
        internal abstract void WritePrimitive(byte[] value);

        #endregion

        protected Writer(MetaData metadata, bool writeHeaderAndFooter, bool overwriteKeys)
        {
            this.metadata = metadata;
            this.writeHeaderAndFooter = writeHeaderAndFooter;
            this.overwriteKeys = overwriteKeys;
        }

        /* User-facing methods used when writing randomly-accessible Key-Value pairs. */

        #region Write(key, value) Methods

        internal virtual void Write(string key, Type type, byte[] value)
        {
            StartWriteProperty(key);
            StartWriteObject(key);
            WriteType(type);
            WriteRawProperty("value", value);
            EndWriteObject(key);
            EndWriteProperty(key);
            MarkKeyForDeletion(key);
        }

        /// <summary>Writes a value to the writer with the given key.</summary>
        /// <param name="key">The key which uniquely identifies this value.</param>
        /// <param name="value">The value we want to write.</param>
        public virtual void Write<T>(string key, object value)
        {
            if (typeof(T) == typeof(object))
                Write(value.GetType(), key, value);
            else
                Write(typeof(T), key, value);
        }

        /// <summary>Writes a value to the writer with the given key, using the given type rather than the generic parameter.</summary>
        /// <param name="key">The key which uniquely identifies this value.</param>
        /// <param name="value">The value we want to write.</param>
        /// <param name="type">The type we want to use for the header, and to retrieve an CustomType.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual void Write(Type type, string key, object value)
        {
            StartWriteProperty(key);
            StartWriteObject(key);
            WriteType(type);
            WriteProperty("value", value, TypeManager.GetOrCreateCustomType(type));
            EndWriteObject(key);
            EndWriteProperty(key);
            MarkKeyForDeletion(key);
        }

        #endregion

        #region Write(value) & Write(value, CustomType) Methods

        /// <summary>Writes a value to the writer. Note that this should only be called within an CustomType.</summary>
        /// <param name="value">The value we want to write.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual void Write(object value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            var type = TypeManager.GetOrCreateCustomType(value.GetType());
            Write(value, type);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual void Write(object value, CustomType type)
        {
            // Note that we have to check UnityEngine.Object types for null by casting it first, otherwise
            // it will always return false.
            if (value == null || (Reflection.IsAssignableFrom(typeof(UnityEngine.Object), value.GetType()) && value as UnityEngine.Object == null))
            {
                WriteNull();
                return;
            }

            // Deal with System.Objects
            if (type == null || type.type == typeof(object))
            {
                var valueType = value.GetType();
                type = TypeManager.GetOrCreateCustomType(valueType);

                if (type == null)
                    throw new NotSupportedException("Types of " + valueType + " are not supported.");

                if (!type.isCollection && !type.isDictionary)
                {
                    StartWriteObject(null);
                    WriteType(valueType);

                    type.Write(value, this);

                    EndWriteObject(null);
                    return;
                }
            }

            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (type.isUnsupported)
            {
                if (type.isCollection || type.isDictionary)
                    throw new NotSupportedException(type.type + " is not supported because it's element type is not supported.");
                else
                    throw new NotSupportedException("Types of " + type.type + " are not supported.");
            }

            if (type.isPrimitive || type.isEnum)
                type.Write(value, this);
            else if (type.isCollection)
            {
                StartWriteCollection();
                ((CustomCollectionType) type).Write(value, this);
                EndWriteCollection();
            }
            else if (type.isDictionary)
            {
                StartWriteDictionary();
                ((DictionaryType) type).Write(value, this);
                EndWriteDictionary();
            }
            else
            {
                StartWriteObject(null);
                type.Write(value, this);
                EndWriteObject(null);
            }
        }

        #endregion

        /* Writes a property as a name value pair. */

        #region WriteProperty(name, value) methods

        /// <summary>Writes a field or property to the writer. Note that this should only be called within an CustomType.</summary>
        /// <param name="name">The name of the field or property.</param>
        /// <param name="value">The value we want to write.</param>
        public virtual void WriteProperty(string name, object value)
        {
            if (SerializationDepthLimitExceeded())
                return;

            StartWriteProperty(name);
            Write(value);
            EndWriteProperty(name);
        }

        /// <summary>Writes a field or property to the writer. Note that this should only be called within an CustomType.</summary>
        /// <param name="name">The name of the field or property.</param>
        /// <param name="value">The value we want to write.</param>
        public virtual void WriteProperty<T>(string name, object value) { WriteProperty(name, value, TypeManager.GetOrCreateCustomType(typeof(T))); }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual void WriteProperty(string name, object value, CustomType type)
        {
            if (SerializationDepthLimitExceeded())
                return;

            StartWriteProperty(name);
            Write(value, type);
            EndWriteProperty(name);
        }

        /// <summary>Writes a private property to the writer. Note that this should only be called within an CustomType.</summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="objectContainingProperty">The object containing the property we want to write.</param>
        public void WritePrivateProperty(string name, object objectContainingProperty)
        {
            var property = Reflection.GetReflectedProperty(objectContainingProperty.GetType(), name);
            if (property.IsNull)
                throw new MissingMemberException("A private property named " + name + " does not exist in the type " + objectContainingProperty.GetType());
            WriteProperty(name, property.GetValue(objectContainingProperty), TypeManager.GetOrCreateCustomType(property.MemberType));
        }

        /// <summary>Writes a private field to the writer. Note that this should only be called within an CustomType.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="objectContainingField">The object containing the property we want to write.</param>
        public void WritePrivateField(string name, object objectContainingField)
        {
            var field = Reflection.GetReflectedMember(objectContainingField.GetType(), name);
            if (field.IsNull)
                throw new MissingMemberException("A private field named " + name + " does not exist in the type " + objectContainingField.GetType());
            WriteProperty(name, field.GetValue(objectContainingField), TypeManager.GetOrCreateCustomType(field.MemberType));
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void WritePrivateProperty(string name, object objectContainingProperty, CustomType type)
        {
            var property = Reflection.GetReflectedProperty(objectContainingProperty.GetType(), name);
            if (property.IsNull)
                throw new MissingMemberException("A private property named " + name + " does not exist in the type " + objectContainingProperty.GetType());
            WriteProperty(name, property.GetValue(objectContainingProperty), type);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void WritePrivateField(string name, object objectContainingField, CustomType type)
        {
            var field = Reflection.GetReflectedMember(objectContainingField.GetType(), name);
            if (field.IsNull)
                throw new MissingMemberException("A private field named " + name + " does not exist in the type " + objectContainingField.GetType());
            WriteProperty(name, field.GetValue(objectContainingField), type);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void WriteType(Type type) { WriteProperty(CustomType.TYPE_FIELD_NAME, Reflection.GetTypeString(type)); }

        #endregion

        #region Create methods

        /// <summary>Creates a new Writer.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to write to.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static Writer Create(string filePath, MetaData metadata) { return Create(new MetaData(filePath, metadata)); }

        /// <summary>Creates a new Writer.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static Writer Create(MetaData metadata) { return Create(metadata, true, true, false); }

        // Implicit Stream Methods.
        internal static Writer Create(MetaData metadata, bool writeHeaderAndFooter, bool overwriteKeys, bool append)
        {
            var stream = ArchiveStream.CreateStream(metadata, (append ? EFileMode.Append : EFileMode.Write));
            if (stream == null) return null;
            return Create(stream, metadata, writeHeaderAndFooter, overwriteKeys);
        }

        // Explicit Stream Methods.

        internal static Writer Create(Stream stream, MetaData metadata, bool writeHeaderAndFooter, bool overwriteKeys)
        {
            if (stream.GetType() == typeof(MemoryStream))
            {
                metadata = (MetaData) metadata.Clone();
                metadata.Location = ELocation.InternalMS;
            }

            // Get the baseWriter using the given Stream.
            if (metadata.format == EFormat.Json)
                return new JsonWriter(stream, metadata, writeHeaderAndFooter, overwriteKeys);
            else
                return null;
        }

        #endregion

        /*
         * Checks whether serialization depth limit has been exceeded
         */
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        protected bool SerializationDepthLimitExceeded()
        {
            if (serializationDepth > metadata.serializationDepthLimit)
            {
                Console.LogWarning("Serialization depth limit of " + metadata.serializationDepthLimit +
                                   " has been exceeded, indicating that there may be a circular reference.\nIf this is not a circular reference, you can increase the depth by going to Tools > Pancake > Archive > Settings > Advanced Settings > Serialization Depth Limit");
                return true;
            }

            return false;
        }

        /*
         * 	Marks a key for deletion.
         * 	When merging files, keys marked for deletion will not be included.
         */
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual void MarkKeyForDeletion(string key) { keysToDelete.Add(key); }

        /*
         * 	Merges the contents of the non-temporary file with this Writer,
         * 	ignoring any keys which are marked for deletion.
         */
        protected void Merge()
        {
            using (var reader = Reader.Create(metadata))
            {
                if (reader == null)
                    return;
                Merge(reader);
            }
        }

        /*
         * 	Merges the contents of the Reader with this Writer,
         * 	ignoring any keys which are marked for deletion.
         */
        protected void Merge(Reader reader)
        {
            foreach (KeyValuePair<string, BinaryData> kvp in reader.RawEnumerator)
                if (!keysToDelete.Contains(kvp.Key) || kvp.Value.type == null) // Don't add keys whose data is of a type which no longer exists in the project.
                    Write(kvp.Key, kvp.Value.type.type, kvp.Value.bytes);
        }

        /// <summary>Stores the contents of the writer and overwrites any existing keys if overwriting is enabled.</summary>
        public virtual void Save() { Save(overwriteKeys); }

        /// <summary>Stores the contents of the writer and overwrites any existing keys if overwriting is enabled.</summary>
        /// <param name="overwriteKeys">Whether we should overwrite existing keys.</param>
        public virtual void Save(bool overwriteKeys)
        {
            if (overwriteKeys) Merge();
            EndWriteFile();
            Dispose();

            // If we're writing to a location which can become corrupted, rename the backup file to the file we want.
            // This prevents corrupt data.
            if (metadata.Location == ELocation.File || metadata.Location == ELocation.PlayerPrefs)
                IO.CommitBackup(metadata);
        }
    }
}