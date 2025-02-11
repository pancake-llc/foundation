//#define SHOW_REF_TAGS_IN_INSPECTOR

using System.Collections.Generic;
using Sisus.Init.Internal;
using UnityEngine;
using Id = Sisus.Init.Internal.Id;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Sisus.Init
{
	[DefaultExecutionOrder(ExecutionOrder.Referenceable)]
	internal sealed class RefTag : MonoBehaviour, IInitializable<Object>, IIdentifiable
	{
		private static readonly Dictionary<Id, Object> activeObjects = new();
		private static readonly Dictionary<Id, List<Object>> activeClones = new(); // one prefab can have more than one instance

		[SerializeField]
		internal Id guid = Id.Empty;

		[SerializeField]
		internal Object target = null;

		[SerializeField]
		private bool isPrefab = false;

		#if DEBUG || INIT_ARGS_SAFE_MODE
		#pragma warning disable CS0414
		[SerializeField]
		private string globalObjectIdSlow = null;
		[SerializeField]
		private Texture icon = null;
		#pragma warning restore CS0414
		#endif

		#if UNITY_EDITOR
		#pragma warning disable CS0414
		[SerializeField]
		private SceneAsset scene = null;
		#pragma warning restore CS0414
		#endif

		public Id Guid => guid;
		public Object Target => target;
		Id IIdentifiable.Id => guid;

		void IInitializable<Object>.Init(Object target)
		{
			this.target = target;

			#if UNITY_EDITOR
			if(!target)
			{
				scene = null;
				globalObjectIdSlow = null;
				isPrefab = false;
				guid = Id.Empty;
				icon = null;
				return;
			}

			var gameObject = GetGameObject(target);
			scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(gameObject.scene.path);
			globalObjectIdSlow = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
			isPrefab = IsPrefabAssetOrOpenInPrefabStage();
			icon = EditorGUIUtility.ObjectContent(target, target.GetType()).image;
			#endif
		}

		private void OnEnable() => Register();
		private void OnDisable() => Deregister();

		internal static Object GetInstance(Id guid)
		{
			lock(activeObjects)
			{
				if(activeObjects.TryGetValue(guid, out Object result))
				{
					return result;
				}
			}

			lock(activeClones)
			{
				if(activeClones.TryGetValue(guid, out var clones) && clones.Count > 0)
				{
					return clones[clones.Count - 1];
				}
			}

			return null;
		}

		private void Register()
		{
			if(IsRuntimeClone())
			{
				List<Object> clones;
				lock(activeClones)
				{
					if(!activeClones.TryGetValue(guid, out clones))
					{
						clones = new List<Object>();
						activeClones[guid] = clones;
					}

					clones.Add(target);
				}
				return;
			}

			lock(activeObjects)
			{
				#if UNITY_EDITOR
				if(activeObjects.TryGetValue(guid, out var existing))
				{
					// prioritize scene instances over prefabs
					if(existing == target || isPrefab)
					{
						return;
					}
				}
				#endif

				activeObjects[guid] = target;
			}

			bool IsRuntimeClone()
			{
				if(!isPrefab)
				{
					return false;
				}

				#if UNITY_EDITOR
				if(!Application.isPlaying || IsPrefabAssetOrOpenInPrefabStage())
				{
					return false;
				}
				#endif

				return true;
			}
		}

		#if UNITY_EDITOR
		private bool IsPrefabAssetOrOpenInPrefabStage() => PrefabUtility.IsPartOfPrefabAsset(this) || PrefabStageUtility.GetPrefabStage(gameObject);
		#endif

		private void Deregister()
		{
			if(isPrefab)
			{
				lock(activeClones)
				{
					if(activeClones.TryGetValue(guid, out var list))
					{
						list.Remove(target);
					}
				}

				#if !UNITY_EDITOR
				return;
				#endif
			}

			lock(activeObjects)
			{
				activeObjects.Remove(guid);
			}
		}

		#if UNITY_EDITOR
		private void Reset()
		{
			guid = Id.NewId();

			#if !DEV_MODE || !SHOW_REF_TAGS_IN_INSPECTOR
			hideFlags = HideFlags.HideInInspector;
			#endif
		}
		
		private async void OnValidate()
		{
			await Until.UnitySafeContext();

			if(!this)
			{
				return;
			}

			if(!TargetExistsOnSameGameObject())
			{
				#if DEV_MODE && UNITY_EDITOR
				Debug.LogWarning($"Cross-scene reference target of '{AssetDatabase.GetAssetOrScenePath(this)}/{name}' exist on another GameObject '{AssetDatabase.GetAssetOrScenePath(this)}/{(target ? target.name : "null")}'. Destroying RefTag...", target);
				#endif

				DestroySelf();
				return;
			}

			#if DEV_MODE && SHOW_REF_TAGS_IN_INSPECTOR
			hideFlags = HideFlags.None;
			#else
			hideFlags = HideFlags.HideInInspector;
			#endif

			bool dirty = false;

			if(!Application.isPlaying)
			{
				bool setIsPrefab = IsPrefabAssetOrOpenInPrefabStage();
				if(isPrefab != setIsPrefab)
				{
					Undo.RecordObject(this, "Update Cross-Scene Reference");
					isPrefab = IsPrefabAssetOrOpenInPrefabStage();
					dirty = true;
				}
			}

			var setScene = isPrefab ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(gameObject.scene.path);
			if(setScene != scene)
			{
				Undo.RecordObject(this, "Update Cross-Scene Reference");
				scene = isPrefab ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(gameObject.scene.path);
				dirty = true;
			}

			// Handle events where prefab, scene or GameObject containing this Referenceable has been duplicated.
			var setGlobalObjectIdSlow = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
			if(setGlobalObjectIdSlow == default(GlobalObjectId).ToString())
			{
				setGlobalObjectIdSlow = globalObjectIdSlow;
			}

			if(!setGlobalObjectIdSlow.Equals(globalObjectIdSlow))
			{
				#if DEV_MODE
				if(!target)
				{
					Debug.LogWarning($"GlobalObjectId of {target.GetType().Name} on game object '{name}' in '{AssetDatabase.GetAssetOrScenePath(target)}' has changed from {globalObjectIdSlow} to {setGlobalObjectIdSlow}. GameObject has potentially been duplicated. Updating Id...", this);
				}
				else if(!scene)
				{
					Debug.LogWarning($"GlobalObjectId of target on game object '{name}' in scene '{scene}' has changed from {globalObjectIdSlow} to {setGlobalObjectIdSlow}. GameObject has potentially been duplicated. Updating Id...", this);
				}
				else
				{
					Debug.LogWarning($"GlobalObjectId of '{name}' in scene '{guid}' has changed from {globalObjectIdSlow} to {setGlobalObjectIdSlow}. GameObject has potentially been duplicated. Updating Id...", this);
				}
				#endif

				Undo.RecordObject(this, "Update Cross-Scene Reference");
				globalObjectIdSlow = setGlobalObjectIdSlow;
				dirty = true;
			}
			
			if(!HasValidId() && (!Application.isPlaying || PrefabUtility.IsPartOfPrefabAsset(gameObject)))
			{
				Undo.RecordObject(this, "Update Cross-Scene Reference");
				GenerateNewId();
				dirty = true;
			}

			var setIcon = EditorGUIUtility.ObjectContent(target, target.GetType()).image;
			if(icon != setIcon)
			{
				Undo.RecordObject(this, "Update Cross-Scene Reference");
				icon = setIcon;
				dirty = true;
			}

			if(dirty && isPrefab && PrefabUtility.IsPartOfPrefabAsset(gameObject))
			{
				EditorApplication.delayCall += ()=>
				{
					if(this == null || !isPrefab)
					{
						return;
					}

					PrefabUtility.SavePrefabAsset(gameObject);
				};
			}

			Register();

			void DestroySelf()
			{
				if(Application.isPlaying)
				{
					Destroy(this);
				}
				else
				{
					DestroyImmediate(this);
				}
			}
		}

		private bool HasValidId()
		{
			if(guid == Id.Empty)
			{
				return false;
			}

			var other = GetInstance(guid);
			if(other == target || other == null)
			{
				return true;
			}

			var otherGameObject = GetGameObject(other);
			if(otherGameObject == null)
			{
				return true;
			}

			bool targetIsPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(gameObject);
			bool targetIsOpenInPrefabStage = PrefabStageUtility.GetPrefabStage(gameObject) != null;
			bool targetIsPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject);
			bool targetIsNormalSceneObject = !targetIsPrefabAsset && !targetIsOpenInPrefabStage && !targetIsPrefabInstance;

			bool otherIsPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(otherGameObject);
			bool otherIsOpenInPrefabStage = PrefabStageUtility.GetPrefabStage(otherGameObject) != null;
			bool otherIsPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(otherGameObject);
			bool otherIsNormalSceneObject = !otherIsPrefabAsset && !otherIsOpenInPrefabStage && !otherIsPrefabInstance;

			// If either one is a scene object which is not a prefab instance, we have an id conflict.
			// A scene object could have been duplicated, or a prefab instance unpacked.
			if(targetIsNormalSceneObject || otherIsNormalSceneObject)
			{
				return false;
			}

			// Prefab assets can share their id with a prefab instance or an object being edited in prefab stage.
			if(targetIsPrefabAsset)
			{
				return otherIsPrefabInstance || otherIsOpenInPrefabStage;
			}

			// Objects being edited in prefab stage can share their id with a prefab asset and a prefab instance.
			if(targetIsOpenInPrefabStage)
			{
				return otherIsPrefabAsset ||otherIsPrefabInstance;
			}

			// Prefab instances can share their id with a prefab asset and an object being edited in prefab stage.
			return otherIsPrefabAsset || otherIsOpenInPrefabStage;
		}

		private void GenerateNewId() => guid = Id.NewId();
		private bool TargetExistsOnSameGameObject() => GetGameObject(target) == gameObject;
		private static GameObject GetGameObject(Object target) => target is Component component && component ? component.gameObject : target as GameObject;
		#endif
	}
}