using System;

using System.Text;

namespace Pancake
{
    // ReSharper disable once InconsistentNaming
    public sealed class HMACSHA
    {
        #region HMACSHA1

        /// <summary>
        /// HMAC_SHA1
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string A1(string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "");
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(key)))
            {
                hmac.Initialize();
                var strHmacOut = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strHmacOut = strHmacOut.Replace("-", "");
                return strHmacOut;
            }
        }

        #endregion

        #region HMACSHA256

        /// <summary>
        /// HMAC_SHA256 
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string A256(string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "");
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                hmac.Initialize();
                var strHmacOut = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strHmacOut = strHmacOut.Replace("-", "");
                return strHmacOut;
            }
        }

        #endregion

        #region HMACSHA384

        /// <summary>
        /// HMAC_SHA384
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string A384(string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "");
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA384(Encoding.UTF8.GetBytes(key)))
            {
                hmac.Initialize();
                var strHmacOut = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strHmacOut = strHmacOut.Replace("-", "");
                return strHmacOut;
            }
        }

        #endregion

        #region HMACSHA512

        /// <summary>
        /// HMAC_SHA512
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string A512(string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "");
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                hmac.Initialize();
                var strHmacOut = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strHmacOut = strHmacOut.Replace("-", "");
                return strHmacOut;
            }
        }

        #endregion
    }
}