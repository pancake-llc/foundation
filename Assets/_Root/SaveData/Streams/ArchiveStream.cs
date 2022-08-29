using System.IO;
using System.IO.Compression;
using UnityEngine;
using System;

namespace Pancake.SaveData
{
    public static class ArchiveStream
    {
        public static Stream CreateStream(MetaData metadata, EFileMode fileMode)
        {
            bool isWriteStream = fileMode != EFileMode.Read;
            Stream stream = null;

            // Check that the path is in a valid format. This will throw an exception if not.
            var _ = new FileInfo(metadata.FullPath);

            try
            {
                if (metadata.Location == ELocation.InternalMS)
                {
                    // There's no point in creating an empty MemoryStream if we're only reading from it.
                    if (!isWriteStream) return null;
                    stream = new MemoryStream(metadata.bufferSize);
                }
                else if (metadata.Location == ELocation.File)
                {
                    if (!isWriteStream && !IO.FileExists(metadata.FullPath)) return null;
                    stream = new ArchiveFileStream(metadata.FullPath, fileMode, metadata.bufferSize, false);
                }
                else if (metadata.Location == ELocation.PlayerPrefs)
                {
                    if (isWriteStream) stream = new PlayerPrefsStream(metadata.FullPath, metadata.bufferSize, fileMode == EFileMode.Append);
                    else
                    {
                        if (!PlayerPrefs.HasKey(metadata.FullPath)) return null;
                        stream = new PlayerPrefsStream(metadata.FullPath);
                    }
                }

                return CreateStream(stream, metadata, fileMode);
            }
            catch (Exception)
            {
                if (stream != null) stream.Dispose();
                throw;
            }
        }

        public static Stream CreateStream(Stream stream, MetaData metadata, EFileMode fileMode)
        {
            try
            {
                bool isWriteStream = (fileMode != EFileMode.Read);

#if !DISABLE_ENCRYPTION
                // Encryption
                if (metadata.encryptionType != EEncryptionType.None && stream.GetType() != typeof(UnbufferedCryptoStream))
                {
                    EncryptionAlgorithm alg = null;
                    if (metadata.encryptionType == EEncryptionType.Aes) alg = new AESEncryptionAlgorithm();
                    stream = new UnbufferedCryptoStream(stream,
                        !isWriteStream,
                        metadata.encryptionPassword,
                        metadata.bufferSize,
                        alg);
                }
#endif

                // Compression
                if (metadata.compressionType != ECompressionType.None && stream.GetType() != typeof(GZipStream))
                {
                    if (metadata.compressionType == ECompressionType.Gzip)
                        stream = isWriteStream ? new GZipStream(stream, CompressionMode.Compress) : new GZipStream(stream, CompressionMode.Decompress);
                }

                return stream;
            }
            catch (Exception e)
            {
                if (stream != null) stream.Dispose();
                if (e.GetType() == typeof(System.Security.Cryptography.CryptographicException))
                    throw new System.Security.Cryptography.CryptographicException(
                        "Could not decrypt file. Please ensure that you are using the same password used to encrypt the file.");
                throw;
            }
        }

        public static void CopyTo(Stream source, Stream destination)
        {
#if UNITY_2019_1_OR_NEWER
            source.CopyTo(destination);
#else
            byte[] buffer = new byte[2048];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                destination.Write(buffer, 0, bytesRead);
#endif
        }
    }
}