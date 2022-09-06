using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class AdvancedLog
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			ConsoleFilter.ClearingCachedData += OnClear;
			ConsoleFilter.CustomAddEntry += CustomAdd;
			ConsoleList.LogEntryContextMenu += OnLogEntryContext;
			FilterFoldoutContent.FoldoutOnGUI += OnFoldoutOnGUI;

			var list = AdvancedLogUserSettings.instance.selections;
			collapse = new AdvancedLogCollapse(list);
			drawer = new AdvancedLogDrawer();
			ConsoleList.RegisterCustomDrawer(drawer);
		}

		private static AdvancedLogDrawer drawer;
		private static AdvancedLogCollapse collapse;

		private static bool CollapsedLogFoldout
		{
			get => SessionState.GetBool("NeedleConsole.CollapsedLogFoldout", false);
			set => SessionState.SetBool("NeedleConsole.CollapsedLogFoldout", value);
		}

		private static void OnFoldoutOnGUI(Rect obj)
		{
			var settings = AdvancedLogUserSettings.instance;
			if (settings.selections?.Count <= 0) return;
			
			CollapsedLogFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(CollapsedLogFoldout, new GUIContent("Collapsed Logs"), EditorStyles.foldoutHeader, ShowOptionsContextMenu);

			void ShowOptionsContextMenu(Rect r)
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Clear"), false, () =>
				{
					if (settings.selections != null)
					{
						settings.selections.Clear();
						settings.Save();
						ConsoleFilter.MarkDirty();
					}
				});
				menu.DropDown(r);
			}
			
			if (CollapsedLogFoldout)
			{
				using (var change = new EditorGUI.ChangeCheckScope())
				{
					EditorGUI.indentLevel++;
					var list = settings.selections;
					for (var index = 0; index < list.Count; index++)
					{
						var log = list[index];
						if (string.IsNullOrEmpty(log.File) || !File.Exists(log.File))
						{
							list.RemoveAt(index--);
							continue;
						}

						using (new EditorGUILayout.HorizontalScope())
						{
							var label = log.File;
							label += "::" + log.Line;
							var full = label;

							const int maxLength = 50;
							var overflow = Mathf.Max(0, label.Length - maxLength);
							if (overflow > 0)
							{
								label = "..." + label.Substring(overflow);
							}

							EditorGUILayout.LabelField(new GUIContent(label, full));
							if (GUILayout.Button(new GUIContent(Textures.Remove, "Remove entry"), Styles.FilterToggleButton(), GUILayout.Width(Styles.RemoveIconWith)))
							{
								list.RemoveAt(index);
								index -= 1;
							}

							GUILayout.Space(7);
						}

					}
					EditorGUI.indentLevel--;
					if (change.changed)
					{
						settings.Save();
						ConsoleFilter.MarkDirty();
					}
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static void OnLogEntryContext(GenericMenu menu, int itemIndex)
		{
			if (!NeedleConsoleSettings.instance.IndividualCollapse) return;
			if (itemIndex < 0) return;
			var log = ConsoleList.CurrentEntries[itemIndex];
			// var content = new GUIContent("Collapse " + Path.GetFileName(log.entry.file) + "::" + log.entry.line);
			var content = new GUIContent("Collapse");
			var contains = TryGetIndex(log.entry, out var index);
			var on = contains && Entries[index].Active;
			menu.AddItem(content, on, () =>
			{
				if (on)
				{
					Entries.RemoveAt(index);
					SaveEntries();
					ConsoleFilter.MarkDirty();
				}
				else
				{
					Select(log.entry);
					SaveEntries();
					ConsoleFilter.MarkDirty();
					// var item = ConsoleList.CurrentEntries[itemIndex];
					// var list = ConsoleLogAdvancedUserSettings.instance.selections;
				}
			});
		}

		private static void OnClear()
		{
			collapse.ClearCache();
		}
		
		private static bool CustomAdd(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries)
		{
			if (!NeedleConsoleSettings.instance.IndividualCollapse)
			{
				return false;
			}

			using (new ProfilerMarker("Console Log Grouping").Auto())
			{
				return collapse.OnHandleLog(entry, row, preview, entries);
			}
		}

		internal static void SaveEntries() => AdvancedLogUserSettings.instance.Save();
		private static List<AdvancedLogEntry> Entries => AdvancedLogUserSettings.instance.selections;

		private static bool TryGetIndex(LogEntryInfo entry, out int index)
		{
			for (var i = 0; i < Entries.Count; i++)
			{
				var e = Entries[i];
				if (e.Line == entry.line && e.File == entry.file)
				{
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		private static void Select(LogEntryInfo entry)
		{
			Entries.Add(new AdvancedLogEntry()
			{
				Active = true,
				File = entry.file,
				Line = entry.line
			});
		}
	}
}