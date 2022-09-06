using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal class AdvancedLogDrawer : ICustomLogDrawer
	{
		private readonly float graphHeight = 60;

		public float GetContentHeight(float defaultRowHeight, int totalRows, out uint linesHandled)
		{
			linesHandled = 0;
			return defaultRowHeight;
			// linesHandled = (uint)ConsoleList.CurrentEntries.Count(e => e.groupSize > 0);
			// return linesHandled * graphHeight;
		}

		private float height;

		public bool OnDrawStacktrace(int index, string rawText, Rect rect)
		{
			if (index < 0 || index >= ConsoleList.CurrentEntries.Count) return false;
			var log = ConsoleList.CurrentEntries[index];
			var custom = log.collapseCount > 0;
			if (!custom) return false;
			var graphRect = new Rect(1, 1, Screen.width, graphHeight);
			// var totalHeight = graphRect.height * 2 + stacktraceHeight;
			// if (totalHeight > rect.height)
			graphRect.width -= 13; // scrollbar
			if (Event.current.type == EventType.Repaint)
			{
				DrawGraph(index, rect, out height);
			}

			GUILayout.Space(height);

			// GUILayout.Space(graphHeight + EditorGUIUtility.singleLineHeight);
			ConsoleList.DrawDefaultStacktrace(log.entry.message);
			return true;
		}

		public bool OnDrawEntry(int index, bool isSelected, Rect rect, bool visible, out float height)
		{
			height = 0;
			return false;
		}

		private static readonly List<ILogData<float>> floatValues = new List<ILogData<float>>(300);
		private static readonly List<ILogData<Vector3>> vecValues = new List<ILogData<Vector3>>(300);
		private static readonly List<ILogData<string>> values = new List<ILogData<string>>(300);
		private static readonly StringBuilder plain = new StringBuilder();
		private static GUIStyle labelStyle;

		private void DrawGraph(int index, Rect rect, out float height)
		{
			height = 0;
			if (!AdvancedLogCollapse.LogsData.ContainsKey(index)) return;
			var data = AdvancedLogCollapse.LogsData[index];

			if (labelStyle == null)
			{
				labelStyle = new GUIStyle(EditorStyles.label);
			}

			// GraphUtils.DrawRect(rect, new Color(0,0,0,.1f));
			// GraphUtils.DrawOutline(rect, new Color(.7f,.7f,.7f,.3f));

			var list = values;
			data.TryGetData(list, 0);

			var plainTextRect = new Rect(0, 0, rect.width, EditorGUIUtility.singleLineHeight);
			plain.Clear();
			for (var i = list.Count - 1; i >= 0; i--)
			{
				var e = list[i];
				plain.AppendLine(e.Value);
			}

			var content = new GUIContent(plain.ToString());
			plainTextRect.height = labelStyle.CalcHeight(content, plainTextRect.width);
			GUI.Label(plainTextRect, content, labelStyle);
			height = plainTextRect.height;
			plain.Clear();

			// GraphUtils.DrawGraph(rect, floatValues, -1, 1, Color.white);

			// floatValues.Clear();
			// data.GetData(floatValues, 1);
			// GraphUtils.DrawGraph(rect, floatValues, -1, 1, Color.gray);
		}

		


		private static DateTime playmodeStartTime;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void EnterPlaymode()
		{
			playmodeStartTime = DateTime.Now;
		}
	}
}