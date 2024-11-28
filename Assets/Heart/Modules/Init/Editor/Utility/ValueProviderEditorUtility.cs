using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sisus.Init.ValueProviders;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	internal static class ValueProviderEditorUtility
	{
		/// <summary>
		/// NOTE: Slow method; should not be called during every OnGUI.
		/// </summary>
		public static bool IsSingleSharedInstanceSlow(ScriptableObject valueProvider)
			=> !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(valueProvider))
			&& TryGetSingleSharedInstanceSlow(valueProvider.GetType(), out var singleSharedInstance)
			&& ReferenceEquals(singleSharedInstance, valueProvider);

		/// <summary>
		/// NOTE: Slow method; should not be called during every OnGUI.
		/// </summary>
		public static bool TryGetSingleSharedInstanceSlow(Type valueProviderType, out ScriptableObject singleSharedInstance)
		{
			var guidsOfPotentialInstancesInProject = AssetDatabase.FindAssets("t:" + valueProviderType.Name);
			if(guidsOfPotentialInstancesInProject.Length == 0)
			{
				#if DEV_MODE
				Debug.Log($"No instances of value provider {valueProviderType} found in the project.");
				#endif

				singleSharedInstance = null;
				return false;
			}

			var attribute = valueProviderType.GetCustomAttribute<ValueProviderMenuAttribute>();
			singleSharedInstance = guidsOfPotentialInstancesInProject
									.Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
									.Where(asset => asset && asset.GetType() == valueProviderType && AssetNameEqualsItemName(asset.name, attribute.ItemName))
									.SingleOrDefault();

			if(!singleSharedInstance)
			{
				#if DEV_MODE
				Debug.Log($"No single shared instance of value provider {valueProviderType} found in the project. Found {guidsOfPotentialInstancesInProject.Length} instances.");
				#endif
				return false;
			}

			using(var serializedObject = new SerializedObject(singleSharedInstance))
			{
				var firstProperty = serializedObject.GetIterator();
				if(firstProperty.NextVisible(true) && !firstProperty.NextVisible(false))
				{
					#if DEV_MODE
					Debug.Log($"Single shared instance found: {singleSharedInstance.name}.", singleSharedInstance);
					#endif
					return true;
				}
			}

			singleSharedInstance = null;

			#if DEV_MODE
			Debug.Log($"Single shared instance disqualified due to having serialized fields: {singleSharedInstance.name}.", singleSharedInstance);
			#endif

			return false;
		}

		private static bool AssetNameEqualsItemName(string assetName, string menuPath) => string.Equals(assetName, Path.GetFileName(menuPath));

		public static IEnumerable<Type> GetAllValueProviderMenuItemTargetTypes()
			=> TypeCache.GetTypesWithAttribute<ValueProviderMenuAttribute>()
			.Where(t => !t.IsAbstract && typeof(ScriptableObject).IsAssignableFrom(t) && ValueProviderUtility.IsValueProvider(t));
	}
}
