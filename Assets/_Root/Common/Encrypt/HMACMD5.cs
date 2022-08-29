using System;
using System.Text;

namespace Pancake
{
    // ReSharper disable once InconsistentNaming
    public class HMACMD5
    {
        /// <summary>
        /// HMACMD5 hash
        /// </summary>
        /// <param name="source">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string Create(string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source), "");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "");
            }

            using (var md5 = new System.Security.Cryptography.HMACMD5(Encoding.UTF8.GetBytes(key)))
            {
                var strMd5Out = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(source)));
                strMd5Out = strMd5Out.Replace("-", "");
                return strMd5Out;
            }
        }
    }
}