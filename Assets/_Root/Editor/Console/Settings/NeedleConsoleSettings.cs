using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Needle.Console
{
	[FilePath("Preferences/NeedleConsoleSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
	internal class NeedleConsoleSettings : ScriptableSingleton<NeedleConsoleSettings>
	{
		[SerializeField]
		internal bool Enabled = true;
		
		public static event Action Changed;

		internal static void RaiseChangedEvent() => Changed?.Invoke();
		
		internal void Save()
		{
			Undo.RegisterCompleteObjectUndo(this, "Save Needle Console Settings");
			base.Save(true);
		}

		public static bool DevelopmentMode
		{
			get => SessionState.GetBool("Needle.Console.DevelopmentMode", false);
			set => SessionState.SetBool("Needle.Console.DevelopmentMode", value);
		}

		public Highlighting SyntaxHighlighting = Highlighting.Simple;
		public bool UseSyntaxHighlighting => SyntaxHighlighting != Highlighting.None;

		[SerializeField] private Theme Theme;

		public static event Action ThemeChanged;

		public Theme CurrentTheme
		{
			get
			{
				return Theme;
			}
			set
			{
				if (value == Theme) return;
				Theme = value;
				UpdateCurrentTheme();
			}
		}
 
		public bool SetDefaultTheme()
		{
			var theme = TryLoadDefaultThemeAssetInstance();
			if (theme != null)
			{
				CurrentTheme = theme;
				if (CurrentTheme.isDirty)
					CurrentTheme.SetActive();
				return true;
			}

			return false;
		}
		
		private static Theme TryLoadDefaultThemeAssetInstance()
		{
			var themes = AssetDatabase.FindAssets("t:" + nameof(SyntaxHighlightingTheme));
			foreach (var theme in themes)
			{
				var path = AssetDatabase.GUIDToAssetPath(theme);
				var name = Path.GetFileName(path);
				if (name.ToLowerInvariant().Contains("default"))
				{
					var loaded = AssetDatabase.LoadAssetAtPath<SyntaxHighlightingTheme>(path);
					// Debug.Log("Set default theme " + path + " loaded: " + loaded, loaded);
					if (loaded) return loaded.theme; 
				}
			}
			return null;
		}

		public void UpdateCurrentTheme()
		{
			Theme.EnsureEntries();
			Theme.SetActive();
			ThemeChanged?.Invoke();
			RaiseChangedEvent();
			InternalEditorUtility.RepaintAllViews();
		}

		public string Separator = "—";
		public bool AllowCodePreview = false;
		public KeyCode CodePreviewKeyCode = KeyCode.None;

		public bool ShortenFilePaths = true;
		[FormerlySerializedAs("ShowFileName")] public bool ShowLogPrefix = true;
		public bool UseColorMarker = true;
		public string ColorMarker = "┃";// "┃";
		
		[FormerlySerializedAs("CustomList")] public bool CustomConsole = true;
		public bool RowColors = true;
		public bool IndividualCollapse = true;
		/// <summary>
		/// collapsed logs will be broken by other logs
		/// </summary>
		public bool IndividualCollapsePreserveContext = true;
		
		[Header("Experimental")]
		public bool UseCustomFont = false;
		public string InstalledLogEntryFont = "Arial";
		public Font CustomLogEntryFont;
	}
	
}
