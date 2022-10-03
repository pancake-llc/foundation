using System.Linq;
using Pancake.Init.Internal;
using UnityEditor;
using UnityEngine;

namespace Pancake.Init.EditorOnly
{
    [CustomEditor(typeof(Services), true, isFallback = true), CanEditMultipleObjects]
    public sealed class ServicesEditor : UnityEditor.Editor
    {
        private SerializedProperty providesServices;
        private SerializedProperty toClients;

        private static readonly GUIContent clientsLabel = new GUIContent("For Clients");

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
                providesServices.serializedObject.ApplyModifiedProperties();
                
                var all = FindObjectsOfType<Services>();
                foreach(var services in all)
				{
                    Services.Deregister(services);
				}

                foreach(var services in all)
                {
                    Services.Register(services, services.providesServices.Where(s => s.service != null && s.definingType.Value != null), services.toClients);
                }
            }
        }
    }
}