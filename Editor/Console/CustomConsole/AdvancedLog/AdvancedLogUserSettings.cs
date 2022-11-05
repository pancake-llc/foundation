using System.Collections.Generic;
using UnityEditor;

namespace Pancake.Console
{
	[UnityEditor.FilePath("UserSettings/ConsoleAdvancedUserSettings.asset", UnityEditor.FilePathAttribute.Location.ProjectFolder)]
	internal class AdvancedLogUserSettings : ScriptableSingleton<AdvancedLogUserSettings>
	{
		public void Save() => Save(true);

		public List<AdvancedLogEntry> selections = new List<AdvancedLogEntry>();
	}
}