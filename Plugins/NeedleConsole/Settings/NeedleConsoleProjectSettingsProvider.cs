// using System;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
//
// namespace Needle.Demystify
// {
// 	public class DemystifyProjectSettingsProvider : SettingsProvider
// 	{
// 		public const string SettingsPath = "Project/Needle/Demystify";
//
// 		[SettingsProvider]
// 		public static SettingsProvider CreateDemystifySettings()
// 		{
// 			try
// 			{
// 				DemystifySettings.instance.Save();
// 				return new DemystifyProjectSettingsProvider(SettingsPath, SettingsScope.Project);
// 			}
// 			catch (Exception e)
// 			{
// 				Debug.LogException(e);
// 			}
//
// 			return null;
// 		}
//
// 		public DemystifyProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
// 		{
// 		}
//
// 		public override void OnGUI(string searchContext)
// 		{
// 			base.OnGUI(searchContext);
// 			EditorGUILayout.HelpBox("Global settings can be found in Preferences/Needle/Demystify", MessageType.Info);
// 			var settings = DemystifyProjectSettings.instance;
//
// 			EditorGUI.BeginChangeCheck();
//
// 			EditorGUILayout.LabelField("Log Colors", EditorStyles.boldLabel);
// 			settings.UseColors = EditorGUILayout.Toggle("Use Log Colors", settings.UseColors);
//
// 			if (!settings.UseColors) GUI.color = new Color(1, 1, 1, .5f);
// 			
// 			settings.ProceduralColors = EditorGUILayout.Toggle("Procedural Colors", settings.ProceduralColors);
//
// 			if (settings.ProceduralColors) GUI.color = new Color(1, 1, 1, .5f);
//
// 			GUILayout.Space(10);
// 			EditorGUILayout.LabelField("Custom Colors", EditorStyles.boldLabel);
// 			for (var index = 0; index < settings.LogColors.Count; index++)
// 			{
// 				var col = settings.LogColors[index];
// 				using (new GUILayout.HorizontalScope())
// 				{
// 					col.Key = EditorGUILayout.TextField(col.Key);
// 					col.Color = EditorGUILayout.ColorField(col.Color, GUILayout.MaxWidth(150));
// 					if (GUILayout.Button("x", GUILayout.Width(20)))
// 					{
// 						settings.LogColors.RemoveAt(index);
// 						index -= 1;
// 					}
// 				}
// 			}
//
// 			GUILayout.Space(5);
// 			if (GUILayout.Button("Add Color", GUILayout.Height(30)))
// 			{
// 				settings.LogColors.Add(new DemystifyProjectSettings.LogColor());
// 			}
// 			GUI.color = Color.white;
//
// 			if (EditorGUI.EndChangeCheck())
// 			{
// 				settings.Save();
// 				DemystifyProjectSettings.RaiseColorsChangedEvent();
// 			}
// 		}
// 	}
// }