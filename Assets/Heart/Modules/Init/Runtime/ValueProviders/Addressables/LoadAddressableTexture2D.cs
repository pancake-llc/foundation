#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Sisus.Init.ValueProviders.ValueProviderUtility;
using Object = UnityEngine.Object;

namespace Sisus.Init.ValueProviders
{
	/// <summary>
	/// Loads an addressable <see cref="Texture2D"/> asset with the given key.
	/// <para>
	/// Can be used to retrieve an Init argument at runtime.
	/// </para>
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, typeof(Texture2D), Order = 10)]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME)]
	#endif
	internal sealed class LoadAddressableTexture2D : ScriptableObject<AssetReferenceTexture2D>, IEquatable<LoadAddressableTexture2D>, IValueReleaser<Texture2D>
		, IValueProviderAsync<Texture2D>
		#if UNITY_EDITOR
		, INullGuard
		#endif
	{
		private const string MENU_NAME = "Load Addressable Texture2D";

		[SerializeField]
		private AssetReferenceTexture2D addressableAsset;

		#if !UNITY_EDITOR
		#pragma warning disable CS0649 // value is assigned to in the editor, but said code is stripped from builds
		#endif

		[SerializeField, HideInInspector]
		private bool isSharedInstance;

		#if !UNITY_EDITOR
		#pragma warning restore CS0649
		#endif

		private readonly HashSet<Component> clients = new HashSet<Component>();

		protected override void Init(AssetReferenceTexture2D addressableAsset) => this.addressableAsset = addressableAsset;

		/// <summary>
		/// Loads an addressable asset with the configured key asynchronously.
		/// </summary>
		/// <typeparam name="TValue"> Type of asset to load. </typeparam>
		/// <param name="client"> This parameter is ignored. </param>
		/// <returns>
		/// An asset of type <typeparamref name="TValue"/>.
		/// </returns>
		public async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<Texture2D>
		#else
		System.Threading.Tasks.Task<Texture2D>
		#endif
		GetForAsync([AllowNull] Component client)
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				return addressableAsset.editorAsset;
			}
			#endif

			#if DEV_MODE && UNITY_EDITOR
			Debug.Log($"IsValid:{addressableAsset.IsValid()}, RuntimeKeyIsValid:{addressableAsset.RuntimeKeyIsValid()}, SubObjectName:{addressableAsset.SubObjectName}, IsDone:{addressableAsset.IsDone}, Status:{(addressableAsset.IsValid() ? addressableAsset.OperationHandle.Status : AsyncOperationStatus.None)}, editorAsset:{(addressableAsset.editorAsset == null ? "null" : addressableAsset.editorAsset.name)}, HasValue:{(this as INullGuard).EvaluateNullGuard(null)}");
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(!addressableAsset.RuntimeKeyIsValid() || string.IsNullOrEmpty(addressableAsset.SubObjectName))
			{
				return default;
			}
			#endif

			if(clients.Add(client) && !addressableAsset.IsValid())
			{
				return await addressableAsset.LoadAssetAsync<Texture2D>().Task;
			}

			return addressableAsset.Asset as Texture2D;
		}

		#if UNITY_EDITOR
		private void OnValidate()
		{
			bool setIsSharedInstance = UnityEditor.AssetDatabase.IsMainAsset(this);
			if(setIsSharedInstance != isSharedInstance)
			{
				isSharedInstance = setIsSharedInstance;
				UnityEditor.EditorUtility.SetDirty(this);
			}

			#if DEV_MODE && UNITY_EDITOR
			Debug.Log($"name:\"{name}\" isSharedInstance:{isSharedInstance}");
			#endif

			if(isSharedInstance)
			{
				return;
			}

			string setName = name;
			string key = addressableAsset.RuntimeKey?.ToString();

			if(addressableAsset.editorAsset is Object asset && asset)
			{
				setName = asset.name;
			}
			else if(!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(name))
			{
				// Preserve the old name, because it could help in debugging,
				// e.g. if the asset has been deleted
				return;
			}

			if(string.IsNullOrEmpty(setName))
			{
				setName = key;
			}

			if(!string.IsNullOrEmpty(addressableAsset.SubObjectName))
			{
				if(!string.IsNullOrEmpty(setName))
				{
					setName += "/" + addressableAsset.SubObjectName;
				}
				else
				{
					setName = addressableAsset.SubObjectName;
				}
			}

			if(string.IsNullOrEmpty(setName))
			{
				setName = "None";
			}

			if(name != setName)
			{
				name = setName;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
		#endif

		private void OnDestroy()
		{
			clients.Clear();
			ReleaseAsset();
		}

		private void ReleaseAsset()
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				return;
			}
			#endif

			#if DEV_MODE && UNITY_EDITOR
			Debug.Log($"ReleaseAsset IsValid:{addressableAsset.IsValid()}, RuntimeKeyIsValid:{addressableAsset.RuntimeKeyIsValid()}, SubObjectName:{addressableAsset.SubObjectName}, IsDone:{addressableAsset.IsDone}, Status:{(addressableAsset.IsValid() ? addressableAsset.OperationHandle.Status : AsyncOperationStatus.None)}, editorAsset:{(addressableAsset.editorAsset == null ? "null" : addressableAsset.editorAsset.name)}, HasValue:{(this as INullGuard).EvaluateNullGuard(null)}");
			#endif

			if(addressableAsset.IsValid())
			{
				addressableAsset.ReleaseAsset();
			}
		}

		public bool Equals(LoadAddressableTexture2D other) => other is not null && addressableAsset == other.addressableAsset;
		public override bool Equals(object other) => other is LoadAddressableTexture2D loadAddressable && Equals(loadAddressable);
		public override int GetHashCode() => addressableAsset?.AssetGUID?.GetHashCode() ?? 0;

		public void Release([AllowNull] Component client, Texture2D value)
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				return;
			}
			#endif

			if(!isSharedInstance && this != null
			#if UNITY_EDITOR
			&& !UnityEditor.AssetDatabase.IsMainAsset(this)
			#endif
			)
			{
				Destroy(this);
				return;
			}

			if(clients.Remove(client) && clients.Count == 0)
			{
				ReleaseAsset();
			}
		}

		#if UNITY_EDITOR
		NullGuardResult INullGuard.EvaluateNullGuard([AllowNull] Component client)
		{
			if(!addressableAsset.RuntimeKeyIsValid())
			{
				return NullGuardResult.InvalidValueProviderState;
			}
			
			if(addressableAsset.OperationHandle.IsValid() && addressableAsset.OperationHandle.OperationException is not null)
			{
				return NullGuardResult.ValueProviderException;
			}

			if(addressableAsset.editorAsset == null)
			{
				return NullGuardResult.ValueProviderValueMissing;
			}

			return NullGuardResult.Passed;
		}
		#endif
	}
}
#endif