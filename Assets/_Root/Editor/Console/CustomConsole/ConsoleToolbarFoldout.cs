using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class ConsoleToolbarFoldout
	{
		internal static void OnDrawFoldouts()
		{
			var rect = GUILayoutUtility.GetLastRect();
			rect.x += rect.width;
			var label = new GUIContent("Filter");
			rect.width = EditorStyles.toolbarDropDown.CalcSize(label).x;
			if (EditorGUI.DropdownButton(rect, label, FocusType.Keyboard, EditorStyles.toolbarDropDown))
			{
				var popupLocation = new[]
				{
#if UNITY_2019_4_OR_NEWER
					PopupLocation.BelowAlignLeft,
#else
					PopupLocation.Left,
#endif
					PopupLocation.Above
				};
				PopupWindow.Show(rect, new FilterFoldoutContent(), popupLocation);
			}

			GUILayout.Space(rect.width);
		}
	}
}