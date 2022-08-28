using System;
using System.Security.Cryptography;
using System.Text;

namespace Pancake.Core
{
    public sealed class SHA
    {
        #region SHA1

        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <returns></returns>
        public static string A1(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            using (var sha1 = new SHA1CryptoServiceProvider()) //new SHA1CryptoServiceProvider() faster than using SHA1.Create()
            {
                var strSha1Out = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strSha1Out = strSha1Out.Replace("-", "");
                return strSha1Out;
            }
        }

        #endregion

        #region SHA256

        /// <summary>
        /// SHA256 encrypt
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <returns></returns>
        public static string A256(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            using (var sha256 = new SHA256CryptoServiceProvider()) //new SHA256CryptoServiceProvider() faster than using SHA256.Create()
            {
                var strSha256Out = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strSha256Out = strSha256Out.Replace("-", "");
                return strSha256Out;
            }
        }

        #endregion

        #region SHA384

        /// <summary>
        /// SHA384 encrypt
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <returns></returns>
        public static string A384(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            using (var sha384 = new SHA384CryptoServiceProvider()) //new SHA384CryptoServiceProvider() faster than using SHA384.Create()
            {
                var strSha384Out = BitConverter.ToString(sha384.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strSha384Out = strSha384Out.Replace("-", "");
                return strSha384Out;
            }
        }

        #endregion

        #region SHA512

        /// <summary>
        /// SHA512 encrypt
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <returns></returns>
        public static string A512(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            using (var sha512 = new SHA512CryptoServiceProvider()) //new SHA512CryptoServiceProvider() faster than using SHA512.Create()
            {
                var strSha512Out = BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strSha512Out = strSha512Out.Replace("-", "");
                return strSha512Out;
            }
        }

        #endregion
    }
}