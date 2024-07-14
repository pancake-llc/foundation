#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Extensions methods for <see cref="GameObject"/> that can be used to easily determine if they are prefab assets or not.
	/// </summary>
	internal static class PrefabExtensions
	{
		public static bool IsPartOfPrefabAssetOrOpenInPrefabStage([DisallowNull] this GameObject gameObject) => gameObject.IsPartOfPrefabAsset() || gameObject.IsOpenInPrefabStage();

		public static bool IsPartOfPrefabAsset([DisallowNull] this GameObject gameObject) => !gameObject.scene.IsValid();

		public static bool IsOpenInPrefabStage([DisallowNull] this GameObject gameObject)
		    #if UNITY_2020_1_OR_NEWER
		    => StageUtility.GetStage(gameObject) != null;
		    #else
		    => StageUtility.GetStageHandle(gameObject).IsValid();
		    #endif

		public static bool IsInvalidOrPrefabStage([DisallowNull] this Scene scene) => !scene.IsValid() || (PrefabStageUtility.GetCurrentPrefabStage() is var prefabStage && prefabStage != null && prefabStage.scene == scene);
    }
}
#endif