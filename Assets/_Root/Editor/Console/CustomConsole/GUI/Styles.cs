using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class Styles
	{
		private static GUIStyle toggleButton;
		
		internal const int RemoveIconWith = 15;

		public static GUIStyle FilterToggleButton(float height = -1)
		{
			if (toggleButton == null)
			{
				toggleButton = new GUIStyle(); //(EditorStyles.miniButton);
				toggleButton.alignment = TextAnchor.MiddleCenter;
				toggleButton.imagePosition = ImagePosition.ImageLeft;
				toggleButton.stretchHeight = toggleButton.stretchWidth = false;
				toggleButton.normal.textColor = Color.white;
			}
			
			toggleButton.fixedHeight = height < 0 ? EditorGUIUtility.singleLineHeight + 2 : height;
			return toggleButton;
		}
	}
}