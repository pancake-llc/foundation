//#define DEBUG_DEPENDENCY_WARNING_BOX

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Sisus.Init.Internal;

namespace Sisus.Init.EditorOnly
{
	internal abstract class BaseConstructorBehaviourEditorDecorator : InitializableEditorDecorator
	{
		private const string sceneObjectWarningBoxTextSuffix = " has non-service dependencies which it might not be able to receive when the scene is loaded unless they have been provided manually using InitArgs.Set.";
		private static Type[] allServiceDefiningTypes;

		private bool mightNotHaveDependencies;
		private GUIContent sceneObjectWarningBoxContent;

		public BaseConstructorBehaviourEditorDecorator(Editor decoratedEditor) : base(decoratedEditor)
		{
			var targetType = target.GetType();

			sceneObjectWarningBoxContent = new GUIContent(targetType.Name + sceneObjectWarningBoxTextSuffix);
			mightNotHaveDependencies = IsSceneObjectWithNonServiceDependenciesAndNoInitializer(target as Component);
		}

		private bool IsSceneObjectWithNonServiceDependenciesAndNoInitializer(Component target)
			=> !target.gameObject.IsAsset(resultIfSceneObjectInEditMode: false) && HasNonServiceDependencies(target) && !InitializerUtility.HasInitializer(target);

		private bool HasNonServiceDependencies(Component target)
		{
			var baseType = BaseType;

			for(var type = target.GetType().BaseType; type != null; type = type.BaseType)
			{
				if(!type.IsGenericType)
				{
					continue;
				}

				var typeDefinition = type.GetGenericTypeDefinition();
				if(typeDefinition != baseType)
				{
					continue;
				}

				var argumentTypes = type.GetGenericArguments();
				int argumentCount = argumentTypes.Length;
				for(int i = 0; i < argumentCount; i++)
				{
					if(!IsService(argumentTypes[i]))
					{
						#if DEV_MODE && DEBUG_DEPENDENCY_WARNING_BOX
						Debug.Log($"{argumentTypes[i].Name} not found among {allServiceDefiningTypes.Length} services.");
						#endif
						return true;
					}
					#if DEV_MODE && DEBUG_DEPENDENCY_WARNING_BOX
					Debug.Log($"{argumentTypes[i].Name} found among services.");
					#endif
				}

				return false;
			}

			return false;
		}

		private static Type[] GetAllServiceDefiningTypes()
		{
			var results = new HashSet<Type>();

			foreach(var type in TypeCache.GetTypesWithAttribute<ServiceAttribute>())
			{
				// Only consider concrete classes.
				if(type.IsAbstract)
				{
					continue;
				}

				var implicitDefiningType = true; 

				foreach(var serviceAttribute in CustomAttributeExtensions.GetCustomAttributes<ServiceAttribute>(type))
				{
					var definingTypes = serviceAttribute.definingTypes;
					if(definingTypes.Length > 0)
					{
						implicitDefiningType = false;
						foreach(var definingType in definingTypes)
						{
							results.Add(definingType);
						}
					}
				}
				
				if(implicitDefiningType)
				{
					results.Add(type);
				}
			}

			var array = new Type[results.Count];
			results.CopyTo(array);
			return array;
		}

		private bool IsService(Type type)
		{
			allServiceDefiningTypes ??= GetAllServiceDefiningTypes();
			return Array.IndexOf(allServiceDefiningTypes, type) != -1;
		}

		public override void OnBeforeInspectorGUI()
		{
			base.OnBeforeInspectorGUI();
			HandleSceneObjectWarningBox();
		}

		private void HandleSceneObjectWarningBox()
		{
			if(!mightNotHaveDependencies || Application.isPlaying)
			{
				return;
			}

			EditorGUILayout.HelpBox(sceneObjectWarningBoxContent);
		}
	}
}