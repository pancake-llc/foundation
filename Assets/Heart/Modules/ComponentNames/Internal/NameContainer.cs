#define SHOW_NAME_CONTAINERS

using System;
using System.Collections.Concurrent;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
[assembly: InternalsVisibleTo("ComponentNames.Editor")]
#endif

namespace Sisus.ComponentNames.Editor
{
	/// <summary>
	/// Component that acts as a container for the custom name given to a component.
	/// </summary>
	[AddComponentMenu(DontShowInMenu)]
	internal class NameContainer : MonoBehaviour
	{
		#pragma warning disable CS0414
		private const string DontShowInMenu = "";

		internal static bool NowRenaming;
		internal static Component StartingToRename;
		private static readonly ConcurrentDictionary<Component, NameContainer> instances = new();

		[SerializeField]
		private string nameOverride = "";

		[SerializeField]
		private string tooltipOverride = "";

		[SerializeField]
		private Component target = null;

		#pragma warning restore CS0414

		#if UNITY_EDITOR

		internal const string ContainerName = "NameContainer";
		internal const string ContainerTag = "EditorOnly";

		internal string NameOverride
		{
			get => nameOverride;

			set
			{
				if(value == nameOverride)
				{
					return;
				}

				nameOverride = value;
				EditorUtility.SetDirty(this);
			}
		}

		internal string TooltipOverride
		{
			get => tooltipOverride;

			set
			{
				if(value == tooltipOverride)
				{
					return;
				}

				tooltipOverride = value;
				EditorUtility.SetDirty(this);
			}
		}

		private void Awake() => OnValidate();

		private void OnValidate()
		{
			if(NowRenaming)
			{
				return;
			}

			UpdateHideFlags();

			if(!target)
			{
				#if DEV_MODE
				Debug.Log($"Destroying {name} under parent \"{(transform.parent ? transform.parent.name : "null")}\" @ \"{AssetDatabase.GetAssetOrScenePath(gameObject)}\" because target is null...");
				#endif

				EditorApplication.delayCall += () =>
				{
					if(this && !target)
					{
						Remove(ModifyOptions.Immediate | ModifyOptions.NonUndoable);
					}
				};
				return;
			}

			if(transform.parent != target.transform)
			{
				if(transform == target.transform)
				{
					#if DEV_MODE
					Debug.Log($"Destroying {name} because transform == target.transform...");
					#endif

					target = null;
					Remove(ModifyOptions.NonUndoable);
					return;
				}

				#if DEV_MODE
				Debug.Log($"Destroying {name} because transform.parent != target.transform...");
				#endif

				EditorApplication.delayCall += () =>
				{
					if (!this)
					{
						return;
					}

					if(!target)
					{
						Remove(ModifyOptions.Immediate | ModifyOptions.NonUndoable);
						return;
					}

					if(NameContainer.TryGet(target, out var existingNameContainer) && existingNameContainer != this)
					{
						#if DEV_MODE
						Debug.LogWarning($"Destroying NameContainer {name} with name \"{nameOverride}\" targeting {target.GetType().Name} on game object \"{target.name}\", because target already has another NameContainer {existingNameContainer.name} targeting it.");
						#endif

						Remove(ModifyOptions.Immediate | ModifyOptions.NonUndoable);
						return;
					}

					if(transform.parent == target.transform)
					{
						return;
					}

					// Avoid error 'Setting the parent of a transform which resides in a Prefab Asset is disabled to prevent data corruption'.
					bool thisIsPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(this);
					bool targetIsPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(target);
					if(thisIsPrefabAsset || targetIsPrefabAsset)
					{
						if(PrefabUtils.TrySetParent(transform, target.transform, false) is { IsSuccess: false } setParentResult)
						{
							HandleLogWarning(transform, setParentResult);

							if(!PrefabUtility.IsPartOfPrefabAsset(this))
							{
								#if DEV_MODE
								Debug.Log($"DestroyImmediate({name}) @ {AssetDatabase.GetAssetOrScenePath(this)}");
								#endif

								DestroyImmediate(gameObject);
							}
						}

						return;
					}

					#if !UNITY_2022_3_OR_NEWER
					// In older versions of Unity it's not possible to reparent/remove game objects inside prefab instances.
					// Would need to unpack the prefab, perform the modifications on the unpacked game object, and apply
					// the changes on top of the old prefab asset - which can get quite complex and error-prone.
					if(PrefabUtility.IsPartOfPrefabInstance(gameObject) && !PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject))
					{
						#if DEV_MODE
						Debug.Log($"Won't reparent NameContainer(\"{nameOverride}\") because it's part of a prefab instance.", transform.parent);
						#endif
						return;
					}
					#endif

					transform.SetParent(target.transform, false);
				};
			}

			if(instances.TryGetValue(target, out var existingContainer) && existingContainer != this && existingContainer && existingContainer.target == target)
			{
				#if DEV_MODE
				Debug.Log($"Destroying {name} because existing duplicate instance found instances.TryGetValue...");
				#endif

				// Copy over name and tooltip from this container to the other one and destroy this one.
				// It is likely that this container contains the name/tooltip for a prefab instance and the other
				// one contains it for the prefab asset.
				// In this situation we want to convert the name and tooltips into instance value overrides
				// instead of having two different name containers for one target.
				// In any case we never want to have two different name containers when it can be avoided.
				existingContainer.NameOverride = NameOverride;
				existingContainer.TooltipOverride = TooltipOverride;
				Remove(ModifyOptions.NonUndoable);
				return;
			}

			instances[target] = this;

			if(ComponentName.IsNullEmptyOrDefault(target, nameOverride))
			{
				if(TooltipOverride.Length == 0)
				{
					#if DEV_MODE
					Debug.Log($"Destroying {name} because name and tooltip are empty");
					#endif

					Remove(ModifyOptions.Defaults);
					return;
				}

				ComponentName.ResetToDefault(target, ModifyOptions.DontUpdateNameContainer);
			}
			else
			{
				EditorApplication.delayCall += ()=>
				{
					if(!this)
					{
						return;
					}

					if(!target)
					{
						#if DEV_MODE
						Debug.Log($"Destroying {name} because target was null after delay.");
						#endif

						Remove(ModifyOptions.Immediate | ModifyOptions.NonUndoable);
						return;
					}

					if(ComponentName.IsNullEmptyOrDefault(target, nameOverride))
					{
						if(TooltipOverride.Length == 0)
						{
							Remove(ModifyOptions.Immediate);
							return;
						}

						ComponentName.ResetToDefault(target, ModifyOptions.Immediate | ModifyOptions.DontUpdateNameContainer);
						return;
					}

					ComponentTooltip.Set(target, tooltipOverride, ModifyOptions.Immediate | ModifyOptions.DontUpdateNameContainer);
					ComponentName.Set(target, nameOverride, ModifyOptions.Immediate | ModifyOptions.DontUpdateNameContainer);
				};
			}

			if(string.Equals(name, "NameContainer(EditorOnly)"))
			{
				name = GetNameContainerGameObjectName(target);
			}
		}

		internal static void StartRenaming(Component component)
		{
			#if DEV_MODE
			Debug.Assert(component);
			Debug.Assert(component is not NameContainer);
			#endif

			NowRenaming = true;
			StartingToRename = component;

			EditorGUIUtility.editingTextField = true;
		}

		internal static void TryGetOrCreate([AllowNull] Component component, ModifyOptions modifyOptions, Action<NameContainer> onAcquired, string initialName, string initialTooltip)
		{
			if(modifyOptions.IsDelayed())
			{
				EditorApplication.delayCall += TryGetOrCreateNow;
			}
			else
			{
				TryGetOrCreateNow();
			}

			void TryGetOrCreateNow()
			{
				if(!component)
				{
					return;
				}

				if(TryGetOrCreateImmediate(component, out var nameContainer, initialName, initialTooltip))
				{
					onAcquired?.Invoke(nameContainer);
				}
			}
		}

		private static bool TryGetOrCreateImmediate([AllowNull] Component target, [MaybeNullWhen(false), NotNullWhen(true)] out NameContainer nameContainer, string initialName = null, string initialTooltip = null)
		{
			if(!target)
			{
				nameContainer = null;
				return false;
			}

			var gameObjectWithComponent = target.gameObject;
			if(!gameObjectWithComponent)
			{
				nameContainer = null;
				return false;
			}

			if(TryGet(target, out nameContainer))
			{
				return nameContainer;
			}

			if(!PrefabUtils.CanAddChild(target.transform, target))
			{
				return false;
			}

			var name = GetNameContainerGameObjectName(target);
			var containerGameObject = new GameObject(name);
			bool wasRenaming = NowRenaming;
			NowRenaming = true;
			var prefabPath = AssetDatabase.GetAssetPath(gameObjectWithComponent);

			try
			{
				if(!Application.isPlaying || PrefabUtility.IsPartOfPrefabAsset(target) || UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(target.gameObject))
				{
					Undo.RegisterCreatedObjectUndo(containerGameObject, "Set Component Name");
				}

				nameContainer = containerGameObject.AddComponent<NameContainer>();
				nameContainer.UpdateHideFlags();
				containerGameObject.tag = ContainerTag;
				nameContainer.target = target;

				if(initialName != null)
				{
					nameContainer.NameOverride = initialName;
				}

				if(initialTooltip != null)
				{
					nameContainer.TooltipOverride = initialTooltip;
				}

				if(PrefabUtils.TrySetParent(containerGameObject.transform, gameObjectWithComponent.transform, false) is { IsSuccess: false } setParentResult)
				{
					HandleLogWarning(target, setParentResult);
					if(nameContainer)
					{
						nameContainer.RemoveImmediate(true);
					}

					return false;
				}
			}
			catch(Exception e)
			{
				Debug.LogWarning(e);
				if(nameContainer)
				{
					nameContainer.RemoveImmediate(true);
				}

				return false;
			}
			finally
			{
				if(!wasRenaming)
				{
					NowRenaming = false;
				}
			}

			if(nameContainer)
			{
				instances[target] = nameContainer;
				EditorUtility.SetDirty(nameContainer);
			}
			else if(prefabPath is  { Length: > 0 })
			{
				var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				if(prefabAsset)
				{
					var containerTransform = prefabAsset.transform.Find(name);
					if(containerTransform && containerTransform.TryGetComponent(out nameContainer))
					{
						instances[target] = nameContainer;
						EditorUtility.SetDirty(nameContainer);
					}
				}
			}

			return true;
		}

		private static string GetNameContainerGameObjectName(Component target) => ContainerName + " " + target.GetType().Name + " " + Guid.NewGuid();

		private static void HandleLogWarning(Component component, ModifyPrefabResult result) =>
			Debug.LogWarning($"Unable to set name of component {component.GetType().Name}, because " + result.FailReason switch
			{
				#if !UNITY_2022_3_OR_NEWER
				FailModifyPrefabReason.PartOfPrefabInstance => $"its target \"{result.Context}\" in scene {result.PrefabPath} is part of a prefab instance. Change the name of the component once in Prefab Mode to resolve the issue.",
				#endif
				FailModifyPrefabReason.MissingComponent => $"the prefab {result.PrefabPath} contains a missing component. Fix or remove the missing component to resolve the issue.",
				FailModifyPrefabReason.MultipleComponentsOfSameType => $"the prefab {result.PrefabPath} has a game object with more than one component of the same type {result.Context}. Change the name of the component once in Prefab Mode to resolve the issue.",
				FailModifyPrefabReason.MultipleGameObjectsWithSameName => $"the prefab {result.PrefabPath} contains multiple game objects with the same name \"{result.Context}\". Change the name of the component once in Prefab Mode to resolve the issue.",
				FailModifyPrefabReason.SaveAsPrefabAssetFailed => $"unable to save changes onto the prefab {result.PrefabPath}. Change the name of the component once in Prefab Mode to resolve the issue.",
				_ => "something went wrong."
			}, component);

		internal static bool TryGet(Component target, out NameContainer nameContainer)
		{
			if(instances.TryGetValue(target, out nameContainer))
			{
				if(!nameContainer || nameContainer.target != target)
				{
					nameContainer = null;
				}
				// prioritize containers that are direct children of the target
				else if(nameContainer.transform.parent == target.transform)
				{
					return nameContainer;
				}
			}

			var parent = target.transform;
			for(int i = 0, childCount = parent.childCount; i < childCount; i++)
			{
				if(parent.GetChild(i).TryGetComponent(out NameContainer someNameContainer) && someNameContainer.target == target)
				{
					nameContainer = someNameContainer;
					return true;
				}
			}

			return nameContainer;
		}

		internal void Remove(ModifyOptions modifyOptions)
		{
			if(!modifyOptions.IsRemovingNameContainerAllowed())
			{
				nameOverride = "";
				tooltipOverride = "";
				return;
			}

			bool isUndoable = modifyOptions.IsUndoable();
			if(modifyOptions.IsDelayed())
			{
				EditorApplication.delayCall += ()=> RemoveImmediate(isUndoable);
			}
			else
			{
				RemoveImmediate(isUndoable);
			}
		}

		private void RemoveImmediate(bool undoable)
		{
			if(!this)
			{
				return;
			}

			#if !UNITY_2022_3_OR_NEWER
			// In older versions of Unity it's not possible to simply remove game objects from prefab instances.
			// Would need to unpack the prefab, perform the modifications on the unpacked game object, and apply
			// the changes on top of the old prefab asset - which can get quite complex and error-prone.
			if(PrefabUtility.IsPartOfPrefabInstance(gameObject) && !PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject))
			{
				#if DEV_MODE && DEBUG_DESTROY
				Debug.Log($"Won't destroy NameContainer(\"{nameOverride}\") because it's part of a prefab instance.", transform.parent);
				#endif

				nameOverride = "";
				tooltipOverride = "";
				return;
			}
			#endif

			#if DEV_MODE && DEBUG_DESTROY
			Debug.Log($"Destroying NameContainer(\"{nameOverride}\")");
			#endif

			if(gameObject.GetComponents<Component>().Length > 2)
			{
				#if DEV_MODE
				Debug.LogWarning($"Destroying only NameContainer component, instead of the whole game object, because the game object it is attached to contains extra components: {string.Join(", ", GetComponents<Component>().Select(c => c?.GetType().Name))}.", transform);
				#endif

				ObjectUtility.Destroy(this, undoable);
				return;
			}

			if(transform.childCount > 0)
			{
				#if DEV_MODE
				Debug.LogWarning($"Destroying only NameContainer component, instead of the whole game object, because the game object contains child game objects.\nFirst Child:{transform.GetChild(0)}.", transform);
				#endif

				ObjectUtility.Destroy(this, undoable);
				return;
			}

			#if DEV_MODE && DEBUG_DESTROY
			Debug.Log($"Destroying NameContainer(\"{nameOverride}\").", transform.parent);
			#endif

			ObjectUtility.Destroy(gameObject, undoable);
		}

		private void UpdateHideFlags()
		{
			if(IsAttachedToNameContainerGameObject())
			{
				#if DEV_MODE && SHOW_NAME_CONTAINERS
				gameObject.hideFlags = HideFlags.None;
				hideFlags = HideFlags.None;
				#else
				gameObject.hideFlags = HideFlags.HideInHierarchy;
				#endif
				return;
			}

			hideFlags = HideFlags.None;
		}

		private bool IsAttachedToNameContainerGameObject() => name.StartsWith(ContainerName) && gameObject.GetComponents<Component>().Length == 2 && transform.childCount == 0;
		#endif
	}
}