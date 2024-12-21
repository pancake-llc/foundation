using System;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Class that can be used to insert boxes containing texts into the Inspector
	/// below the headers of components of particular types.
	/// </summary>
	[InitializeOnLoad]
	internal static class InspectorHelpBoxInjector
	{
		const float topPadding = 3f;
		const float minHeight = 30f;
		const float insidePadding = 8f;
		const float bottomPadding = 3f;
		const float leftPadding = 15f;
		static readonly Vector2 iconSize = new(15f, 15f);

		static readonly GUIContent content = new("");

		static InspectorHelpBoxInjector()
		{
			ComponentHeader.AfterHeaderGUI -= OnAfterHeaderGUI;
			ComponentHeader.AfterHeaderGUI += OnAfterHeaderGUI;
		}

		static void OnAfterHeaderGUI(Component[] targets, Rect headerRect, bool HeaderIsSelected, bool supportsRichText)
		{
			var component = targets[0];
			if(component is Services servicesComponent)
			{
				foreach(var serviceDefinition in servicesComponent.providesServices)
				{
					var service = serviceDefinition.service;
					if(!service)
					{
						continue;
					}

					Type definingType = serviceDefinition.definingType;
					definingType ??= service.GetType();
					if(IsServiceAttributeDefiningType(definingType))
					{
						content.text = GetReplacesDefaultServiceText(definingType, servicesComponent.toClients);
						DrawHelpBox(MessageType.Info, content);
					}
					else if(service is Component serviceComponent && TryGetServiceTag(serviceComponent, serviceDefinition.definingType, out _))
					{
						content.text = GetReplacesDefaultServiceText(definingType, servicesComponent.toClients);
						DrawHelpBox(MessageType.Info, content);
					}
				}
			}
			else
			{
				foreach(var serviceTag in ServiceTagUtility.GetServiceTags(component))
				{
					if(serviceTag.DefiningType is Type definingType && IsServiceAttributeDefiningType(definingType))
					{
						content.text = GetReplacesDefaultServiceText(definingType, serviceTag.ToClients);
						DrawHelpBox(MessageType.Info, content);
					}
				}
			}
		}

		static bool IsServiceAttributeDefiningType([DisallowNull] Type definingType) => ServiceAttributeUtility.definingTypes.ContainsKey(definingType);
		
		static bool TryGetServiceTag(Component component, Type matchingDefiningType, out ServiceTag result)
		{
			foreach(var serviceTag in ServiceTagUtility.GetServiceTags(component))
			{
				if(serviceTag.DefiningType == matchingDefiningType)
				{
					result = serviceTag;
					return true;
				}
			}

			result = null;
			return false;
		}

		static string GetReplacesDefaultServiceText(Type serviceType, Clients toClients)
		{
			return toClients switch
			{
				Clients.Everywhere => $"Replaces the default {serviceType.Name} service for all clients.",
				Clients.InGameObject => $"This Replaces the default {serviceType.Name} service for clients in this game object.",
				Clients.InChildren => $"Replaces the default {serviceType.Name} service for clients in this game object and all of its children.",
				Clients.InParents => $"Replaces the default {serviceType.Name} service for clients in this game object and all of its parents.",
				Clients.InHierarchyRootChildren => $"Replaces the default {serviceType.Name} service for clients in the root game object and all of its children.",
				Clients.InScene => $"Replaces the default {serviceType.Name} service for clients in this scene.",
				Clients.InAllScenes => $"Replaces the default {serviceType.Name} service for clients in all scenes.",
				_ => $"Replaces the default {serviceType.Name} service for clients {toClients}."
			};
		}

		static void DrawHelpBox(MessageType messageType, GUIContent label)
		{
			GUILayout.Space(3f);

			EditorGUIUtility.SetIconSize(iconSize);
			Vector2 size = EditorStyles.helpBox.CalcSize(label);
			size.y = Mathf.Max(size.y + insidePadding, minHeight);
			Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(size.y));
			rect.x += leftPadding;
			rect.width -= leftPadding;
			
			EditorGUI.HelpBox(rect, label.text, messageType);

			GUILayout.Space(5f);
		}
	}
}