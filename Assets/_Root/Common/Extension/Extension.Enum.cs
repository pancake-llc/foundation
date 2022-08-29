namespace Pancake
{
    using System;

    public static partial class C
    {
        /// <summary>
        /// return enum by string name <paramref name="value"/>
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ParseEnumType<T>(this string value) where T : struct
        {
            Enum.TryParse(value, out T type);
            return type;
        }
    }
}