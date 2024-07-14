#define HIDE_RUNTIME_MEMBERS_IN_EDIT_MODE

//#define DEBUG_DEPENDENCY_WARNING_BOX
//#define DEBUG_FIELDS

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
#if UNITY_2021_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly
{
    public abstract class BaseConstructorBehaviourEditor : InitializableEditor
    {
        private const string sceneObjectWarningBoxTextSuffix = " has non-service dependencies which it might not be able to receive when the scene is loaded unless they have been provided manually using InitArgs.Set.";
        private static Type[] serviceTypes;

        private GameObject gameObject;
        private bool mightNotHaveDependencies;
        private GUIContent sceneObjectWarningBoxContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            var targetType = target.GetType();

            sceneObjectWarningBoxContent = new GUIContent(targetType.Name + sceneObjectWarningBoxTextSuffix);

            gameObject = (target as Component).gameObject;
            mightNotHaveDependencies = IsSceneObjectWithNonServiceDependenciesAndNoInitializer(target, gameObject);
        }

        private bool IsSceneObjectWithNonServiceDependenciesAndNoInitializer(Object target, GameObject gameObject)
        {
            return IsSceneObject(target, gameObject) && HasNonServiceDependencies(target, gameObject) && !HasInitializer(target, gameObject);
        }

        private bool IsSceneObject(Object target, GameObject gameObject) => !PrefabUtility.IsPartOfPrefabAsset(gameObject) && PrefabStageUtility.GetPrefabStage(gameObject) == null;

        private bool HasNonServiceDependencies(Object target, GameObject gameObject)
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
                        Debug.Log($"{argumentTypes[i].Name} not found among {serviceTypes.Length} services.");
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

        private bool HasInitializer(Object target, GameObject gameObject)
        {
            foreach(var initializer in gameObject.GetComponents<IInitializer>())
            {
                if(initializer.Target == target)
                {
                    return true;
                }
            }

            return false;
        }

        private static Type[] GetAllServices()
        {
            var list = new HashSet<Type>();

            foreach(var type in TypeCache.GetTypesWithAttribute<ServiceAttribute>())
            {
                // Only consider concrete classes.
                if(type.IsAbstract)
                {
                    continue;
                }

                foreach(var serviceAttribute in type.GetCustomAttributes<ServiceAttribute>())
                {
                    var definingType = serviceAttribute.definingType;

                    if(definingType is null)
                    {
                        definingType = type;
                    }

                    list.Add(definingType);
                }
            }

            var array = new Type[list.Count];
            list.CopyTo(array);
            return array;
        }

        private bool IsService(Type type)
        {
            if(serviceTypes == null)
            {
                serviceTypes = GetAllServices();
            }
            return Array.IndexOf(serviceTypes, type) != -1;
        }
        
        public override void OnInspectorGUI()
        {
            HandleSceneObjectWarningBox();

            base.OnInspectorGUI();
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