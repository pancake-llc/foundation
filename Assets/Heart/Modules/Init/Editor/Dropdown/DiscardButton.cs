using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	internal sealed class DiscardButton
	{
		public const float Width = 15f;

		private readonly Action discardValue;
		private readonly GUIContent tooltip;

		public DiscardButton(string tooltip, [DisallowNull] Action discardValue)
		{
			this.tooltip = new GUIContent("", tooltip);
			this.discardValue = discardValue;
		}

		public void Draw(Rect position)
		{
			var discardRect = position;
			discardRect.width = Width;
			discardRect.height -= 2f;
			discardRect.y += 2f;

			if(GUI.Button(discardRect, tooltip, Styles.Discard))
			{
				GUI.color = Color.white;
				discardValue();
			}
		}
	}
}