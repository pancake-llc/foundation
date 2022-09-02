using System.Collections.Generic;
using JetBrains.Annotations;
using Pancake.Editor;
using UnityEditor;
using UnityEngine;

namespace Pancake.Init.EditorOnly
{
	[InitializeOnLoad]
	internal static class RefTagDrawer
	{
		private static readonly List<RefTag> tags = new List<RefTag>();

		static RefTagDrawer()
		{
			ComponentHeader.BeforeHeaderGUI -= OnBeforeComponentHeaderGUI;
			ComponentHeader.BeforeHeaderGUI += OnBeforeComponentHeaderGUI;
			ComponentHeader.AfterHeaderGUI -= OnAfterComponentHeaderGUI;
			ComponentHeader.AfterHeaderGUI += OnAfterComponentHeaderGUI;
			UnityEditor.Editor.finishedDefaultHeaderGUI -= OnAfterInspectorRootEditorHeaderGUI;
			UnityEditor.Editor.finishedDefaultHeaderGUI += OnAfterInspectorRootEditorHeaderGUI;
		}

		private static void OnAfterInspectorRootEditorHeaderGUI(UnityEditor.Editor editor)
		{
			if(editor.target is GameObject)
			{
				// Handle InspectorWindow
				OnAfterGameObjectHeaderGUI(editor);
			}
		}

		private static void OnAfterGameObjectHeaderGUI([NotNull] UnityEditor.Editor gameObjectEditor)
		{
			GameObject gameObject = gameObjectEditor.target as GameObject;
			if(!TryGetRefTag(gameObject, gameObject, out RefTag referenceable))
			{
				return;
			}

			var refLabel = GetRefLabel(referenceable);
			bool isAddressableAssetOrPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject);
			#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
			isAddressableAssetOrPrefabInstance |= PrefabUtility.IsPartOfPrefabAsset(gameObject);
			#endif
			var refRect = GetRefRectForGameObject(refLabel, Styles.ServiceTag, isAddressableAssetOrPrefabInstance);
			DrawRefLabel(refLabel, refRect);
			HandleContextMenu(referenceable, refRect);
		}

		private static float OnBeforeComponentHeaderGUI(Component component, Rect headerRect, bool HeaderIsSelected, bool supportsRichText)
		{
			if(!TryGetRefTag(component.gameObject, component, out RefTag referenceable))
			{
				return 0f;
			}

			var refLabel = GetRefLabel(referenceable);
			var refRect = GetRefRectForComponent(component, headerRect, refLabel, Styles.ServiceTag);
			HandleContextMenu(referenceable, refRect);
			return 0f;
		}

		private static float OnAfterComponentHeaderGUI(Component component, Rect headerRect, bool HeaderIsSelected, bool supportsRichText)
		{
			if(!TryGetRefTag(component.gameObject, component, out RefTag referenceable))
			{
				return 0f;
			}

			var refLabel = GetRefLabel(referenceable);
			var refRect = GetRefRectForComponent(component, headerRect, refLabel, Styles.ServiceTag);
			DrawRefLabel(refLabel, refRect);
			return 0f;
		}

		private static void DrawRefLabel(GUIContent refLabel, Rect refRect)
		{
			GUI.BeginClip(refRect);
			refRect.x = 0f;
			refRect.y = 0f;
			GUI.Label(refRect, refLabel, Styles.ServiceTag);
			GUI.EndClip();
		}

		private static void HandleContextMenu(RefTag referenceable, Rect refRect)
		{
			if(Event.current.type == EventType.MouseDown && refRect.Contains(Event.current.mousePosition))
			{
				Event.current.Use();

				// TODO: Add menu item for finding all references.
				// TODO: Ping scenes that reference this on click? (each additional click pings another one?)
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Copy"), false, CopyToClipboard);
				menu.AddItem(new GUIContent("Delete"), false, Delete);
				menu.DropDown(refRect);
			}

			void CopyToClipboard() => GUIUtility.systemCopyBuffer = referenceable.guid.ToString();

			void Delete()
			{
				if(!EditorUtility.DisplayDialog("Delete Cross-Scene Reference Id?", "Are you sure you want to delete the cross-scene reference id from this GameObject?\n\nAny references to this object from other scenes and assets will be broken.", "Delete", "Cancel"))
				{
					return;
				}

				if(!Application.isPlaying || AssetDatabase.IsMainAsset(referenceable.gameObject))
				{
					Undo.DestroyObjectImmediate(referenceable);
					return;
				}

				Object.Destroy(referenceable);
			}
		}

		private static GUIContent GetRefLabel(RefTag referenceable) => new GUIContent("Ref", "Cross-Scene Reference Id:\n" + referenceable.Guid.ToString());

		private static Rect GetRefRectForGameObject(GUIContent label, GUIStyle style, bool isAddressableAssetOrPrefabInstance)
		{
			GUILayout.Label(" ", GUILayout.Height(0f));
			var labelRect = GUILayoutUtility.GetLastRect();
			labelRect.size = style.CalcSize(label);
			labelRect.x += 4f;
			labelRect.y -= labelRect.height - 4f;
			
			if(isAddressableAssetOrPrefabInstance)
			{
				labelRect.y -= 23f;
			}

			return labelRect;
		}

		private static Rect GetRefRectForComponent(Component component, Rect headerRect, GUIContent label, GUIStyle style)
        {
			var componentTitle = new GUIContent(ObjectNames.GetInspectorTitle(component));
			float componentTitleEndX = 54f + EditorStyles.largeLabel.CalcSize(componentTitle).x + 10f;
			float availableSpace = headerRect.width - componentTitleEndX - 69f;
			float labelWidth = style.CalcSize(label).x;
			if(labelWidth > availableSpace)
			{
				labelWidth = availableSpace;
			}
			const float MinWidth = 18f;
			if(labelWidth < MinWidth)
			{
				labelWidth = MinWidth;
			}

			var labelRect = headerRect;
			labelRect.x = headerRect.width - 69f - labelWidth;
			#if POWER_INSPECTOR
			labelRect. x -= EditorGUIUtility.singleLineHeight; // add room for Debug Mode+ button
			#endif
			labelRect.y += 4f;

			// Fixes Transform header label rect position.
			// For some reason the Transform header rect starts
			// lower and is shorter than all other headers.
			if(labelRect.height < 22f)
            {
                labelRect.y -= 22f - 15f;
            }

            labelRect.height = 20f;
			labelRect.width = labelWidth;

			return labelRect;
        }

		private static bool TryGetRefTag(GameObject gameObject, Object target, out RefTag result)
		{
			gameObject.GetComponents(tags);

			foreach(var referenceable in tags)
			{
				if(referenceable.Target == target)
				{
					tags.Clear();
					result = referenceable;
					return true;
				}
			}

			tags.Clear();
			result = null;
			return false;
		}
	}
}