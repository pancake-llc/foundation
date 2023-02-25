namespace Pancake
{
    public static partial class C
    {
        public static string Format(this string fmt, params object[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, fmt, args);
    }
}