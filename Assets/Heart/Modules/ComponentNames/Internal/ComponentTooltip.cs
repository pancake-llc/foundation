#if UNITY_EDITOR
using System;

using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.ComponentNames.Editor
{
	/// <summary>
	/// Utility class for getting and setting component tooltips.
	/// </summary>
	internal static class ComponentTooltip
	{
		private static readonly Dictionary<Object, string> tooltips = new();
		private static readonly Dictionary<Type, string> xmlDocumentationSummaries = new()
		{
			{ typeof(Transform), "Position, rotation and scale of an object." },
			{ typeof(MeshFilter), "A class to access the Mesh of the mesh filter." },
			{ typeof(Light), "Script interface for light components." },
			{ typeof(MeshCollider), "A mesh collider allows you to do collision detection between meshes and primitives." }
		};
		private static readonly HashSet<Component> hasOverride = new();

		internal static bool HasOverride([NotNull] Component component)
			=> hasOverride.Contains(component);

		internal static void Set([NotNull] Component component, [CanBeNull] string tooltip, ModifyOptions modifyOptions)
		{
			if(string.IsNullOrEmpty(tooltip))
			{
				tooltips.Remove(component);
				hasOverride.Remove(component);

				if(modifyOptions.IsUpdatingContainerAllowed())
				{
					UpdateNameContainer(component, modifyOptions);
				}

				return;

				static void UpdateNameContainer(Component component, ModifyOptions modifyOptions)
				{
					if(modifyOptions.IsDelayed())
					{
						EditorApplication.delayCall += ()=> UpdateNameContainer(component, modifyOptions.Immediately());
					}

					if(!NameContainer.TryGet(component, out var nameContainer))
					{
						return;
					}

					if(ComponentName.IsNullEmptyOrDefault(component, nameContainer.NameOverride) && modifyOptions.IsRemovingNameContainerAllowed())
					{
						nameContainer.Remove(modifyOptions);
					}
					else
					{
						nameContainer.TooltipOverride = "";
					}
				}
			}

			hasOverride.Add(component);
			tooltips[component] = tooltip;

			if(modifyOptions.IsUpdatingContainerAllowed())
			{
				NameContainer.TryGetOrCreate(component, modifyOptions, nameContainer =>
				{
					nameContainer.TooltipOverride = tooltip;
				}, null, tooltip);
			}
		}

		internal static string Get([NotNull] Component component)
		{
			if(tooltips.TryGetValue(component, out string tooltip))
			{
				return tooltip;
			}

			return GetDefault(component);
		}

		internal static string GetDefault(Component component)
		{
			var componentType = component.GetType();
			// TODO: Is it bad that the tooltip gets discarded here? do we need to then call this twice to also get the tooltip?
			// Maybe not, if it's only updated on mouse enter for example. Investigate!
			var (_, _, tooltip) = HeaderContentUtility.GetCustomHeaderContent(component, componentType);

			return !tooltip.IsDefault ? tooltip : GetXmlDocumentationSummary(component);
		}

		internal static string GetXmlDocumentationSummary(Component component)
		{
			var type = component.GetType();
			if(xmlDocumentationSummaries.TryGetValue(type, out string summary))
			{
				return summary;
			}

			if(component is not MonoBehaviour monoBehaviour || !MonoScriptSummaryParser.TryParseSummary(monoBehaviour, out summary))
			{
				DLLSummaryParser.TryParseSummary(type, out summary);
			}

			summary ??= "";
			xmlDocumentationSummaries[type] = summary;
			return summary;
		}
	}
}
#endif