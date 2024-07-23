using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	internal static class ServiceTagDrawer
	{
		private static readonly GUIContent refLabel = new("Ref");
		private static readonly StringBuilder sb = new();

		private static readonly GUIContent tagIdleLabel = new("Service");

		private static readonly Dictionary<Clients, GUIContent> mouseoverLabels = new()
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
			// Draw the Service tag for components that have a ServiceTag,
			// components whose class has the ServiceAttribute
			// components listed in a Services component,
			// and wrappers.
			var definingTypes = ServiceTagUtility.GetServiceDefiningTypes(component);
			if(!definingTypes.Any() && ServiceTagEditorUtility.openSelectTagsMenuFor != component)
			{
				return 0f;
			}

			Component serviceOrServiceProvider = component;

			if(headerRect.Contains(Event.current.mousePosition))
			{
				GUI.changed = true;
			}

			var tagLabel = GetTagLabel(serviceOrServiceProvider, headerRect, definingTypes);
			var tagRect = GetTagRect(serviceOrServiceProvider, headerRect, tagLabel, Styles.ServiceTag);

			if(ServiceTagEditorUtility.openSelectTagsMenuFor == serviceOrServiceProvider)
			{
				ServiceTagEditorUtility.openSelectTagsMenuFor = null;
				ServiceTagEditorUtility.OpenSelectTagsMenu(serviceOrServiceProvider, tagRect);
				return 0f;
			}

			if(GUI.Button(tagRect, GUIContent.none, EditorStyles.label))
			{
				switch(Event.current.button)
				{
					case 0:
						ServiceTagEditorUtility.OpenToClientsMenu(serviceOrServiceProvider, tagRect);
						break;
					case 1:
						ServiceTagEditorUtility.OpenContextMenuForService(serviceOrServiceProvider, tagRect);
						break;
					case 2:
						ServiceTagEditorUtility.PingServiceDefiningObject(serviceOrServiceProvider);
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

			var backgroundColorWas = GUI.backgroundColor;
			GUI.backgroundColor = new Color(1f, 1f, 0f);

			GUI.Label(tagRect, tagLabel, Styles.ServiceTag);

			GUI.backgroundColor = backgroundColorWas;

			GUI.EndClip();
			return 0f;
		}

		private static GUIContent GetTagLabel(Component serviceOrServiceProvider, Rect headerRect, IEnumerable<Type> definingTypes)
		{
			var mouseoverRect = GetTagRect(serviceOrServiceProvider, headerRect, tagIdleLabel, Styles.ServiceTag);
			bool isMouseovered = mouseoverRect.Contains(Event.current.mousePosition);
			var toClients = GetClientVisibility(serviceOrServiceProvider, definingTypes);
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
			sb.Append(TypeUtility.ToString(first));

			while(enumerator.MoveNext())
			{
				sb.Append("\nor ");
				sb.Append(TypeUtility.ToString(enumerator.Current));
			}

			sb.Append('.');

			string result = sb.ToString();
			sb.Clear();
			return result;
		}

		private static Rect GetTagRect(Component service, Rect headerRect, GUIContent label, GUIStyle style)
        {
			var componentTitle = new GUIContent(ObjectNames.GetInspectorTitle(service));
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
			labelRect. x -= 16 * 6 + 3 * 5; // for 6 button in header component
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

			if(ContainsRefTag(service))
			{
				var refTagRect = ServiceTagEditorUtility.GetTagRect(service, headerRect, refLabel, style);
				labelRect.x -= refTagRect.width + 5f;
			}

			return labelRect;
        }

		private static Clients GetClientVisibility(Component serviceOrServiceProvider, IEnumerable<Type> definingTypes)
		{
			Clients? result = null;

			foreach(var serviceTag in ServiceTagUtility.GetServiceTags(serviceOrServiceProvider))
			{
				var toClients = serviceTag.ToClients;
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
				if(ServiceUtility.TryGetClients(serviceOrServiceProvider, definingType, out var toClients))
				{
					return toClients;
				}
			}

			return Clients.Everywhere;
		}

		private static bool ContainsRefTag(Component component)
		{
			foreach(var referenceable in component.gameObject.GetComponentsNonAlloc<RefTag>())
			{
				if(referenceable.Target == component)
				{
					return true;
				}
			}

			return false;
		}
	}
}