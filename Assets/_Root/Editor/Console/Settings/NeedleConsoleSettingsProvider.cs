using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Console
{
	public class NeedleConsoleSettingsProvider : SettingsProvider
	{
		public const string SettingsPath = "Preferences/Heart/Console";
		[SettingsProvider]
		public static SettingsProvider CreateDemystifySettings()
		{
			try
			{
				NeedleConsoleSettings.instance.Save();
				return new NeedleConsoleSettingsProvider(SettingsPath, SettingsScope.User);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			return null;
		}

		private NeedleConsoleSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
		{
		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			base.OnActivate(searchContext, rootElement);
			ThemeNames = null;
		}

		[MenuItem("Tools/Needle Console/Enable Development Mode", true)]
		private static bool EnableDevelopmentModeValidate() => !NeedleConsoleSettings.DevelopmentMode;
		[MenuItem("Tools/Needle Console/Enable Development Mode")]
		private static void EnableDevelopmentMode() => NeedleConsoleSettings.DevelopmentMode = true;
		[MenuItem("Tools/Needle Console/Disable Development Mode", true)]
		private static bool DisableDevelopmentModeValidate() => NeedleConsoleSettings.DevelopmentMode;
		[MenuItem("Tools/Needle Console/Disable Development Mode")]
		private static void DisableDevelopmentMode() => NeedleConsoleSettings.DevelopmentMode = false;

		private Vector2 scroll;

		public override void OnGUI(string searchContext)
		{
			base.OnGUI(searchContext);
			var settings = NeedleConsoleSettings.instance;

			EditorGUI.BeginChangeCheck();

			using (var s = new EditorGUILayout.ScrollViewScope(scroll))
			{
				scroll = s.scrollPosition;
				DrawActivateGUI(settings);
				DrawSyntaxGUI(settings);

				GUILayout.Space(10);
				EditorGUILayout.LabelField("Console Options", EditorStyles.boldLabel);
				settings.Separator = EditorGUILayout.TextField(new GUIContent("Stacktrace Separator", "Adds a separator to Console stacktrace output between each stacktrace"), settings.Separator);
				settings.ShortenFilePaths = EditorGUILayout.Toggle(new GUIContent("Shorten File Paths", "When enabled demystify tries to shorten package paths to <package_name>@<version> <fileName><line>"), settings.ShortenFilePaths); 
				settings.ShowLogPrefix = EditorGUILayout.Toggle(new GUIContent("Show Filename", "When enabled demystify will prefix console log entries with the file name of the log source"), settings.ShowLogPrefix); 
				
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Experimental", EditorStyles.boldLabel);
				settings.AllowCodePreview = EditorGUILayout.Toggle(new GUIContent("Code Preview", "Show code context in popup window when hovering over console log line with file path"), settings.AllowCodePreview);
				EditorGUI.BeginDisabledGroup(!settings.AllowCodePreview);
				EditorGUI.indentLevel++;
				settings.CodePreviewKeyCode = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Shortcut", "If None: code preview popup will open on hover. If any key assigned: code preview popup will only open if that key is pressed on hover"), settings.CodePreviewKeyCode);
				EditorGUI.indentLevel--;
				EditorGUI.EndDisabledGroup();
				using (var _scope = new EditorGUI.ChangeCheckScope())
				{
					settings.UseColorMarker = EditorGUILayout.Toggle(new GUIContent("Draw Color Marker"), settings.UseColorMarker);
					settings.ColorMarker = EditorGUILayout.TextField(new GUIContent("Color Marker", "Colored marker added before console log"), settings.ColorMarker);
					if(_scope.changed) NeedleConsoleProjectSettings.RaiseColorsChangedEvent();
				}
				
				settings.CustomConsole = EditorGUILayout.Toggle(new GUIContent("Custom Console", "The custom list replaces the console log drawing with a custom implementation that allows for advanced features such like very custom log filtering via context menus"), settings.CustomConsole);
				using (new EditorGUI.DisabledScope(!settings.CustomConsole))
				{
					EditorGUI.indentLevel++;
					settings.RowColors = EditorGUILayout.Toggle(new GUIContent("Row Colors", "Allow custom list to tint row background for warnings and errors"), settings.RowColors);
				
					settings.IndividualCollapse = EditorGUILayout.Toggle(new GUIContent("Individual Collapse", "When enabled the log context menu allows to collapse individual logs"), settings.IndividualCollapse);
					using (new EditorGUI.DisabledScope(!settings.IndividualCollapse))
					{
						EditorGUI.indentLevel++;
						settings.IndividualCollapsePreserveContext = EditorGUILayout.Toggle(new GUIContent("Keep Context", "When enabled collapsing will be interupted by other log messages"), settings.IndividualCollapsePreserveContext);
						EditorGUI.indentLevel--;
					}
				
					settings.UseCustomFont = EditorGUILayout.Toggle(new GUIContent("Use Custom Font", "Allow using a custom font. Specify a font name that you have installed below"), settings.UseCustomFont);
					using (new EditorGUI.DisabledScope(!settings.UseCustomFont))
					{
						EditorGUI.indentLevel++;
						var fontOptions = Font.GetOSInstalledFontNames();
						var selectedFont = EditorGUILayout.Popup(new GUIContent("Installed Fonts"), fontOptions.IndexOf(f => f == settings.InstalledLogEntryFont), fontOptions);
						if (selectedFont >= 0 && selectedFont < fontOptions.Length) settings.InstalledLogEntryFont = fontOptions[selectedFont];
						settings.CustomLogEntryFont = (Font) EditorGUILayout.ObjectField(new GUIContent("Custom Font", "Will override installed font" ), settings.CustomLogEntryFont, typeof(Font), false);
						EditorGUI.indentLevel--;
					}
					EditorGUI.indentLevel--;
				}
				
				if(NeedleConsoleSettings.DevelopmentMode)
				// using(new EditorGUI.DisabledScope(!settings.DevelopmentMode))
				{
					GUILayout.Space(10);
					EditorGUILayout.LabelField("Development Settings", EditorStyles.boldLabel);
					if (GUILayout.Button("Refresh Themes List"))
						Themes = null;
				}

				GUILayout.Space(20);
			}

			// GUILayout.FlexibleSpace();
			// EditorGUILayout.Space(10);
			// using (new EditorGUILayout.HorizontalScope())
			// {
			// 	settings.DevelopmentMode = EditorGUILayout.ToggleLeft("Development Mode", settings.DevelopmentMode);
			// }

			if (EditorGUI.EndChangeCheck())
			{
				settings.Save();
				NeedleConsoleSettings.RaiseChangedEvent();
			}
		}

		private static bool SyntaxHighlightSettingsThemeFoldout
		{
			get => SessionState.GetBool("NeedleConsole.SyntaxHighlightingThemeFoldout", false);
			set => SessionState.SetBool("NeedleConsole.SyntaxHighlightingThemeFoldout", value);
		}

		public static event Action ThemeEditedOrChanged;

		private static readonly string[] AlwaysInclude = new[] {"keywords", "link", "string_literal", "comment"};

		private static void DrawSyntaxGUI(NeedleConsoleSettings settings)
		{
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Syntax Highlighting", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			settings.SyntaxHighlighting = (Highlighting) EditorGUILayout.EnumPopup("Syntax Highlighting", settings.SyntaxHighlighting);
			if (EditorGUI.EndChangeCheck())
			{
				SyntaxHighlighting.OnSyntaxHighlightingModeHasChanged();
				ThemeEditedOrChanged?.Invoke();
			}
			DrawThemePopup();
			SyntaxHighlightSettingsThemeFoldout = EditorGUILayout.Foldout(SyntaxHighlightSettingsThemeFoldout, "Colors");
			if (SyntaxHighlightSettingsThemeFoldout)
			{
				var theme = settings.CurrentTheme;
				if (theme != null)
				{
					EditorGUI.BeginChangeCheck();
					EditorGUI.indentLevel++;
					DrawThemeColorOptions(theme);
					EditorGUI.indentLevel--;
					if (EditorGUI.EndChangeCheck())
					{
						theme.SetActive();
						ThemeEditedOrChanged?.Invoke();
					}
			
				}
			}
		}

		internal static void DrawThemeColorOptions(Theme theme, bool skipUnused = true)
		{
			var currentPattern = SyntaxHighlighting.CurrentPatternsList;
			for (var index = 0; index < theme.Entries?.Count; index++)
			{
				var entry = theme.Entries[index];
				var usedByCurrentRegex = AlwaysInclude.Contains(entry.Key) || (currentPattern?.Any(e => e.Contains("?<" + entry.Key)) ?? true);
				if (skipUnused  && !usedByCurrentRegex) continue;
				// using(new EditorGUI.DisabledScope(!usedByCurrentRegex))
				{
					var col = GUI.color;
					GUI.color = !usedByCurrentRegex || Theme.Ignored(entry.Color) ? Color.gray : col;
					entry.Color = EditorGUILayout.ColorField(entry.Key, entry.Color);
					GUI.color = col;
				}
			}
		}

		private static void DrawActivateGUI(NeedleConsoleSettings settings)
		{
			if (!settings.Enabled)// !UnityDemystify.Patches().All(PatchManager.IsActive))
			{
				if (GUILayout.Button(new GUIContent("Enable Needle Console")))
					NeedleConsole.Enable();
				EditorGUILayout.HelpBox("Needle Console is disabled, click the Button above to enable it", MessageType.Info);
			}
			else
			{
				if (GUILayout.Button(new GUIContent("Disable Needle Console")))
					NeedleConsole.Disable();
			}
		}
		
		
		
		/// <summary>
		/// this is just for internal use and "visualizing" via GUI
		/// </summary>
		internal static void ApplySyntaxHighlightingMultiline(ref string str, Dictionary<string, string> colorDict = null)
		{
			var lines = str.Split('\n');
			str = "";
			// Debug.Log("lines: " + lines.Count());
			foreach (var t in lines)
			{
				var line = t;
				var pathIndex = line.IndexOf("C:/git/", StringComparison.Ordinal);
				if (pathIndex > 0) line = line.Substring(0, pathIndex - 4);
				if (!line.TrimStart().StartsWith("at "))
					line = "at " + line;
				SyntaxHighlighting.AddSyntaxHighlighting(ref line, colorDict);
				line = line.Replace("at ", "");
				str += line + "\n";
			}
		}

		private static string[] ThemeNames;
		private static Theme[] Themes;

		private static void EnsureThemeOptions()
		{
			if (ThemeNames == null || Themes == null)
			{
				var themeAssets = AssetDatabase.FindAssets("t:" + nameof(SyntaxHighlightingTheme)).Select(AssetDatabase.GUIDToAssetPath).ToArray();
				ThemeNames = new string[themeAssets.Length];
				Themes = new Theme[ThemeNames.Length];
				for (var index = 0; index < themeAssets.Length; index++)
				{
					var path = themeAssets[index];
					var asset = AssetDatabase.LoadAssetAtPath<SyntaxHighlightingTheme>(path);
					if (asset.theme == null) asset.theme = new Theme("Unknown");
					ThemeNames[index] = asset.theme.Name;
					Themes[index] = asset.theme;
				}
			}
			else if(ThemeNames != null && Themes != null && ThemeNames.Length == Themes.Length)
			{
				for (var index = 0; index < Themes.Length; index++)
				{
					var t = Themes[index];
					ThemeNames[index] = t.Name;
				}
			}
		}

		private static int ActiveThemeIndex()
		{
			var active = NeedleConsoleSettings.instance.CurrentTheme;
			for (var index = 0; index < Themes.Length; index++) 
			{
				var theme = Themes[index];
				if (theme.Equals(active) || theme.Name == active.Name) return index; 
			}
			return -1;
		}

		private static void DrawThemePopup()
		{
			EnsureThemeOptions(); 
			EditorGUI.BeginChangeCheck(); 
			var selected = EditorGUILayout.Popup("Theme", ActiveThemeIndex(), ThemeNames);
			if(selected >= 0 && selected < Themes.Length)
				NeedleConsoleSettings.instance.CurrentTheme = Themes[selected];
			if (EditorGUI.EndChangeCheck())
			{
				NeedleConsoleSettings.instance.Save();
				ThemeEditedOrChanged?.Invoke();
			}
		}

		private class AssetProcessor : AssetPostprocessor
		{
			private void OnPreprocessAsset()
			{
				ThemeNames = null;
				Themes = null;
			}
		}
	}
}
