using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class CodePreview
	{
		private static readonly Dictionary<string, string[]> cache = new Dictionary<string, string[]>();

		public static string GetPreviewText(string filePath, int lineNumber, out int lines)
		{
			lines = 0;
			if (!cache.ContainsKey(filePath))
			{
				if (!File.Exists(filePath)) return null;
				var content = File.ReadAllLines(filePath);
				cache.Add(filePath, content);
			}

			var windowText = GetText(cache[filePath], lineNumber - 1, 8, out lines);
			return windowText;
		}

		private static string TypesPatterns;

		private static string GetText(IReadOnlyList<string> lines, int line, int showLinesAboveAndBelow, out int lineCount)
		{
			if (TypesPatterns == null)
			{
				var patterns = SyntaxHighlighting.GetCodeSyntaxHighlightingPatterns();
				TypesPatterns = string.Join("|", patterns);
				if(NeedleConsoleSettings.DevelopmentMode)
					Debug.Log("Code Preview Patterns: " + TypesPatterns);
			}
				
			lineCount = 0;
			if (lines == null || lines.Count <= 0) return null;
			showLinesAboveAndBelow = Mathf.Max(0, showLinesAboveAndBelow);
			var from = Mathf.Max(0, line - showLinesAboveAndBelow);
			var to = Mathf.Min(lines.Count - 1, line + showLinesAboveAndBelow);
			var str = string.Empty;
			var lastLineWasEmpty = false;
			for (var index = from; index < to; index++)
			{
				var l = lines[index];
				var empty = string.IsNullOrWhiteSpace(l);
				if (lastLineWasEmpty && empty) continue;
				lastLineWasEmpty = empty;
				// if (index == line) l = $"<color={HighlightTextColor}><b>{l}</b></color>";
				// else
				{
					SyntaxHighlighting.AddSyntaxHighlighting(TypesPatterns, SyntaxHighlighting.CurrentTheme, ref l, false);
					if (index == line) l = "<b>" + l + "</b>";
					// l = $"<color={NormalTextColor}>{l}</color>";
				}
				str += l + "\n";
				lineCount += 1;
			}

			return str;
		}


		internal class Window : EditorWindow
		{
			// Hacky workaround to remove instances created during previous
			[InitializeOnLoadMethod]
			private static async void InitStart()
			{
				await Task.Delay(100);
				foreach (var prev in Resources.FindObjectsOfTypeAll<Window>())
				{
					if (prev)
						prev.Close();
				}
			}

			internal static Window Create()
			{
				var window = CreateInstance<Window>();
				window.minSize = Vector2.zero;
				window.ShowPopup();
				style = EditorStyles.wordWrappedLabel;
				style.richText = true;
				return window;
			}
			
			public string Text;
			public Vector2 Mouse;

			private static GUIStyle style;


			private void OnGUI()
			{
				if (string.IsNullOrEmpty(Text))
				{
					position = Rect.zero;
					minSize = Vector2.zero;
					return;
				}

				EditorGUI.DrawRect(new Rect(0,0, position.width,position.height), new Color(0,0,0,.2f));
				DrawBorders(Color.gray * .8f, 1);
				EditorGUILayout.LabelField(Text, style);
			}

			private void DrawBorders(Color color, float thickness)
			{
				EditorGUI.DrawRect(new Rect(0, 0, position.width, thickness), color);
				EditorGUI.DrawRect(new Rect(0, position.height - thickness, position.width, thickness), color);
				EditorGUI.DrawRect(new Rect(0, 0, thickness, position.height), color);
				EditorGUI.DrawRect(new Rect(position.width - thickness, 0, thickness, position.height), color);
			}
		}
	}
}