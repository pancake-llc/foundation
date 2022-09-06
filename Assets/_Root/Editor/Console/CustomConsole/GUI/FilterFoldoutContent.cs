using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal class FilterFoldoutContent : PopupWindowContent
	{
		public override Vector2 GetWindowSize()
		{
			// var enabled = DemystifySettings.instance.CustomList;
			// var noConfig = ConsoleFilterConfig.AllConfigs.Count <= 0;
			// if (!enabled) return new Vector2(150, EditorGUIUtility.singleLineHeight * 1.3f);
			return new Vector2(400, 300);
		}

		private Vector2 scroll;

		private bool configsFoldout
		{
			get => SessionState.GetBool("ConsoleFilterConfigListFoldout", false);
			set => SessionState.SetBool("ConsoleFilterConfigListFoldout", value);
		}

		public override void OnOpen()
		{
			base.OnOpen();
		}

		internal static event Action<Rect> FoldoutOnGUI;

		public override void OnGUI(Rect rect)
		{
			var enabled = NeedleConsoleSettings.instance.CustomConsole;
			if (!enabled)
			{
				EditorGUILayout.HelpBox("To support console filtering you need to enable \"Custom List\" in settings", MessageType.Warning);
				if (GUILayout.Button("Enable Custom List", GUILayout.Height(30)))
				{
					NeedleConsoleSettings.instance.CustomConsole = true;
				}
			}

			// if (ConsoleFilterConfig.AllConfigs.Count <= 0)
			// {
			// 	if (enabled)
			// 	{
			// 		GUILayout.FlexibleSpace();
			// 		if (GUILayout.Button(new GUIContent("Create Filter Config",
			// 			"A filter config is used to store your settings for filtering console logs. Don't worry, logs are not deleted or anything, they will just not be shown when filtered and this is can be changed at any time")))
			// 		{
			// 			var config = ConsoleFilterConfig.CreateAsset();
			// 			if (config)
			// 				config.Activate();
			// 		}
			// 		GUILayout.FlexibleSpace();
			// 	}
			//
			// 	return;
			// }

			scroll = EditorGUILayout.BeginScrollView(scroll);

			if (ConsoleFilterPreset.AllConfigs.Count > 0)
			{
				configsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(configsFoldout, "Presets", null, r =>
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("New"), false, () => ConsoleFilterPreset.CreateAsset());
					menu.DropDown(r);
				});
				if (configsFoldout)
				{
					EditorGUI.indentLevel++;
					foreach (var config in ConsoleFilterPreset.AllConfigs)
					{
						// using (new GUILayout.HorizontalScope())
						{
							Rect labelRect;
							using (new GUILayout.HorizontalScope())
							{
								GUILayout.Space(16);
								GUILayout.Label(config.name);
								labelRect = GUILayoutUtility.GetLastRect();
								GUILayout.FlexibleSpace();
								if (GUILayout.Button("From Preset"))
								{
									config.Apply();
								}
								if (GUILayout.Button("Save To"))
								{
									ConsoleFilterUserSettings.instance.SaveToPreset(config);
								}
							}

							if (Event.current.type == EventType.MouseDown)
							{
								if (labelRect.Contains(Event.current.mousePosition))
								{
									if (Event.current.button == 0)
										EditorGUIUtility.PingObject(config);
								}
							}
						}
					}
					EditorGUI.indentLevel--;

				}

				EditorGUILayout.EndFoldoutHeaderGroup();
			}
			
			Draw.FilterList(ConsoleFilter.RegisteredFilter);
			
			FoldoutOnGUI?.Invoke(rect);

			EditorGUILayout.EndScrollView();
		}
	}
}