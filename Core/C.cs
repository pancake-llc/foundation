using System;

namespace Pancake
{
    public static partial class C
    {
        public static string Format(this string fmt, params object[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, fmt, args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        // ReSharper disable Unity.PerformanceAnalysis
        public static void CallActionClean(ref Action action)
        {
            var a = action;
            if (a == null) return;
            
            a();
            action = null;
        }
    }
}