using UnityEditor;
using UnityEngine;
using Sisus.Init.Internal;
using Sisus.Init.EditorOnly.Internal;

namespace Sisus.Init.EditorOnly
{
	[CustomPropertyDrawer(typeof(Id))]
	public sealed class IdDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			int a = property.FindPropertyRelative(nameof(a)).intValue;
			short b = (short)property.FindPropertyRelative(nameof(b)).intValue;
			short c = (short)property.FindPropertyRelative(nameof(c)).intValue;
			byte d = (byte)property.FindPropertyRelative(nameof(d)).intValue;
			byte e = (byte)property.FindPropertyRelative(nameof(e)).intValue;
			byte f = (byte)property.FindPropertyRelative(nameof(f)).intValue;
			byte g = (byte)property.FindPropertyRelative(nameof(g)).intValue;
			byte h = (byte)property.FindPropertyRelative(nameof(h)).intValue;
			byte i = (byte)property.FindPropertyRelative(nameof(i)).intValue;
			byte j = (byte)property.FindPropertyRelative(nameof(j)).intValue;
			byte k = (byte)property.FindPropertyRelative(nameof(k)).intValue;

			var value = new Id(a, b, c, d, e, f, g, h, i, j, k);
			var controlRect = EditorGUI.PrefixLabel(position, label);
			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.TextField(controlRect, value.ToString());
			EditorGUI.EndDisabledGroup();
		}
	}
}