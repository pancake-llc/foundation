using UnityEngine;
using UnityEngine.UI;

namespace Pancake
{
    public static partial class C
    {
        /// <summary>
        /// Makes a copy of the color with a changed alpha value.
        /// </summary>
        /// <param name="color">The Color to copy.</param>
        /// <param name="a">The new a component.</param>
        /// <returns>A copy of the Color with a changed alpha.</returns>
        public static Color ChangeAlpha(this Color color, float a)
        {
            color.a = a;
            return color;
        }

        public static void SetRed(this SpriteRenderer spriteRenderer, float red)
        {
            var color = spriteRenderer.color;
            color.r = red;
            spriteRenderer.color = color;
        }


        public static void SetGreen(this SpriteRenderer spriteRenderer, float green)
        {
            var color = spriteRenderer.color;
            color.g = green;
            spriteRenderer.color = color;
        }


        public static void SetBlue(this SpriteRenderer spriteRenderer, float blue)
        {
            var color = spriteRenderer.color;
            color.b = blue;
            spriteRenderer.color = color;
        }


        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }


        public static void SetRGB(this SpriteRenderer spriteRenderer, float r, float g, float b) { spriteRenderer.color = new Color(r, g, b, spriteRenderer.color.a); }


        public static void SetRed(this Graphic graphic, float red)
        {
            var color = graphic.color;
            color.r = red;
            graphic.color = color;
        }


        public static void SetGreen(this Graphic graphic, float green)
        {
            var color = graphic.color;
            color.g = green;
            graphic.color = color;
        }


        public static void SetBlue(this Graphic graphic, float blue)
        {
            var color = graphic.color;
            color.b = blue;
            graphic.color = color;
        }


        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }


        public static void SetRGB(this Graphic graphic, float r, float g, float b) { graphic.color = new Color(r, g, b, graphic.color.a); }


        /// <summary>
        ///   <para>Returns the color as a hexadecimal string in the format "#RRGGBB".</para>
        /// </summary>
        /// <param name="color">The color to be converted.</param>
        /// <returns>
        ///   <para>Hexadecimal string representing the color.</para>
        /// </returns>
        public static string ToHtmlStringRGB(this Color color)
        {
            var color32 = new Color32((byte) Mathf.Clamp(Mathf.RoundToInt(color.r * (float) byte.MaxValue), 0, (int) byte.MaxValue),
                (byte) Mathf.Clamp(Mathf.RoundToInt(color.g * (float) byte.MaxValue), 0, (int) byte.MaxValue),
                (byte) Mathf.Clamp(Mathf.RoundToInt(color.b * (float) byte.MaxValue), 0, (int) byte.MaxValue),
                (byte) 1);

            return "#{0:X2}{1:X2}{2:X2}".Format((object) color32.r, (object) color32.g, (object) color32.b);
        }


        /// <summary>
        ///   <para>Returns the color as a hexadecimal string in the format "#RRGGBBAA".</para>
        /// </summary>
        /// <param name="color">The color to be converted.</param>
        /// <returns>
        ///   <para>Hexadecimal string representing the color.</para>
        /// </returns>
        // ReSharper disable once InconsistentNaming
        public static string ToHtmlStringRGBA(this Color color)
        {
            var color32 = new Color32((byte) Mathf.Clamp(Mathf.RoundToInt(color.r * (float) byte.MaxValue), 0, (int) byte.MaxValue),
                (byte) Mathf.Clamp(Mathf.RoundToInt(color.g * (float) byte.MaxValue), 0, (int) byte.MaxValue),
                (byte) Mathf.Clamp(Mathf.RoundToInt(color.b * (float) byte.MaxValue), 0, (int) byte.MaxValue),
                (byte) Mathf.Clamp(Mathf.RoundToInt(color.a * (float) byte.MaxValue), 0, (int) byte.MaxValue));

            return "#{0:X2}{1:X2}{2:X2}{3:X2}".Format((object) color32.r, (object) color32.g, (object) color32.b, (object) color32.a);
        }


        public static bool TryParseHtmlString(this string htmlString, out Color color)
        {
            string stringColor = htmlString;
            if (!stringColor[0].Equals('#'))
            {
                stringColor = stringColor.Insert(0, "#");
            }

            return ColorUtility.TryParseHtmlString(stringColor, out color);
        }


        public static string TextColor(this string text, string color)
        {
            if (color.IndexOf('#') == -1) color = '#' + color;
            return $"<color={color}>{text}</color>";
        }


        public static string TextColor(this string text, Color color) { return $"<color={color.ToHtmlStringRGBA()}>{text}</color>"; }
    }
}