using System;

namespace Pancake
{
    public static partial class C
    {
        public class Encrypt
        {
            internal const int QUICK_ENCRYPT_LENGTH = 220;

            /// <summary>
            /// Faster version of XORing <paramref name="bytes"/> with <paramref name="code"/>.
            /// </summary>
            /// <param name="bytes">Raw binary stream.</param>
            /// <param name="code">XOR a binary stream.</param>
            /// <returns>XORed binary stream.</returns>
            public static byte[] GetQuickXorBytes(byte[] bytes, byte[] code) { return GetXorBytes(bytes, 0, QUICK_ENCRYPT_LENGTH, code); }

            /// <summary>
            /// Faster version of XORing <paramref name="bytes"/> with <paramref name="code"/>. This method will reuse and overwrite the passed bytes as the return value without allocating additional memory space.
            /// </summary>
            /// <param name="bytes">Raw and XORed binary stream.</param>
            /// <param name="code">XOR a binary stream.</param>
            public static void GetQuickSelfXorBytes(byte[] bytes, byte[] code) { GetSelfXorBytes(bytes, 0, QUICK_ENCRYPT_LENGTH, code); }

            /// <summary>
            /// XOR <paramref name="bytes"/> with <paramref name="code"/>.
            /// </summary>
            /// <param name="bytes">The original binary stream. </param>
            /// <param name="code">XOR binary stream. </param>
            /// <returns>XORed binary stream. </returns>
            public static byte[] GetXorBytes(byte[] bytes, byte[] code)
            {
                if (bytes == null) return null;

                return GetXorBytes(bytes, 0, bytes.Length, code);
            }

            /// <summary>
            /// XOR the <paramref name="bytes"/> with <paramref name="code"/>.. This method will reuse and overwrite the passed bytes as the return value without allocating additional memory space.
            /// </summary>
            /// <param name="bytes">Original and XORed binary stream. </param>
            /// <param name="code">XOR binary stream. </param>
            public static void GetSelfXorBytes(byte[] bytes, byte[] code)
            {
                if (bytes == null) return;

                GetSelfXorBytes(bytes, 0, bytes.Length, code);
            }

            /// <summary>
            /// XOR the <paramref name="bytes"/> with <paramref name="code"/>.
            /// </summary>
            /// <param name="bytes">The original binary stream. </param>
            /// <param name="startIndex">The start position of XOR calculation. </param>
            /// <param name="length">XOR calculation length, if less than 0, calculate the entire binary stream. </param>
            /// <param name="code">XOR binary stream. </param>
            /// <returns>XORed binary stream. </returns>
            public static byte[] GetXorBytes(byte[] bytes, int startIndex, int length, byte[] code)
            {
                if (bytes == null) return null;

                int bytesLength = bytes.Length;
                byte[] results = new byte[bytesLength];
                Array.Copy(bytes,
                    0,
                    results,
                    0,
                    bytesLength);
                GetSelfXorBytes(results, startIndex, length, code);
                return results;
            }

            /// <summary>
            /// XOR the <paramref name="bytes"></paramref> with <paramref name="code"></paramref>. This method will reuse and overwrite the passed bytes as the return value without allocating additional memory space.
            /// </summary>
            /// <param name="bytes">Original and XORed binary stream. </param>
            /// <param name="startIndex">The start position of XOR calculation. </param>
            /// <param name="length">XOR calculation length. </param>
            /// <param name="code">XOR binary stream. </param>
            public static void GetSelfXorBytes(byte[] bytes, int startIndex, int length, byte[] code)
            {
                if (bytes == null) return;

                if (code == null) throw new Exception("Code is invalid.");

                int codeLength = code.Length;
                if (codeLength <= 0) throw new Exception("Code length is invalid.");

                if (startIndex < 0 || length < 0 || startIndex + length > bytes.Length) throw new Exception("Start index or length is invalid.");

                int codeIndex = startIndex % codeLength;
                for (int i = startIndex; i < length; i++)
                {
                    bytes[i] ^= code[codeIndex++];
                    codeIndex %= codeLength;
                }
            }
        }
    }
}