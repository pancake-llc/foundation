#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using static Sisus.Init.ValueProviders.ValueProviderUtility;
using Object = UnityEngine.Object;

namespace Sisus.Init.ValueProviders
{
	/// <summary>
	/// Loads a <see cref="Sprite"/> from an addressable <see cref="SpriteAtlas"/> asset with the given key.
	/// <para>
	/// Can be used to retrieve an Init argument at runtime.
	/// </para>
	/// </summary>
	/// <seealso cref="LoadAddressableSprite"/>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, typeof(Sprite), Order = 10)]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME)]
	#endif
	internal sealed class LoadAddressableAtlasedSprite : ScriptableObject<AssetReferenceAtlasedSprite>, IEquatable<LoadAddressableAtlasedSprite>, IValueReleaser<Sprite>
		, IValueProviderAsync<Sprite>
		#if UNITY_EDITOR
		, INullGuard
		#endif
	{
		private const string MENU_NAME = "Load Addressable Atlased Sprite";

		[SerializeField]
		private AssetReferenceAtlasedSprite atlasedSprite = new("");

		#if !UNITY_EDITOR
		#pragma warning disable CS0649 // value is assigned to in the editor, but said code is stripped from builds
		#endif

		[SerializeField, HideInInspector]
		private bool isSharedInstance;

		#if !UNITY_EDITOR
		#pragma warning restore CS0649
		#endif

		private readonly HashSet<Component> clients = new HashSet<Component>();

		protected override void Init(AssetReferenceAtlasedSprite addressableAsset) => this.atlasedSprite = addressableAsset;

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
		Awaitable<Sprite>
		#else
		System.Threading.Tasks.Task<Sprite>
		#endif
		GetForAsync([AllowNull] Component client)
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				return atlasedSprite.editorAsset
					? atlasedSprite.editorAsset.GetSprite(atlasedSprite.SubObjectName)
					: null;
			}
			#endif

			#if DEV_MODE && UNITY_EDITOR
			Debug.Log($"IsValid:{atlasedSprite.IsValid()}, RuntimeKeyIsValid:{atlasedSprite.RuntimeKeyIsValid()}, SubObjectName:{atlasedSprite.SubObjectName}, IsDone:{atlasedSprite.IsDone}, Status:{(atlasedSprite.IsValid() ? atlasedSprite.OperationHandle.Status : AsyncOperationStatus.None)}, editorAsset:{(atlasedSprite.editorAsset == null ? "null" : atlasedSprite.editorAsset.name)}, HasValue:{(this as INullGuard).EvaluateNullGuard(null)}");
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(!atlasedSprite.RuntimeKeyIsValid())
			{
				Debug.LogWarning($"Invalid Runtime Key - {GetType().Name}.GetForAsync({client.GetType().Name}) failed.\n{atlasedSprite}", client);
				return default;
			}

			if(string.IsNullOrEmpty(atlasedSprite.SubObjectName))
			{
				Debug.LogWarning($"Missing Sub Object Name - {GetType().Name}.GetForAsync({client.GetType().Name}) failed.\n{atlasedSprite}", client);
				return default;
			}

			if(atlasedSprite.IsValid() && atlasedSprite.OperationHandle.Status == AsyncOperationStatus.Failed)
			{
				return null;
			}
			#endif
			
			if(clients.Add(client) && clients.Count == 1)
			{
				#if UNITY_EDITOR
				if(atlasedSprite.OperationHandle.IsValid())
				{
					#if DEV_MODE
					Debug.LogWarning("Releasing previous operation handle, because internal counter is at zero.");
					#endif
					atlasedSprite.ReleaseAsset();
				}
				#endif

				return await atlasedSprite.LoadAssetAsync().Task;
			}

			if(!atlasedSprite.IsDone && atlasedSprite.OperationHandle.PercentComplete > 0f)
			{
				await atlasedSprite.OperationHandle.Task;
			}

			return atlasedSprite.Asset as Sprite;
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
			string key = atlasedSprite.RuntimeKey?.ToString();

			if(atlasedSprite.editorAsset is Object asset && asset)
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

			if(!string.IsNullOrEmpty(atlasedSprite.SubObjectName))
			{
				if(!string.IsNullOrEmpty(setName))
				{
					setName += "/" + atlasedSprite.SubObjectName;
				}
				else
				{
					setName = atlasedSprite.SubObjectName;
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
			Debug.Log($"ReleaseAsset IsValid:{atlasedSprite.IsValid()}, RuntimeKeyIsValid:{atlasedSprite.RuntimeKeyIsValid()}, SubObjectName:{atlasedSprite.SubObjectName}, IsDone:{atlasedSprite.IsDone}, Status:{(atlasedSprite.IsValid() ? atlasedSprite.OperationHandle.Status : AsyncOperationStatus.None)}, editorAsset:{(atlasedSprite.editorAsset == null ? "null" : atlasedSprite.editorAsset.name)}, HasValue:{(this as INullGuard).EvaluateNullGuard(null)}");
			#endif

			if(atlasedSprite.IsValid())
			{
				atlasedSprite.ReleaseAsset();
			}
		}

		public bool Equals(LoadAddressableAtlasedSprite other) => other is not null && atlasedSprite == other.atlasedSprite;
		public override bool Equals(object other) => other is LoadAddressableAtlasedSprite loadAddressable && Equals(loadAddressable);
		public override int GetHashCode() => atlasedSprite?.AssetGUID?.GetHashCode() ?? 0;

		public void Release([AllowNull] Component client, Sprite value)
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				return;
			}
			#endif

			if(value != null)
			{
				Destroy(value);
			}
			#if DEV_MODE
			else { Debug.LogWarning($"{GetType().Name}.Release({client}, {value}) called with null value.", this); }
			#endif

			if(!isSharedInstance && this
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
			if(string.IsNullOrEmpty(atlasedSprite.SubObjectName))
			{
				return NullGuardResult.InvalidValueProviderState;
			}

			SpriteAtlas spriteAtlas = atlasedSprite.editorAsset;
			if(spriteAtlas == null)
			{
				return NullGuardResult.ValueProviderValueMissing;
			}

			if(atlasedSprite.OperationHandle.IsValid() && atlasedSprite.OperationHandle.OperationException is not null)
			{
				return NullGuardResult.ValueProviderException;
			}

			Sprite sprite = spriteAtlas.GetSprite(atlasedSprite.SubObjectName);
			if(sprite == null)
			{
				return NullGuardResult.ValueProviderValueMissing;
			}

			// SpriteAtlas.GetSprite returns a copy; we should destroy it to avoid memory leaking.
			DestroyImmediate(sprite);

			return NullGuardResult.Passed;
		}
		#endif
	}
}
#endif