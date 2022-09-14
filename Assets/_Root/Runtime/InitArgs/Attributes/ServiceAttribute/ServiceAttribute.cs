using JetBrains.Annotations;
using System;

namespace Pancake.Init
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
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class ServiceAttribute : Attribute
	{
		/// <summary>
		/// Class or interface type that uniquely defines the service and can be used to retrieve an instance of the it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from, or the exact type of the service.
		/// </para>
		/// </summary>
		[CanBeNull]
		public readonly Type definingType;

		private LoadMethod loadMethod;
		private string loadData;

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		/// <summary>
		/// Addressable key using which an instance of the service can be loaded during initialization.
		/// <para>
		/// <see langword="null"/> if the service is not loaded using the Addressable asset system.
		/// </para>
		/// </summary>
		[CanBeNull]
		public string AddressableKey
		{
			get => loadMethod == LoadMethod.LoadAddressable ? loadData : null;

			set
			{
				if(!string.IsNullOrEmpty(value))
				{
					loadMethod = LoadMethod.LoadAddressable;
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
		[CanBeNull]
		public string ResourcePath
		{
			get => loadMethod == LoadMethod.LoadResource ? loadData : null;

			set
			{
				if(!string.IsNullOrEmpty(value))
				{
					loadMethod = LoadMethod.LoadResource;
					loadData = value;
				}
			}
        }

		/// <summary>
		/// If <see langword="true"/> then an instance of the service
		/// will be loaded from the initial scene.
		/// <para>
		/// <seealso cref="UnityEngine.Object.FindObjectOfType(true)"/>
		/// </para>
		/// </summary>
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
		/// Class or interface type that uniquely defines the service and can be used to retrieve an instance of the it.
		/// <para>
		/// This should be an interface that the service implement, a base type that the service derives from, or the exact type of the service.
		/// </para>
		/// </param>
		public ServiceAttribute([CanBeNull] Type definingType)
		{
			this.definingType = definingType;
			loadMethod = LoadMethod.CreateInstance;
			loadData = null;
		}

		private enum LoadMethod
		{
			CreateInstance = 0,
			FindObjectOfType = 1,
			LoadResource = 2,

			#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
			LoadAddressable = 100
			#endif
		}
	}
}