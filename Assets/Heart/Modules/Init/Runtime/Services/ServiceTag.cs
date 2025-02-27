//#define SHOW_SERVICE_TAGS

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sisus.Init.Serialization;
using Sisus.Init.ValueProviders;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
#endif

namespace Sisus.Init.Internal
{
	[ExecuteAlways, AddComponentMenu(Hidden), DefaultExecutionOrder(ExecutionOrder.ServiceTag)]
	public sealed partial class ServiceTag : MonoBehaviour, IServiceProvider, IValueByTypeProvider, IValueProvider<Component>, IInitializable<Component, Clients, Type>
		#if UNITY_EDITOR
		, ISerializationCallbackReceiver
		#endif
	{
		private const string Hidden = "";

		[SerializeField]
		private Component service;

		[SerializeField,
		Tooltip("Specifies which clients can use this service.\n\n" +
				"When set to " + nameof(Clients.InChildren) + ", only clients that are attached to this GameObject or its children (including nested children) can access this service.\n\n" +
				"When set to " + nameof(Clients.InScene) + ", only clients that are in the same scene can access this service.\n\n" +
				"When set to " + nameof(Clients.Everywhere) + ", all clients can access this service, regardless of their location in a scene, or whether they are a scene object at all.")]
		private Clients toClients = Clients.Everywhere;

		[SerializeField]
		private _Type definingType = new();

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

				#if UNITY_EDITOR
				EditorApplication.delayCall = OnValidateDelayed;
				EditorApplication.delayCall += OnValidateDelayed;
				#endif

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(value is null)
				{
					Debug.LogWarning($"ServiceTag on GameObject \"{name}\" was assigned an invalid {nameof(DefiningType)} value: null.", gameObject);
				}
				else if(service && !IsValidDefiningTypeFor(value, service))
				{
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

		/// <summary>
		/// Specifies which clients can receive services from this provider.
		/// <para>
		/// When set to <see cref="Clients.InChildren"/>, only clients that are attached to the same GameObject as this provider, or any of its children (including nested children), can access its services.
		/// </para>
		/// <para>
		/// When set to <see cref="Clients.InScene"/>, only clients that are in the same scene as this provider, can access its services.
		/// </para>
		/// <para>
		/// When set to <see cref="Clients.Everywhere"/>, all clients are allowed to access its services, regardless of their location in scenes hierarchies, or whether they are a scene object at all.
		/// </para>
		/// </summary>
		internal Clients ToClients
		{
			get => toClients;

			set
			{
				toClients = value;
				
				#if UNITY_EDITOR
				EditorApplication.delayCall = OnValidateDelayed;
				EditorApplication.delayCall += OnValidateDelayed;
				#endif
			}
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
		
		NullGuardResult INullGuardByType.EvaluateNullGuard<TService>(Component client)
		{
			if(definingType.Value is not Type serviceType)
			{
				return NullGuardResult.InvalidValueProviderState;
			}

			if(serviceType != typeof(TService))
			{
				return NullGuardResult.TypeNotSupported;
			}

			if(!IsAvailableToAnyClient() && (!client || !IsAvailableToClient(client.gameObject)))
			{
				return NullGuardResult.ClientNotSupported;
			}

			#if UNITY_EDITOR
			// In the editor, perform some additional checks to be able to provide more
			// detailed information in the Inspector and help catch potential issues earlier.

			// In builds, trust that services will be configured properly.

			if(!service)
			{
				return NullGuardResult.InvalidValueProviderState;
			}

			if(service is not TService && !ValueProviderUtility.IsValueProvider(service))
			{
				return NullGuardResult.InvalidValueProviderState;
			}

			if(service is IWrapper wrapper)
			{
				if(wrapper.WrappedObject is not TService)
				{
					return GetValueProviderMissingResult();
				}
			}
			else if(service is IInitializer initializer)
			{
				if(initializer.Target is not object target || !Find.In<TService>(target, out _))
				{
					return GetValueProviderMissingResult();
				}
			}
			else if(ValueProviderUtility.TryGetValueProviderValue(service, out object value) || !Find.In<TService>(value, out _))
			{
				return GetValueProviderMissingResult();
			}

			NullGuardResult GetValueProviderMissingResult() =>
				EditorOnly.ThreadSafe.Application.TryGetIsPlaying(Context.Default, out bool isPlaying) && !isPlaying
					? NullGuardResult.ValueProviderValueNullInEditMode
					: NullGuardResult.ValueProviderValueMissing;
			#endif

			return NullGuardResult.Passed;
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

		/// <inheritdoc cref="IServiceProvider.TryGet{TService}(out TService)"/>
		public bool TryGet<TService>(out TService service) => TryGetFor(null, out service);

		/// <inheritdoc cref="IServiceProvider.TryGetFor{TService}(Component, out TService)"/>
		public bool TryGetFor<TService>([AllowNull] Component client, out TService service)
		{
			if(definingType.Value == typeof(TService)
			&& this.service
			&& Find.In(this.service, out service)
			&& (IsAvailableToAnyClient() || (client && IsAvailableToClient(client.gameObject))))
			{
				return true;
			}

			service = default;
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
				ServiceUtility.RemoveFrom(service, definingTypeValue, toClients, this);
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

			if(DefiningType is not { } definingType)
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
		#endif
	}
}