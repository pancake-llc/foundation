using System;
using System.ComponentModel;
using System.Globalization;
using UnityEngine;
using Object = UnityEngine.Object;
using Component = UnityEngine.Component;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pancake.Init.Internal
{
	[Serializable, TypeConverter(typeof(Converter))]
	public class CrossSceneReference : ScriptableObject<GameObject, Object>, IValueProvider<Object>
		#if UNITY_EDITOR
		, ISerializationCallbackReceiver
		#endif
	{
		[SerializeField, HideInInspector]
		private Id guid;

		#pragma warning disable CS0414

		[SerializeField]
		internal bool isCrossScene;

		#if DEBUG // To make this persist through builds, could store in a separate ScriptableObject asset instead? Or just take the hit in increased build size.
		[SerializeField]
		private Object target;
		[SerializeField]
		private string targetName;
		[SerializeField]
		private string globalObjectIdSlow;
		[SerializeField]
		private string sceneName;
		[SerializeField]
		private string sceneOrAssetGuid;
		[SerializeField]
		private Texture icon = null;
		#endif

		#if UNITY_EDITOR
		[SerializeField]
		private SceneAsset sceneAsset;
		#endif

		#pragma warning restore CS0414

		public string Guid => guid.ToString();
		
		#if UNITY_EDITOR
		public bool HasValue => guid != Id.Empty;
		#endif

		public Object Value => GetTarget(Context.MainThread);
		object IValueProvider.Value => GetTarget(Context.MainThread);

		public Object GetTarget(Context context = Context.MainThread)
		{
			#if !UNITY_EDITOR
			return RefTag.GetInstance(guid);
			#else
			target = RefTag.GetInstance(guid);
			
			// In older versions of Unity an error will occur if GlobalObjectId.GlobalObjectIdentifierToObjectSlow
			// is called and the target object is in an unloaded scene.
			if(target == null && context != Context.Threaded && !Application.isPlaying && IsTargetSceneLoaded()
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

			#if DEBUG
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

			#if DEBUG
			globalObjectIdSlow = GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
			targetName = value.name;
			icon = EditorGUIUtility.ObjectContent(target, target.GetType()).image;			

			if(!(value is GameObject))
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

		#if UNITY_EDITOR
		void ISerializationCallbackReceiver.OnAfterDeserialize() { }
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if(isCrossScene) target = null; // Avoid warnings from Unity's serialization system about serialized cross-scene references
		}
		#endif

		private sealed class Converter : TypeConverter
        {
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type destinationType) => false;
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => typeof(Component).IsAssignableFrom(destinationType) || destinationType == typeof(GameObject) || destinationType.IsInterface;

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if(value is CrossSceneReference reference)
                {
                    var result = reference.Value;
					return result == null || destinationType.IsAssignableFrom(result.GetType()) ? result : null;
				}

				return base.ConvertTo(context, culture, value, destinationType);
            }
        }
	}
}