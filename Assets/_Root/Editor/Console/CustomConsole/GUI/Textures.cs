using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class Textures
	{
		private static Texture2D eyeClosed;
		public static Texture2D EyeClosed
		{
			get
			{
				if (!eyeClosed) eyeClosed = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff");
				return eyeClosed;
			}
		}
		
		private static Texture2D eyeOpen;
		public static Texture2D EyeOpen
		{
			get
			{
				if (!eyeOpen) eyeOpen = EditorGUIUtility.FindTexture("animationvisibilitytoggleon");
				return eyeOpen;
			}
		}
		
		private static Texture2D solo;
		public static Texture2D Solo
		{
			get
			{
				if (!solo) solo = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.needle.console/Editor/CustomConsole/GUI/Textures/Solo.png");
				return solo;
			}
		}
		
		private static Texture2D remove;
		public static Texture2D Remove
		{
			get
			{
				if (!remove) remove = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.needle.console/Editor/CustomConsole/GUI/Textures/Remove.png");
				return remove;
			}
		}
	}
}