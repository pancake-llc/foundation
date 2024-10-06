using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using static Sisus.Init.EditorOnly.Internal.DragAndDropUtility;
using static UnityEditorInternal.ComponentUtility;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Class responsible for setting Initializer inspector hide flags on/off
	/// based on whether their editor has already been embedded in their
	/// target components Editor.
	/// </summary>
	[InitializeOnLoad]
	internal static class InitializerEditors
	{
		internal static readonly Dictionary<Object, List<IInitializer>> InitializersOnInspectedObjects = new();
		internal static readonly HashSet<IInitializer> InitializersWithClientOnSameGameObject = new();

		static InitializerEditors()
		{
			Selection.selectionChanged -= SelectionChanged;
			Selection.selectionChanged += SelectionChanged;
			Editor.finishedDefaultHeaderGUI -= AfterInspectorRootEditorHeaderGUI;
			Editor.finishedDefaultHeaderGUI += AfterInspectorRootEditorHeaderGUI;
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
				if(!(initializer is Component initializerComponent) || !initializerComponent || initializer.Target != DragAndDroppedComponent || DragSourceGameObject == DropTargetGameObject)
				{
					continue;
				}

				CopyComponent(initializerComponent);

				using var potentialDraggedComponents = DropTargetGameObject.GetComponentsNonAlloc(DragAndDroppedComponentType);
				var draggedTarget = potentialDraggedComponents.LastOrDefault();

				if(!PasteComponentAsNew(DropTargetGameObject))
				{
					continue;
				}

				using var potentialMigratedComponent = DropTargetGameObject.GetComponentsNonAlloc(DragAndDroppedComponentType);
				if(DropTargetGameObject.GetComponents(initializer.GetType()).LastOrDefault() is Component pastedComponent and IInitializer pastedInitializer)
				{
					pastedInitializer.Target = draggedTarget;
					pastedComponent.hideFlags = HideFlags.HideInInspector;
				}

				Object.DestroyImmediate(initializerComponent);
			}
		}

		private static void AfterInspectorRootEditorHeaderGUI([DisallowNull] Editor editor)
		{
			foreach(var target in editor.targets)
			{
				if(target is GameObject gameObject)
				{
					AfterGameObjectHeaderGUI(gameObject);
					continue;
				}

				if(target is ScriptableObject scriptableObject)
				{
					AfterScriptableObjectHeaderGUI(scriptableObject);
				}
			}
		}

		private static void AfterGameObjectHeaderGUI([DisallowNull] GameObject gameObject)
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

		private static void AfterScriptableObjectHeaderGUI([DisallowNull] ScriptableObject scriptableObject)
		{
			if(InitializersOnInspectedObjects.TryGetValue(scriptableObject, out List<IInitializer> initializers))
			{
				initializers.Clear();
			}
			else
			{
				initializers = new List<IInitializer>(1);
				InitializersOnInspectedObjects.Add(scriptableObject, initializers);
			}

			using(var serializedObject = new SerializedObject(scriptableObject))
			{
				if(serializedObject.FindProperty("initializer") is SerializedProperty initializerProperty
					&& initializerProperty.propertyType == SerializedPropertyType.ObjectReference
					&& initializerProperty.objectReferenceValue is IInitializer initializer)
				{
					initializers.Add(initializer);
				}
			}
		}
	}
}