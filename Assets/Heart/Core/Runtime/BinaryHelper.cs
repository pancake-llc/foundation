using System;
using System.IO;

namespace Pancake
{
    /// <summary>
    /// Provides methods for easy use of <see cref="System.IO.BinaryWriter"/> and <see cref="System.IO.BinaryReader"/>
    /// </summary>
    public static class BinaryHelper
    {
        private static readonly byte[] CachedBytes = new byte[byte.MaxValue + 1];

        /// <summary>
        /// Reads an encoded 32-bit signed integer from a binary stream.
        /// </summary>
        /// <param name="binaryReader">The binary stream to read.</param>
        /// <returns>32-bit signed integer to read.</returns>
        public static int Read7BitEncodedInt32(this BinaryReader binaryReader)
        {
            int value = 0;
            int shift = 0;
            byte b;
            do
            {
                if (shift >= 35) throw new Exception("7 bit encoded int is invalid.");

                b = binaryReader.ReadByte();
                value |= (b & 0x7f) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return value;
        }

        /// <summary>
        /// Writes an encoded 32-bit signed integer to a binary stream.
        /// </summary>
        /// <param name="binaryWriter">The binary stream to write.</param>
        /// <param name="value">32-bit signed integer to write.</param>
        public static void Write7BitEncodedInt32(this BinaryWriter binaryWriter, int value)
        {
            uint num = (uint) value;
            while (num >= 0x80)
            {
                binaryWriter.Write((byte) (num | 0x80));
                num >>= 7;
            }

            binaryWriter.Write((byte) num);
        }

        /// <summary>
        /// Reads an encoded 32-bit unsigned integer from a binary stream.
        /// </summary>
        /// <param name="binaryReader">The binary stream to read.</param>
        /// <returns>32-bit unsigned integer to read.</returns>
        public static uint Read7BitEncodedUInt32(this BinaryReader binaryReader) { return (uint) Read7BitEncodedInt32(binaryReader); }

        /// <summary>
        /// Writes an encoded 32-bit unsigned integer to a binary stream.
        /// </summary>
        /// <param name="binaryWriter">The binary stream to write.</param>
        /// <param name="value">32-bit unsigned integer to write.</param>
        public static void Write7BitEncodedUInt32(this BinaryWriter binaryWriter, uint value) { Write7BitEncodedInt32(binaryWriter, (int) value); }

        /// <summary>
        /// Reads an encoded 64-bit signed integer from a binary stream.
        /// </summary>
        /// <param name="binaryReader">The binary stream to read.</param>
        /// <returns>64-bit signed integer to read.</returns>
        public static long Read7BitEncodedInt64(this BinaryReader binaryReader)
        {
            long value = 0L;
            int shift = 0;
            byte b;
            do
            {
                if (shift >= 70) throw new Exception("7 bit encoded int is invalid.");

                b = binaryReader.ReadByte();
                value |= (b & 0x7fL) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return value;
        }

        /// <summary>
        /// Writes an encoded 64-bit signed integer to a binary stream.
        /// </summary>
        /// <param name="binaryWriter">The binary stream to write.</param>
        /// <param name="value">64-bit signed integer to write.</param>
        public static void Write7BitEncodedInt64(this BinaryWriter binaryWriter, long value)
        {
            ulong num = (ulong) value;
            while (num >= 0x80)
            {
                binaryWriter.Write((byte) (num | 0x80));
                num >>= 7;
            }

            binaryWriter.Write((byte) num);
        }

        /// <summary>
        /// Reads an encoded 64-bit unsigned integer from a binary stream.
        /// </summary>
        /// <param name="binaryReader">The binary stream to read.</param>
        /// <returns>64-bit unsigned integer to read.</returns>
        public static ulong Read7BitEncodedUInt64(this BinaryReader binaryReader) { return (ulong) Read7BitEncodedInt64(binaryReader); }

        /// <summary>
        /// Writes an encoded 64-bit unsigned integer to a binary stream.
        /// </summary>
        /// <param name="binaryWriter">The binary stream to write.</param>
        /// <param name="value">The 64-bit unsigned integer to write.</param>
        public static void Write7BitEncodedUInt64(this BinaryWriter binaryWriter, ulong value) { Write7BitEncodedInt64(binaryWriter, (long) value); }

        /// <summary>
        /// Read an encrypted string from a binary stream.
        /// </summary>
        /// <param name="binaryReader">The binary stream to read.</param>
        /// <param name="encryptBytes">array of keys.</param>
        /// <returns>The string to read.</returns>
        public static string ReadEncryptedString(this BinaryReader binaryReader, byte[] encryptBytes)
        {
            byte length = binaryReader.ReadByte();
            if (length <= 0)
            {
                return null;
            }

            if (length > byte.MaxValue) throw new Exception("String is too long.");

            for (byte i = 0; i < length; i++)
            {
                CachedBytes[i] = binaryReader.ReadByte();
            }

            C.Encrypt.GetSelfXorBytes(CachedBytes, 0, length, encryptBytes);
            string value = C.Convert.GetString(CachedBytes, 0, length);
            Array.Clear(CachedBytes, 0, length);
            return value;
        }

        /// <summary>
        /// Writes an encrypted string to a binary stream.
        /// </summary>
        /// <param name="binaryWriter">The binary stream to write.</param>
        /// <param name="value">The string to write.</param>
        /// <param name="encryptBytes">array of keys.</param>
        public static void WriteEncryptedString(this BinaryWriter binaryWriter, string value, byte[] encryptBytes)
        {
            if (string.IsNullOrEmpty(value))
            {
                binaryWriter.Write((byte) 0);
                return;
            }

            int length = C.Convert.GetBytes(value, CachedBytes);
            if (length > byte.MaxValue) throw new Exception(string.Format("String '{0}' is too long.", value));

            C.Encrypt.GetSelfXorBytes(CachedBytes, encryptBytes);
            binaryWriter.Write((byte) length);
            binaryWriter.Write(CachedBytes, 0, length);
        }
    }
}