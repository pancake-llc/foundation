#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Extensions methods for <see cref="GameObject"/> that can be used to easily determine if they are prefab assets or not.
	/// <para>
	/// These method are only available in the editor.
	/// </para>
	/// </summary>
	internal static class PrefabExtensions
	{
		public static bool IsAsset([DisallowNull] this GameObject gameObject, bool resultIfSceneObjectInEditMode) => gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage() || (!Application.isPlaying && resultIfSceneObjectInEditMode);
		public static bool IsPartOfPrefabAssetOrOpenInPrefabStage([DisallowNull] this GameObject gameObject) => gameObject.IsPartOfPrefabAsset() || gameObject.IsOpenInPrefabStage();
		public static bool IsPartOfPrefabAsset([DisallowNull] this GameObject gameObject) => !gameObject.scene.IsValid();
		public static bool IsOpenInPrefabStage([DisallowNull] this GameObject gameObject) => StageUtility.GetStage(gameObject) == PrefabStageUtility.GetCurrentPrefabStage();
		public static bool IsInvalidOrPrefabStage(this Scene scene) => !scene.IsValid() || (PrefabStageUtility.GetCurrentPrefabStage() is var prefabStage && prefabStage && prefabStage.scene == scene);
	}
}
#endif