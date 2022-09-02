using UnityEngine;

namespace Pancake.Init.EditorOnly
{
	internal static class Styles
	{
		internal readonly static GUIStyle ServiceTag;

		static Styles()
		{
			ServiceTag = new GUIStyle("AssetLabel");
			ServiceTag.contentOffset = new Vector2(0f, -1f);
		}
	}
}