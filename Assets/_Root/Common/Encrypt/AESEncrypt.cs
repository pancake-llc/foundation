using System;
using System.IO;
using System.Security.Cryptography;

// ReSharper disable InconsistentNaming
namespace Pancake.Core
{
    public class AESEncrypt
    {
        #region properties

        /// <summary>
        /// Encryption/Decryption password.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Encryption/Decryption bits.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public EBits EncryptionBits { get; }

        public AESKey AesKey { get; }

        /// <summary>
        /// Salt bytes (bytes length must be 15).
        /// </summary>
        public byte[] Salt { get; } = {0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4};

        #endregion

        #region contructor

        /// <summary>
        /// Initialize new AESEncrypt.
        /// </summary>
        /// <param name="password">The password to use for encryption/decryption.</param>
        /// <param name="encryptionBits">Encryption bits (128,192,256).</param>
        public AESEncrypt(string password, EBits encryptionBits)
        {
            Password = password;
            EncryptionBits = encryptionBits;
            var pdb = new Rfc2898DeriveBytes(Password, Salt, 50);
            AesKey = new AESKey(pdb.GetBytes((int) encryptionBits), pdb.GetBytes((int) EBits.Bits128));
        }

        /// <summary>
        /// Initialize new AESEncrypt.
        /// </summary>
        /// <param name="password">The password to use for encryption/decryption.</param>
        /// <param name="encryptionBits">Encryption bits (128,192,256).</param>
        /// <param name="salt">Salt bytes. Bytes length must be 15.</param>
        public AESEncrypt(string password, EBits encryptionBits, byte[] salt)
        {
            Password = password;
            EncryptionBits = encryptionBits;
            Salt = salt;
            var pdb = new Rfc2898DeriveBytes(Password, Salt, 50);
            AesKey = new AESKey(pdb.GetBytes((int) encryptionBits), pdb.GetBytes((int) EBits.Bits128));
        }

        #endregion

        #region encrypt

        private byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
#if DISABLE_ENCRYPTION
			return data;
#else
            if (data == null || data.Length <= 0)
                throw new ArgumentNullException(nameof(data));

            var plain = data;
            using (var aes = new AesCryptoServiceProvider()) // new AesCryptoServiceProvider() faster than Aes.Create()
            {
                byte[] encryptedData;
                try
                {
                    using (var memory = new MemoryStream())
                    {
                        using (var encryptor = new CryptoStream(memory, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                        {
                            encryptor.Write(plain, 0, plain.Length);
                            encryptor.FlushFinalBlock();
                            encryptedData = memory.ToArray();
                        }
                    }
                }
                catch
                {
                    encryptedData = null;
                }

                return encryptedData;
#endif
            }
        }

        /// <summary>
        /// Encrypt byte array with AES algorithm.
        /// </summary>
        /// <param name="data">Bytes to encrypt.</param>
        public byte[] Encrypt(byte[] data) { return Encrypt(data, AesKey.Key, AesKey.Iv); }

        #endregion

        #region decrypt

        private byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
#if DISABLE_ENCRYPTION
     			return data;
#else
            using (var aes = new AesCryptoServiceProvider()) // new AesCryptoServiceProvider() faster than Aes.Create()
            {
                byte[] decryptedData; // decrypted data
                try
                {
                    using (var memory = new MemoryStream(data))
                    {
                        using (var decryptor = new CryptoStream(memory, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            using (var tempMemory = new MemoryStream())
                            {
                                var buffer = new byte[1024];
                                int readBytes;
                                while ((readBytes = decryptor.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    tempMemory.Write(buffer, 0, readBytes);
                                }

                                decryptedData = tempMemory.ToArray();
                            }
                        }
                    }
                }
                catch
                {
                    decryptedData = null;
                }

                return decryptedData;
            }
#endif
        }

        /// <summary>
        /// Decrypt byte array with AES algorithm.
        /// </summary>
        /// <param name="data">Encrypted byte array.</param>
        public byte[] Decrypt(byte[] data) { return Decrypt(data, AesKey.Key, AesKey.Iv); }

        #endregion
    }

    public struct AESKey
    {
        /// <summary>
        /// ase key
        /// </summary>
        public byte[] Key { get; }

        /// <summary>
        /// ase IV
        /// </summary>
        public byte[] Iv { get; }

        public AESKey(byte[] key, byte[] iv)
        {
            Key = key;
            Iv = iv;
        }
    }

    public enum EBits
    {
        Bits128 = 16,
        Bits192 = 24,
        Bits256 = 32
    }
}