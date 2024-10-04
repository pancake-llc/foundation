using System;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
    [CustomEditor(typeof(Services), true, isFallback = true), CanEditMultipleObjects]
    internal sealed class ServicesEditor : Editor
    {
        private SerializedProperty providesServices;
        private SerializedProperty toClients;

        private static readonly GUIContent clientsLabel = new("For Clients");

        private void OnEnable() => Setup();
        
        private void Setup()
        {
            providesServices = serializedObject.FindProperty(nameof(providesServices));
            toClients = serializedObject.FindProperty(nameof(toClients));
        }

        public override void OnInspectorGUI()
        {
            if(providesServices is null)
            {
                Setup();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(providesServices);
            EditorGUILayout.PropertyField(toClients, clientsLabel);
            if(EditorGUI.EndChangeCheck())
			{
                var allInstances =
                #if UNITY_2023_1_OR_NEWER
                FindObjectsByType<Services>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                #else
                FindObjectsOfType<Services>();
                #endif

                foreach(var servicesComponent in allInstances)
                {
                    foreach(var definition in servicesComponent.providesServices)
					{
                        if(definition.definingType?.Value is Type definingType && definition.service is Object service && service)
                        {
                            ServiceUtility.RemoveFromClients(service, definingType, servicesComponent.toClients, servicesComponent);
                        }
					}
                }

                providesServices.serializedObject.ApplyModifiedProperties();

                foreach(var servicesComponent in allInstances)
				{
                    foreach(var definition in servicesComponent.providesServices)
					{
                        if(definition.definingType?.Value is Type definingType && definition.service is Object service && service)
                        {
                            ServiceUtility.AddFor(service, definingType, servicesComponent.toClients, servicesComponent);
                        }
					}
				}
            }
        }
    }
}