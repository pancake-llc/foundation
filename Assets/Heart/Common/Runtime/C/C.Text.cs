using System.Runtime.CompilerServices;

namespace Pancake.Common
{
    public static partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color">html color, not include #</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetColor(this string text, string color) { return $"<color=#{color}>{text}</color>"; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetColor(this string text, UnityEngine.Color color) { return $"<color={color.ToHtmlStringRGBA()}>{text}</color>"; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetSize(this string text, int size) { return $"<size={size}>{text}</size>"; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBold(this string text) { return $"<b>{text}</b>"; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToItalic(this string text) { return $"<i>{text}</i>"; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToWhiteBold(this string text) { return text.ToBold().SetColor(UnityEngine.Color.white); }
    }
}