using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sisus.Init
{
	/// <summary>
	/// Classes that have the <see cref="EditorServiceAttribute"/> can provide services
	/// for one or more client objects in the editor in edit mode.
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
	/// <see cref="Service{TDefiningClassOrInterface}.Instance"/> can also be used to manually retrieve a service object.
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
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false), Conditional("UNITY_EDITOR")]
	public class EditorServiceAttribute : Attribute
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
		private string loadData;

		public bool FindAssetByType
{
			get => loadMethod == LoadMethod.FindAssetByType;

			set
			{
				if(value)
				{
					loadMethod = LoadMethod.FindAssetByType;
				}
			}
		}

		/// <summary>
		/// Resources path from which an instance of the service can be loaded during initialization.
		/// <para>
		/// <see langword="null"/> if the service is not loaded from a Resources folder.
		/// </para>
		/// </summary>
		[MaybeNull][DisallowNull]
		public string AssetPath
		{
			get => loadMethod == LoadMethod.LoadAssetAtPath ? loadData : null;

			set
			{
				if(!string.IsNullOrEmpty(value))
				{
					loadMethod = LoadMethod.LoadAssetAtPath;
					loadData = value;
				}
			}
		}

		/// <summary>
		/// Resources path from which an instance of the service can be loaded during initialization.
		/// <para>
		/// <see langword="null"/> if the service is not loaded from a Resources folder.
		/// </para>
		/// </summary>
		[MaybeNull][DisallowNull]
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

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		/// <summary>
		/// Addressable key using which an instance of the service can be loaded during initialization.
		/// <para>
		/// <see langword="null"/> if the service is not loaded using the Addressable asset system.
		/// </para>
		/// </summary>
		[MaybeNull][DisallowNull]
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
		/// Editor Default Resources path from which an instance of the service can be loaded during initialization.
		/// <para>
		/// <see langword="null"/> if the service is not loaded from the Editor Default Resources folder.
		/// </para>
		/// </summary>
		[MaybeNull][DisallowNull]
		public string EditorDefaultResourcesPath
		{
			get => loadMethod == LoadMethod.EditorDefaultResources ? loadData : null;

			set
			{
				if(!string.IsNullOrEmpty(value))
				{
					loadMethod = LoadMethod.EditorDefaultResources;
					loadData = value;
				}
			}
		}

		/// <summary>
		/// Classes that have the <see cref="EditorServiceAttribute"/> can provide services
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
		/// A fresh instance of the service will be created unless <see cref="EditorDefaultResourcesPath"/> or
		/// <see cref="AddressableKey"/> is used to specify a location from which an existing
		/// instance of the service can be loaded.
		/// </para>
		/// </summary>
		public EditorServiceAttribute() : this(null) { }

		/// <summary>
		/// Classes that have the <see cref="EditorServiceAttribute"/> can provide services
		/// for one or more client objects.
		/// <para>
		/// A single instance of each class that has the attribute will automatically
		/// get cached behind the scenes making them ready for clients to retrieve as needed.
		/// </para>
		/// <para>
		/// A fresh instance of the service will be created unless <see cref="EditorDefaultResourcesPath"/> or
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
		public EditorServiceAttribute([AllowNull] Type definingType)
		{
			this.definingType = definingType;
			loadMethod = LoadMethod.CreateInstance;
			loadData = null;
		}

		private enum LoadMethod
		{
			CreateInstance = 0, // CreateInstance
			FindAssetByType, // AssetDatabase.FindAssets($"t:{type.Name}");
			LoadAssetAtPath, // AssetDatabase.LoadAssetAtPath(addressableAssetEntry.AssetPath);
			LoadAddressable, // AssetDatabase.LoadAssetAtPath(addressableAssetEntry.AssetPath);
			LoadResource, // Resources.Load
			EditorDefaultResources // EditorGUIUtility.Load
		}
	}
}