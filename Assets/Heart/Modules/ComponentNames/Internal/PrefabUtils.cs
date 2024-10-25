#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.ComponentNames.Editor
{
	internal delegate void ModifyGameObjectEventHandler(GameObject gameObjectUnpacked);
	internal delegate void ModifyGameObjectsEventHandler(GameObject gameObject1Unpacked, GameObject gameObject2Unpacked);

	internal static class PrefabUtils
	{
		private static readonly Stack<string> childNames = new();
		private static readonly HashSet<string> names = new();
		private static readonly List<Component> components = new();
		private static readonly HashSet<Type> componentsDistinct = new();
		private static readonly Stack<Transform> visitQueue = new();

		public static ModifyPrefabResult TrySetParent([DisallowNull] Transform child, [MaybeNull] Transform parent, bool worldPositionStays)
		{
			if(CanSetParent(child, parent)  is { IsSuccess: false } setParentResult)
			{
				return setParentResult;
			}

			bool childIsPartOfPrefab = PrefabUtility.IsPartOfPrefabAsset(child);
			bool parentIsPartOfPrefab = parent && PrefabUtility.IsPartOfPrefabAsset(parent);

			if(childIsPartOfPrefab)
			{
				#if !UNITY_2022_3_OR_NEWER
				// In older versions of Unity it's not possible to simply remove game objects from prefab instances.
				// Would need to unpack the prefab, perform the modifications on the unpacked game object, and apply
				// the changes on top of the old prefab asset - which can get quite complex and error-prone.
				if(PrefabUtility.IsPartOfPrefabInstance(child.gameObject) && !PrefabUtility.IsAddedGameObjectOverride(child.gameObject) && !PrefabUtility.IsOutermostPrefabInstanceRoot(child.gameObject))
				{
					return new(FailModifyPrefabReason.PartOfPrefabInstance, AssetDatabase.GetAssetOrScenePath(parent), parent.name);
				}
				#endif

				if(parentIsPartOfPrefab)
				{
					return TryModify(child.root.gameObject,
							  parent.root.gameObject,
							  (childUnpacked, parentUnpacked)
								=> childUnpacked.transform.SetParent(parentUnpacked.transform, worldPositionStays));
				}

				return TryModify(child.transform.root.gameObject, childUnpacked => childUnpacked.transform.SetParent(parent, worldPositionStays));
			}

			if(parentIsPartOfPrefab)
			{
				return TryModify(parent ? parent.gameObject : child.gameObject, parentUnpacked => child.SetParent(parentUnpacked ? parentUnpacked.transform : null, worldPositionStays));
			}

			child.SetParent(parent, worldPositionStays);
			return ModifyPrefabResult.Success;

		}

		private static void GetNameStack(Transform root, Transform leaf, Stack<string> childNames)
		{
			for(var t = leaf; t != root; t = t.parent)
			{
				childNames.Push(t.name);
			}
		}

		private static ModifyPrefabResult TryModify([DisallowNull] GameObject gameObjectToModify, ModifyGameObjectEventHandler applyModifications)
		{
			if(PrefabUtility.IsPartOfPrefabAsset(gameObjectToModify))
			{
				var root = gameObjectToModify.transform.root;
				string prefabPath = AssetDatabase.GetAssetPath(root);
				PrefabUtility.SavePrefabAsset(gameObjectToModify);
				var prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

				#if DEV_MODE
				Debug.Assert(string.Equals(prefabContents.name, root.name), prefabContents.name + " != " + root.name);
				#endif

				GetNameStack(root, gameObjectToModify.transform, childNames);
				var equivalentTransformInPrefab = prefabContents.transform;
				while(childNames.Count > 0)
				{
					var childName = childNames.Pop();
					equivalentTransformInPrefab = equivalentTransformInPrefab.Find(childName);
				}

				applyModifications(equivalentTransformInPrefab.gameObject);

				PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath, out bool success);

				PrefabUtility.UnloadPrefabContents(prefabContents);

				if(!success)
				{
					#if DEV_MODE
					Debug.LogError("Failed to save changes to prefab @ "+prefabPath);
					#endif

					return new(FailModifyPrefabReason.SaveAsPrefabAssetFailed, prefabPath, null);
				}

				return ModifyPrefabResult.Success;
			}

			applyModifications(gameObjectToModify);
			return ModifyPrefabResult.Success;
		}

		private static ModifyPrefabResult TryModify([MaybeNull] GameObject prefab1, GameObject prefab2, ModifyGameObjectsEventHandler applyModifications)
		{
			PrefabUtility.SavePrefabAsset(prefab1);
			PrefabUtility.SavePrefabAsset(prefab2);

			var root1 = prefab1.transform.root;
			string prefab1Path = AssetDatabase.GetAssetPath(root1);
			var prefab1Contents = PrefabUtility.LoadPrefabContents(prefab1Path);

			#if DEV_MODE
			Debug.Assert(string.Equals(prefab1Contents.name, root1.name), prefab1Contents.name + " != " + root1.name);
			#endif

			GetNameStack(root1, prefab1.transform, childNames);
			var equivalentTransformInPrefab1 = prefab1Contents.transform;
			while(childNames.Count > 0)
			{
				var childName = childNames.Pop();
				equivalentTransformInPrefab1 = equivalentTransformInPrefab1.Find(childName);
			}

			var root2 = prefab2.transform.root;

			string prefab2Path = AssetDatabase.GetAssetPath(root2);
			var prefab2Contents = PrefabUtility.LoadPrefabContents(prefab2Path);

			#if DEV_MODE
			Debug.Assert(string.Equals(prefab2Contents.name, root2.name), prefab2Contents.name + " != " + root2.name);
			#endif

			GetNameStack(root2, prefab2.transform, childNames);
			var equivalentTransformInPrefab2 = prefab2Contents.transform;
			while(childNames.Count > 0)
			{
				var childName = childNames.Pop();
				equivalentTransformInPrefab2 = equivalentTransformInPrefab2.Find(childName);
			}

			applyModifications(equivalentTransformInPrefab1.gameObject,equivalentTransformInPrefab2.gameObject);

			PrefabUtility.SaveAsPrefabAsset(prefab1Contents, prefab1Path, out bool success1);
			PrefabUtility.SaveAsPrefabAsset(prefab2Contents, prefab2Path, out bool success2);

			#if DEV_MODE
			if(!success1) Debug.LogError("Failed to save changes to prefab @ " + prefab1Path);
			if(!success2) Debug.LogError("Failed to save changes to prefab @ " + prefab2Path);
			#endif

			PrefabUtility.UnloadPrefabContents(prefab1Contents);
			PrefabUtility.UnloadPrefabContents(prefab2Contents);

			if(!success1)
			{
				return new(FailModifyPrefabReason.SaveAsPrefabAssetFailed, prefab1Path, null);
			}

			if(!success2)
			{
				return new(FailModifyPrefabReason.SaveAsPrefabAssetFailed, prefab2Path, null);
			}

			return ModifyPrefabResult.Success;
		}

		private static ModifyPrefabResult CanSetParent([NotNull] Transform child, Transform parent)
		{
			#if !UNITY_2022_3_OR_NEWER
			// In older versions of Unity it's not possible to reparent/remove game objects inside prefab instances.
			if(UnityEditor.PrefabUtility.IsPartOfPrefabInstance(child.gameObject) && !UnityEditor.PrefabUtility.IsAddedGameObjectOverride(child.gameObject) && !UnityEditor.PrefabUtility.IsOutermostPrefabInstanceRoot(child.gameObject))
			{
				return new(FailModifyPrefabReason.PartOfPrefabInstance, AssetDatabase.GetAssetOrScenePath(parent), parent.name);
			}
			#endif

			if(PrefabUtility.IsPartOfPrefabAsset(child) && CanSafelyModifyPrefabContents(child, child) is { IsSuccess: false } canModifyChildResult)
			{
				return canModifyChildResult;
			}

			if(PrefabUtility.IsPartOfPrefabAsset(parent) && CanSafelyModifyPrefabContents(parent, child) is { IsSuccess: false } canModifyParentResult)
			{
				return canModifyParentResult;
			}

			return ModifyPrefabResult.Success;
		}

		internal static bool CanAddChild(Transform parent, Object context)
			=> !PrefabUtility.IsPartOfPrefabAsset(parent) || CanSafelyModifyPrefabContents(parent, context) is not { IsSuccess: false };

		private static ModifyPrefabResult CanSafelyModifyPrefabContents(Transform prefabRoot, Object context)
		{
			names.Clear();
			visitQueue.Clear();

			var visit = prefabRoot;
			do
			{
				if(!names.Add(visit.name))
				{
					Debug.LogWarning($"Can't make changes to prefab asset {AssetDatabase.GetAssetPath(prefabRoot)}, because it contains multiple game objects with the same name \"{visit.name}\".", context);
					return new(FailModifyPrefabReason.MultipleGameObjectsWithSameName, AssetDatabase.GetAssetPath(prefabRoot), visit.name);
				}

				visit.GetComponents(components);
				componentsDistinct.Clear();
				foreach(var component in components)
				{
					if(!component)
					{
						Debug.LogWarning($"Can't make changes to prefab asset {AssetDatabase.GetAssetPath(prefabRoot)}, because the game object \"{visit.name}\" contains a missing component.", context);
						return new(FailModifyPrefabReason.MissingComponent, AssetDatabase.GetAssetPath(prefabRoot), visit.name);
					}

					if(!componentsDistinct.Add(component.GetType()))
					{
						Debug.LogWarning($"Can't make changes to prefab asset {AssetDatabase.GetAssetPath(prefabRoot)}, because the game object \"{visit.name}\" contains more than one of the same component type {component.GetType().Name}.", context);
						return new(FailModifyPrefabReason.MultipleComponentsOfSameType, AssetDatabase.GetAssetPath(prefabRoot), visit.name);
					}
				}

				for(int i = visit.childCount - 1; i >= 0; i--)
				{
					visitQueue.Push(visit.GetChild(i));
				}
			}
			while(visitQueue.TryPop(out visit));

			return ModifyPrefabResult.Success;
		}

		private enum ModificationType
		{
			AddContent,
			RemoveGameObject
		}
	}
}
#endif