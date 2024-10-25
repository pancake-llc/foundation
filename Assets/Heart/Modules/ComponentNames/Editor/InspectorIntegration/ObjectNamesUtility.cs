using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Sisus.ComponentNames.Editor
{
    internal static class ObjectNamesUtility
    {
        private static Dictionary<Type, string> internalInspectorTitlesCache = null;

        public static Dictionary<Type, string> InternalInspectorTitlesCache
        {
            get
            {
                if (internalInspectorTitlesCache == null)
                {
                    var inspectorTitlesType = typeof(ObjectNames).GetNestedType("InspectorTitles", BindingFlags.Static | BindingFlags.NonPublic);
                    var inspectorTitlesField = inspectorTitlesType.GetField("s_InspectorTitles", BindingFlags.Static | BindingFlags.NonPublic);
                    internalInspectorTitlesCache = (Dictionary<Type, string>) inspectorTitlesField.GetValue(null);
                }

                return internalInspectorTitlesCache;
            }
        }
    }
}