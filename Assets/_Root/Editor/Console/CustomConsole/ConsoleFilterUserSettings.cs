using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.Console
{
	[FilePath("UserSettings/ConsoleFilterUserSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class ConsoleFilterUserSettings : ScriptableSingleton<ConsoleFilterUserSettings>
	{
		public void Save() => Save(true);

		public void ApplyPreset(ConsoleFilterPreset preset)
		{
			if (!preset) return;
			ConsoleFilter.RegisterUndo(this, "Apply filter preset");
			Apply(messages, preset.messages);
			Apply(files, preset.files);
			Apply(packages, preset.packages);
			Apply(ids, preset.ids);
			Apply(lines, preset.lines);
			Apply(times, preset.times);
			RecreateFilters();
			Save(true);
		}
		
		public void SaveToPreset(ConsoleFilterPreset preset)
		{
			if (!preset) return;
			ConsoleFilter.RegisterUndo(preset, "Save filter to preset");
			Apply(preset.messages, messages);
			Apply(preset.files, files);
			Apply(preset.packages, packages);
			Apply(preset.ids, ids);
			Apply(preset.lines, lines);
			Apply(preset.times, times);
		}

		private static void Apply<T>(List<T> self, List<T> other)
		{
			self.Clear();
			if (other != null) self.AddRange(other);
		}
		
		public IEnumerable<IConsoleFilter> EnumerateFilter()
		{
			yield return messageFilter;
			yield return lineFilter;
			yield return fileFilter;
			yield return idFilter;
			yield return packageFilter;
			yield return timeFilter;
			yield return warningsFilter;
		}
		
		[SerializeField]
		private List<FilterBase<string>.FilterEntry> messages, files, packages, warnings;
		[SerializeField]
		private List<FilterBase<int>.FilterEntry> ids;
		[SerializeField]
		private List<FilterBase<FileLine>.FilterEntry> lines;
		[SerializeField]
		private List<FilterBase<LogTime>.FilterEntry> times;

		private MessageFilter messageFilter;
		private LineFilter lineFilter;
		private FileFilter fileFilter;
		private ObjectIdFilter idFilter;
		private PackageFilter packageFilter;
		private TimeFilter timeFilter;
		private WarningFilter warningsFilter;

		private void RecreateFilters()
		{
			foreach (var f in EnumerateFilter())
			{
				if(f != null)
				{
					f.WillChange -= OnFilterWillChange;
					f.HasChanged -= OnFilterChanged;
				}
			}
			ConsoleFilter.RemoveAllFilter();
			
			messageFilter = new MessageFilter(ref messages);
			lineFilter = new LineFilter(ref lines);
			fileFilter = new FileFilter(ref files);
			idFilter = new ObjectIdFilter(ref ids);
			packageFilter = new PackageFilter(ref packages);
			timeFilter = new TimeFilter(ref times);
			warningsFilter = new WarningFilter(ref warnings);
			foreach (var f in EnumerateFilter())
			{
				f.WillChange += OnFilterWillChange;
				f.HasChanged += OnFilterChanged;
				ConsoleFilter.AddFilter(f);
			}
		}
		
		private void OnEnable()
		{
			RecreateFilters();
		}

		private void OnDisable()
		{
			foreach (var f in EnumerateFilter())
			{
				f.WillChange -= OnFilterWillChange;
				f.HasChanged -= OnFilterChanged;
			}
		}
		
		private void OnFilterWillChange(IConsoleFilter filter)
		{
			ConsoleFilter.RegisterUndo(this, filter.GetType().Name + " changed");
		}

		private void OnFilterChanged(IConsoleFilter filter)
		{
			Save(true);
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			ConsoleFilterUserSettings.instance.Save();
		}
	}
}