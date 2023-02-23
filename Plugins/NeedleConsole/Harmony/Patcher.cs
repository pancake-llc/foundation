using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace Pancake.Console
{
	public static class Patcher
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			if (NeedleConsoleSettings.instance.Enabled) 
				ApplyPatches();
		}

		private static Harmony _harmony;
		private static List<IPatch> patches;

		internal static void ApplyPatches()
		{
			if (patches == null)
			{
				patches = new List<IPatch>();
				var col = TypeCache.GetTypesDerivedFrom(typeof(IPatch));
				foreach (var t in col)
				{
					if (t.IsAbstract) continue;
					var inst = Activator.CreateInstance(t) as IPatch;
					patches.Add(inst);
				}
				
			}
			
			if (_harmony == null)
				_harmony = new Harmony("com.needle.console");
			
			foreach (var p in patches)
				p.Apply(_harmony);
		}

		internal static void RemovePatches()
		{
			if (_harmony == null) return;
			
			foreach (var p in patches)
				p.Remove(_harmony);
		}
	}
}