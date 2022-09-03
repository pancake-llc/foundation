using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Networking;

// ReSharper disable RedundantTypeArgumentsOfMethod
#endif

namespace Pancake.SaveData
{
    public static class Archive
    {
        #region Archive.Save

        // <summary>Saves the value to the default file with the given key.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        public static void Save(string key, object value) { Save<object>(key, value, new MetaData()); }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our value to.</param>
        public static void Save(string key, object value, string filePath) { Save<object>(key, value, new MetaData(filePath)); }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our value to.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void Save(string key, object value, string filePath, MetaData metadata) { Save<object>(key, value, new MetaData(filePath, metadata)); }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void Save(string key, object value, MetaData metadata) { Save<object>(key, value, metadata); }

        /// <summary>Saves the value to the default file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to save.</typeparam>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        public static void Save<T>(string key, T value) { Save<T>(key, value, new MetaData()); }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to save.</typeparam>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our value to.</param>
        public static void Save<T>(string key, T value, string filePath) { Save<T>(key, value, new MetaData(filePath)); }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to save.</typeparam>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our value to.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void Save<T>(string key, T value, string filePath, MetaData metadata) { Save<T>(key, value, new MetaData(filePath, metadata)); }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to save.</typeparam>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void Save<T>(string key, T value, MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache)
            {
                ArchiveFile.GetOrCreateCachedFile(metadata).Save(key, value);
                return;
            }

            using (var writer = Writer.Create(metadata))
            {
                writer.Write<T>(key, value);
                writer.Save();
            }
        }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="bytes">The bytes we want to store.</param>
        public static void SaveRaw(byte[] bytes) { SaveRaw(bytes, new MetaData()); }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="bytes">The bytes we want to store.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our bytes to.</param>
        public static void SaveRaw(byte[] bytes, string filePath) { SaveRaw(bytes, new MetaData(filePath)); }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="bytes">The bytes we want to store.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our bytes to.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void SaveRaw(byte[] bytes, string filePath, MetaData metadata) { SaveRaw(bytes, new MetaData(filePath, metadata)); }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="bytes">The bytes we want to store.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void SaveRaw(byte[] bytes, MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache)
            {
                ArchiveFile.GetOrCreateCachedFile(metadata).SaveRaw(bytes, metadata);
                return;
            }

            using (var stream = ArchiveStream.CreateStream(metadata, EFileMode.Write))
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            IO.CommitBackup(metadata);
        }

        /// <summary>Creates or overwrites the default file with the specified raw bytes.</summary>
        /// <param name="str">The string we want to store.</param>
        public static void SaveRaw(string str) { SaveRaw(str, new MetaData()); }

        /// <summary>Creates or overwrites the default file with the specified raw bytes.</summary>
        /// <param name="str">The string we want to store.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our bytes to.</param>
        public static void SaveRaw(string str, string filePath) { SaveRaw(str, new MetaData(filePath)); }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="str">The string we want to store.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our bytes to.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void SaveRaw(string str, string filePath, MetaData metadata) { SaveRaw(str, new MetaData(filePath, metadata)); }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="str">The string we want to store.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void SaveRaw(string str, MetaData metadata)
        {
            var bytes = metadata.encoding.GetBytes(str);
            SaveRaw(bytes, metadata);
        }

        /// <summary>Creates or appends the specified bytes to a file.</summary>
        /// <param name="bytes">The bytes we want to append.</param>
        public static void AppendRaw(byte[] bytes) { AppendRaw(bytes, new MetaData()); }

        /// <summary>Creates or appends the specified bytes to a file.</summary>
        /// <param name="bytes">The bytes we want to append.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to append our bytes to.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void AppendRaw(byte[] bytes, string filePath, MetaData metadata) { AppendRaw(bytes, new MetaData(filePath, metadata)); }

        /// <summary>Creates or appends the specified bytes to a file.</summary>
        /// <param name="bytes">The bytes we want to append.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void AppendRaw(byte[] bytes, MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache)
            {
                ArchiveFile.GetOrCreateCachedFile(metadata).AppendRaw(bytes);
                return;
            }

            MetaData data = new MetaData(metadata.path, metadata) {encryptionType = EEncryptionType.None, compressionType = ECompressionType.None};

            using (var stream = ArchiveStream.CreateStream(data, EFileMode.Append))
                stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>Creates or appends the specified bytes to the default file.</summary>
        /// <param name="str"></param>
        public static void AppendRaw(string str) { AppendRaw(str, new MetaData()); }

        /// <summary>Creates or appends the specified bytes to a file.</summary>
        /// <param name="str"></param>
        /// <param name="filePath">The relative or absolute path of the file we want to append our bytes to.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void AppendRaw(string str, string filePath, MetaData metadata) { AppendRaw(str, new MetaData(filePath, metadata)); }

        /// <summary>Creates or appends the specified bytes to a file.</summary>
        /// <param name="str"></param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void AppendRaw(string str, MetaData metadata)
        {
            var bytes = metadata.encoding.GetBytes(str);
            MetaData newSettings = new MetaData(metadata.path, metadata);
            newSettings.encryptionType = EEncryptionType.None;
            newSettings.compressionType = ECompressionType.None;

            if (metadata.Location == ELocation.Cache)
            {
                ArchiveFile.GetOrCreateCachedFile(metadata).SaveRaw(bytes);
                return;
            }

            using (var stream = ArchiveStream.CreateStream(newSettings, EFileMode.Append))
                stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to create.</param>
        public static void SaveImage(Texture2D texture, string imagePath) { SaveImage(texture, new MetaData(imagePath)); }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to create.</param>
        /// <param name="metadata"></param>
        public static void SaveImage(Texture2D texture, string imagePath, MetaData metadata) { SaveImage(texture, new MetaData(imagePath, metadata)); }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void SaveImage(Texture2D texture, MetaData metadata) { SaveImage(texture, 75, metadata); }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to create.</param>
        /// <param name="quality"></param>
        public static void SaveImage(Texture2D texture, int quality, string imagePath) { SaveImage(texture, new MetaData(imagePath)); }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="quality"></param>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to create.</param>
        /// <param name="metadata"></param>
        public static void SaveImage(Texture2D texture, int quality, string imagePath, MetaData metadata)
        {
            SaveImage(texture, quality, new MetaData(imagePath, metadata));
        }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="quality"></param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void SaveImage(Texture2D texture, int quality, MetaData metadata)
        {
            // Get the file extension to determine what format we want to save the image as.
            string extension = IO.GetExtension(metadata.path).ToLower();
            if (string.IsNullOrEmpty(extension))
                throw new System.ArgumentException("File path must have a file extension when using Archive.SaveImage.");
            byte[] bytes;
            if (extension == ".jpg" || extension == ".jpeg")
                bytes = texture.EncodeToJPG(quality);
            else if (extension == ".png")
                bytes = texture.EncodeToPNG();
            else
                throw new System.ArgumentException("File path must have extension of .png, .jpg or .jpeg when using Archive.SaveImage.");

            Archive.SaveRaw(bytes, metadata);
        }

        #endregion

        #region Archive.Load<T>

        /* Standard load methods */

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        public static object Load(string key) { return Load<object>(key, new MetaData()); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        public static object Load(string key, string filePath) { return Load<object>(key, new MetaData(filePath)); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static object Load(string key, string filePath, MetaData metadata) { return Load<object>(key, new MetaData(filePath, metadata)); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static object Load(string key, MetaData metadata) { return Load<object>(key, metadata); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        public static T Load<T>(string key) { return Load<T>(key, new MetaData()); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        public static T Load<T>(string key, string filePath) { return Load<T>(key, new MetaData(filePath)); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static T Load<T>(string key, string filePath, MetaData metadata) { return Load<T>(key, new MetaData(filePath, metadata)); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static T Load<T>(string key, MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache)
                return ArchiveFile.GetOrCreateCachedFile(metadata).Load<T>(key);

            using (var reader = Reader.Create(metadata))
            {
                if (reader == null)
                    throw new System.IO.FileNotFoundException("File \"" + metadata.FullPath + "\" could not be found.");
                return reader.Read<T>(key);
            }
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        public static T Load<T>(string key, T defaultValue) { return Load<T>(key, defaultValue, new MetaData()); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        public static T Load<T>(string key, string filePath, T defaultValue) { return Load<T>(key, defaultValue, new MetaData(filePath)); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static T Load<T>(string key, string filePath, T defaultValue, MetaData metadata) { return Load<T>(key, defaultValue, new MetaData(filePath, metadata)); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static T Load<T>(string key, T defaultValue, MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache)
                return ArchiveFile.GetOrCreateCachedFile(metadata).Load<T>(key, defaultValue);

            using (var reader = Reader.Create(metadata))
            {
                if (reader == null)
                    return defaultValue;
                return reader.Read<T>(key, defaultValue);
            }
        }

        /* Self-assigning load methods */

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new Instance.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public static void LoadInto<T>(string key, object obj) where T : class { LoadInto<object>(key, obj, new MetaData()); }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new Instance.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public static void LoadInto(string key, string filePath, object obj) { LoadInto<object>(key, obj, new MetaData(filePath)); }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new Instance.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void LoadInto(string key, string filePath, object obj, MetaData metadata) { LoadInto<object>(key, obj, new MetaData(filePath, metadata)); }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new Instance.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void LoadInto(string key, object obj, MetaData metadata) { LoadInto<object>(key, obj, metadata); }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new Instance.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public static void LoadInto<T>(string key, T obj) where T : class { LoadInto<T>(key, obj, new MetaData()); }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new Instance.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public static void LoadInto<T>(string key, string filePath, T obj) where T : class { LoadInto<T>(key, obj, new MetaData(filePath)); }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new Instance.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void LoadInto<T>(string key, string filePath, T obj, MetaData metadata) where T : class { LoadInto<T>(key, obj, new MetaData(filePath, metadata)); }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new Instance.</summary>
        /// <typeparam name="T">The type of the data that we want to load.</typeparam>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void LoadInto<T>(string key, T obj, MetaData metadata) where T : class
        {
            if (Reflection.IsValueType(obj.GetType()))
                throw new InvalidOperationException(
                    "Archive.LoadInto can only be used with reference types, but the data you're loading is a value type. Use Archive.Load instead.");

            if (metadata.Location == ELocation.Cache)
            {
                ArchiveFile.GetOrCreateCachedFile(metadata).LoadInto<T>(key, obj);
                return;
            }

            //if (metadata == null) metadata = new MetaData();
            using (var reader = Reader.Create(metadata))
            {
                if (reader == null)
                    throw new System.IO.FileNotFoundException("File \"" + metadata.FullPath + "\" could not be found.");
                reader.ReadInto<T>(key, obj);
            }
        }

        /* LoadString method, as this can be difficult with overloads. */

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string LoadString(string key, string defaultValue, MetaData metadata) { return Load<string>(key, null, defaultValue, metadata); }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string LoadString(string key, string defaultValue, string filePath = null, MetaData metadata = null)
        {
            return Load<string>(key, filePath, defaultValue, metadata);
        }

        #endregion

        #region Other Archive.Load Methods

        /// <summary>Loads the default file as a byte array.</summary>
        public static byte[] LoadRawBytes() { return LoadRawBytes(new MetaData()); }

        /// <summary>Loads a file as a byte array.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to load as a byte array.</param>
        public static byte[] LoadRawBytes(string filePath) { return LoadRawBytes(new MetaData(filePath)); }

        /// <summary>Loads a file as a byte array.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to load as a byte array.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static byte[] LoadRawBytes(string filePath, MetaData metadata) { return LoadRawBytes(new MetaData(filePath, metadata)); }

        /// <summary>Loads the default file as a byte array.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static byte[] LoadRawBytes(MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache) return ArchiveFile.GetOrCreateCachedFile(metadata).LoadRawBytes();

            using (var stream = ArchiveStream.CreateStream(metadata, EFileMode.Read))
            {
                if (stream == null)
                    throw new System.IO.FileNotFoundException("File " + metadata.path + " could not be found");

                if (stream.GetType() == typeof(System.IO.Compression.GZipStream))
                {
                    var gZipStream = (System.IO.Compression.GZipStream) stream;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        ArchiveStream.CopyTo(gZipStream, ms);
                        return ms.ToArray();
                    }
                }
                else
                {
                    var bytes = new byte[stream.Length];
                    var _ = stream.Read(bytes, 0, bytes.Length);
                    return bytes;
                }
            }
        }

        /// <summary>Loads the default file as a byte array.</summary>
        public static string LoadRawString() { return LoadRawString(new MetaData()); }

        /// <summary>Loads a file as a byte array.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to load as a byte array.</param>
        public static string LoadRawString(string filePath) { return LoadRawString(new MetaData(filePath)); }

        /// <summary>Loads a file as a byte array.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to load as a byte array.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string LoadRawString(string filePath, MetaData metadata) { return LoadRawString(new MetaData(filePath, metadata)); }

        /// <summary>Loads the default file as a byte array.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string LoadRawString(MetaData metadata)
        {
            var bytes = Archive.LoadRawBytes(metadata);
            return metadata.encoding.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>Loads a PNG or JPG as a Texture2D.</summary>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to load as a Texture2D.</param>
        public static Texture2D LoadImage(string imagePath) { return LoadImage(new MetaData(imagePath)); }

        /// <summary>Loads a PNG or JPG as a Texture2D.</summary>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to load as a Texture2D.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static Texture2D LoadImage(string imagePath, MetaData metadata) { return LoadImage(new MetaData(imagePath, metadata)); }

        /// <summary>Loads a PNG or JPG as a Texture2D.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static Texture2D LoadImage(MetaData metadata)
        {
            byte[] bytes = Archive.LoadRawBytes(metadata);
            return LoadImage(bytes);
        }

        /// <summary>Loads a PNG or JPG as a Texture2D.</summary>
        /// <param name="bytes">The raw bytes of the PNG or JPG.</param>
        public static Texture2D LoadImage(byte[] bytes)
        {
            var texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            return texture;
        }

        /// <summary>Loads an audio file as an AudioClip. Note that MP3 files are not supported on standalone platforms and Ogg Vorbis files are not supported on mobile platforms.</summary>
        /// <param name="audioFilePath">The relative or absolute path of the audio file we want to load as an AudioClip.</param>
        /// <param name="audioType"></param>
        public static AudioClip LoadAudio(
            string audioFilePath
#if UNITY_2018_3_OR_NEWER
            ,
            AudioType audioType
#endif
        )
        {
            return LoadAudio(audioFilePath,
#if UNITY_2018_3_OR_NEWER
                audioType,
#endif
                new MetaData());
        }

        /// <summary>Loads an audio file as an AudioClip. Note that MP3 files are not supported on standalone platforms and Ogg Vorbis files are not supported on mobile platforms.</summary>
        /// <param name="audioFilePath">The relative or absolute path of the audio file we want to load as an AudioClip.</param>
        /// <param name="audioType"></param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static AudioClip LoadAudio(
            string audioFilePath,
#if UNITY_2018_3_OR_NEWER
            AudioType audioType,
#endif
            MetaData metadata)
        {
            if (metadata.Location != ELocation.File)
                throw new InvalidOperationException("Archive.LoadAudio can only be used with the File save location");

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                throw new InvalidOperationException("You cannot use Archive.LoadAudio with WebGL");

            string extension = IO.GetExtension(audioFilePath).ToLower();

            if (extension == ".mp3" && (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer))
                throw new System.InvalidOperationException("You can only load Ogg, WAV, XM, IT, MOD or S3M on Unity Standalone");

            if (extension == ".ogg" && (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android ||
                                        Application.platform == RuntimePlatform.WSAPlayerARM))
                throw new System.InvalidOperationException("You can only load MP3, WAV, XM, IT, MOD or S3M on Unity Standalone");

            var newSettings = new MetaData(audioFilePath, metadata);

#if UNITY_2018_3_OR_NEWER
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + newSettings.FullPath, audioType))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    // Wait for it to load.
                }

                if (IsNetworkError(www)) throw new System.Exception(www.error);
                return DownloadHandlerAudioClip.GetContent(www);
            }
#elif UNITY_2017_1_OR_NEWER
		WWW www = new WWW(newSettings.FullPath);

		while(!www.isDone)
		{
		// Wait for it to load.
		}

		if(!string.IsNullOrEmpty(www.error))
			throw new System.Exception(www.error);
#else
		WWW www = new WWW("file://"+newSettings.FullPath);

		while(!www.isDone)
		{
			// Wait for it to load.
		}

		if(!string.IsNullOrEmpty(www.error))
			throw new System.Exception(www.error);
#endif

#if UNITY_2017_3_OR_NEWER && !UNITY_2018_3_OR_NEWER
		return www.GetAudioClip(true);
#elif UNITY_5_6_OR_NEWER && !UNITY_2018_3_OR_NEWER
		return WWWAudioExtensions.GetAudioClip(www);
#endif
        }

        public static bool IsNetworkError(UnityWebRequest www)
        {
#if UNITY_2020_1_OR_NEWER
            return www.result == UnityWebRequest.Result.ConnectionError;
#else
            return www.isNetworkError;
#endif
        }

        #endregion

        #region Serialize/Deserialize

        public static byte[] Serialize<T>(T value, MetaData metadata = null) { return Serialize(value, TypeManager.GetOrCreateCustomType(typeof(T)), metadata); }

        internal static byte[] Serialize(object value, CustomType type, MetaData metadata = null)
        {
            if (metadata == null) metadata = new MetaData();

            using (var ms = new System.IO.MemoryStream())
            {
                using (var stream = ArchiveStream.CreateStream(ms, metadata, EFileMode.Write))
                {
                    using (var baseWriter = Writer.Create(stream, metadata, false, false))
                    {
                        // If T is object, use the value to get it's type. Otherwise, use T so that it works with inheritence.
                        //var type = typeof(T) != typeof(object) ? typeof(T) : (value == null ? typeof(T) : value.GetType());
                        baseWriter.Write(value, type);
                    }

                    return ms.ToArray();
                }
            }
        }

        public static T Deserialize<T>(byte[] bytes, MetaData metadata = null) { return (T) Deserialize(TypeManager.GetOrCreateCustomType(typeof(T)), bytes, metadata); }

        internal static object Deserialize(CustomType type, byte[] bytes, MetaData metadata = null)
        {
            if (metadata == null) metadata = new MetaData();

            using (var ms = new System.IO.MemoryStream(bytes, false))
            using (var stream = ArchiveStream.CreateStream(ms, metadata, EFileMode.Read))
            using (var reader = Reader.Create(stream, metadata, false))
                return reader.Read<object>(type);
        }

        public static void DeserializeInto<T>(byte[] bytes, T obj, MetaData metadata = null) where T : class
        {
            DeserializeInto(TypeManager.GetOrCreateCustomType(typeof(T)), bytes, obj, metadata);
        }

        public static void DeserializeInto<T>(CustomType type, byte[] bytes, T obj, MetaData metadata = null) where T : class
        {
            if (metadata == null)
                metadata = new MetaData();

            using (var ms = new System.IO.MemoryStream(bytes, false))
            using (var reader = Reader.Create(ms, metadata, false))
                reader.ReadInto<T>(obj, type);
        }

        #endregion

        #region Other Archive Methods

        public static byte[] EncryptBytes(byte[] bytes, string password = null)
        {
            if (string.IsNullOrEmpty(password))
                password = MetaData.Default.encryptionPassword;
            return new AESEncryptionAlgorithm().Encrypt(bytes, password, MetaData.Default.bufferSize);
        }

        public static byte[] DecryptBytes(byte[] bytes, string password = null)
        {
            if (string.IsNullOrEmpty(password))
                password = MetaData.Default.encryptionPassword;
            return new AESEncryptionAlgorithm().Decrypt(bytes, password, MetaData.Default.bufferSize);
        }

        public static string EncryptString(string str, string password = null)
        {
            return MetaData.Default.encoding.GetString(EncryptBytes(MetaData.Default.encoding.GetBytes(str), password));
        }

        public static string DecryptString(string str, string password = null)
        {
            return MetaData.Default.encoding.GetString(DecryptBytes(MetaData.Default.encoding.GetBytes(str), password));
        }

        /// <summary>Deletes the default file.</summary>
        public static void DeleteFile() { DeleteFile(new MetaData()); }

        /// <summary>Deletes the file at the given path using the default settings.</summary>
        /// <param name="filePath">The relative or absolute path of the file we wish to delete.</param>
        public static void DeleteFile(string filePath) { DeleteFile(new MetaData(filePath)); }

        /// <summary>Deletes the file at the given path using the settings provided.</summary>
        /// <param name="filePath">The relative or absolute path of the file we wish to delete.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void DeleteFile(string filePath, MetaData metadata) { DeleteFile(new MetaData(filePath, metadata)); }

        /// <summary>Deletes the file specified by the MetaData object provided as a parameter.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void DeleteFile(MetaData metadata)
        {
            if (metadata.Location == ELocation.File) IO.DeleteFile(metadata.FullPath);
            else if (metadata.Location == ELocation.PlayerPrefs) PlayerPrefs.DeleteKey(metadata.FullPath);
            else if (metadata.Location == ELocation.Cache) ArchiveFile.RemoveCachedFile(metadata);
        }

        /// <summary>Copies a file from one path to another.</summary>
        /// <param name="oldFilePath">The relative or absolute path of the file we want to copy.</param>
        /// <param name="newFilePath">The relative or absolute path of the copy we want to create.</param>
        public static void CopyFile(string oldFilePath, string newFilePath) { CopyFile(new MetaData(oldFilePath), new MetaData(newFilePath)); }

        /// <summary>Copies a file from one location to another, using the MetaData provided to override any default settings.</summary>
        /// <param name="oldFilePath">The relative or absolute path of the file we want to copy.</param>
        /// <param name="newFilePath">The relative or absolute path of the copy we want to create.</param>
        /// <param name="oldSettings">The settings we want to use when copying the old file.</param>
        /// <param name="newSettings">The settings we want to use when creating the new file.</param>
        public static void CopyFile(string oldFilePath, string newFilePath, MetaData oldSettings, MetaData newSettings)
        {
            CopyFile(new MetaData(oldFilePath, oldSettings), new MetaData(newFilePath, newSettings));
        }

        /// <summary>Copies a file from one location to another, using the MetaData provided to determine the locations.</summary>
        /// <param name="oldSettings">The settings we want to use when copying the old file.</param>
        /// <param name="newSettings">The settings we want to use when creating the new file.</param>
        public static void CopyFile(MetaData oldSettings, MetaData newSettings)
        {
            if (oldSettings.Location != newSettings.Location)
                throw new InvalidOperationException("Cannot copy file from " + oldSettings.Location + " to " + newSettings.Location +
                                                    ". Location must be the same for both source and destination.");

            if (oldSettings.Location == ELocation.File)
            {
                if (IO.FileExists(oldSettings.FullPath))
                {
                    IO.DeleteFile(newSettings.FullPath);
                    IO.CopyFile(oldSettings.FullPath, newSettings.FullPath);
                }
            }
            else if (oldSettings.Location == ELocation.PlayerPrefs)
            {
                PlayerPrefs.SetString(newSettings.FullPath, PlayerPrefs.GetString(oldSettings.FullPath));
            }
            else if (oldSettings.Location == ELocation.Cache)
            {
                ArchiveFile.CopyCachedFile(oldSettings, newSettings);
            }
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldFilePath">The relative or absolute path of the file we want to rename.</param>
        /// <param name="newFilePath">The relative or absolute path we want to rename the file to.</param>
        public static void RenameFile(string oldFilePath, string newFilePath) { RenameFile(new MetaData(oldFilePath), new MetaData(newFilePath)); }

        /// <summary>Renames a file.</summary>
        /// <param name="oldFilePath">The relative or absolute path of the file we want to rename.</param>
        /// <param name="newFilePath">The relative or absolute path we want to rename the file to.</param>
        /// <param name="oldSettings">The settings for the file we want to rename.</param>
        /// <param name="newSettings">The settings for the file we want our source file to be renamed to.</param>
        public static void RenameFile(string oldFilePath, string newFilePath, MetaData oldSettings, MetaData newSettings)
        {
            RenameFile(new MetaData(oldFilePath, oldSettings), new MetaData(newFilePath, newSettings));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldSettings">The settings for the file we want to rename.</param>
        /// <param name="newSettings">The settings for the file we want our source file to be renamed to.</param>
        public static void RenameFile(MetaData oldSettings, MetaData newSettings)
        {
            if (oldSettings.Location != newSettings.Location)
                throw new InvalidOperationException("Cannot rename file in " + oldSettings.Location + " to " + newSettings.Location +
                                                    ". Location must be the same for both source and destination.");

            if (oldSettings.Location == ELocation.File)
            {
                if (IO.FileExists(oldSettings.FullPath))
                {
                    IO.DeleteFile(newSettings.FullPath);
                    IO.MoveFile(oldSettings.FullPath, newSettings.FullPath);
                }
            }
            else if (oldSettings.Location == ELocation.PlayerPrefs)
            {
                PlayerPrefs.SetString(newSettings.FullPath, PlayerPrefs.GetString(oldSettings.FullPath));
                PlayerPrefs.DeleteKey(oldSettings.FullPath);
            }
            else if (oldSettings.Location == ELocation.Cache)
            {
                ArchiveFile.CopyCachedFile(oldSettings, newSettings);
                ArchiveFile.RemoveCachedFile(oldSettings);
            }
        }

        /// <summary>Copies a file from one path to another.</summary>
        /// <param name="oldDirectoryPath">The relative or absolute path of the directory we want to copy.</param>
        /// <param name="newDirectoryPath">The relative or absolute path of the copy we want to create.</param>
        public static void CopyDirectory(string oldDirectoryPath, string newDirectoryPath)
        {
            CopyDirectory(new MetaData(oldDirectoryPath), new MetaData(newDirectoryPath));
        }

        /// <summary>Copies a file from one location to another, using the MetaData provided to override any default settings.</summary>
        /// <param name="oldDirectoryPath">The relative or absolute path of the directory we want to copy.</param>
        /// <param name="newDirectoryPath">The relative or absolute path of the copy we want to create.</param>
        /// <param name="oldSettings">The settings we want to use when copying the old directory.</param>
        /// <param name="newSettings">The settings we want to use when creating the new directory.</param>
        public static void CopyDirectory(string oldDirectoryPath, string newDirectoryPath, MetaData oldSettings, MetaData newSettings)
        {
            CopyDirectory(new MetaData(oldDirectoryPath, oldSettings), new MetaData(newDirectoryPath, newSettings));
        }

        /// <summary>Copies a file from one location to another, using the MetaData provided to determine the locations.</summary>
        /// <param name="oldSettings">The settings we want to use when copying the old file.</param>
        /// <param name="newSettings">The settings we want to use when creating the new file.</param>
        public static void CopyDirectory(MetaData oldSettings, MetaData newSettings)
        {
            if (oldSettings.Location != ELocation.File)
                throw new InvalidOperationException("Archive.CopyDirectory can only be used when the save location is 'File'");

            if (!DirectoryExists(oldSettings))
                throw new System.IO.DirectoryNotFoundException("Directory " + oldSettings.FullPath + " not found");

            if (!DirectoryExists(newSettings))
                IO.CreateDirectory(newSettings.FullPath);

            foreach (var fileName in Archive.GetFiles(oldSettings))
                CopyFile(IO.CombinePathAndFilename(oldSettings.path, fileName), IO.CombinePathAndFilename(newSettings.path, fileName));

            foreach (var directoryName in GetDirectories(oldSettings))
                CopyDirectory(IO.CombinePathAndFilename(oldSettings.path, directoryName), IO.CombinePathAndFilename(newSettings.path, directoryName));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldDirectoryPath">The relative or absolute path of the file we want to rename.</param>
        /// <param name="newDirectoryPath">The relative or absolute path we want to rename the file to.</param>
        public static void RenameDirectory(string oldDirectoryPath, string newDirectoryPath)
        {
            RenameDirectory(new MetaData(oldDirectoryPath), new MetaData(newDirectoryPath));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldDirectoryPath">The relative or absolute path of the file we want to rename.</param>
        /// <param name="newDirectoryPath">The relative or absolute path we want to rename the file to.</param>
        /// <param name="oldSettings">The settings for the file we want to rename.</param>
        /// <param name="newSettings">The settings for the file we want our source file to be renamed to.</param>
        public static void RenameDirectory(string oldDirectoryPath, string newDirectoryPath, MetaData oldSettings, MetaData newSettings)
        {
            RenameDirectory(new MetaData(oldDirectoryPath, oldSettings), new MetaData(newDirectoryPath, newSettings));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldSettings">The settings for the file we want to rename.</param>
        /// <param name="newSettings">The settings for the file we want our source file to be renamed to.</param>
        public static void RenameDirectory(MetaData oldSettings, MetaData newSettings)
        {
            if (oldSettings.Location == ELocation.File)
            {
                if (IO.DirectoryExists(oldSettings.FullPath))
                {
                    IO.DeleteDirectory(newSettings.FullPath);
                    IO.MoveDirectory(oldSettings.FullPath, newSettings.FullPath);
                }
            }
            else if (oldSettings.Location == ELocation.PlayerPrefs || oldSettings.Location == ELocation.Cache)
                throw new System.NotSupportedException("Directories cannot be renamed when saving to Cache, PlayerPrefs, tvOS or using WebGL.");
        }

        /// <summary>Deletes the directory at the given path using the settings provided.</summary>
        /// <param name="directoryPath">The relative or absolute path of the folder we wish to delete.</param>
        public static void DeleteDirectory(string directoryPath) { DeleteDirectory(new MetaData(directoryPath)); }

        /// <summary>Deletes the directory at the given path using the settings provided.</summary>
        /// <param name="directoryPath">The relative or absolute path of the folder we wish to delete.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void DeleteDirectory(string directoryPath, MetaData metadata) { DeleteDirectory(new MetaData(directoryPath, metadata)); }

        /// <summary>Deletes the directory at the given path using the settings provided.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void DeleteDirectory(MetaData metadata)
        {
            if (metadata.Location == ELocation.File)
                IO.DeleteDirectory(metadata.FullPath);
            else if (metadata.Location == ELocation.PlayerPrefs || metadata.Location == ELocation.Cache)
                throw new System.NotSupportedException("Deleting Directories using Cache or PlayerPrefs is not supported.");
        }

        /// <summary>Deletes a key in the default file.</summary>
        /// <param name="key">The key we want to delete.</param>
        public static void DeleteKey(string key) { DeleteKey(key, new MetaData()); }

        public static void DeleteKey(string key, string filePath) { DeleteKey(key, new MetaData(filePath)); }

        /// <summary>Deletes a key in the file specified.</summary>
        /// <param name="key">The key we want to delete.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to delete the key from.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void DeleteKey(string key, string filePath, MetaData metadata) { DeleteKey(key, new MetaData(filePath, metadata)); }

        /// <summary>Deletes a key in the file specified by the MetaData object.</summary>
        /// <param name="key">The key we want to delete.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void DeleteKey(string key, MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache)
                ArchiveFile.DeleteKey(key, metadata);
            else if (Archive.FileExists(metadata))
            {
                using (var writer = Writer.Create(metadata))
                {
                    writer.MarkKeyForDeletion(key);
                    writer.Save();
                }
            }
        }

        /// <summary>Checks whether a key exists in the default file.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <returns>True if the key exists, otherwise False.</returns>
        public static bool KeyExists(string key) { return KeyExists(key, new MetaData()); }

        /// <summary>Checks whether a key exists in the specified file.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to search.</param>
        /// <returns>True if the key exists, otherwise False.</returns>
        public static bool KeyExists(string key, string filePath) { return KeyExists(key, new MetaData(filePath)); }

        /// <summary>Checks whether a key exists in the default file.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to search.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>True if the key exists, otherwise False.</returns>
        public static bool KeyExists(string key, string filePath, MetaData metadata) { return KeyExists(key, new MetaData(filePath, metadata)); }

        /// <summary>Checks whether a key exists in a file.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool KeyExists(string key, MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache)
                return ArchiveFile.KeyExists(key, metadata);

            using (var reader = Reader.Create(metadata))
            {
                if (reader == null)
                    return false;
                return reader.Goto(key);
            }
        }

        /// <summary>Checks whether the default file exists.</summary>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool FileExists() { return FileExists(new MetaData()); }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to check the existence of.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool FileExists(string filePath) { return FileExists(new MetaData(filePath)); }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to check the existence of.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool FileExists(string filePath, MetaData metadata) { return FileExists(new MetaData(filePath, metadata)); }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool FileExists(MetaData metadata)
        {
            if (metadata.Location == ELocation.File) return IO.FileExists(metadata.FullPath);
            if (metadata.Location == ELocation.PlayerPrefs) return PlayerPrefs.HasKey(metadata.FullPath);
            if (metadata.Location == ELocation.Cache) return ArchiveFile.FileExists(metadata);

            return false;
        }

        /// <summary>Checks whether a folder exists.</summary>
        /// <param name="folderPath">The relative or absolute path of the folder we want to check the existence of.</param>
        /// <returns>True if the folder exists, otherwise False.</returns>
        public static bool DirectoryExists(string folderPath) { return DirectoryExists(new MetaData(folderPath)); }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="folderPath">The relative or absolute path of the folder we want to check the existence of.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>True if the folder exists, otherwise False.</returns>
        public static bool DirectoryExists(string folderPath, MetaData metadata) { return DirectoryExists(new MetaData(folderPath, metadata)); }

        /// <summary>Checks whether a folder exists.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>True if the folder exists, otherwise False.</returns>
        public static bool DirectoryExists(MetaData metadata)
        {
            if (metadata.Location == ELocation.File)
                return IO.DirectoryExists(metadata.FullPath);
            else if (metadata.Location == ELocation.PlayerPrefs || metadata.Location == ELocation.Cache)
                throw new System.NotSupportedException("Directories are not supported for the Cache and PlayerPrefs location.");
            return false;
        }

        /// <summary>Gets an array of all of the key names in the default file.</summary>
        public static string[] GetKeys() { return GetKeys(new MetaData()); }

        /// <summary>Gets an array of all of the key names in a file.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to get the key names from.</param>
        public static string[] GetKeys(string filePath) { return GetKeys(new MetaData(filePath)); }

        /// <summary>Gets an array of all of the key names in a file.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to get the key names from.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string[] GetKeys(string filePath, MetaData metadata) { return GetKeys(new MetaData(filePath, metadata)); }

        /// <summary>Gets an array of all of the key names in a file.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string[] GetKeys(MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache)
                return ArchiveFile.GetKeys(metadata);

            var keys = new List<string>();
            using (var reader = Reader.Create(metadata))
            {
                foreach (string key in reader.Properties)
                {
                    keys.Add(key);
                    reader.Skip();
                }
            }

            return keys.ToArray();
        }

        /// <summary>Gets an array of all of the file names in a directory.</summary>
        public static string[] GetFiles()
        {
            var settings = new MetaData();
            if (settings.Location == ELocation.File)
            {
                if (settings.eDirectory == EDirectory.PersistentDataPath)
                    settings.path = Application.persistentDataPath;
                else
                    settings.path = Application.dataPath;
            }

            return GetFiles(settings);
        }

        /// <summary>Gets an array of all of the file names in a directory.</summary>
        /// <param name="directoryPath">The relative or absolute path of the directory we want to get the file names from.</param>
        public static string[] GetFiles(string directoryPath) { return GetFiles(new MetaData(directoryPath)); }

        /// <summary>Gets an array of all of the file names in a directory.</summary>
        /// <param name="directoryPath">The relative or absolute path of the directory we want to get the file names from.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string[] GetFiles(string directoryPath, MetaData metadata) { return GetFiles(new MetaData(directoryPath, metadata)); }

        /// <summary>Gets an array of all of the file names in a directory.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string[] GetFiles(MetaData metadata)
        {
            if (metadata.Location == ELocation.Cache) return ArchiveFile.GetFiles();
            if (metadata.Location != ELocation.File)
                throw new System.NotSupportedException("Archive.GetFiles can only be used when the location is set to File or Cache.");
            return IO.GetFiles(metadata.FullPath, false);
        }

        /// <summary>Gets an array of all of the sub-directory names in a directory.</summary>
        public static string[] GetDirectories() { return GetDirectories(new MetaData()); }

        /// <summary>Gets an array of all of the sub-directory names in a directory.</summary>
        /// <param name="directoryPath">The relative or absolute path of the directory we want to get the sub-directory names from.</param>
        public static string[] GetDirectories(string directoryPath) { return GetDirectories(new MetaData(directoryPath)); }

        /// <summary>Gets an array of all of the sub-directory names in a directory.</summary>
        /// <param name="directoryPath">The relative or absolute path of the directory we want to get the sub-directory names from.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string[] GetDirectories(string directoryPath, MetaData metadata) { return GetDirectories(new MetaData(directoryPath, metadata)); }

        /// <summary>Gets an array of all of the sub-directory names in a directory.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static string[] GetDirectories(MetaData metadata)
        {
            if (metadata.Location != ELocation.File)
                throw new System.NotSupportedException("Archive.GetDirectories can only be used when the location is set to File.");
            return IO.GetDirectories(metadata.FullPath, false);
        }

        /// <summary>Creates a backup of the default file .</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        public static void CreateBackup() { CreateBackup(new MetaData()); }

        /// <summary>Creates a backup of a file.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        /// <param name="filePath">The relative or absolute path of the file we wish to create a backup of.</param>
        public static void CreateBackup(string filePath) { CreateBackup(new MetaData(filePath)); }

        /// <summary>Creates a backup of a file.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        /// <param name="filePath">The relative or absolute path of the file we wish to create a backup of.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void CreateBackup(string filePath, MetaData metadata) { CreateBackup(new MetaData(filePath, metadata)); }

        /// <summary>Creates a backup of a file.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public static void CreateBackup(MetaData metadata)
        {
            var backupSettings = new MetaData(metadata.path + IO.BACKUP_FILE_SUFFIX, metadata);
            Archive.CopyFile(metadata, backupSettings);
        }

        /// <summary>Restores a backup of a file.</summary>
        /// <param name="filePath">The relative or absolute path of the file we wish to restore the backup of.</param>
        /// <returns>True if a backup was restored, or False if no backup could be found.</returns>
        public static bool RestoreBackup(string filePath) { return RestoreBackup(new MetaData(filePath)); }

        /// <summary>Restores a backup of a file.</summary>
        /// <param name="filePath">The relative or absolute path of the file we wish to restore the backup of.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>True if a backup was restored, or False if no backup could be found.</returns>
        public static bool RestoreBackup(string filePath, MetaData metadata) { return RestoreBackup(new MetaData(filePath, metadata)); }

        /// <summary>Restores a backup of a file.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>True if a backup was restored, or False if no backup could be found.</returns>
        public static bool RestoreBackup(MetaData metadata)
        {
            var backupSettings = new MetaData(metadata.path + IO.BACKUP_FILE_SUFFIX, metadata);

            if (!FileExists(backupSettings))
                return false;

            Archive.RenameFile(backupSettings, metadata);

            return true;
        }

        public static DateTime GetTimestamp() { return GetTimestamp(new MetaData()); }

        public static DateTime GetTimestamp(string filePath) { return GetTimestamp(new MetaData(filePath)); }

        public static DateTime GetTimestamp(string filePath, MetaData metadata) { return GetTimestamp(new MetaData(filePath, metadata)); }

        /// <summary>Gets the date and time the file was last updated, in the UTC timezone.</summary>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        /// <returns>A DateTime object represeting the UTC date and time the file was last updated.</returns>
        public static DateTime GetTimestamp(MetaData metadata)
        {
            if (metadata.Location == ELocation.File)
                return IO.GetTimestamp(metadata.FullPath);
            else if (metadata.Location == ELocation.PlayerPrefs)
                return new DateTime(long.Parse(PlayerPrefs.GetString("timestamp_" + metadata.FullPath, "0")), DateTimeKind.Utc);
            else if (metadata.Location == ELocation.Cache)
                return ArchiveFile.GetTimestamp(metadata);
            else
                return new DateTime(1970,
                    1,
                    1,
                    0,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);
        }

        /// <summary>Stores the default cached file to persistent archive.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        public static void StoreCachedFile() { ArchiveFile.Store(); }

        /// <summary>Stores a cached file to persistent archive.</summary>
        /// <param name="filePath">The filename or path of the file we want to store the cached file to.</param>
        public static void StoreCachedFile(string filePath) { StoreCachedFile(new MetaData(filePath)); }

        /// <summary>Creates a backup of a file.</summary>
        /// <param name="filePath">The filename or path of the file we want to store the cached file to.</param>
        /// <param name="metadata">The settings of the file we want to store to.</param>
        public static void StoreCachedFile(string filePath, MetaData metadata) { StoreCachedFile(new MetaData(filePath, metadata)); }

        /// <summary>Stores a cached file to persistent archive.</summary>
        /// <param name="metadata">The settings of the file we want to store to.</param>
        public static void StoreCachedFile(MetaData metadata) { ArchiveFile.Store(metadata); }

        /// <summary>Loads the default file in persistent archive into the cache.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        public static void CacheFile() { CacheFile(new MetaData()); }

        /// <summary>Loads a file from persistent archive into the cache.</summary>
        /// <param name="filePath">The filename or path of the file we want to store the cached file to.</param>
        public static void CacheFile(string filePath) { CacheFile(new MetaData(filePath)); }

        /// <summary>Creates a backup of a file.</summary>
        /// <param name="filePath">The filename or path of the file we want to store the cached file to.</param>
        /// <param name="metadata">The settings of the file we want to store to.</param>
        public static void CacheFile(string filePath, MetaData metadata) { CacheFile(new MetaData(filePath, metadata)); }

        /// <summary>Stores a cached file to persistent archive.</summary>
        /// <param name="metadata">The settings of the file we want to store to.</param>
        public static void CacheFile(MetaData metadata) { ArchiveFile.CacheFile(metadata); }

        /// <summary>Initialises Save Data. This happens automatically when any Archive methods are called, but is useful if you want to perform initialisation before calling an Archive method.</summary>
        public static void Init()
        {
            var settings = MetaData.Default;
            TypeManager.Init();
        }

        #endregion
    }
}