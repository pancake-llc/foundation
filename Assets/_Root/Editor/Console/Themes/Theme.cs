using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	[Serializable]
	public class Theme
	{
		internal static readonly Dictionary<string, string> DefaultThemeDark = new Dictionary<string, string>()
		{
			{"define", "#888888"},
			{"comment", "#3A903A"},
			{"new", "#FFE234"},
			{"async", "#4394F1"},
			{"return_tuple", "#4394F1"},
			{"return_type", "#4394F1"},
			{"namespace", "#DBDBDB"},
			{"class", "#3EDD9D"},
			{"method_name", "#4394F1"},
			{"params", "#4394F1"},
			{"func", "#4394F1"},
			{"local_func", "#4394F1"},
			{"local_func_params", "#4394F1"},
			{"exception", "#FF3636"},
			{"link", "#959595"},
			{"keywords", "#4394F1"},
			{"string_literal", "#FFB48F"},
		};

		internal static readonly Dictionary<string, string> DefaultThemeLight = new Dictionary<string, string>()
		{
			{"define", "#555555"},
			{"comment", "#2E7228"},
			{"new", "#C600FF"},
			{"async", "#240AE7"},
			{"return_tuple", "#240AE7"},
			{"return_type", "#240AE7"},
			{"namespace", "#000000"},
			{"class", "#000000"},
			{"method_name", "#240AE7"},
			{"params", "#240AE7"},
			{"func", "#240AE7"},
			{"local_func", "#240AE7"},
			{"local_func_params", "#240AE7"},
			{"exception", "#FF0000"},
			{"link", "#414141"},
			{"keywords", "#FFB48F"},
		};

		public string Name;
		public List<Entry> Entries;
		public bool IsDefault => Name == DefaultThemeName;
		public const string DefaultThemeName = "Default";

		public static bool Ignored(Color color) => color.a < .1f;

		public Theme(string name) => Name = name;

		internal void SetActive()
		{
			if (this.Entries.Count >= 0)
			{
				SetActive(SyntaxHighlighting.CurrentTheme);
				isDirty = false;
			}
		}
		
		internal void SetActive(Dictionary<string, string> dict)
		{
			if (this.Entries.Count >= 0)
			{
				dict.Clear();
				foreach (var entry in this.Entries)
				{
					if (Ignored(entry.Color)) continue;
					var html = ColorUtility.ToHtmlStringRGBA(entry.Color);
					if (string.IsNullOrEmpty(html)) continue;
					if (!html.StartsWith("#")) html = "#" + html;
					dict.Add(entry.Key, html);
				}

				isDirty = false;
			}
		}

		internal bool isDirty;

		internal bool EnsureEntries()
		{
			if (Entries == null)
				Entries = new List<Entry>();
			var changed = false;
			foreach (var kvp in EditorGUIUtility.isProSkin ? DefaultThemeDark : DefaultThemeLight)
			{
				var token = kvp.Key;
				var hex = kvp.Value;
				if (Entries.Any(e => e.Key == token)) continue;
				ColorUtility.TryParseHtmlString(hex, out var color);
				Entries.Add(new Entry(token, color));
				changed = true;
			}

			isDirty |= changed;
			return isDirty;
		}


		[Serializable]
		public class Entry
		{
			public string Key;
			public Color Color;

			public Entry(string key, Color col)
			{
				this.Key = key;
				this.Color = col;
			}
		}
	}
}