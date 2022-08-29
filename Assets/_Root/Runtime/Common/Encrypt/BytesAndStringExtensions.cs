using System.Text;

namespace Pancake
{
    internal static class BytesAndStringExtensions
    {
        /// <summary>
        /// byte to hex string extension
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        internal static string ToHexString(this byte[] bytes)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// hex string to byte extension
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        internal static byte[] ToBytes(this string hex)
        {
            if (hex.Length == 0)
            {
                return new byte[] {0};
            }

            if (hex.Length % 2 == 1)
            {
                hex = "0" + hex;
            }

            var result = new byte[hex.Length / 2];
            for (var i = 0; i < hex.Length / 2; i++)
            {
                result[i] = byte.Parse(hex.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            return result;
        }
    }
}