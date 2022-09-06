using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Console
{
	internal class Patch_Console
	{
		
		internal static bool IsDrawingConsole { get; private set; }
		private static Type _consoleWindowType;

		internal static Type ConsoleWindowType
		{
			get
			{
				if (_consoleWindowType == null)
					_consoleWindowType = typeof(EditorWindow).Assembly.GetTypes().FirstOrDefault(t => t.FullName == "UnityEditor.ConsoleWindow");
				return _consoleWindowType;
			}
			set => _consoleWindowType = value;
		}

		private static EditorWindow _consoleWindow;

		internal static EditorWindow ConsoleWindow
		{
			get
			{
				if (!_consoleWindow)
				{
					if (ConsoleWindowType != null)
						_consoleWindow = EditorWindow.GetWindow(ConsoleWindowType);
				}

				return _consoleWindow; 
			}
			private set => _consoleWindow = value;
		}

		private static readonly Type SplitterStateType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SplitterState");
		private static readonly FieldInfo SplitterState = ConsoleWindowType.GetField("spl", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo SplitterRelativeSizes = SplitterStateType.GetField("relativeSizes", BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo TextScroll = ConsoleWindowType.GetField("m_TextScroll", BindingFlags.NonPublic | BindingFlags.Instance);

		public static Rect GetStackScrollViewRect()
		{
			var rect = ConsoleWindow.position;

			var splitState = SplitterState.GetValue(ConsoleWindow);
#if UNITY_2020_2_OR_NEWER
			var splitRealSizes = (float[]) SplitterRelativeSizes.GetValue(splitState);
#else
			var splitRealSizes = (int[]) SplitterRelativeSizes.GetValue(splitState);
#endif

			var stackViewSize = splitRealSizes[1] * rect.height;

			rect.x = 0;
			rect.y = GetStackTextScroll().y;
			rect.height = stackViewSize;

			return rect;
		}

		private static Vector2 GetStackTextScroll()
		{
			return (Vector2) TextScroll.GetValue(ConsoleWindow);
		}

		private class ConsoleDrawingEvent : PatchBase
		{
			protected override IEnumerable<MethodBase> GetPatches()
			{
				yield return ConsoleWindowType?.GetMethod("OnGUI", (BindingFlags) ~0);;
			}

			private static void Prefix()
			{
				IsDrawingConsole = true;

			}

			private static void Postfix()
			{
				IsDrawingConsole = false;
			}
		}

		private class StacktracePatch : PatchBase
		{
			protected override IEnumerable<MethodBase> GetPatches()
			{
				// broke somewhere between 2020.3.4f1 and 2020.3.10f1
				// could #ifdef but seems Unity changed this relatively randomly in some 2020.3 and some 2021.1, so just trying both feels easier
				var methodWithIntParameter = ConsoleWindowType?.GetMethod("StacktraceWithHyperlinks", (BindingFlags) ~0, null, new[] {typeof(string), typeof(int)}, null);
				if (methodWithIntParameter != null)
				{
					yield return methodWithIntParameter;
					yield break;
				}
				
				var methodWithoutIntParameter = ConsoleWindowType?.GetMethod("StacktraceWithHyperlinks", (BindingFlags) ~0, null, new[] {typeof(string)}, null);
				if (methodWithoutIntParameter != null)
					yield return methodWithoutIntParameter;
			}

			private static string lastText;
			private static string lastResult;

			public StacktracePatch()
			{
				NeedleConsoleSettingsProvider.ThemeEditedOrChanged += Repaint;

				void Repaint()
				{
					lastText = null;
					if (ConsoleWindowType != null)
					{
						if (ConsoleWindow)
							ConsoleWindow.Repaint();
					}
				}
			}

			private static bool Prefix(ref string stacktraceText)
			{
				if (NeedleConsoleSettings.instance.CustomConsole == false)
				{
					var r = GetStackScrollViewRect();
					SeparatorLine.Draw(r.y);
				}
				
				var textChanged = lastText != stacktraceText;
				if (textChanged)
				{
					lastText = stacktraceText;
					NeedleConsole.Apply(ref stacktraceText);
					lastResult = stacktraceText;
				}

				stacktraceText = lastResult;
				return true;
			}

			private static void Postfix(ref string __result)
			{
				if (NeedleConsoleSettings.instance.SyntaxHighlighting != Highlighting.None)
					Hyperlinks.ApplyHyperlinkColor(ref __result);
			}
		}
	}
}