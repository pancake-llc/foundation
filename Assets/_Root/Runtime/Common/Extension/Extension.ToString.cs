namespace Pancake
{
    using System;
    using System.Globalization;
    using UnityEngine;

    public static partial class C
    {
        #region vector2

        /// <summary>
        /// return string store vector2
        /// "$v2":"x:y"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
        /// <param name="resultFormat">result string format</param>
        /// <returns></returns>
        public static string ParseToString(this Vector2 source, IFormatProvider formatProvider, string numberFormat = "F1", string resultFormat = "{0}:{1}")
        {
            string str = string.Format(resultFormat, source.x.ToString(numberFormat, formatProvider), source.y.ToString(numberFormat, formatProvider));
            return "{\"$v2\":\"" + str + "\"}";
        }

        /// <summary>
        /// return string store vector2
        /// "$v2":"x:y"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="resultFormat">result string format</param>
        public static string ParseToString(this Vector2 source, string numberFormat, string resultFormat) =>
            source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat, resultFormat);


        /// <summary>
        /// return string store vector2
        /// "$v2":"x:y"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        public static string ParseToString(this Vector2 source, string numberFormat) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat);

        /// <summary>
        /// return string store vector2
        /// "$v2":"x:y"
        /// </summary>
        /// <param name="source"></param>
        public static string ParseToString(this Vector2 source) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat);

        #endregion

        #region vector2Int

        /// <summary>
        /// return string store vector2Int
        /// "$v2Int":"x:y"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
        /// <param name="resultFormat">result string format</param>
        /// <returns></returns>
        public static string ParseToString(this Vector2Int source, IFormatProvider formatProvider, string numberFormat = null, string resultFormat = "{0}:{1}")
        {
            string str = string.Format(resultFormat, source.x.ToString(numberFormat, formatProvider), source.y.ToString(numberFormat, formatProvider));
            return "{\"$v2Int\":\"" + str + "\"}";
        }

        /// <summary>
        /// return string store vector2
        /// "$v2Int":"x:y"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="resultFormat">result string format</param>
        public static string ParseToString(this Vector2Int source, string numberFormat, string resultFormat) =>
            source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat, resultFormat);


        /// <summary>
        /// return string store vector2
        /// "$v2Int":"x:y"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        public static string ParseToString(this Vector2Int source, string numberFormat) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat);

        /// <summary>
        /// return string store vector2
        /// "$v2Int":"x:y"
        /// </summary>
        /// <param name="source"></param>
        public static string ParseToString(this Vector2Int source) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat);

        #endregion

        #region vector3

        /// <summary>
        /// return string store vector3
        /// "$v3":"x:y:z"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
        /// <param name="resultFormat">result string format</param>
        /// <returns></returns>
        public static string ParseToString(this Vector3 source, IFormatProvider formatProvider, string numberFormat = "F1", string resultFormat = "{0}:{1}:{2}")
        {
            if (string.IsNullOrEmpty(numberFormat)) numberFormat = "F1";

            string str = string.Format(resultFormat,
                source.x.ToString(numberFormat, formatProvider),
                source.y.ToString(numberFormat, formatProvider),
                source.z.ToString(numberFormat, formatProvider));
            return "{\"$v3\":\"" + str + "\"}";
        }

        /// <summary>
        /// return string store vector3
        /// "$v3":"x:y:z"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="resultFormat">result string format</param>
        public static string ParseToString(this Vector3 source, string numberFormat, string resultFormat) =>
            source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat, resultFormat);


        /// <summary>
        /// return string store vector3
        /// "$v3":"x:y:z"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        public static string ParseToString(this Vector3 source, string numberFormat) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat);

        /// <summary>
        /// return string store vector3
        /// "$v3":"x:y:z"
        /// </summary>
        /// <param name="source"></param>
        public static string ParseToString(this Vector3 source) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat);

        #endregion

        #region vector3Int

        /// <summary>
        /// return string store vector3
        /// "$v3Int":"x:y:z"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
        /// <param name="resultFormat">result string format</param>
        /// <returns></returns>
        public static string ParseToString(this Vector3Int source, IFormatProvider formatProvider, string numberFormat = null, string resultFormat = "{0}:{1}:{2}")
        {
            if (string.IsNullOrEmpty(numberFormat)) numberFormat = "F1";

            string str = string.Format(resultFormat,
                source.x.ToString(numberFormat, formatProvider),
                source.y.ToString(numberFormat, formatProvider),
                source.z.ToString(numberFormat, formatProvider));
            return "{\"$v3Int\":\"" + str + "\"}";
        }

        /// <summary>
        /// return string store vector3
        /// "$v3Int":"x:y:z"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="resultFormat">result string format</param>
        public static string ParseToString(this Vector3Int source, string numberFormat, string resultFormat) =>
            source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat, resultFormat);


        /// <summary>
        /// return string store vector2
        /// "$v3Int":"x:y:z"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        public static string ParseToString(this Vector3Int source, string numberFormat) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat);

        /// <summary>
        /// return string store vector2
        /// "$v3Int":"x:y:z"
        /// </summary>
        /// <param name="source"></param>
        public static string ParseToString(this Vector3Int source) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat);

        #endregion

        #region vector4

        /// <summary>
        /// return string store vector4
        /// "$v4":"x:y:z:w"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
        /// <param name="resultFormat">result string format</param>
        /// <returns></returns>
        public static string ParseToString(this Vector4 source, IFormatProvider formatProvider, string numberFormat = "F1", string resultFormat = "{0}:{1}:{2}:{3}")
        {
            if (string.IsNullOrEmpty(numberFormat)) numberFormat = "F1";

            string str = string.Format(resultFormat,
                source.x.ToString(numberFormat, formatProvider),
                source.y.ToString(numberFormat, formatProvider),
                source.z.ToString(numberFormat, formatProvider),
                source.w.ToString(numberFormat, formatProvider));
            return "{\"$v4\":\"" + str + "\"}";
        }

        /// <summary>
        /// return string store vector4
        /// "$v4":"x:y:z:w"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        /// <param name="resultFormat">result string format</param>
        public static string ParseToString(this Vector4 source, string numberFormat, string resultFormat) =>
            source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat, resultFormat);


        /// <summary>
        /// return string store vector4
        /// "$v4":"x:y:z:w"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberFormat">A numeric format string.</param>
        public static string ParseToString(this Vector4 source, string numberFormat) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat, numberFormat);

        /// <summary>
        /// return string store vector4
        /// "$v4":"x:y:z:w"
        /// </summary>
        /// <param name="source"></param>
        public static string ParseToString(this Vector4 source) => source.ParseToString(CultureInfo.InvariantCulture.NumberFormat);

        #endregion
        
        
    }
}