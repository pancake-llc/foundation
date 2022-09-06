using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Console
{
	public class TempFilterWindow : EditorWindow
	{
		[MenuItem("internal:Tools/Needle Console/Console Filter Test Window")]
		private static void Open()
		{
			var window = CreateWindow<TempFilterWindow>();
			window.Show();
		}

		private Vector2 scroll;

		private void OnEnable()
		{
			titleContent = new GUIContent("Console Filter");
		}

		private void OnGUI()
		{
			scroll = EditorGUILayout.BeginScrollView(scroll);
			EditorGUI.BeginChangeCheck();
			ConsoleList.DrawCustom = EditorGUILayout.Toggle("Custom List", ConsoleList.DrawCustom);
			GUILayout.Space(10);

			Draw.FilterList(ConsoleFilter.RegisteredFilter);

			if (EditorGUI.EndChangeCheck())
			{
				ConsoleFilter.MarkDirty();
				InternalEditorUtility.RepaintAllViews();
			}

			EditorGUILayout.EndScrollView();
		}
	}
}