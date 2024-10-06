//#define SHOW_SERVICE_TAGS

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sisus.Init.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
#endif

namespace Sisus.Init.Internal
{
	[ExecuteAlways, AddComponentMenu(Hidden), DefaultExecutionOrder(ExecutionOrder.ServiceTag)]
	public sealed partial class ServiceTag : MonoBehaviour, IValueByTypeProvider, IValueProvider<Component>, IInitializable<Component, Clients, Type>
		#if UNITY_EDITOR
		, ISerializationCallbackReceiver
		, INullGuardByType
		#endif
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

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(value is null)
				{
					#if UNITY_EDITOR
					EditorApplication.delayCall = OnValidateDelayed;
					EditorApplication.delayCall += OnValidateDelayed;
					#endif

					Debug.LogWarning($"ServiceTag on GameObject \"{name}\" was assigned an invalid {nameof(DefiningType)} value: null.", gameObject);
				}
				else if(service && !IsValidDefiningTypeFor(value, service))
				{
					#if UNITY_EDITOR
					EditorApplication.delayCall = OnValidateDelayed;
					EditorApplication.delayCall += OnValidateDelayed;
					#endif

					Debug.LogWarning($"ServiceTag on GameObject \"{name}\" was assigned an invalid {nameof(DefiningType)} value {TypeUtility.ToString(value)}, which is not assignable from service instance type {TypeUtility.ToString(service.GetType())} or an object that it wraps.", gameObject);
				}
				#endif
			}
		}

		/// <summary>
		/// Gets service instance that clients that depend on objects of type
		/// <see cref="DefiningType"/> should be able to recieve.
		/// </summary>
		internal Component Service
		{
			get => service;

			set
			{
				service = value;

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(!value)
				{
					#if UNITY_EDITOR
					EditorApplication.delayCall = OnValidateDelayed;
					EditorApplication.delayCall += OnValidateDelayed;
					#endif

					Debug.LogWarning($"ServiceTag on GameObject \"{name}\" was assigned an invalid {nameof(Service)} value: null.", gameObject);
				}
				else if(DefiningType is Type definingType && !IsValidDefiningTypeFor(definingType, value))
				{
					#if UNITY_EDITOR
					EditorApplication.delayCall = OnValidateDelayed;
					EditorApplication.delayCall += OnValidateDelayed;
					#endif

					Debug.LogWarning($"ServiceTag on GameObject \"{name}\" was assigned an invalid {nameof(Service)} instance. {nameof(DefiningType)} value {TypeUtility.ToString(definingType)} is not assignable from the instance of type {TypeUtility.ToString(value.GetType())}.", gameObject);
				}
				#endif
			}
		}

		internal Clients ToClients
		{
			get => toClients;
			set => toClients = value;
		}

		Component IValueProvider<Component>.Value => toClients == Clients.Everywhere ? service : null;

		bool IValueProvider<Component>.TryGetFor(Component client, out Component value)
		{
			if(client ? IsAvailableToClient(client.gameObject) : IsAvailableToAnyClient())
			{
				value = service;
				return service;
			}
			
			value = null;
			return false;
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
			foreach(var serviceTag in component.gameObject.GetComponentsNonAlloc<ServiceTag>())
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

		bool IValueByTypeProvider.TryGetFor<TValue>([AllowNull] Component client, out TValue value)
		{
			if(definingType.Value == typeof(TValue)
			&& service
			&& Find.In(service, out value)
			&& (IsAvailableToAnyClient() || (client && IsAvailableToClient(client.gameObject))))
			{
				return true;
			}

			value = default;
			return false;
		}

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client)
		{
			if(definingType.Value != typeof(TValue) || !service || service is not TValue)
			{
				return false;
			}

			if(!IsAvailableToAnyClient() && (!client || !IsAvailableToClient(client.gameObject)))
			{
				return false;
			}

			return true;
		}

		private void Awake()
		{
			#if UNITY_EDITOR
			if(!service)
			{
				return;
			}
			#endif

			ScopedServiceInitializer.Init(service);
		}

		private void OnEnable() => Register();
		private void OnDisable() => Deregister();

		void IInitializable<Component, Clients, Type>.Init(Component service, Clients toClients, Type definingType)
		{
			this.service = service;
			this.toClients = toClients;
			this.definingType = definingType;

			#if UNITY_EDITOR
			hideFlags = HideFlags.HideInInspector;
			#endif
		}

		internal bool IsAvailableToAnyClient() => toClients == Clients.Everywhere;

		internal bool IsAvailableToClient([DisallowNull] GameObject client)
		{
			#if DEV_MODE
			Debug.Assert(client);
			Debug.Assert(this);
			#endif

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
					for(var parent = transform; parent; parent = parent.parent)
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
			if(!service)
			{
				return;
			}

			Clients registerForClients = toClients switch
			{
				Clients.InGameObject => toClients,
				Clients.InChildren => toClients,
				Clients.InParents => toClients,
				Clients.InHierarchyRootChildren => toClients,
				_ when PrefabUtility.IsPartOfPrefabAsset(this) => Clients.InHierarchyRootChildren,
				Clients.InScene => toClients,
				_ when PrefabStageUtility.GetPrefabStage(gameObject) => Clients.InScene,
				_ => toClients
			};

			ServiceUtility.AddFor(service, definingType, registerForClients, this);
			#else
			ServiceUtility.AddFor(service, definingType, toClients, this);
			#endif
		}

		private void Deregister()
		{
			#if UNITY_EDITOR
			EditorApplication.hierarchyChanged -= OnValidateDelayed;
			#endif

			if(service && definingType.Value is Type definingTypeValue)
			{
				ServiceUtility.RemoveFromClients(service, definingTypeValue, toClients, this);
			}
		}

		#if DEBUG || INIT_ARGS_SAFE_MODE
		private static bool IsValidDefiningTypeFor([DisallowNull] Type definingType, [DisallowNull] Component service)
			=> ServiceTagUtility.IsValidDefiningTypeFor(definingType, service);
		#endif

		#if UNITY_EDITOR
		private void OnValidate()
		{
			if(!service
			|| service.gameObject != gameObject
			|| definingType.Value is not Type type
			|| !IsValidDefiningTypeFor(type, service))
			{
				EditorApplication.delayCall = OnValidateDelayed;
				EditorApplication.delayCall += OnValidateDelayed;
				return;
			}

			OnValidServiceTagLoaded();
		}

		private void OnValidateDelayed()
		{
			if(!this)
			{
				return;
			}

			if(!service)
			{
				HandleMissingService();
				return;
			}

			if(DefiningType is not Type definingType)
			{
				if(EditorUtility.scriptCompilationFailed)
				{
					#if DEV_MODE
					Debug.Log($"ServiceTag on GameObject \"{name}\" is missing its target, but won't remove it from the GameObject because there are compile errors.", gameObject);
					#endif
					return;
				}

				#if DEV_MODE
				Debug.Log($"ServiceTag on GameObject \"{name}\" is missing its {nameof(DefiningType)}. Removing it from the GameObject.", gameObject);
				#endif
				Undo.DestroyObjectImmediate(this);
				return;
			}

			if(!ServiceTagUtility.IsValidDefiningTypeFor(definingType, service))
			{
				#if DEV_MODE
				Debug.Log($"ServiceTag on GameObject \"{name}\" has a {nameof(DefiningType)} that is not assignable from service of type {service.GetType()}. Removing it from the GameObject.", gameObject);
				#endif
				Undo.DestroyObjectImmediate(this);
				return;
			}

			if(service.gameObject != gameObject)
			{
				#if DEV_MODE
				Debug.Log($"Moving Service tag {definingType.Name} of {service.GetType().Name} from {name} to {service.gameObject.name}...", service.gameObject);
				#endif
				ComponentUtility.CopyComponent(this);
				ComponentUtility.PasteComponentAsNew(service.gameObject);
				Undo.DestroyObjectImmediate(this);
				return;
			}
			
			OnValidServiceTagLoaded();

			void HandleMissingService()
			{
				// Handle "Missing" / "Destroyed" service in particular; not unassigned.
				if(service is not null && service.GetHashCode() != 0)
				{
					var instancesOfServiceType = Find.All(service.GetType(), true);

					if(instancesOfServiceType.Where(instance => !ServiceTagUtility.HasServiceTag(instance)).SingleOrDefaultNoException() is Component instance)
					{
						#if DEV_MODE
						Debug.Log($"Moving Service tag {DefiningType.Name} of {instance.GetType().Name} from {name} to {instance.gameObject.name}...", instance.gameObject);
						#endif

						service = instance;
						ComponentUtility.CopyComponent(this);
						Undo.DestroyObjectImmediate(this);
						ComponentUtility.PasteComponentAsNew(instance.gameObject);
						return;
					}

					// Ensure that data corruption does not occur if references are only temporarily missing due to script compillation having failed.
					if(EditorUtility.scriptCompilationFailed)
					{
						#if DEV_MODE
						Debug.Log($"ServiceTag on GameObject \"{name}\" is missing its target, but won't remove it from the GameObject because there are compile errors.", gameObject);
						#endif
						return;
					}

					// Since the game object is being inspected and target has become missing, it's likely
					// that the user removed the component from the game object using the Inspector.
					// In this case silently remove the service tag as well.
					if(Array.IndexOf(Selection.gameObjects, gameObject) != -1)
					{
						#if DEV_MODE
						Debug.LogWarning($"ServiceTag on GameObject \"{name}\" is missing its target. It was probably removed by the user. Removing the ServiceTag as well.", gameObject);
						#endif

						Undo.DestroyObjectImmediate(this);
						return;
					}
				}

				if(EditorUtility.scriptCompilationFailed)
				{
					#if DEV_MODE
					Debug.Log($"ServiceTag on GameObject \"{name}\" is missing its target, but won't remove it from the GameObject because there are compile errors.", gameObject);
					#endif
					return;
				}

				#if DEV_MODE
				Debug.LogWarning($"ServiceTag on GameObject \"{name}\" is missing its target. Removing it from the GameObject.", gameObject);
				#endif

				Undo.DestroyObjectImmediate(this);
			}
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

		void ISerializationCallbackReceiver.OnBeforeSerialize() { }

		public async void OnAfterDeserialize()
		{
			await Until.UnitySafeContext();

			if(!this || !service || definingType.Value is null)
			{
				return;
			}

			Register();
		}

		NullGuardResult INullGuardByType.EvaluateNullGuard<TValue>(Component client)
		{
			if(definingType.Value is not Type serviceType || !service)
			{
				return NullGuardResult.InvalidValueProviderState;
			}

			if(serviceType != typeof(TValue) || typeof(TValue).IsValueType)
			{
				return NullGuardResult.TypeNotSupported;
			}

			if(!IsAvailableToAnyClient() && (!client || !IsAvailableToClient(client.gameObject)))
			{
				return NullGuardResult.ClientNotSupported;
			}

			if(service is not TValue && service is not IWrapper<TValue>)
			{
				return NullGuardResult.TypeNotSupported;
			}

			if(!Find.In<TValue>(service, out _))
			{
				return NullGuardResult.ValueProviderValueMissing;
			}

			return NullGuardResult.Passed;
		}
		#endif
	}
}