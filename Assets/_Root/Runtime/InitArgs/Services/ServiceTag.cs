//#define SHOW_SERVICE_TAGS

using System;
using JetBrains.Annotations;
using Pancake.Init.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Pancake.Init.Internal
{
	[ExecuteAlways, AddComponentMenu(Hidden), DefaultExecutionOrder(ExecutionOrder.ServiceTag)]
	internal sealed class ServiceTag : MonoBehaviour
	{
		private const string Hidden = "";

		[SerializeField]
		private Component service;

		[SerializeField, Tooltip("Limits what clients have access to the services in this Services by their location in the scene hierarchy.\n\nWhen set to 'Children' only clients that are attached to the same GameObject as this Services or or any of its children (including nested children) can access the services in this Services.\n\nWhen set to 'Scene' only clients that are in the same scene as this Services can access the services in this Services.\n\nWhen set to 'Global', all clients are allowed to access the services in this Services regardless of where they are in the scene hierarchy.")]
		private Clients toClients = Clients.Everywhere;

		[SerializeField]
		private _Type definingType = new _Type();

		/// <summary>
		/// Gets the defining type that clients should be able to use to retrieve
		/// an instance of the <see cref="Service"/>.
		/// </summary>
		internal Type DefiningType
		{
			get => definingType;

			set
			{
				definingType = value;

				#if DEBUG
				if(!DefiningTypeIsAssignableFrom(service))
				{
					#if UNITY_EDITOR
					EditorApplication.delayCall += OnValidateDelayed;
					#endif
					Debug.Log($"ServiceTag on GameObject \"{name}\" was assigned an invalid {nameof(DefiningType)} value {TypeUtility.ToString(value.GetType())}, which is not assignable from service instance type {TypeUtility.ToString(service.GetType())}.", gameObject);
				}
				#endif
			}
		}

		/// <summary>
		/// Gets service instance that clients should be able to retrieve 
		/// using its <see cref="DefiningType"/>.
		/// </summary>
		internal Component Service
		{
			get => service;

			set
			{
				#if DEBUG
				if(!DefiningTypeIsAssignableFrom(value))
				{
					#if UNITY_EDITOR
					EditorApplication.delayCall += OnValidateDelayed;
					#endif
					Debug.Log($"ServiceTag on GameObject \"{name}\" was assigned an invalid {nameof(Service)} instance. {nameof(DefiningType)} value {TypeUtility.ToString(definingType.Value)} is not assignable from the instance of type {TypeUtility.ToString(value.GetType())}.", gameObject);
				}
				#endif

				service = value;
			}
		}

		internal int ToClients
		{
			get => (int)toClients;

			set
			{
				toClients = (Clients)value;
			}
		}

		#if UNITY_EDITOR
		public static void Add(Component component, Type definingType)
		{
			var tag = component.gameObject.AddComponent<ServiceTag>();
			tag.hideFlags = HideFlags.HideInInspector;
			tag.DefiningType = definingType;
			tag.Service = component;
		}

		public static bool Remove(Component component, Type definingType)
		{
			foreach(var serviceTag in component.GetComponents<ServiceTag>())
			{
				if(serviceTag.definingType.Value == definingType && serviceTag.service == component)
				{
					Undo.DestroyObjectImmediate(serviceTag);
					return true;
				}
			}

			return false;
		}
		#endif

		private void OnEnable() => Register();
		private void OnDisable() => Deregister();

		internal bool AreAvailableToAnyClient() => toClients == Clients.Everywhere;

		internal bool AreAvailableToClient([NotNull] GameObject client)
		{
			Debug.Assert(client != null);
			Debug.Assert(this != null);

			switch(toClients)
			{
				case Clients.InGameObject:
					return client == gameObject;
				case Clients.InChildren:
					var injectorTransform = transform;
					for(var parent = client.transform; parent != null; parent = parent.parent)
					{
						if(parent == injectorTransform)
						{
							return true;
						}
					}
					return false;
				case Clients.InParents:
					var clientTransform = client.transform;
					for(var parent = transform; parent != null; parent = parent.parent)
					{
						if(parent == clientTransform)
						{
							return true;
						}
					}
					return false;
				case Clients.InHierarchyRootChildren:
					return transform.root == client.transform.root;
				case Clients.InScene:
					return client.scene == gameObject.scene;
				case Clients.InAllScenes:
				case Clients.Everywhere:
					return true;
				default:
					Debug.LogError($"Unrecognized {nameof(Clients)} value: {toClients}.", this);
					return false;
			}
		}

		private void Register()
		{
			#if UNITY_EDITOR
			if(service == null && !EditorOnly.ThreadSafe.Application.IsPlaying)
			{
				return;
			}
			#endif

			Services.Register(this, new ServiceDefinition[] { new ServiceDefinition(service, definingType) }, toClients);
		}

		private void Deregister()
		{
			#if UNITY_EDITOR
			EditorApplication.hierarchyChanged -= OnValidateDelayed;
			#endif

			Services.Deregister(this);
		}

		#if DEBUG
		private bool DefiningTypeIsAssignableFrom([CanBeNull] Component component)
		{
			if(component == null || definingType.Value is null)
			{
				return true;
			}

			if(definingType.Value.IsAssignableFrom(component.GetType()))
			{
				return true;
			}

			return component is IValueProvider valueProvider && valueProvider.Value is object providedValue && definingType.Value.IsAssignableFrom(providedValue.GetType());
		}
		#endif

		#if UNITY_EDITOR
		private void OnValidate()
		{
			if(service == null
			|| service.gameObject != gameObject
			|| definingType.Value is null
			|| !definingType.Value.IsAssignableFrom(service.GetType()))
			{
				EditorApplication.delayCall += OnValidateDelayed;
				return;
			}

			OnValidServiceTagLoaded();
		}

		private void OnValidateDelayed()
		{
			if(this == null)
			{
				return;
			}

			if(service == null)
			{
				// Handle service that is Missing due to being dragged to another GameObject.
				if(!(service is null) && service.GetHashCode() != 0 && definingType.Value != null)
				{
					var instancesMatchingDefiningType = Find.All(definingType.Value, true);
					if(instancesMatchingDefiningType.Length == 1 && instancesMatchingDefiningType[0] is Component instance)
					{
						bool instanceHasOtherTags = false;
						foreach(var tag in instance.GetComponents<ServiceTag>())
						{
							if(tag.service == service)
							{
								instanceHasOtherTags = true;
								break;
							}
						}

						if(!instanceHasOtherTags)
						{
							#if DEV_MODE
							Debug.Log($"Moving Service tag {definingType.Value.Name} of {instance.GetType().Name} from {name} to {instance.gameObject.name}...", instance.gameObject);
							#endif
							service = instance;
							ComponentUtility.CopyComponent(this);
							Undo.DestroyObjectImmediate(this); // Can this handle prefab assets? prefab instances? Test!
							ComponentUtility.PasteComponentAsNew(instance.gameObject);
							return;
						}
					}
				}

				#if DEV_MODE
				Debug.Log($"ServiceTag on GameObject \"{name}\" is missing its target. Removing it from the GameObject.", gameObject);
				#endif
				Undo.DestroyObjectImmediate(this); // Can this handle prefab assets? prefab instances? Test!
				return;
			}

			if(definingType.Value is null)
			{
				Debug.Log($"ServiceTag on GameObject \"{name}\" is missing its {nameof(DefiningType)}. Removing it from the GameObject.", gameObject);
				Undo.DestroyObjectImmediate(this); // Can this handle prefab assets? prefab instances? Test!
				return;
			}

			if(!definingType.Value.IsAssignableFrom(service.GetType()))
			{
				#if DEV_MODE
				Debug.Log($"ServiceTag on GameObject \"{name}\" has a {nameof(DefiningType)} that is not assignable from service of type {service.GetType()}. Removing it from the GameObject.", gameObject);
				#endif
				Undo.DestroyObjectImmediate(this); // Can this handle prefab assets? prefab instances? Test!
				return;
			}

			if(service.gameObject != gameObject)
            {
				#if DEV_MODE
				Debug.Log($"Moving Service tag {definingType.Value.Name} of {service.GetType().Name} from {name} to {service.gameObject.name}...", service.gameObject);
				#endif
				ComponentUtility.CopyComponent(this);
				ComponentUtility.PasteComponentAsNew(service.gameObject);
				Undo.DestroyObjectImmediate(this); // Can this handle prefab assets? prefab instances? Test!
				return;
			}
			
			OnValidServiceTagLoaded();
		}

		private void OnValidServiceTagLoaded()
		{
			#if DEV_MODE && SHOW_SERVICE_TAGS
			const HideFlags setFlags = HideFlags.None;
			#else
			const HideFlags setFlags = HideFlags.HideInInspector;
			#endif

			if(hideFlags != setFlags)
			{
				hideFlags = setFlags;
				EditorUtility.SetDirty(this);
			}

			if(gameObject.scene.IsValid() && !EditorOnly.ThreadSafe.Application.IsPlaying)
			{
				Register();

				EditorApplication.hierarchyChanged -= OnValidateDelayed;
				EditorApplication.hierarchyChanged += OnValidateDelayed;
			}
		}
		#endif
	}
}