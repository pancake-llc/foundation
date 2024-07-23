using UnityEngine;
using UnityEngine.SceneManagement;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
#endif

namespace Sisus.Init.Internal
{
	public sealed class CrossSceneReference : ScriptableObject<GameObject, Object>, IValueByTypeProvider
		#if UNITY_EDITOR
		, INullGuard
		, ISerializationCallbackReceiver
		#endif
	{
		[SerializeField, HideInInspector]
		private Id guid = default;

		[SerializeField]
		internal bool isCrossScene = false;

		#if DEBUG || INIT_ARGS_SAFE_MODE // To make this persist through builds, could store in a separate ScriptableObject asset instead? Or just take the hit in increased build size.
		#pragma warning disable CS0414
		[SerializeField]
		private Object target = null;
		[SerializeField]
		private string targetName = null;
		[SerializeField]
		private string globalObjectIdSlow = null;
		[SerializeField]
		private string sceneName = null;
		[SerializeField]
		private string sceneOrAssetGuid = null;
		[SerializeField]
		private Texture icon = null;
		#pragma warning restore CS0414
		#endif

		#if UNITY_EDITOR
		#pragma warning disable CS0414
		[SerializeField]
		private SceneAsset sceneAsset = null;
		#pragma warning restore CS0414
		#endif

		public string Guid => guid.ToString();
		public Object Value => GetTarget(Context.MainThread);
		
		#if UNITY_EDITOR
		NullGuardResult INullGuard.EvaluateNullGuard([AllowNull] Component client) => guid == Id.Empty ? NullGuardResult.InvalidValueProviderState : NullGuardResult.Passed;
		#endif

		public Object GetTarget(Context context = Context.MainThread)
		{
			#if !UNITY_EDITOR
			return RefTag.GetInstance(guid);
			#else
			target = RefTag.GetInstance(guid);
			
			// In older versions of Unity an error will occur if GlobalObjectId.GlobalObjectIdentifierToObjectSlow
			// is called and the target object is in an unloaded scene.
			if(!target && context.IsUnitySafeContext() && context.IsEditMode() && IsTargetSceneLoaded()
				&& !string.IsNullOrEmpty(globalObjectIdSlow) && GlobalObjectId.TryParse(globalObjectIdSlow, out GlobalObjectId globalObjectId))
			{
				target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
			}

			return target;

			bool IsTargetSceneLoaded()
			{
				if(!isCrossScene)
				{
					return true;
				}

				string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
				for(int i = 0, count = SceneManager.sceneCount; i < count; i++)
				{
					var scene = SceneManager.GetSceneAt(i);
					if(scene.isLoaded && string.Equals(scene.path, scenePath))
					{
						return true;
					}
				}

				return false;
			}
			#endif
		}

		protected override void Init(GameObject containingObject, Object value)
		{
			guid = Id.Empty;
			isCrossScene = false;

			#if DEBUG || INIT_ARGS_SAFE_MODE
			target = value;
			globalObjectIdSlow = default;
			targetName = "";
			icon = null;
			#endif

			#if UNITY_EDITOR
			sceneAsset = default;
			#endif

			if(value == null)
			{
				return;
			}

			GameObject targetGameObject = GetGameObject(value);
			if(targetGameObject == null)
			{
				return;
			}

			#if UNITY_EDITOR
			globalObjectIdSlow = GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
			icon = EditorGUIUtility.ObjectContent(target, target.GetType()).image;
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			targetName = value.name;
			if(value is not GameObject)
			{
				targetName += " (" + value.GetType().Name + ")";
			}
			#endif

			var targetScene = targetGameObject.scene;

			#if UNITY_EDITOR
			sceneAsset = targetScene.IsValid() ? AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScene.path) : null;
			#endif

			if(!targetGameObject.TryGetComponent(out RefTag refTag))
			{
				refTag = targetGameObject.AddComponent<RefTag, Object>(value);
			}

			guid = refTag.guid;
			isCrossScene = containingObject.scene != targetGameObject.scene;
			
			#if UNITY_EDITOR
			sceneName = targetGameObject.scene.IsValid() ? targetGameObject.scene.name : "";
			sceneOrAssetGuid = targetScene.IsValid() ? AssetDatabase.AssetPathToGUID(targetScene.path) : "";
			#endif

			static GameObject GetGameObject(object target) => target is Component component && component != null ? component.gameObject : target as GameObject;
		}

		public bool TryGetFor<TValue>(Component client, out TValue value)
		{
			#if UNITY_EDITOR
			target = RefTag.GetInstance(guid);
			
			if(target == null)
			{
				#if UNITY_EDITOR
				if(EditorOnly.ThreadSafe.Application.IsPlaying
				|| string.IsNullOrEmpty(globalObjectIdSlow)
				// In older versions of Unity an error will occur if GlobalObjectId.GlobalObjectIdentifierToObjectSlow
				// is called and the target object is in an unloaded scene.
				|| !IsTargetSceneLoaded()
				|| !GlobalObjectId.TryParse(globalObjectIdSlow, out GlobalObjectId globalObjectId))
				{
					value = default;
					return false;
				}

				target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
				#endif

				if(target == null)
				{
					value = default;
					return false;
				}
			}

			return Find.In(target, out value);
			
			bool IsTargetSceneLoaded()
			{
				if(!isCrossScene)
				{
					return true;
				}

				string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
				for(int i = 0, count = SceneManager.sceneCount; i < count; i++)
				{
					var scene = SceneManager.GetSceneAt(i);
					if(scene.isLoaded && string.Equals(scene.path, scenePath))
					{
						return true;
					}
				}

				return false;
			}
			#else
			var target = RefTag.GetInstance(guid);
			if(target == null)
			{
				value = default;
				return false;
			}

			return Find.In(target, out value);
			#endif
		}

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => guid != Id.Empty && Find.typesToFindableTypes.ContainsKey(typeof(TValue)) || typeof(TValue) == typeof(GameObject);

		#if UNITY_EDITOR
		void ISerializationCallbackReceiver.OnAfterDeserialize() { }
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if(isCrossScene) target = null; // Avoid warnings from Unity's serialization system about serialized cross-scene references
		}
		#endif
	}
}