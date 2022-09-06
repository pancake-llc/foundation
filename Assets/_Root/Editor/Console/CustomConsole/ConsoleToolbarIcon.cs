using System;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class ConsoleToolbarIcon
	{
		private static bool filterTextureInit;
		private static Texture2D filterIcon, filterIconDisabled;
		private static GUIStyle filterButtonStyle;

		internal static void OnDrawToolbar()
		{
			if (!NeedleConsoleSettings.instance.CustomConsole) return;
			
			if (!filterTextureInit)
			{
				filterTextureInit = true;
				filterIcon = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff");
				filterIconDisabled = EditorGUIUtility.FindTexture("animationvisibilitytoggleon");
			}

			var count = ConsoleFilter.HiddenCount;
			var aboveThreshold = count >= 1000;
			var text = " " + (aboveThreshold ? "999+" : count.ToString());
			var icon = ConsoleFilter.enabled ? filterIcon : filterIconDisabled;
			var tooltip = count > 1 ? count + " logs" : count + " log";
			if (ConsoleFilter.enabled) tooltip += " hidden";
			else tooltip += " would be hidden";
			var content = new GUIContent(text, icon, tooltip);

			if (filterButtonStyle == null)
			{
#if UNITY_2019_4_OR_NEWER
				filterButtonStyle = new GUIStyle(ConsoleWindow.Constants.MiniButtonRight);
#else
				filterButtonStyle = new GUIStyle(ConsoleWindow.Constants.MiniButton);
#endif
				filterButtonStyle.alignment = TextAnchor.MiddleLeft;
			}

			var width = count < 100 ? new[] { GUILayout.MinWidth(52) } : Array.Empty<GUILayoutOption>();
			ConsoleFilter.enabled = !GUILayout.Toggle(!ConsoleFilter.enabled, content, filterButtonStyle, width);
			
			// var rect = GUILayoutUtility.GetLastRect();
			// rect.x += rect.width;
			// rect.width = 50;
			// GUILayout.Space(rect.width);
			// if (EditorGUI.DropdownButton(rect, new GUIContent("Filter"), FocusType.Passive, EditorStyles.toolbarDropDown))
			// {
			// 	PopupWindow.Show(rect, new FilterFoldoutContent(), new[] {PopupLocation.Below});
			// }
		}
	}
}