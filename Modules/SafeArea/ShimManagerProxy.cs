#if !UNITY_2021_1 && !UNITY_2021_2_0 && !UNITY_2021_2_1 && !UNITY_2021_2_2 && !UNITY_2021_2_3 && !UNITY_2021_2_4 && !UNITY_2021_2_5 && !UNITY_2021_2_6 && !UNITY_2021_2_7
#define UNITY_2021_2_8_OR_NEWER
#endif


#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pancake.SafeAreEditor
{
    internal static class ShimManagerProxy
    {
        private const string ASSEMBLY_NAME = "UnityEditor.DeviceSimulatorModule";

#if UNITY_2021_2_8_OR_NEWER
        private const string WIDTH_KEY = "width";
        private const string HEIGHT_KEY = "height";
#else
        private const string WIDTH_KEY = "Width";
        private const string HEIGHT_KEY = "Height";
#endif

        private static readonly Type ShimManagerType = Assembly.Load("UnityEngine.dll").GetType("UnityEngine.ShimManager");
        private static readonly FieldInfo ActiveScreenShimFieldInfo = ShimManagerType.GetField("s_ActiveScreenShim", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly PropertyInfo WidthPropertyInfo;
        private static readonly PropertyInfo HeightPropertyInfo;

        static ShimManagerProxy()
        {
            var screenSimulationType = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().Name == ASSEMBLY_NAME)
                .Select(assembly => assembly.GetType("UnityEditor.DeviceSimulation.ScreenSimulation"))
                .First();

            WidthPropertyInfo = screenSimulationType.GetProperty(WIDTH_KEY);
            HeightPropertyInfo = screenSimulationType.GetProperty(HEIGHT_KEY);
        }

        public static object GetActiveScreenShim()
        {
            var activeScreenShim = ActiveScreenShimFieldInfo.GetValue(null);
            if (activeScreenShim is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    activeScreenShim = enumerator.Current;
                    if (activeScreenShim.GetType().Name == "ScreenSimulation") break;
                }
            }

            return activeScreenShim;
        }

        public static int Width
        {
            get
            {
                var activeScreenShim = GetActiveScreenShim();
                if (activeScreenShim == null) return Screen.width;
                return activeScreenShim.GetType().GetProperty(WIDTH_KEY)?.GetValue(activeScreenShim) as int? ?? Screen.width;
            }
        }

        public static int Height
        {
            get
            {
                var activeScreenShim = GetActiveScreenShim();
                if (activeScreenShim == null) return Screen.height;
                return activeScreenShim.GetType().GetProperty(HEIGHT_KEY)?.GetValue(activeScreenShim) as int? ?? Screen.height;
            }
        }
    }
}
#endif