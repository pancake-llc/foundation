using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pancake.Init.Internal;
using Pancake.Editor;
using Pancake.Init;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.Init
{
	[InitializeOnLoad]
	internal static class ServiceTagDrawer
	{
		private static readonly GUIContent refLabel = new GUIContent("Ref");
		private static readonly List<RefTag> tags = new List<RefTag>();
		private static readonly StringBuilder sb = new StringBuilder();

		private static readonly GUIContent tagIdleLabel = new GUIContent("Service");

		private static readonly Dictionary<Clients, GUIContent> mouseoverLabels = new Dictionary<Clients, GUIContent>()
		{
			{ Clients.InGameObject, new GUIContent("GameObject") },
			{ Clients.InChildren, new GUIContent("Children") },
			{ Clients.InParents, new GUIContent("Parents") },
			{ Clients.InScene, new GUIContent("Scene") },
			{ Clients.InAllScenes, new GUIContent("Scenes") },
			{ Clients.InHierarchyRootChildren, new GUIContent("Root Children") },
			{ Clients.Everywhere, new GUIContent("Everywhere") },
		};

		static ServiceTagDrawer()
		{
			ComponentHeader.BeforeHeaderGUI -= OnBeforeHeaderGUI;
			ComponentHeader.BeforeHeaderGUI += OnBeforeHeaderGUI;
			ComponentHeader.AfterHeaderGUI -= OnAfterHeaderGUI;
			ComponentHeader.AfterHeaderGUI += OnAfterHeaderGUI;
		}

		private static float OnBeforeHeaderGUI(Component component, Rect headerRect, bool HeaderIsSelected, bool supportsRichText)
		{
			// Only draw the Service tag for components that have
			// a ServiceTag, components whose class has the ServiceAttribute
			// and components listed in a Services component.
			var definingTypes = ServiceTagUtility.GetServiceDefiningTypes(component);
			if(!definingTypes.Any() && ServiceTagUtility.openSelectTagsMenuFor != component)
			{
				return 0f;
			}

			if(headerRect.Contains(Event.current.mousePosition))
			{
				GUI.changed = true;
			}

			var tagLabel = GetTagLabel(component, headerRect, definingTypes);
			var tagRect = GetTagRect(component, headerRect, tagLabel, Styles.ServiceTag);

			if(ServiceTagUtility.openSelectTagsMenuFor == component)
			{
				ServiceTagUtility.openSelectTagsMenuFor = null;
				ServiceTagUtility.OpenSelectTagsMenu(component, tagRect);
				return 0f;
			}

			if(GUI.Button(tagRect, GUIContent.none, EditorStyles.label))
			{
				switch(Event.current.button)
				{
					case 0:
						ServiceTagUtility.OpenToClientsMenu(component, tagRect);
						break;
					case 1:
						ServiceTagUtility.OpenContextMenu(component, tagRect);
						break;
					case 2:
						ServiceTagUtility.PingDefiningObject(component);
						break;
				}
			}

			return 0f;
		}

		private static float OnAfterHeaderGUI(Component component, Rect headerRect, bool HeaderIsSelected, bool supportsRichText)
		{
			var definingTypes = ServiceTagUtility.GetServiceDefiningTypes(component);
			if(!definingTypes.Any())
			{
				return 0f;
			}

			var tagLabel = GetTagLabel(component, headerRect, definingTypes);
			var tagRect = GetTagRect(component, headerRect, tagLabel, Styles.ServiceTag);
			GUI.BeginClip(tagRect);
			tagRect.x = 0f;
			tagRect.y = 0f;
			GUI.Label(tagRect, tagLabel, Styles.ServiceTag);
			GUI.EndClip();
			return 0f;
		}

		private static GUIContent GetTagLabel(Component component, Rect headerRect, IEnumerable<Type> definingTypes)
		{
			var mouseoverRect = GetTagRect(component, headerRect, tagIdleLabel, Styles.ServiceTag);
			bool isMouseovered = mouseoverRect.Contains(Event.current.mousePosition);
			var toClients = GetClientVisibility(component, definingTypes);
			var label = isMouseovered ? mouseoverLabels[toClients] : tagIdleLabel;
			label.tooltip = isMouseovered ? GetTooltip(definingTypes, toClients) : "";
			return label;
		}

		private static string GetTooltip(IEnumerable<Type> definingTypes, Clients toClients)
		{
			var enumerator = definingTypes.GetEnumerator();
			enumerator.MoveNext();
			var first = enumerator.Current;

			sb.Append("Available ");
			switch(toClients)
			{
				case Clients.InGameObject:
					sb.Append("in this GameObject");
					break;
				case Clients.InChildren:
					sb.Append("in children");
					break;
				case Clients.InParents:
					sb.Append("in parents");
					break;
				case Clients.InHierarchyRootChildren:
					sb.Append("in hierarchy root children");
					break;
				case Clients.InScene:
					sb.Append("in all scenes");
					break;
				case Clients.InAllScenes:
					sb.Append("in all scenes");
					break;
				case Clients.Everywhere:
					sb.Append("everywhere");
					break;
				default:
					sb.Append(ObjectNames.NicifyVariableName(toClients.ToString()).ToLowerInvariant());
					break;
			}

			sb.Append("\nas ");

			if(!enumerator.MoveNext())
			{
				sb.Append(TypeUtility.ToString(first));
			}
			else
			{
				sb.Append(TypeUtility.ToString(first));

				do
				{
					sb.Append("\nor ");
					sb.Append(TypeUtility.ToString(enumerator.Current));
				}
				while(enumerator.MoveNext());
			}

			sb.Append('.');

			string result = sb.ToString();
			sb.Clear();
			return result;
		}

		private static Rect GetTagRect(Component component, Rect headerRect, GUIContent label, GUIStyle style)
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

			if(ContainsRefTag(component))
			{
				var refTagRect = ServiceTagUtility.GetTagRect(component, headerRect, refLabel, style);
				labelRect.x -= refTagRect.width + 5f;
			}

			return labelRect;
        }

		private static Clients GetClientVisibility(Component component, IEnumerable<Type> definingTypes)
		{
			Clients? result = null;

			foreach(var serviceTag in ServiceTagUtility.GetServiceTags(component))
			{
				var toClients = (Clients)serviceTag.ToClients;
				if(!result.HasValue || toClients.CompareTo(result.Value) < 0)
				{
					result = toClients;
				}
			}

			if(result.HasValue)
			{
				return result.Value;
			}

			foreach(var definingType in definingTypes)
			{
				if(Services.TryGetServiceToClientVisibility(component, definingType, out var toClients))
				{
					return toClients;
				}
			}

			return Clients.Everywhere;
		}

		private static bool ContainsRefTag(Component component)
		{
			component.GetComponents(tags);

			foreach(var referenceable in tags)
			{
				if(referenceable.Target == component)
				{
					tags.Clear();
					return true;
				}
			}

			tags.Clear();
			return false;
		}
	}
}