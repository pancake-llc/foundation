namespace Pancake
{
    using UnityEngine;

    public static partial class C
    {
        /// <summary>
        /// Returns a random color between the two min/max specified
        /// </summary>
        /// <param name="color"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Color RandomColor(this Color color, Color min, Color max)
        {
            Color c = new Color() {r = Random.Range(min.r, max.r), g = Random.Range(min.g, max.g), b = Random.Range(min.b, max.b), a = Random.Range(min.a, max.a)};

            return c;
        }

        /// <summary>
        /// Tint : Uses HSV color conversions, keeps the original values, multiplies alpha
        /// Multiply : The whole color, including alpha, is multiplied over the original 
        /// Replace : completely replaces the original with the target color
        /// ReplaceKeepAlpha : color is replaced but the original alpha channel is ignored
        /// Add : target color gets added (including its alpha)
        /// </summary>
        public enum ColoringMode
        {
            Tint,
            Multiply,
            Replace,
            ReplaceKeepAlpha,
            Add
        }

        public static Color Colorize(this Color originalColor, Color targetColor, ColoringMode coloringMode, float lerpAmount = 1.0f)
        {
            Color resultColor = Color.white;
            switch (coloringMode)
            {
                case ColoringMode.Tint:
                {
                    Color.RGBToHSV(originalColor, out _, out _, out var sV);
                    Color.RGBToHSV(targetColor, out var tH, out var tS, out var tV);
                    resultColor = Color.HSVToRGB(tH, tS, sV * tV);
                    resultColor.a = originalColor.a * targetColor.a;
                }
                    break;
                case ColoringMode.Multiply:
                    resultColor = originalColor * targetColor;
                    break;
                case ColoringMode.Replace:
                    resultColor = targetColor;
                    break;
                case ColoringMode.ReplaceKeepAlpha:
                    resultColor = targetColor;
                    resultColor.a = originalColor.a;
                    break;
                case ColoringMode.Add:
                    resultColor = originalColor + targetColor;
                    break;
            }

            return Color.Lerp(originalColor, resultColor, lerpAmount);
        }

        /// <summary>
        /// Makes a copy of the vector with a changed alpha value.
        /// </summary>
        /// <param name="color">The Color to copy.</param>
        /// <param name="a">The new a component.</param>
        /// <returns>A copy of the Color with a changed alpha.</returns>
        public static Color ChangeAlpha(this Color color, float a)
        {
            color.a = a;
            return color;
        }

        /// <summary>
        ///   <para>Returns the color as a hexadecimal string in the format "#RRGGBB".</para>
        /// </summary>
        /// <param name="color">The color to be converted.</param>
        /// <returns>
        ///   <para>Hexadecimal string representing the color.</para>
        /// </returns>
        public static string ToHtmlStringRGB(Color color)
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

        /// <summary>
        /// Get the perceived brightness of a LDR color (alpha is ignored).
        /// (http://alienryderflex.com/hsp.html)
        /// </summary>
        public static float GetPerceivedBrightness(Color color)
        {
            return Mathf.Sqrt(0.241f * color.r * color.r + 0.691f * color.g * color.g + 0.068f * color.b * color.b);
        }


        /// <summary>
        /// Convert a LDR color value to an ARGB32 format uint value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static uint ToARGB32(Color c) { return ((uint) (c.a * 255) << 24) | ((uint) (c.r * 255) << 16) | ((uint) (c.g * 255) << 8) | ((uint) (c.b * 255)); }


        /// <summary>
        /// Convert an ARGB32 format uint value to a color value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static Color FromARGB32(uint argb)
        {
            return new Color(((argb >> 16) & 0xFF) / 255f, ((argb >> 8) & 0xFF) / 255f, ((argb) & 0xFF) / 255f, ((argb >> 24) & 0xFF) / 255f);
        }


        /// <summary>
        /// Convert a hue value to a color vlue.
        /// 0-red; 0.167-yellow; 0.333-green; 0.5-cyan; 0.667-blue; 0.833-magenta; 1-red
        /// </summary>
        public static Color HueToColor(float hue)
        {
            return new Color(HueToGreen(hue + 1f / 3f), HueToGreen(hue), HueToGreen(hue - 1f / 3f));

            float HueToGreen(float h)
            {
                h = ((h % 1f + 1f) % 1f) * 6f;

                if (h < 1f) return h;
                if (h < 3f) return 1f;
                if (h < 4f) return (4f - h);
                return 0f;
            }
        }

        public static string TextColor(this string text, string color)
        {
            if (color.IndexOf('#') == -1) color = '#' + color;
            return $"<color={color}>{text}</color>";
        }

        public static string TextColor(this string text, Color color) { return $"<color={color.ToHtmlStringRGBA()}>{text}</color>"; }
    }
    
    public static class Paint
    {
        public static readonly Color Green = new(0.31f, 0.98f, 0.48f, 0.66f);
        public static readonly Color Orange = new(1f, 0.72f, 0.42f, 0.66f);
        public static readonly Color Blue = new(0f, 1f, 0.97f, 0.27f);
        public static readonly Color Purple = new(0.74f, 0.58f, 0.98f, 0.39f);
        public static readonly Color Red = new(1f, 0.16f, 0.16f, 0.66f);
        public static readonly Color Pink = new(1f, 0.47f, 0.78f, 0.66f);
    }
}