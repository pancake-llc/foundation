using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace Pancake.Console
{
		internal class Patch_ConsoleWindowListView : PatchBase
		{
			protected override IEnumerable<MethodBase> GetPatches()
			{
				var method = Patch_Console.ConsoleWindowType.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
				yield return method;
			}

			private static IEnumerable<CodeInstruction> Transpiler(MethodBase method, ILGenerator il, IEnumerable<CodeInstruction> instructions)
			{
				var skipLabel = il.DefineLabel();
				var arr = instructions.ToArray();
				var loadListViewElementIndex = -1;

				for (var index = 0; index < arr.Length; index++)
				{
					var inst = arr[index];
					
					// Debug.Log("<color=grey>" + index + ": " + inst + "</color>"); 

					// get local index for current list view element
					if (loadListViewElementIndex == -1 || inst.IsStloc() && inst.operand is LocalBuilder)
					{
						var loc = inst.operand as LocalBuilder;
						if (loc?.LocalType == typeof(ListViewElement))
							loadListViewElementIndex = loc.LocalIndex;
					}

					if (inst.opcode == OpCodes.Call && inst.operand is MethodInfo m) 
					{
						if (m.DeclaringType == typeof(LogEntries) && m.Name == "GetLinesAndModeFromEntryInternal")
						{
							yield return inst;
							// load text is one element before
							var ldStr = arr[index - 1];
							yield return new CodeInstruction(OpCodes.Ldloc, loadListViewElementIndex);
							yield return ldStr;
							yield return CodeInstruction.Call(typeof(ConsoleTextPrefix), nameof(ConsoleTextPrefix.ModifyText));
							continue;
						}
					}

					if (inst.opcode == OpCodes.Ret)
					{
						inst.labels.Add(skipLabel);
					}
					
					// this is before "GUILayout.FlexibleSpace"
					// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/ConsoleWindow.cs#L539
#if UNITY_2022_1_OR_NEWER
					if (index == 241)
#elif UNITY_2021_1_OR_NEWER
					if (index == 172)
#elif UNITY_2020_1_OR_NEWER
					if (index == 189)
#else // 2019
					if (index == -1)
#endif
					{
						yield return CodeInstruction.Call(typeof(ConsoleToolbarFoldout), nameof(ConsoleToolbarFoldout.OnDrawFoldouts)); 
					}
					
					// this is before "EndHorizontal"
					// https://github.com/Unity-Technologies/UnityCsReference/blob/4d031e55aeeb51d36bd94c7f20182978d77807e4/Editor/Mono/ConsoleWindow.cs#L600
#if UNITY_2022_1_OR_NEWER
					if (index == 349)
#elif UNITY_2021_1_OR_NEWER
					if (index == 329)
#elif UNITY_2020_1_OR_NEWER
					if (index == 317)
#else // 2019
					if (index == -1)
#endif
					{
						yield return CodeInstruction.Call(typeof(ConsoleToolbarIcon), nameof(ConsoleToolbarIcon.OnDrawToolbar));
					}

					// TODO: properly search for the right spots
					// this is right before  SplitterGUILayout.BeginVerticalSplit(spl);
#if UNITY_2022_1_OR_NEWER
					if (index == 382)
#elif UNITY_2021_1_OR_NEWER
					if (index == 330)
#elif UNITY_2020_1_OR_NEWER
					if (index == 318)
#else // 2019
					if (index == -1)
#endif
					{
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return CodeInstruction.Call(typeof(ConsoleList), nameof(ConsoleList.OnDrawList));
						yield return new CodeInstruction(OpCodes.Brfalse, skipLabel);
					}
					
					yield return inst;
				}
			}
			
			

			// private class Instructor
			// {
			// 	public List<Func<CodeInstruction, bool>> List = new List<Func<CodeInstruction, bool>>();
			// 	public Func<CodeInstruction, bool> OnFound;
			//
			// 	private int currentListIndex;
			// 	private int firstIndex;
			// 	
			// 	public void Check(CodeInstruction inst, int index)
			// 	{
			// 		var check = List[currentListIndex];
			// 		if (check(inst))
			// 		{
			// 			currentListIndex += 1;
			// 			if (currentListIndex >= List.Count)
			// 			{
			// 				OnFound?.Invoke()
			// 			}
			// 		}
			// 	}
			// }
		}
}