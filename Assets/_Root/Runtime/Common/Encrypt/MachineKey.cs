using System.Security.Cryptography;
using System.Text;

namespace Pancake
{
    public sealed class MachineKey
    {
        /// <summary>
        /// Create decryptionKey
        /// </summary>
        /// <param name="length">decryption key length range from 16 to 48</param>
        /// <returns>DecryptionKey</returns>
        public static string DecryptionKey(int length)
        {
            if (length < 16 || length > 48)
            {
                throw new System.ArgumentOutOfRangeException(nameof(length));
            }

            return CreateMachineKey(length);
        }

        /// <summary>
        /// Create validationKey
        /// </summary>
        /// <param name="length">validation key length range from 48 to 128</param>
        /// <returns>ValidationKey</returns>
        public static string ValidationKey(int length)
        {
            if (length < 48 || length > 128)
            {
                throw new System.ArgumentOutOfRangeException(nameof(length));
            }

            return CreateMachineKey(length);
        }

        /// <summary>
        /// Use cryptographic service provider to implement encryption to generate random numbers
        /// 
        /// description
        /// The value of validationKey can be 48 to 128 characters long. It is strongly recommended to use the longest key available.
        /// The value of decryptionKey can be 16 to 48 characters long. It is recommended to use 48 characters long.
        /// 
        /// use:
        /// string decryptionKey = MachineKey.CreateMachineKey(48);
        /// string validationKey = MachineKey.CreateMachineKey(128);
        /// </summary>
        /// <param name="length">length</param>
        /// <returns></returns>
        private static string CreateMachineKey(int length)
        {
            var buffer = new byte[length / 2];

            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);

            var hexBuilder = new StringBuilder(length);
            for (var i = 0; i < buffer.Length; i++)
            {
                hexBuilder.Append($"{buffer[i]:X2}");
            }

            return hexBuilder.ToString();
        }
    }
}