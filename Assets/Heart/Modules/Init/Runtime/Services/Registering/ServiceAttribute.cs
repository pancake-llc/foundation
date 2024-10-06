using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Classes that have the <see cref="ServiceAttribute"/> can provide services
	/// for one or more client objects.
	/// <para>
	/// A single instance of each class that has the attribute will automatically
	/// get loaded behind the scenes making them ready for clients to retrieve as needed.
	/// </para>
	/// <para>
	/// Objects deriving from <see cref="MonoBehaviour{TDefiningClassOrInterface}"/> or <see cref="ScriptableObject{TDefiningClassOrInterface}"/>
	/// receive the service during initialization automatically.
	/// </para>
	/// <para>
	/// Other clients can retrieve a service by implementing the <see cref="IArgs{TDefiningClassOrInterface}"/>
	/// interface and calling <see cref="InitArgs.TryGet{TClient, TDefiningClassOrInterface}"/> during initialization.
	/// </para>
	/// <para>
	/// It is possible to receive more than one services automatically by implementing an <see cref="IArgs{TFirstService, TSecondService}">IArgs{}</see>
	/// interface with more than one service argument (upto a maximum of five).
	/// </para>
	/// <para>
	/// Services can also receive other services during initialization by implementing an <see cref="IInitializable{TService}"/> interface targeting
	/// the services they depend on.
	/// </para>
	/// <para>
	/// <see cref="Initializer{}"/> classes are also able to retrieve all service instances automatically and inject them to the client's Init method.
	/// </para>
	/// <para>
	/// <see cref="Service{TDefiningClassOrInterface}.Instance"/> can be used to manually retrieve a service object.
	/// </para>
	/// <para>
	/// Service objects that implement <see cref="IAwake"/>, <see cref="IOnEnable"/> or <see cref="IStart"/> receive the relevant event after all
	/// services have been created and all services have received the other services they depend on.
	/// </para>
	/// <para>
	/// A service can optionally receive callbacks during select unity event functions by implementing one or more of the following interfaces:
	/// <list type="bullet">
	/// <item>
	/// <term> <see cref="IAwake"/> </term>
	/// <description> Receive callback during the MonoBehaviour.Awake event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IOnEnable"/> </term>
	/// <description> Receive callback during the MonoBehaviour.OnEnable event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IStart"/> </term>
	/// <description> Receive callback during the MonoBehaviour.Start event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IUpdate"/> </term>
	/// <description> Receive the MonoBehaviour.Update event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="ILateUpdate"/> </term>
	/// <description> Receive callback during the MonoBehaviour.LateUpdate event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IFixedUpdate"/> </term>
	/// <description> Receive callback during the MonoBehaviour.FixedUpdate event. </description>
	/// </item>
	/// </list>
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
	[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	[RequireAttributeUsages]
	public class ServiceAttribute : PreserveAttribute
	{
		/// <summary>
		/// Class or interface type that uniquely defines the service and can be used to retrieve an instance of it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from, or the exact type of the service.
		/// </para>
		/// </summary>
		[MaybeNull]
		public readonly Type definingType;

		private LoadMethod loadMethod;

		[MaybeNull]
		private string loadData;

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		/// <summary>
		/// Addressable key using which an instance of the service can be loaded during initialization.
		/// <para>
		/// <see langword="null"/> if the service is not loaded using the Addressable asset system.
		/// </para>
		/// </summary>
		[DisallowNull]
		public string AddressableKey
		{
			get => loadMethod is LoadMethod.Addressable or LoadMethod.AddressableAsync
							  or LoadMethod.LoadAddressable or LoadMethod.LoadAddressableAsync
							  or LoadMethod.InstantiateAddressable or LoadMethod.InstantiateAddressableAsync
							  ? loadData : null;

			set
			{
				if(!string.IsNullOrEmpty(value))
				{
					loadMethod = loadMethod switch
					{
						LoadMethod.Default => LoadMethod.Addressable,
						LoadMethod.Async => LoadMethod.AddressableAsync,
						LoadMethod.Load => LoadMethod.LoadAddressable,
						LoadMethod.Instantiate => LoadMethod.InstantiateAddressable,
						LoadMethod.LoadAsync =>  LoadMethod.LoadAddressableAsync,
						LoadMethod.InstantiateAsync => LoadMethod.InstantiateAddressableAsync,
						_ => loadMethod
					};

					loadData = value;
				}
			}
		}
		#endif

		/// <summary>
		/// Resources path from which an instance of the service can be loaded during initialization.
		/// <para>
		/// <see langword="null"/> if the service is not loaded from a Resources folder.
		/// </para>
		/// </summary>
		[DisallowNull]
		public string ResourcePath
		{
			get => loadMethod is LoadMethod.Resource or LoadMethod.ResourceAsync
							  or LoadMethod.LoadResource or LoadMethod.LoadResourceAsync
							  or LoadMethod.InstantiateResource or LoadMethod.InstantiateResourceAsync
							  ? loadData : null;

			set
			{
				if(!string.IsNullOrEmpty(value))
				{
					loadMethod = loadMethod switch
					{
						LoadMethod.Default => LoadMethod.Resource,
						LoadMethod.Async => LoadMethod.ResourceAsync,
						LoadMethod.Load => LoadMethod.LoadResource,
						LoadMethod.Instantiate => LoadMethod.InstantiateResource,
						LoadMethod.LoadAsync =>  LoadMethod.LoadResourceAsync,
						LoadMethod.InstantiateAsync => LoadMethod.InstantiateResourceAsync,
						_ => loadMethod
					};

					loadData = value;
				}
			}
		}

		/// <summary>
		/// If <see langword="true"/> then an instance of the service
		/// will be loaded from the initial scene.
		/// </summary>
		/// <seealso cref="UnityEngine.Object.FindObjectOfType(true)"/>
		public bool FindFromScene
		{
			get => loadMethod == LoadMethod.FindObjectOfType;

			set
			{
				if(value)
				{
					loadMethod = LoadMethod.FindObjectOfType;
				}
			}
		}

		/// <summary>
		/// If <see langword="true"/>, then service instance will be a direct reference to an asset that is loaded using a resource path or an addressable key.
		/// <para>
		/// If <see langword="false"/>, then service instance will be a clone instantiated from an asset that is loaded using a resource path or an addressable key.
		/// </para>
		/// <para>
		/// If <see langword="null"/>, and service asset is a prefab, then service instance will be a clone instantiated from the asset.
		/// </para>
		/// <para>
		/// If <see langword="null"/>, and service asset is not a prefab, then service instance will be a direct reference to the asset.
		/// </para>
		/// </summary>
		public bool? Instantiate
		{
			get
			{
				if(loadMethod is LoadMethod.Instantiate or LoadMethod.InstantiateAsync
							  or LoadMethod.InstantiateResource or LoadMethod.InstantiateResourceAsync
							  #if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
							  or LoadMethod.InstantiateAddressable or LoadMethod.InstantiateAddressableAsync
							  #endif
				){
					return true;
				}

				if(loadMethod is LoadMethod.Load or LoadMethod.LoadAsync
							  or LoadMethod.LoadResource or LoadMethod.LoadResourceAsync
							  #if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
							  or LoadMethod.LoadAddressable or LoadMethod.LoadAddressableAsync
							  #endif
				){
					return false;
				}

				return null;
			}

			set
			{
				if(value is not bool instantiate)
				{
					return;
				}

				if(instantiate)
				{
					switch(loadMethod)
					{
						case LoadMethod.Default:
						case LoadMethod.Load:
							loadMethod = LoadMethod.Instantiate;
							return;
						case LoadMethod.Async:
						case LoadMethod.LoadAsync:
							loadMethod = LoadMethod.InstantiateAsync;
							return;
						case LoadMethod.Resource:
						case LoadMethod.LoadResource:
							loadMethod = LoadMethod.InstantiateResource;
							return;
						case LoadMethod.ResourceAsync:
						case LoadMethod.LoadResourceAsync:
							loadMethod = LoadMethod.InstantiateResourceAsync;
							return;
						#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
						case LoadMethod.Addressable:
						case LoadMethod.LoadAddressable:
							loadMethod = LoadMethod.InstantiateAddressable;
							return;
						case LoadMethod.AddressableAsync:
						case LoadMethod.LoadAddressableAsync:
							loadMethod = LoadMethod.InstantiateAddressableAsync;
							return;
						#endif
					}
				}
				else
				{
					switch(loadMethod)
					{
						case LoadMethod.Default:
						case LoadMethod.Instantiate:
							loadMethod = LoadMethod.Load;
							return;
						case LoadMethod.Async:
						case LoadMethod.InstantiateAsync:
							loadMethod = LoadMethod.LoadAsync;
							return;
						case LoadMethod.Resource:
						case LoadMethod.InstantiateResource:
							loadMethod = LoadMethod.LoadResource;
							return;
						case LoadMethod.ResourceAsync:
						case LoadMethod.InstantiateResourceAsync:
							loadMethod = LoadMethod.LoadResourceAsync;
							return;
						#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
						case LoadMethod.Addressable:
						case LoadMethod.InstantiateAddressable:
							loadMethod = LoadMethod.LoadAddressable;
							return;
						case LoadMethod.AddressableAsync:
						case LoadMethod.InstantiateAddressableAsync:
							loadMethod = LoadMethod.LoadAddressableAsync;
							return;
						#endif
					}
				}
			}
		}

		/// <summary>
		/// If set to <see langword="true"/> then an instance of the service will only
		/// be created on demand when the first client requests and instance.
		/// <para>
		/// If <see langword="false"/>, which is the default value of this property,
		/// then the instance will be created immediately when the game loads.
		/// </para>
		/// <para>
		/// Note that the initialization of service instances of some types is not a
		/// thread safe operation, so if <see cref="LazyInit"/> is set to <see langword="true"/>
		/// and the first client that requests the service does so from a background
		/// thread the initialization of the service might fail.
		/// </para>
		/// </summary>
		public bool LazyInit { get; set; }

		/// <summary>
		/// If set to <see langword="true"/> then the service will be loaded asynchronously if possible.
		/// <para>
		/// When set to <see langword="true"/> and <see cref="ResourcePath"/> is given a non-empty value,
		/// then <see cref="UnityEngine.Resources.LoadAsync"/> will be used to load the resource.
		/// </para>
		/// <para>
		/// When set to <see langword="true"/> and <see cref="AddressableKey"/> is given a non-empty value,
		/// then <see cref="Addressables.LoadAssetAsync"/> will be used to load the asset.
		/// </para>
		/// </summary>
		public bool LoadAsync
		{
			get => loadMethod is LoadMethod.Async or LoadMethod.LoadAsync or LoadMethod.InstantiateAsync
							  or LoadMethod.LoadResourceAsync or LoadMethod.InstantiateResourceAsync
							  #if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
							  or LoadMethod.AddressableAsync
							  or LoadMethod.LoadAddressableAsync
							  or LoadMethod.InstantiateAddressableAsync
								#endif
								;

			set
			{
				if(!value)
				{
					return;
				}

				switch(loadMethod)
				{
					case LoadMethod.Default:
						loadMethod = LoadMethod.Async;
						return;
					case LoadMethod.Instantiate:
						loadMethod = LoadMethod.InstantiateAsync;
						return;
					case LoadMethod.Load:
						loadMethod = LoadMethod.LoadAsync;
						return;
					case LoadMethod.Resource:
						loadMethod = LoadMethod.ResourceAsync;
						return;
					case LoadMethod.LoadResource:
						loadMethod = LoadMethod.LoadResourceAsync;
						return;
					case LoadMethod.InstantiateResource:
						loadMethod = LoadMethod.InstantiateResourceAsync;
						return;
					#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
					case LoadMethod.Addressable:
						loadMethod = LoadMethod.AddressableAsync;
						return;
					case LoadMethod.LoadAddressable:
						loadMethod = LoadMethod.LoadAddressableAsync;
						return;
					case LoadMethod.InstantiateAddressable:
						loadMethod = LoadMethod.InstantiateAddressableAsync;
						return;
					#endif
				}
			}
		}

		#if DEV_MODE
		/// <summary>
		/// If set to <see langword="true"/> then the service will be automatically registered
		/// using all the interfaces that it implements as its <see cref="definingType">defining types</see>.
		/// </summary>
		public bool RegisterAsAllInterfaces { get; set; }
		#endif
		
		/// <summary>
		/// Classes that have the <see cref="ServiceAttribute"/> can provide services
		/// for one or more client objects.
		/// <para>
		/// A single instance of each class that has the attribute will automatically
		/// get cached behind the scenes making them ready for clients to retrieve as needed.
		/// </para>
		/// <para>
		/// The type of the class that contains the attribute will be used as the
		/// defining type for the service unless a <see cref="definingType"/> argument is provided.
		/// </para>
		/// <para>
		/// A fresh instance of the service will be created unless <see cref="ResourcePath"/> or
		/// <see cref="AddressableKey"/> is used to specify a location from which an existing
		/// instance of the service can be loaded.
		/// </para>
		/// </summary>
		public ServiceAttribute() : this(null) { }

		/// <summary>
		/// Classes that have the <see cref="ServiceAttribute"/> can provide services
		/// for one or more client objects.
		/// <para>
		/// A single instance of each class that has the attribute will automatically
		/// get cached behind the scenes making them ready for clients to retrieve as needed.
		/// </para>
		/// <para>
		/// A fresh instance of the service will be created unless <see cref="ResourcePath"/> or
		/// <see cref="AddressableKey"/> is used to specify a location from which an existing
		/// instance of the service can be loaded.
		/// </para>
		/// </summary>
		/// <param name="definingType">
		/// Class or interface type that uniquely defines the service and can be used to retrieve an instance of it.
		/// <para>
		/// This should be an interface that the service implement, a base type that the service derives from, or the exact type of the service.
		/// </para>
		/// </param>
		public ServiceAttribute([AllowNull] Type definingType)
		{
			this.definingType = definingType;
			loadMethod = LoadMethod.Default;
			loadData = null;
		}

		private enum LoadMethod
		{
			/// <summary>
			/// Uses <see cref="Instantiate"/> for prefabs and <see cref="Load"/> for other assets.
			/// </summary>
			Default,

			/// <summary>
			/// Loads an asset using either resources or addressables, and registers it as a service directly.
			/// </summary>
			Load,

			/// <summary>
			/// Creates a new instance from scratch or by cloning an asset.
			/// </summary>
			Instantiate,

			/// <summary>
			/// Finds an object from the initial scene, and registers it as a service.
			/// </summary>
			FindObjectOfType,

			/// <summary>
			/// Loads or instantiates an asset using resources, and registers it as a service directly.
			/// </summary>
			Resource,

			/// <summary>
			/// Loads an asset using resources, and registers it as a service directly.
			/// </summary>
			LoadResource,
			
			/// <summary>
			/// Loads an asset using resources, clones it, and registers the clone as a service.
			/// </summary>
			InstantiateResource,

			/// <summary>
			/// Asynchronously loads or instantiates an asset using either resources or addressables.
			/// </summary>
			Async,

			/// <summary>
			/// Asynchronously loads an asset using either resources or addressables, and registers it as a service directly.
			/// </summary>
			LoadAsync,

			/// <summary>
			/// Asynchronously loads an asset using either resources or addressables, clones it,
			/// and registers the clone as a service.
			/// </summary>
			InstantiateAsync,

			/// <summary>
			/// Asynchronously loads or instantiates an asset using resources.
			/// </summary>
			ResourceAsync,

			/// <summary>
			/// Asynchronously loads an asset using resources, and registers it as a service directly.
			/// </summary>
			LoadResourceAsync,

			/// <summary>
			/// Asynchronously loads an asset using resources, clones it, and registers the clone as a service.
			/// </summary>
			InstantiateResourceAsync,

			#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
			/// <summary>
			/// Loads or instantiates an asset using addressables, and registers it as a service directly.
			/// </summary>
			Addressable,

			/// <summary>
			/// Asynchronously loads or instantiates an asset using addressables.
			/// </summary>
			AddressableAsync,

			/// <summary>
			/// Loads an asset using addressables, and registers it as a service directly.
			/// </summary>
			LoadAddressable,
			
			/// <summary>
			/// Asynchronously loads an asset using addressables, and registers it as a service directly.
			/// </summary>
			LoadAddressableAsync,
			
			/// <summary>
			/// Loads an asset using addressables, clones it, and registers the clone as a service.
			/// </summary>
			InstantiateAddressable,

			/// <summary>
			/// Asynchronously loads an asset using addressables, clones it, and registers the clone as a service.
			/// </summary>
			InstantiateAddressableAsync
			#endif
		}
	}
}