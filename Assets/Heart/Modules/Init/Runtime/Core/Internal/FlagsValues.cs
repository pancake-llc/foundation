namespace Sisus.Init
{
    /// <summary>
    /// Helper class that lists all valid values that an int-backed bit field enumeration type can have.
    /// <example>
    /// <code>
    /// using System;
    /// using static Sisus.Init.FlagsValues;
    /// 
    /// [Flags]
    /// public enum MyEnum
    /// {
    ///     None	= _0,
    ///     First	= _1,
    ///     Second	= _2,
    ///     Third	= _3,
    ///     Fourth	= _4,
    ///     Fifth	= _5
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public static class FlagsValues
    {
        /// <summary>
        /// Value of 0.
        /// </summary>
        public const int _0 = 0;

        /// <summary>
        /// Value of 1.
        /// </summary>
        public const int _1 = 1 << 0;

        /// <summary>
        /// Value of 2.
        /// </summary>
        public const int _2 = 1 << 1;

        /// <summary>
        /// Value of 4.
        /// </summary>
        public const int _3 = 1 << 2;

        /// <summary>
        /// Value of 8.
        /// </summary>
        public const int _4 = 1 << 3;

        /// <summary>
        /// Value of 16.
        /// </summary>
        public const int _5 = 1 << 4;

        /// <summary>
        /// Value of 32.
        /// </summary>
        public const int _6 = 1 << 5;

        /// <summary>
        /// Value of 64.
        /// </summary>
        public const int _7 = 1 << 6;

        /// <summary>
        /// Value of 128.
        /// </summary>
        public const int _8 = 1 << 7;

        /// <summary>
        /// Value of 256.
        /// </summary>
        public const int _9 = 1 << 8;

        /// <summary>
        /// Value of 512.
        /// </summary>
        public const int _10 = 1 << 9;

        /// <summary>
        /// Value of 1024.
        /// </summary>
        public const int _11 = 1 << 10;

        /// <summary>
        /// Value of 2048.
        /// </summary>
        public const int _12 = 1 << 11;

        /// <summary>
        /// Value of 4096.
        /// </summary>
        public const int _13 = 1 << 12;

        /// <summary>
        /// Value of 8192.
        /// </summary>
        public const int _14 = 1 << 13;

        /// <summary>
        /// Value of 16384.
        /// </summary>
        public const int _15 = 1 << 14;

        /// <summary>
        /// Value of 32768.
        /// </summary>
        public const int _16 = 1 << 15;

        /// <summary>
        /// Value of 65536.
        /// </summary>
        public const int _17 = 1 << 16;

        /// <summary>
        /// Value of 131072.
        /// </summary>
        public const int _18 = 1 << 17;

        /// <summary>
        /// Value of 262144.
        /// </summary>
        public const int _19 = 1 << 18;

        /// <summary>
        /// Value of 524288.
        /// </summary>
        public const int _20 = 1 << 19;

        /// <summary>
        /// Value of 1048576.
        /// </summary>
        public const int _21 = 1 << 20;

        /// <summary>
        /// Value of 2097152.
        /// </summary>
        public const int _22 = 1 << 21;

        /// <summary>
        /// Value of 4194304.
        /// </summary>
        public const int _23 = 1 << 22;

        /// <summary>
        /// Value of 8388608.
        /// </summary>
        public const int _24 = 1 << 23;

        /// <summary>
        /// Value of 16777216.
        /// </summary>
        public const int _25 = 1 << 24;

        /// <summary>
        /// Value of 33554432.
        /// </summary>
        public const int _26 = 1 << 25;

        /// <summary>
        /// Value of 67108864.
        /// </summary>
        public const int _27 = 1 << 26;

        /// <summary>
        /// Value of 134217728.
        /// </summary>
        public const int _28 = 1 << 27;

        /// <summary>
        /// Value of 268435456.
        /// </summary>
        public const int _29 = 1 << 28;

        /// <summary>
        /// Value of 536870912.
        /// </summary>
        public const int _30 = 1 << 29;

        /// <summary>
        /// Value of 1073741824.
        /// <para>
        /// The largest possible value that an int-backed bit field enumeration type can have.
        /// </para>
        /// </summary>
        public const int _31 = 1 << 30;
    }
}
