using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	/// <summary>
	/// patch that adds code preview support to console
	/// </summary>
	public class Patch_CodePreview : PatchBase
	{
		protected override IEnumerable<MethodBase> GetPatches()
		{
			MethodInfo doTextField;
			try
			{
				doTextField = AccessTools.Method(typeof(EditorGUI), "DoTextField");
			}
			catch (AmbiguousMatchException)
			{
				doTextField = AccessTools.Method(typeof(EditorGUI), "DoTextField", new Type[]  
				{
					// 2021.1 introduces a new overload here, so we need to specify explicitly which version we want.
					// (EditorGUI.RecycledTextEditor editor,											 int id,	  Rect position, string text,	 GUIStyle style,   string allowedLetters, out bool changed,                 bool reset,   bool multiline, bool passwordField)
					typeof(EditorGUI).Assembly.GetType("UnityEditor.EditorGUI+RecycledTextEditor"), typeof(int), typeof(Rect),  typeof(string), typeof(GUIStyle), typeof(string),        typeof(bool).MakeByRefType(),     typeof(bool), typeof(bool),   typeof(bool)
				});
			}

			yield return doTextField;
		}
		
		private static CodePreview.Window window;
		private static double lastTimeFoundKey;

		private static void ClearPopupWindow()
		{
			if (!window) return; 
			if (window.Text == null) return;
			window.Text = null;
			EditorUtility.SetDirty(window);
			window.Repaint();
		}
		
		// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/EditorGUI.cs#L790
		private static void Prefix(TextEditor editor, Rect position, GUIStyle style)
		{
			var settings = NeedleConsoleSettings.instance;
			if (!settings || !settings.AllowCodePreview) return;
			
			if (!Patch_Console.IsDrawingConsole && style.name != "CN Message") return;
			if (!Patch_Console.ConsoleWindow) return;
			
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == settings.CodePreviewKeyCode)
			{
				lastTimeFoundKey = DateTime.Now.TimeOfDay.TotalSeconds;
				EditorUtility.SetDirty(Patch_Console.ConsoleWindow);
				Patch_Console.ConsoleWindow.Repaint();
			}
			
			var evt = Event.current;
			var mouse = evt.mousePosition;
			if (!position.Contains(mouse))
			{
				ClearPopupWindow();
				return;
			}
			
			if (evt.type == EventType.Repaint && Patch_Console.ConsoleWindow)
			{
				EditorUtility.SetDirty(Patch_Console.ConsoleWindow);
				Patch_Console.ConsoleWindow.Repaint();
			}

			if (settings.CodePreviewKeyCode != KeyCode.None)
			{
				var time = DateTime.Now.TimeOfDay.TotalSeconds;
				if (Event.current.keyCode == settings.CodePreviewKeyCode)
				{
					lastTimeFoundKey = time;
				}

				var diff = time - lastTimeFoundKey;
				if (diff > .3f)
				{
					ClearPopupWindow();
					return;
				}
			}
			
			if (evt == null || (evt.type != EventType.Repaint && evt.type != EventType.MouseMove)) return;
			
			var stackViewRect = Patch_Console.GetStackScrollViewRect();

			if (!stackViewRect.Contains(mouse)) return;

			var prevSelection = editor.selectIndex;
			var prevSelectionEnd = editor.cursorIndex;
			editor.MoveCursorToPosition(mouse);
			editor.SelectCurrentParagraph();
			var selection = editor.SelectedText;
			editor.selectIndex = prevSelection;
			editor.cursorIndex = prevSelectionEnd;
			
			var matchCollection = new Regex("(?<=\\b=\")[^\"]*").Matches(selection);
			if (matchCollection.Count == 2)
			{
				var path = matchCollection[0].Value;
				if (int.TryParse(matchCollection[1].Value, out var line))
				{
					var prev = CodePreview.GetPreviewText(path, line, out var lines);
					if (!string.IsNullOrEmpty(prev) && !window)
					{
						window = CodePreview.Window.Create();
					}

					if (window)
					{
						var pos = GUIUtility.GUIToScreenPoint(mouse);
						window.Text = prev;
						window.Mouse = pos;
						window.ShowPopup();

						var cornerTopLeft = GUIUtility.GUIToScreenPoint(new Vector2(0, 0));
						// var consoleWindow = Patch_Console.ConsoleWindow.position;
						var width = (Screen.width - 2);
						const int padding = 0;
						var height = EditorGUIUtility.singleLineHeight * lines;
						height -= EditorGUIUtility.singleLineHeight;
						var rect = new Rect(Vector2.zero, new Vector2( width - padding, height));
						pos.x = cornerTopLeft.x + padding * .5f;
						const int linesDistance = 4;
						pos.y -= rect.height + EditorGUIUtility.singleLineHeight * linesDistance;
						// pos.y = rect.height - 1;
						// pos.y = consoleWindow.y;// cornerTopLeft.y - rect.height - 1;
						// rect.height = GUIUtility.GUIToScreenPoint(consoleWindow.position).y - cornerTopLeft.y;
						// var headerHeight = EditorGUIUtility.singleLineHeight * 2;
						// rect.height -= headerHeight;
						// pos.y += headerHeight;
						rect.position = pos;
						window.position = rect;
						EditorUtility.SetDirty(window);
						window.Repaint();
					}

					return;
				}
			}

			if (window)
			{
				ClearPopupWindow();
			}
		}
	}
}