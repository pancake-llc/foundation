#define UNITY_DEMYSTIFY_DEV
// #undef UNITY_DEMYSTIFY_DEV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using Debug = UnityEngine.Debug;

namespace Needle.Console
{
	// ReSharper disable once UnusedType.Global
	public class Patch_Exception : PatchBase
	{
		protected override IEnumerable<MethodBase> GetPatches()
		{
			yield return AccessTools.Method(typeof(Exception), "GetStackTrace");
		}
		
		// capture exception stacktrace.
		// unity is internally looping stackFrames instead of using GetStackTrace
		// but we capture all other cases in other patches
		private static void Postfix(object __instance, ref string __result)
		{
			if (__instance is Exception ex)
			{
				try
				{
					__result = ex.ToStringDemystified();
					Hyperlinks.FixStacktrace(ref __result);
					StacktraceMarkerUtil.AddMarker(ref __result);
				}
				catch (TypeLoadException tl)
				{
					if (NeedleConsoleSettings.DevelopmentMode)
						Debug.LogException(tl);
				}
			}
		}
	}
}