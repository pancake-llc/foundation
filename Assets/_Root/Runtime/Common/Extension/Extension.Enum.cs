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
        
        public static object ToEnumsNumericType(System.Enum e)
        {
            if (e == null) return null;

            switch (e.GetTypeCode())
            {
                case System.TypeCode.SByte:
                    return System.Convert.ToSByte(e);
                case System.TypeCode.Byte:
                    return System.Convert.ToByte(e);
                case System.TypeCode.Int16:
                    return System.Convert.ToInt16(e);
                case System.TypeCode.UInt16:
                    return System.Convert.ToUInt16(e);
                case System.TypeCode.Int32:
                    return System.Convert.ToInt32(e);
                case System.TypeCode.UInt32:
                    return System.Convert.ToUInt32(e);
                case System.TypeCode.Int64:
                    return System.Convert.ToInt64(e);
                case System.TypeCode.UInt64:
                    return System.Convert.ToUInt64(e);
                default:
                    return null;
            }
        }
    }
}