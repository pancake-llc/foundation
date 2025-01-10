using System;
using System.Collections.Generic;
using System.Linq;
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

			singleSharedInstance = guidsOfPotentialInstancesInProject
									.Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
									.Where(asset => asset && asset.GetType() == valueProviderType)
									.SingleOrDefault();

			if(!singleSharedInstance)
			{
				#if DEV_MODE
				Debug.Log($"No single shared instance of value provider {valueProviderType} found in the project. Found {guidsOfPotentialInstancesInProject.Length} instances.");
				#endif
				return false;
			}

			if(AssetDatabase.IsSubAsset(singleSharedInstance))
			{
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

			#if DEV_MODE
			Debug.Log($"Single shared instance disqualified due to having serialized fields: {singleSharedInstance.name}.", singleSharedInstance);
			#endif

			singleSharedInstance = null;
			return false;
		}

		public static IEnumerable<Type> GetAllValueProviderMenuItemTargetTypes()
			=> TypeCache.GetTypesWithAttribute<ValueProviderMenuAttribute>()
			.Where(t => !t.IsAbstract && typeof(ScriptableObject).IsAssignableFrom(t) && ValueProviderUtility.IsValueProvider(t));
	}
}
