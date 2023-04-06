using System;
using System.Text;

namespace Pancake
{
    public static partial class C
    {
        public class Convert
        {
            /// <summary>
            /// Gets the specified Boolean value as a byte array.
            /// </summary>
            /// <param name="value">The boolean value to convert.</param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(bool value)
            {
                byte[] buffer = new byte[1];
                GetBytes(value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified Boolean value as a byte array.
            /// </summary>
            /// <param name="value">The boolean value to convert.</param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static void GetBytes(bool value, byte[] buffer) { GetBytes(value, buffer, 0); }

            /// <summary>
            /// Gets the specified Boolean value as a byte array.
            /// </summary>
            /// <param name="value">The boolean value to convert.</param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static void GetBytes(bool value, byte[] buffer, int startIndex)
            {
                if (buffer == null) throw new Exception("Buffer is invalid.");

                if (startIndex < 0 || startIndex + 1 > buffer.Length) throw new Exception("Start index is invalid.");

                buffer[startIndex] = value ? (byte) 1 : (byte) 0;
            }

            /// <summary>
            /// Returns the Boolean value converted from the first byte in the byte array
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>true if the first byte in value is nonzero, otherwise false.</returns>
            public static bool GetBoolean(byte[] value) { return BitConverter.ToBoolean(value, 0); }

            /// <summary>
            /// Returns the Boolean value converted from a byte at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>true if the byte at the specified position in value is non-zero, otherwise false.</returns>
            public static bool GetBoolean(byte[] value, int startIndex) { return BitConverter.ToBoolean(value, startIndex); }

            /// <summary>
            /// Gets the specified Unicode character value as a byte array.
            /// </summary>
            /// <param name="value">The character to be converted. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(char value)
            {
                byte[] buffer = new byte[2];
                GetBytes((short) value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified Unicode character value as a byte array.
            /// </summary>
            /// <param name="value">The character to be converted. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static void GetBytes(char value, byte[] buffer) { GetBytes((short) value, buffer, 0); }

            /// <summary>
            /// Gets the specified Unicode character value as a byte array.
            /// </summary>
            /// <param name="value">The character to be converted. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static void GetBytes(char value, byte[] buffer, int startIndex) { GetBytes((short) value, buffer, startIndex); }

            /// <summary>
            /// Returns the Unicode character converted from the first two bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>A character consisting of two bytes. </returns>
            public static char GetChar(byte[] value) { return BitConverter.ToChar(value, 0); }

            /// <summary>
            /// Returns the Unicode character converted from the two bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>A character consisting of two bytes. </returns>
            public static char GetChar(byte[] value, int startIndex) { return BitConverter.ToChar(value, startIndex); }

            /// <summary>
            /// Gets the specified 16-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(short value)
            {
                byte[] buffer = new byte[2];
                GetBytes(value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified 16-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static void GetBytes(short value, byte[] buffer) { GetBytes(value, buffer, 0); }

            /// <summary>
            /// Gets the specified 16-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static unsafe void GetBytes(short value, byte[] buffer, int startIndex)
            {
                if (buffer == null) throw new Exception("Buffer is invalid.");

                if (startIndex < 0 || startIndex + 2 > buffer.Length) throw new Exception("Start index is invalid.");

                fixed (byte* valueRef = buffer)
                {
                    *(short*) (valueRef + startIndex) = value;
                }
            }

            /// <summary>
            /// Returns the 16-bit signed integer converted from the first two bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>A 16-bit signed integer consisting of two bytes. </returns>
            public static short GetInt16(byte[] value) { return BitConverter.ToInt16(value, 0); }

            /// <summary>
            /// Returns the 16-bit signed integer converted from the two bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>A 16-bit signed integer consisting of two bytes. </returns>
            public static short GetInt16(byte[] value, int startIndex) { return BitConverter.ToInt16(value, startIndex); }

            /// <summary>
            /// Gets the specified 16-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(ushort value)
            {
                byte[] buffer = new byte[2];
                GetBytes((short) value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified 16-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static void GetBytes(ushort value, byte[] buffer) { GetBytes((short) value, buffer, 0); }

            /// <summary>
            /// Gets the specified 16-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static void GetBytes(ushort value, byte[] buffer, int startIndex) { GetBytes((short) value, buffer, startIndex); }

            /// <summary>
            /// Returns the 16-bit unsigned integer converted from the first two bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>A 16-bit unsigned integer consisting of two bytes. </returns>
            public static ushort GetUInt16(byte[] value) { return BitConverter.ToUInt16(value, 0); }

            /// <summary>
            /// Returns the 16-bit unsigned integer converted from the two bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>A 16-bit unsigned integer consisting of two bytes. </returns>
            public static ushort GetUInt16(byte[] value, int startIndex) { return BitConverter.ToUInt16(value, startIndex); }

            /// <summary>
            /// Gets the specified 32-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(int value)
            {
                byte[] buffer = new byte[4];
                GetBytes(value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified 32-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static void GetBytes(int value, byte[] buffer) { GetBytes(value, buffer, 0); }

            /// <summary>
            /// Gets the specified 32-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static unsafe void GetBytes(int value, byte[] buffer, int startIndex)
            {
                if (buffer == null) throw new Exception("Buffer is invalid.");

                if (startIndex < 0 || startIndex + 4 > buffer.Length) throw new Exception("Start index is invalid.");

                fixed (byte* valueRef = buffer)
                {
                    *(int*) (valueRef + startIndex) = value;
                }
            }

            /// <summary>
            /// Returns the 32-bit signed integer converted from the first four bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>A 32-bit signed integer consisting of four bytes. </returns>
            public static int GetInt32(byte[] value) { return BitConverter.ToInt32(value, 0); }

            /// <summary>
            /// Returns the 32-bit signed integer converted from the four bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>A 32-bit signed integer consisting of four bytes. </returns>
            public static int GetInt32(byte[] value, int startIndex) { return BitConverter.ToInt32(value, startIndex); }

            /// <summary>
            /// Gets the specified 32-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(uint value)
            {
                byte[] buffer = new byte[4];
                GetBytes((int) value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified 32-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static void GetBytes(uint value, byte[] buffer) { GetBytes((int) value, buffer, 0); }

            /// <summary>
            /// Gets the specified 32-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static void GetBytes(uint value, byte[] buffer, int startIndex) { GetBytes((int) value, buffer, startIndex); }

            /// <summary>
            /// Returns the 32-bit unsigned integer converted from the first four bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>由四个字节构成的 32 位无符号整数。</returns>
            public static uint GetUInt32(byte[] value) { return BitConverter.ToUInt32(value, 0); }

            /// <summary>
            /// Returns the 32-bit unsigned integer converted from the four bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>由四个字节构成的 32 位无符号整数。</returns>
            public static uint GetUInt32(byte[] value, int startIndex) { return BitConverter.ToUInt32(value, startIndex); }

            /// <summary>
            /// Gets the specified 64-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(long value)
            {
                byte[] buffer = new byte[8];
                GetBytes(value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified 64-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static void GetBytes(long value, byte[] buffer) { GetBytes(value, buffer, 0); }

            /// <summary>
            /// Gets the specified 64-bit signed integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static unsafe void GetBytes(long value, byte[] buffer, int startIndex)
            {
                if (buffer == null) throw new Exception("Buffer is invalid.");

                if (startIndex < 0 || startIndex + 8 > buffer.Length) throw new Exception("Start index is invalid.");

                fixed (byte* valueRef = buffer)
                {
                    *(long*) (valueRef + startIndex) = value;
                }
            }

            /// <summary>
            /// Returns the 64-bit signed integer converted from the first eight bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>A 64-bit signed integer consisting of eight bytes. </returns>
            public static long GetInt64(byte[] value) { return BitConverter.ToInt64(value, 0); }

            /// <summary>
            /// Returns the 64-bit signed integer converted from the eight bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>A 64-bit signed integer consisting of eight bytes. </returns>
            public static long GetInt64(byte[] value, int startIndex) { return BitConverter.ToInt64(value, startIndex); }

            /// <summary>
            /// Gets the specified 64-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(ulong value)
            {
                byte[] buffer = new byte[8];
                GetBytes((long) value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified 64-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static void GetBytes(ulong value, byte[] buffer) { GetBytes((long) value, buffer, 0); }

            /// <summary>
            /// Gets the specified 64-bit unsigned integer value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static void GetBytes(ulong value, byte[] buffer, int startIndex) { GetBytes((long) value, buffer, startIndex); }

            /// <summary>
            /// Returns the 64-bit unsigned integer converted from the first eight bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>A 64-bit unsigned integer consisting of eight bytes. </returns>
            public static ulong GetUInt64(byte[] value) { return BitConverter.ToUInt64(value, 0); }

            /// <summary>
            /// Returns the 64-bit unsigned integer converted from the eight bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>A 64-bit unsigned integer consisting of eight bytes. </returns>
            public static ulong GetUInt64(byte[] value, int startIndex) { return BitConverter.ToUInt64(value, startIndex); }

            /// <summary>
            /// Gets the specified single-precision floating-point value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static unsafe byte[] GetBytes(float value)
            {
                byte[] buffer = new byte[4];
                GetBytes(*(int*) &value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified single-precision floating-point value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static unsafe void GetBytes(float value, byte[] buffer) { GetBytes(*(int*) &value, buffer, 0); }

            /// <summary>
            /// Gets the specified single-precision floating-point value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static unsafe void GetBytes(float value, byte[] buffer, int startIndex) { GetBytes(*(int*) &value, buffer, startIndex); }

            /// <summary>
            /// Returns the single-precision floating-point number converted from the first four bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>A single-precision floating-point number composed of four bytes. </returns>
            public static float GetSingle(byte[] value) { return BitConverter.ToSingle(value, 0); }

            /// <summary>
            /// Returns the single-precision floating-point number converted from the four bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>A single-precision floating-point number composed of four bytes. </returns>
            public static float GetSingle(byte[] value, int startIndex) { return BitConverter.ToSingle(value, startIndex); }

            /// <summary>
            /// Gets the specified double-precision floating-point value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static unsafe byte[] GetBytes(double value)
            {
                byte[] buffer = new byte[8];
                GetBytes(*(long*) &value, buffer, 0);
                return buffer;
            }

            /// <summary>
            /// Gets the specified double-precision floating-point value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            public static unsafe void GetBytes(double value, byte[] buffer) { GetBytes(*(long*) &value, buffer, 0); }

            /// <summary>
            /// Gets the specified double-precision floating-point value as a byte array.
            /// </summary>
            /// <param name="value">Number to convert. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            public static unsafe void GetBytes(double value, byte[] buffer, int startIndex) { GetBytes(*(long*) &value, buffer, startIndex); }

            /// <summary>
            /// Returns the double-precision floating-point number converted from the first eight bytes in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>A double-precision floating-point number composed of eight bytes. </returns>
            public static double GetDouble(byte[] value) { return BitConverter.ToDouble(value, 0); }

            /// <summary>
            /// Returns the double-precision floating-point number converted from the eight bytes at the specified position in the byte array.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <returns>A double-precision floating-point number composed of eight bytes. </returns>
            public static double GetDouble(byte[] value, int startIndex) { return BitConverter.ToDouble(value, startIndex); }

            /// <summary>
            /// Get a UTF-8 encoded string as a byte array.
            /// </summary>
            /// <param name="value">The string to be converted. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(string value) { return GetBytes(value, Encoding.UTF8); }

            /// <summary>
            /// Get a UTF-8 encoded string as a byte array.
            /// </summary>
            /// <param name="value">The string to be converted. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <returns>How many bytes are actually filled in the buffer. </returns>
            public static int GetBytes(string value, byte[] buffer) { return GetBytes(value, Encoding.UTF8, buffer, 0); }

            /// <summary>
            /// Get a UTF-8 encoded string as a byte array.
            /// </summary>
            /// <param name="value">The string to be converted. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            /// <returns>How many bytes are actually filled in the buffer. </returns>
            public static int GetBytes(string value, byte[] buffer, int startIndex) { return GetBytes(value, Encoding.UTF8, buffer, startIndex); }

            /// <summary>
            /// Get the specified encoding string as a byte array.
            /// </summary>
            /// <param name="value">The string to be converted. </param>
            /// <param name="encoding">Encoding to use. </param>
            /// <returns>The byte array used to store the result. </returns>
            public static byte[] GetBytes(string value, Encoding encoding)
            {
                if (value == null) throw new Exception("Value is invalid.");

                if (encoding == null) throw new Exception("Encoding is invalid.");

                return encoding.GetBytes(value);
            }

            /// <summary>
            /// Get the specified encoding string as a byte array.
            /// </summary>
            /// <param name="value">The string to be converted. </param>
            /// <param name="encoding">Encoding to use. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <returns>How many bytes are actually filled in the buffer. </returns>
            public static int GetBytes(string value, Encoding encoding, byte[] buffer) { return GetBytes(value, encoding, buffer, 0); }

            /// <summary>
            /// Get the specified encoding string as a byte array.
            /// </summary>
            /// <param name="value">The string to be converted. </param>
            /// <param name="encoding">Encoding to use. </param>
            /// <param name="buffer">The byte array used to store the result. </param>
            /// <param name="startIndex">The starting position in the buffer. </param>
            /// <returns>How many bytes are actually filled in the buffer. </returns>
            public static int GetBytes(string value, Encoding encoding, byte[] buffer, int startIndex)
            {
                if (value == null) throw new Exception("Value is invalid.");

                if (encoding == null) throw new Exception("Encoding is invalid.");

                return encoding.GetBytes(value,
                    0,
                    value.Length,
                    buffer,
                    startIndex);
            }

            /// <summary>
            /// Returns the string converted from the byte array using UTF-8 encoding.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <returns>The converted string. </returns>
            public static string GetString(byte[] value) { return GetString(value, Encoding.UTF8); }

            /// <summary>
            /// Returns the string converted from the byte array using the specified encoding.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="encoding">Encoding to use. </param>
            /// <returns>The converted string. </returns>
            public static string GetString(byte[] value, Encoding encoding)
            {
                if (value == null) throw new Exception("Value is invalid.");

                if (encoding == null) throw new Exception("Encoding is invalid.");

                return encoding.GetString(value);
            }

            /// <summary>
            /// Returns the string converted from the byte array using UTF-8 encoding.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <param name="length">length</param>
            /// <returns>The converted string. </returns>
            public static string GetString(byte[] value, int startIndex, int length) { return GetString(value, startIndex, length, Encoding.UTF8); }

            /// <summary>
            /// Returns the string converted from the byte array using the specified encoding.
            /// </summary>
            /// <param name="value">Byte array. </param>
            /// <param name="startIndex">The starting position in value. </param>
            /// <param name="length">length</param>
            /// <param name="encoding">Encoding to use. </param>
            /// <returns>The converted string. </returns>
            public static string GetString(byte[] value, int startIndex, int length, Encoding encoding)
            {
                if (value == null) throw new Exception("Value is invalid.");

                if (encoding == null) throw new Exception("Encoding is invalid.");

                return encoding.GetString(value, startIndex, length);
            }
        }
    }
}