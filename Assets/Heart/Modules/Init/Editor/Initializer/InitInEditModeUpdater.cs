//#define DEBUG_SETUP_DURATION

#if !INIT_ARGS_DISABLE_INIT_IN_EDIT_MODE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sisus.Init.Internal;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
    [InitializeOnLoad]
	internal static class InitInEditModeUpdater
	{
		private static HashSet<Type> typesWithAttribute;
		private static HashSet<Scene> scenesToProcess = new();
		private static HashSet<GameObject> prefabsToProcess = new();

		static InitInEditModeUpdater() => SetupAsync();

		private static async void SetupAsync()
		{
			await Until.UnitySafeContext();

			#if DEV_MODE
			UnityEngine.Profiling.Profiler.BeginSample(nameof(InitInEditModeUpdater));
			#if DEBUG_SETUP_DURATION
			var timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			#endif
			#endif

			typesWithAttribute = new HashSet<Type>(TypeCache.GetTypesWithAttribute<InitInEditModeAttribute>());

			if(typesWithAttribute.Count > 0)
			{
				foreach(var initializerType in TypeCache.GetTypesDerivedFrom<IInitializer>())
				{
					var clientType = InitializerEditorUtility.GetClientType(initializerType);
					if(typesWithAttribute.Contains(clientType))
					{
						typesWithAttribute.Add(initializerType);
					}
				}
			}

			ResubscribeToEvents();

			#if DEV_MODE
			UnityEngine.Profiling.Profiler.EndSample();
			#if DEBUG_SETUP_DURATION
			timer.Stop();
			Debug.Log(nameof(InitInEditModeUpdater) + " took " + timer.Elapsed.TotalSeconds + "s.");
			#endif
			#endif
		}

		private static void ResubscribeToEvents()
		{
			UnsubscribeFromEvents();

			if(typesWithAttribute.Count > 0)
			{
				SubscribeToEvents();
			}
		}

		private static void UnsubscribeFromEvents()
		{
			ObjectChangeEvents.changesPublished -= OnObjectChanged;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			InitInEditModeUtility.UpdateAllRequested -= UpdateAllObjectsInAllOpenScenes;
		}

		private static void SubscribeToEvents()
		{
			ObjectChangeEvents.changesPublished += OnObjectChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			InitInEditModeUtility.UpdateAllRequested += UpdateAllObjectsInAllOpenScenes;
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange playModeState) => ResubscribeToEvents();

		private static void OnObjectChanged(ref ObjectChangeEventStream stream)
		{
			if(EditorApplication.isCompiling
			|| EditorApplication.isUpdating
			|| BuildPipeline.isBuildingPlayer)
			{
				return;
			}

			for(int eventIndex = stream.length - 1; eventIndex >= 0; eventIndex--)
			{
                switch(stream.GetEventType(eventIndex))
				{
					case ObjectChangeKind.ChangeScene:
						stream.GetChangeSceneEvent(eventIndex, out var changeSceneEventArgs);
						HandleSceneChanged(changeSceneEventArgs.scene);
						break;
					case ObjectChangeKind.CreateGameObjectHierarchy:
						stream.GetCreateGameObjectHierarchyEvent(eventIndex, out var createGameObjectHierarchyEventArgs);
						HandleObjectChanged(createGameObjectHierarchyEventArgs.instanceId, createGameObjectHierarchyEventArgs.scene);
						break;
					case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
						stream.GetChangeGameObjectStructureHierarchyEvent(eventIndex, out var changeGameObjectStructureHierarchyEventArgs);
						HandleObjectChanged(changeGameObjectStructureHierarchyEventArgs.instanceId, changeGameObjectStructureHierarchyEventArgs.scene);
						break;
					case ObjectChangeKind.ChangeGameObjectStructure:
						stream.GetChangeGameObjectStructureEvent(eventIndex, out var changeGameObjectStructureEventArgs);
						HandleObjectChanged(changeGameObjectStructureEventArgs.instanceId, changeGameObjectStructureEventArgs.scene);
						break;
					case ObjectChangeKind.ChangeGameObjectParent:
						stream.GetChangeGameObjectParentEvent(eventIndex, out var changeGameObjectParentEventArgs);
						HandleObjectChanged(changeGameObjectParentEventArgs.instanceId, changeGameObjectParentEventArgs.newScene);
						break;
					case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
						stream.GetChangeGameObjectOrComponentPropertiesEvent(eventIndex, out var changeGameObjectOrComponentPropertiesEventArgs);
						HandleObjectChanged(changeGameObjectOrComponentPropertiesEventArgs.instanceId, changeGameObjectOrComponentPropertiesEventArgs.scene);
						break;
					case ObjectChangeKind.DestroyGameObjectHierarchy:
						stream.GetDestroyGameObjectHierarchyEvent(eventIndex, out var destroyGameObjectHierarchyEventArgs);
						HandleSceneChanged(destroyGameObjectHierarchyEventArgs.scene);
						break;
					case ObjectChangeKind.CreateAssetObject:
						stream.GetCreateAssetObjectEvent(eventIndex, out var createAssetObjectEventArgs);
						HandleObjectChanged(createAssetObjectEventArgs.instanceId, createAssetObjectEventArgs.scene);
						break;
					case ObjectChangeKind.DestroyAssetObject:
						stream.GetDestroyAssetObjectEvent(eventIndex, out var destroyAssetObjectEventArgs);
						HandleObjectChanged(destroyAssetObjectEventArgs.instanceId, destroyAssetObjectEventArgs.scene);
						break;
					case ObjectChangeKind.ChangeAssetObjectProperties:
						stream.GetChangeAssetObjectPropertiesEvent(eventIndex, out var changeAssetObjectPropertiesEventArgs);
						HandleObjectChanged(changeAssetObjectPropertiesEventArgs.instanceId);
						break;
					case ObjectChangeKind.UpdatePrefabInstances:
						stream.GetUpdatePrefabInstancesEvent(eventIndex, out var updatePrefabInstancesEventArgs);
						HandleObjectsChanged(updatePrefabInstancesEventArgs.instanceIds, updatePrefabInstancesEventArgs.scene);
						break;
				}
			}
		}

		private static void HandleObjectChanged(int instanceId)
		{
			if(Thread.CurrentThread.IsBackground)
			{
				EditorApplication.delayCall += ()=> HandleObjectChanged(instanceId);
				return;
			}

			if(EditorUtility.InstanceIDToObject(instanceId) is Object obj && obj)
			{
				HandleObjectChanged(obj);
			}
		}

		private static void HandleObjectChanged(int instanceId, Scene scene)
		{
			if(Thread.CurrentThread.IsBackground)
			{
				EditorApplication.delayCall += ()=> HandleObjectChanged(instanceId, scene);
				return;
			}

			if(scene.IsValid())
			{
				HandleSceneChanged(scene);
				return;
			}

			if(EditorUtility.InstanceIDToObject(instanceId) is Object obj && obj)
			{
				HandleObjectChanged(obj);
			}
		}

		private static void HandleObjectChanged(Object changedObject)
		{
			if(Thread.CurrentThread.IsBackground)
			{
				EditorApplication.delayCall += ()=> HandleObjectChanged(changedObject);
				return;
			}

			if(changedObject is not Component component)
			{
				// When a value provider changes, update all objects in open scenes, because any of them could be using the value provider.
				// E.g. if editing a LocalizedString, we might want to update a TextInitializer.
				if(changedObject is IValueProvider or IValueByTypeProvider or IValueByTypeProviderAsync or IValueProviderAsync)
				{
					UpdateAllObjectsInAllOpenScenes();
				}

				return;
			}

			var scene = component.gameObject.scene;
			if(scene.IsValid())
			{
				SceneInitializer.ProcessChangedScene(scene);
			}
			else
			{
				PrefabInitializer.ProcessChangedPrefab(component.transform.root.gameObject);
			}
		}

		private static void HandleObjectsChanged(NativeArray<int>.ReadOnly instanceIds)
		{
			if(Thread.CurrentThread.IsBackground)
			{
				EditorApplication.delayCall += ()=> HandleObjectsChanged(instanceIds);
				return;
			}

			if(instanceIds.Length == 1)
			{
				HandleObjectChanged(instanceIds[0]);
				return;
			}

			foreach(int instanceId in instanceIds)
			{
				HandleObjectsChanged(instanceIds.Select(id => EditorUtility.InstanceIDToObject(id)).Where(o => o).ToArray());
			}
		}

		private static void HandleObjectsChanged(NativeArray<int>.ReadOnly instanceIds, Scene scene)
		{
			if(Thread.CurrentThread.IsBackground)
			{
				EditorApplication.delayCall += ()=> HandleObjectsChanged(instanceIds, scene);
				return;
			}

			if(scene.IsValid())
			{
				HandleSceneChanged(scene);
				return;
			}

			if(instanceIds.Length == 1)
			{
				HandleObjectChanged(instanceIds[0]);
				return;
			}

			foreach(int instanceId in instanceIds)
			{
				HandleObjectsChanged(instanceIds.Select(id => EditorUtility.InstanceIDToObject(id)).Where(o => o).ToArray());
			}
		}

		private static void HandleObjectsChanged(Object[] changedObjects)
		{
			if(Thread.CurrentThread.IsBackground)
			{
				EditorApplication.delayCall += ()=> HandleObjectsChanged(changedObjects);
				return;
			}

			foreach(var changedObject in changedObjects)
			{
				if(changedObject is not Component component)
				{
					continue;
				}

				var scene = component.gameObject.scene;
				if(scene.IsValid())
				{
					scenesToProcess.Add(scene);
				}
				else
				{
					prefabsToProcess.Add(component.transform.root.gameObject);
				}
			}

			foreach(var scene in scenesToProcess)
			{
				SceneInitializer.ProcessChangedScene(scene);
			}

			scenesToProcess.Clear();

			foreach(var prefab in prefabsToProcess)
			{
				PrefabInitializer.ProcessChangedPrefab(prefab);
			}

			prefabsToProcess.Clear();
		}

		private static void HandleSceneChanged(Scene scene)
		{
			if(Thread.CurrentThread.IsBackground)
			{
				EditorApplication.delayCall += ()=> HandleSceneChanged(scene);
				return;
			}

			SceneInitializer.ProcessChangedScene(scene);
		}

		private static void UpdateAllObjectsInAllOpenScenes()
		{
			for(int i = SceneManager.sceneCount - 1; i >= 0; i--)
			{
				var openScene = SceneManager.GetSceneAt(i);
				if(openScene.isLoaded)
				{
					SceneInitializer.ProcessChangedScene(openScene);
				}
			}
		}

		private abstract class MassInitializer
		{
			protected static readonly List<IInitializable> componentsInChildrenToProcess = new(8);
		}

		private abstract class MassInitializer<TMassInitializer, TTarget> : MassInitializer where TMassInitializer : MassInitializer<TMassInitializer, TTarget>, new()
		{
			protected TTarget targetToProcess;
			private CancellationTokenSource cancellationTokenSource;

			private static readonly Dictionary<TTarget, TMassInitializer> runningInstances = new();

			public static async void Process(TTarget target)
			{
				if(!runningInstances.TryGetValue(target, out var massInitializer))
				{
					massInitializer = new();
					massInitializer.targetToProcess = target;
					runningInstances.Add(target, massInitializer);
				}

				CancellationTokenSource cancellationTokenSource = new();
				await massInitializer.Restart(cancellationTokenSource);

				if(!cancellationTokenSource.IsCancellationRequested)
				{
					runningInstances.Remove(target);
				}
			}

			private Task Restart(CancellationTokenSource cancellationTokenSource)
			{
				this.cancellationTokenSource?.Cancel();
				this.cancellationTokenSource = cancellationTokenSource;
				return Process(targetToProcess, cancellationTokenSource.Token);
			}

			protected abstract Task Process(TTarget targetToProcess, CancellationToken cancellationToken);

			protected static async Task ProcessGameObjectAndChildren(GameObject rootGameObject, CancellationToken cancellationToken)
			{
				const int MAX_TO_INIT_PER_FRAME = 10;

				await Task.Yield();

				if(rootGameObject == null || cancellationToken.IsCancellationRequested)
				{
					return;
				}

				rootGameObject.GetComponentsInChildren(false, componentsInChildrenToProcess);

				await Task.Yield();
				if(cancellationToken.IsCancellationRequested) return;

				for(int c = componentsInChildrenToProcess.Count - 1; c >= 0; c--)
				{
					var component = componentsInChildrenToProcess[c];
					if(component != null && typesWithAttribute.Contains(component.GetType()))
					{
						component.Init(Context.EditMode | Context.MainThread);
					}

					if(c % MAX_TO_INIT_PER_FRAME == 0)
					{
						await Task.Yield();
						if(cancellationToken.IsCancellationRequested) return;
					}
				}

				componentsInChildrenToProcess.Clear();
			}
		}

		private sealed class SceneInitializer : MassInitializer<SceneInitializer, Scene>
		{
			private readonly List<GameObject> rootGameObjectsToProcess = new(32);

			public static void ProcessChangedScene(Scene scene) => Process(scene);

			protected override async Task Process(Scene scene, CancellationToken cancellationToken)
			{
				await Task.Yield();

				if(!scene.IsValid() || !scene.isLoaded || cancellationToken.IsCancellationRequested)
				{
					return;
				}

				scene.GetRootGameObjects(rootGameObjectsToProcess);

				for(int i = rootGameObjectsToProcess.Count - 1; i >= 0; i--)
				{
					await ProcessGameObjectAndChildren(rootGameObjectsToProcess[i], cancellationToken);

					if(cancellationToken.IsCancellationRequested)
					{
						return;
					}
				}

				rootGameObjectsToProcess.Clear();
			}
		}

		private sealed class PrefabInitializer : MassInitializer<PrefabInitializer, GameObject>
		{
			public static void ProcessChangedPrefab(GameObject prefabRoot) => Process(prefabRoot);

			protected override Task Process(GameObject prefabRoot, CancellationToken cancellationToken)
				=> ProcessGameObjectAndChildren(prefabRoot, cancellationToken);
		}
	}
}
#endif