namespace Pancake
{
    public static partial class C
    {
        public static string TextColor(this string text, string color)
        {
            if (color.IndexOf('#') == -1) color = '#' + color;
            return $"<color={color}>{text}</color>";
        }

        public static string TextColor(this string text, UnityEngine.Color color) { return $"<color={color.ToHtmlStringRGBA()}>{text}</color>"; }

        public static string TextSize(this string text, int size) { return $"<size={size}>{text}</size>"; }

        public static string TextBold(this string text) { return $"<b>{text}</b>"; }
        public static string TextItalic(this string text) { return $"<i>{text}</i>"; }
    }
}