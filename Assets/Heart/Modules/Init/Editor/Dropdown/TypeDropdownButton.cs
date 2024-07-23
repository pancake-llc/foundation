using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class TypeDropdownButton : DropdownButton
	{
		public TypeDropdownButton
		(
			GUIContent prefixLabel, GUIContent buttonLabel,
			IEnumerable<Type> types, IEnumerable<Type> selectedItems,
			Action<Type> onSelectedItemChanged,
			string menuTitle,
			Func<Type, (string fullPath, Texture icon)> itemContentGetter
		)
			: base(prefixLabel, buttonLabel, (Rect belowRect) => TypeDropdownWindow.Show(belowRect, types, selectedItems, onSelectedItemChanged, menuTitle, itemContentGetter)) { }
	}
}