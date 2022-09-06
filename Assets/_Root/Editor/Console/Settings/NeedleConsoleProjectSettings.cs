using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	[FilePath("ProjectSettings/NeedleConsoleProjectSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class NeedleConsoleProjectSettings : ScriptableSingleton<NeedleConsoleProjectSettings>
	{
		public static event Action ColorSettingsChanged;

		internal static void RaiseColorsChangedEvent() => ColorSettingsChanged?.Invoke();
		
		internal void Save()
		{
			Undo.RegisterCompleteObjectUndo(this, "Save Needle Console Project Settings");
			base.Save(true);
		}
		
		[SerializeField]
		internal bool FirstInstall = true;

		// internal bool UseColors = false;
		// internal bool ProceduralColors = false;
		//
		// [SerializeField]
		// internal List<LogColor> LogColors = new List<LogColor>();
		//
		// [Serializable]
		// public class LogColor
		// {
		// 	public string Key;
		// 	public Color Color;
		// }
	}
}