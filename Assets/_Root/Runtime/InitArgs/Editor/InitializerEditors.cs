using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Pancake.Init.EditorOnly.DragAndDropUtility;
using static UnityEditorInternal.ComponentUtility;

namespace Pancake.Init.EditorOnly
{
	/// <summary>
	/// Class responsible for setting Initializer inspector hide flags on/off
	/// based on whether or not their editor has already been embedded in their
	/// target components Editor.
	/// </summary>
	[InitializeOnLoad]
	internal static class InitializerEditors
	{
		internal static readonly Dictionary<GameObject, List<IInitializer>> InitializersOnInspectedObjects = new Dictionary<GameObject, List<IInitializer>>();
		internal static readonly HashSet<IInitializer> InitializersWithClientOnSameGameObject = new HashSet<IInitializer>();

		static InitializerEditors()
		{
			Selection.selectionChanged -= SelectionChanged;
			Selection.selectionChanged += SelectionChanged;
			UnityEditor.Editor.finishedDefaultHeaderGUI -= AfterInspectorRootEditorHeaderGUI;
			UnityEditor.Editor.finishedDefaultHeaderGUI += AfterInspectorRootEditorHeaderGUI;
		}

		private static void SelectionChanged()
		{
			// Selection is cleared when drag-and-dropping a component to another GameObject.
			// Check if Initializer client was dragged to another GameObject before clearing
			// InitializersOnInspectedObjects.
			MoveInitializerWithClientIfDragAndDroppedToAnotherGameObject();

			InitializersOnInspectedObjects.Clear();
			InitializersWithClientOnSameGameObject.Clear();
		}

		private static void MoveInitializerWithClientIfDragAndDroppedToAnotherGameObject()
		{
			if(DragAndDroppedComponentType == null)
			{
				return;
			}

			foreach(var initializer in InitializersWithClientOnSameGameObject)
			{
				if(!(initializer is Component initializerComponent) || initializerComponent == null || initializer.Target != DragAndDroppedComponent || DragSourceGameObject == DropTargetGameObject)
				{
					continue;
				}

				CopyComponent(initializerComponent);

				var client = DropTargetGameObject.GetComponents(DragAndDroppedComponentType).LastOrDefault();

				if(!PasteComponentAsNew(DropTargetGameObject))
				{
					continue;
				}

				if(DropTargetGameObject.GetComponents(initializer.GetType()).LastOrDefault() is Component pastedComponent && pastedComponent is IInitializer pastedInitializer)
				{
					pastedInitializer.Target = client;
					pastedComponent.hideFlags = HideFlags.HideInInspector;
				}

				Object.DestroyImmediate(initializerComponent);
			}
		}

		private static void AfterInspectorRootEditorHeaderGUI([JetBrains.Annotations.NotNull] UnityEditor.Editor editor)
		{
			foreach(var target in editor.targets)
			{
				if(target is GameObject gameObject)
				{
					AfterGameObjectHeaderGUI(gameObject);
				}
			}
		}

		private static void AfterGameObjectHeaderGUI([JetBrains.Annotations.NotNull] GameObject gameObject)
		{
			if(InitializersOnInspectedObjects.TryGetValue(gameObject, out List<IInitializer> initializers))
			{
				initializers.Clear();
			}
			else
			{
				initializers = new List<IInitializer>();
				InitializersOnInspectedObjects.Add(gameObject, initializers);
			}

			gameObject.GetComponents(initializers);

			for(int i = initializers.Count - 1; i >= 0; i--)
			{
				var initializer = initializers[i];
				var initializerComponent = initializer as Component;
				if(initializerComponent == null)
				{
					continue;
				}

				var clientComponent = initializer.Target as Component;

				bool hideInInspector;
				bool clientIsNullOrDestroyed = clientComponent == null;
				if(clientIsNullOrDestroyed)
				{
					// If client component on same GameObject was just destroyed also destroy the Initializer.
					// This feels more intuitive since the Initializer is embedded inside the client inspector.
					bool clientHasBeenDestroyed = !(clientComponent is null) && clientComponent.GetHashCode() != 0;
					if(clientHasBeenDestroyed && InitializersWithClientOnSameGameObject.Contains(initializer))
					{
						initializers.RemoveAt(i);
						Undo.DestroyObjectImmediate(initializerComponent);
						continue;
					}

					hideInInspector = false;
				}
				else if(clientComponent.gameObject == gameObject)
				{
					hideInInspector = true;
					InitializersWithClientOnSameGameObject.Add(initializer);
				}
				else
				{
					hideInInspector = false;
					InitializersWithClientOnSameGameObject.Remove(initializer);
				}

				initializerComponent.hideFlags = hideInInspector ? HideFlags.HideInInspector : HideFlags.None;
			}
		}
	}
}