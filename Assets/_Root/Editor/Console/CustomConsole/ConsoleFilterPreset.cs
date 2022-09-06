using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	[CreateAssetMenu(fileName = nameof(ConsoleFilterPreset), menuName = "Needle/Console/Console Filter Preset")]
	public class ConsoleFilterPreset : ScriptableObject
	{
		#region Static API
		private static readonly List<ConsoleFilterPreset> _allConfigs = new List<ConsoleFilterPreset>();
		private static void Add(ConsoleFilterPreset p)
		{
			if (!_allConfigs.Contains(p)) _allConfigs.Add(p);
		}
		private static bool _didSearchAll;
		public static IReadOnlyList<ConsoleFilterPreset> AllConfigs
		{
			get
			{
				if (!_didSearchAll)
				{
					_didSearchAll = true;
					var guids = AssetDatabase.FindAssets("t:" + nameof(ConsoleFilterPreset));
					foreach (var p in guids)
					{
						var instance = AssetDatabase.LoadAssetAtPath<ConsoleFilterPreset>(AssetDatabase.GUIDToAssetPath(p));
						Add(instance);
					}
				}

				// because OnDestroy is not called when item is deleted in ProjectView we need to double check if an item in this list still exists
				for (var i = _allConfigs.Count - 1; i >= 0; i--)
				{
					if (!_allConfigs[i]) _allConfigs.RemoveAt(i);
				}

				return _allConfigs;
			}
		}

		private static string LastSelectedPath
		{
			get => EditorPrefs.GetString("ConsoleFilterConfigLastPath", Application.dataPath);
			set => EditorPrefs.SetString("ConsoleFilterConfigLastPath", value);
		}

		public static ConsoleFilterPreset CreateAsset()
		{
			var dir = LastSelectedPath;
			if (!Directory.Exists(dir) || !dir.StartsWith(Application.dataPath.Replace("\\", "/"))) dir = Application.dataPath;
			var path = EditorUtility.SaveFilePanel("Create Console Filter", dir, "Console Filter Config", "asset");
			path = path.Replace("\\", "/");
			var validPath = Path.GetFullPath(Application.dataPath + "/../").Replace("\\", "/");
			if (!path.StartsWith(validPath))
			{
				Debug.Log("Please select a path in your project " + validPath);
				return null;
			}

			LastSelectedPath = Path.GetDirectoryName(path);
			path = path.Substring(validPath.Length);
			var inst = CreateInstance<ConsoleFilterPreset>();
			AssetDatabase.CreateAsset(inst, path);
			return inst;
		}
		#endregion

		[SerializeField] public List<FilterBase<string>.FilterEntry> messages, files, packages, warnings;
		[SerializeField] public List<FilterBase<int>.FilterEntry> ids;
		[SerializeField] public List<FilterBase<FileLine>.FilterEntry> lines;
		[SerializeField] public List<FilterBase<LogTime>.FilterEntry> times;

		private MessageFilter messageFilter;
		private LineFilter lineFilter;
		private FileFilter fileFilter;
		private ObjectIdFilter idFilter;
		private PackageFilter packageFilter;
		private TimeFilter timeFilter;
		private WarningFilter warningFilter;
		private IEnumerable<IConsoleFilter> EnumerateFilter()
		{
			yield return timeFilter;
			yield return messageFilter;
			yield return lineFilter;
			yield return fileFilter;
			yield return idFilter;
			yield return packageFilter;
			yield return warningFilter;
		}

		private void OnEnable()
		{
			if (!_allConfigs.Contains(this))
				_allConfigs.Add(this);

			timeFilter = new TimeFilter(ref times);
			messageFilter = new MessageFilter(ref messages);
			lineFilter = new LineFilter(ref lines);
			fileFilter = new FileFilter(ref files);
			idFilter = new ObjectIdFilter(ref ids);
			packageFilter = new PackageFilter(ref packages);
			warningFilter = new WarningFilter(ref warnings);
			

			foreach (var f in EnumerateFilter())
			{
				f.WillChange += OnFilterWillChange;
				f.HasChanged += OnFilterChanged;
			}
		}

		private void OnDisable()
		{
			foreach (var f in EnumerateFilter())
			{
				f.WillChange -= OnFilterWillChange;
				f.HasChanged -= OnFilterChanged;
			}
		}

		private void OnDestroy()
		{
			_allConfigs.Remove(this);
		}

		private void OnFilterWillChange(IConsoleFilter filter)
		{
			ConsoleFilter.RegisterUndo(this, filter.GetType().Name + " changed");
		}

		private void OnFilterChanged(IConsoleFilter filter)
		{
			EditorUtility.SetDirty(this);
		}

		[ContextMenu(nameof(Apply))]
		public void Apply()
		{
			var settings = ConsoleFilterUserSettings.instance;
			settings.ApplyPreset(this);
		}

		[CustomEditor(typeof(ConsoleFilterPreset))]
		private class ConsoleFilterEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				var t = (ConsoleFilterPreset) target;

				if (GUILayout.Button(nameof(Apply), GUILayout.Height(30)))
				{
					t.Apply();
				}

				GUILayout.Space(10);
				var list = t.EnumerateFilter().ToList();
				Draw.FilterList(list);
			}
		}

		internal static void DrawHowToFilterHelpBox()
		{
			EditorGUILayout.HelpBox(
				"You haven't filtered any logs yet. Context click a log message for options",
				MessageType.Info);
		}
	}
}