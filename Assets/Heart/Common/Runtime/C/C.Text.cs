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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnglishLetter(char word) { return (word >= 65 && word <= 90) || (word >= 97 && word <= 122); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToLower(this char word)
        {
            if (word >= 65 && word <= 90) return (char) (word + 32);
            if (word >= 97 && word <= 122) return word;
            return char.ToLower(word);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToUpper(this char word)
        {
            if (word >= 65 && word <= 90) return word;
            if (word >= 97 && word <= 122) return (char) (word - 32);
            return char.ToUpper(word);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStartAndEnd(this string text)
        {
            if (char.IsWhiteSpace(text[0])) text = text.TrimStart();
            if (char.IsWhiteSpace(text[^1])) text = text.TrimEnd();
            return text;
        }
    }
}