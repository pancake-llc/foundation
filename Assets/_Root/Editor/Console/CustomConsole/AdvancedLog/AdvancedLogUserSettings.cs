using System.Collections.Generic;
using UnityEditor;

namespace Pancake.Console
{
	[FilePath("UserSettings/ConsoleAdvancedUserSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class AdvancedLogUserSettings : ScriptableSingleton<AdvancedLogUserSettings>
	{
		public void Save() => Save(true);

		public List<AdvancedLogEntry> selections = new List<AdvancedLogEntry>();
	}
}