using Pancake.Editor;
using UnityEditor;
using UnityEngine;

namespace Pancake.Console
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
				if (!solo) solo = InEditor.FindAssetWithPath<Texture2D>("Solo.png", "Editor/Console/CustomConsole/GUI/Textures");
				return solo;
			}
		}
		
		private static Texture2D remove;
		public static Texture2D Remove
		{
			get
			{
				if (!remove) remove = InEditor.FindAssetWithPath<Texture2D>("Remove.png", "Editor/Console/CustomConsole/GUI/Textures");
				return remove;
			}
		}
	}
}