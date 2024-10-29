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

            int hashCodeBefore = ((Services)target).GetStateBasedHashCode();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(providesServices);
            EditorGUILayout.PropertyField(toClients, clientsLabel);

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();

            int hashCodeAfter = ((Services)target).GetStateBasedHashCode();
            // double check that contents has actually changed, before moving on doing FindObjectsByType
            // and re-registering services, as that can be
            // as that can
            if(hashCodeBefore == hashCodeAfter)
            {
                return;
            }

            try
            {
                // Avoid raising the Service.AnyChangedEditorOnly more than once
                Service.BatchEditingServices = true;

                var servicesComponent = (Services)target;
                ServiceUtility.RemoveAllServicesProvidedBy(servicesComponent);

                foreach(var definition in servicesComponent.providesServices)
                {
                    if(definition.definingType?.Value is Type definingType && definition.service is Object service && service)
                    {
                        ServiceUtility.AddFor(service, definingType, servicesComponent.toClients, servicesComponent);
                    }
                }
            }
            finally
            {
                Service.BatchEditingServices = false;
            }
        }
    }
}