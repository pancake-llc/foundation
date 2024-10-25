using System.Diagnostics.CodeAnalysis;
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sisus.ComponentNames.Editor;
using UnityEditor;
using UnityEngine;
#endif

namespace Sisus.ComponentNames
{
	internal static class HeaderContentUtility
	{
		internal static string Concat([AllowNull] string x, [AllowNull] string y)
		{
			if(x is null)
			{
				return y;
			}

			if(y is null)
			{
				return x;
			}

			return x + y;
		}

		#if UNITY_EDITOR
		private static readonly Dictionary<Type, ICustomHeader[]> customHeadersByType = new();
		internal static readonly Type customHeaderGenericTypeDefinition;

		static HeaderContentUtility()
		{
			foreach(var type in TypeCache.GetTypesDerivedFrom(typeof(ICustomHeader)))
			{
				if(type.IsGenericTypeDefinition && string.Equals(type.FullName, "Sisus.ComponentNames.Editor.CustomHeader`1"))
				{
					customHeaderGenericTypeDefinition = type;
					return;
				}
			}

			#if DEV_MODE
			Debug.LogError("Type Sisus.ComponentNames.Editor.CustomHeader`1 not found!");
			#endif
		}

		// TODO: So this will also be used by ComponentTooltips?
		// So, this should probably be moved to a different class like
		// DefaultTitle? InspectorTitleUtility?
		internal static HeaderContent GetCustomHeaderContent([DisallowNull] Component target, [DisallowNull] Type targetType)
		{
			// var name = Name.Default;
			// var suffix = Suffix.Default;
			// var tooltip = Tooltip.Default;
			var (defaultName, defaultSuffix) = ComponentName.GetDefaultBuiltIn(target, targetType, ComponentName.RemoveDefaultScriptSuffix);
			Tooltip defaultTooltip = ComponentTooltip.GetXmlDocumentationSummary(target);
			var name = defaultName;
			var suffix = defaultSuffix;
			var tooltip = defaultTooltip;

			if(!customHeadersByType.TryGetValue(targetType, out var customHeaders))
			{
				customHeaders = GetAllCustomHeadersFor(target);
				customHeadersByType.Add(targetType, customHeaders);
			}

			foreach(var customHeader in customHeaders)
			{
				customHeader.Init(name, suffix, tooltip, defaultName, defaultSuffix, defaultTooltip);

				if(customHeader.GetName(target) is { IsDefault: false } setName)
				{
					name = setName;
				}

				if(customHeader.GetSuffix(target) is { IsDefault: false } setSuffix)
				{
					suffix = setSuffix;
				}

				if(customHeader.GetTooltip(target) is { IsDefault: false } setTooltip)
				{
					tooltip = setTooltip;
				}
			}

			return new(name, suffix, tooltip);

			static ICustomHeader[] GetAllCustomHeadersFor(Component component)
			{
				var results = new SortedSet<ICustomHeader>();

				foreach(var customHeaderType in TypeCache.GetTypesDerivedFrom(typeof(ICustomHeader)))
				{
					if(customHeaderType.IsAbstract)
					{
						continue;
					}

					for(var baseType = customHeaderType.BaseType; baseType != null; baseType = baseType.BaseType)
					{
						if (!baseType.IsGenericType
						|| baseType.GetGenericTypeDefinition() != customHeaderGenericTypeDefinition
						|| !baseType.GetGenericArguments()[0].IsInstanceOfType(component))
						{
							continue;
						}

						var targetType = baseType.GetGenericArguments()[0];
						if(!targetType.IsInstanceOfType(component))
						{
							continue;
						}

						var customHeader = (ICustomHeader)Activator.CreateInstance(customHeaderType);
						results.Add(customHeader);
					}
				}

				return results.ToArray();
			}
		}
		#endif
	}
}