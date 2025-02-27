//#define DEBUG_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Base class for Editors that handle drawing an Init section at the top of a target's editor,
	/// that can have more than one initializer.
	/// </summary>
	/// <typeparam name="TTarget"> The component or scriptable object being inspected in the Inspector. </typeparam>
	/// <typeparam name="TClient"> Type of the object receiving Init arguments </typeparam>
	public abstract class MultiInitializableEditorDecorator<TTarget, TClient> : EditorDecorator where TTarget : Component where TClient : class
	{
		private static readonly List<TClient> tempClientsList = new();

		private Editor decoratedEditor;
		private readonly List<InitializerGUI> initializerGUIs = new();
		private bool drawInitializerGUI;
		private TTarget[] inspected;
		private GameObject[] gameObjects;
		private readonly List<TClient> initializablesLastFrame = new();
		private bool setupDone;

		private TTarget Inspected => inspected[0];

		private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] compilerMessages)
		{
			string assemblyName = Path.GetFileName(assemblyPath);
			bool changed = false;

			if(decoratedEditor && string.Equals(assemblyName, Path.GetFileName(decoratedEditor.GetType().Assembly.Location)))
			{
				Object.DestroyImmediate(decoratedEditor);
				changed = true;
			}

			for(int i = initializerGUIs.Count - 1; i >= 0; i--)
			{
				var initializerDrawer = initializerGUIs[i];
				if(initializerDrawer is null)
				{
					initializerGUIs.RemoveAt(i);
					changed = true;
				}
			}

			if(changed)
			{
				SetupDuringNextOnGUI();
			}
		}

		private void ClearInitializerGUIs()
		{
			for(int i = initializerGUIs.Count - 1; i >= 0; i--)
			{
				InitializerGUI initializerGUI = initializerGUIs[i];
				initializerGUI.Changed -= OnInitializerGUIChanged;
				initializerGUI.Dispose();
			}

			initializerGUIs.Clear();
		}

		private void Setup()
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.Setup() with Event.current:{Event.current.type}");
			#endif

			setupDone = true;

			int targetCount = targets.Length;
			Array.Resize(ref inspected, targetCount);
			Array.Resize(ref gameObjects, targetCount);
			var firstInspected = targets[0] as TTarget;
			inspected[0] = firstInspected;
			gameObjects[0] = firstInspected.gameObject;
			GetInitializablesFromTarget(firstInspected, tempClientsList);
			var firstInitializables = tempClientsList.ToArray();
			tempClientsList.Clear();
			RebuildInitializerGUIs(firstInitializables, out drawInitializerGUI);
			Editor.CreateCachedEditor(targets, null, ref decoratedEditor);
			UpdateInitializablesLastFrame();
			Repaint();
		}

		private void RebuildInitializerGUIs(TClient[] firstInitializables, out bool drawInitializerGUI)
		{
			ClearInitializerGUIs();

			int targetCount = targets.Length;
			var initializerTypes = new HashSet<Type>(targetCount);
			bool canDrawInitializers = targetCount == 1;
			foreach(TClient initializable in firstInitializables)
			{
				Type concreteType = initializable.GetType();
				var initParameterTypes = GetInitArgumentTypes(initializable);

				foreach(var initializerType in InitializerEditorUtility.GetInitializerTypes(concreteType))
				{
					if(!initializerTypes.Add(initializerType))
					{
						continue;
					}

					for(int i = 0, initializerCount = gameObjects[0].GetComponents(initializerType).Length; i < initializerCount; i++)
					{
						var initializers = gameObjects.Select(gameObject => gameObject.GetComponents(initializerType).ElementAtOrDefault(i)).ToArray<Object>();
						if(Array.Exists(initializers, x => x == null))
						{
							canDrawInitializers = false;
							break;
						}

						if(initializers[0] is IInitializer initializer && Array.IndexOf(targets, initializer.Target) == -1)
						{
							continue;
						}

						var initializerGUI = AddInitializerGUI(initializable, initParameterTypes);
						initializerGUI.Initializers = Array.Exists(initializers, obj => !obj) ? Array.Empty<Object>() : initializers;
					}
				}
			}
			
			drawInitializerGUI = initializerTypes.Count > 0 && canDrawInitializers && !Application.isPlaying;

			if(drawInitializerGUI && initializerGUIs.Count == 0)
			{
				var initializerGUI = new InitializerGUI(SerializedObject, Array.Empty<object>(), Array.Empty<Type>(), null, gameObjects);
				initializerGUI.OnAddInitializerButtonPressedOverride = OnAddButtonPressed;
				initializerGUI.Changed += OnInitializerGUIChanged;
				initializerGUIs.Add(initializerGUI);
			}
		}

		protected virtual InitializerGUI AddInitializerGUI(TClient initializable, Type[] initParameterTypes)
		{
			var initializerGUI = new InitializerGUI(SerializedObject, new object[] { initializable }, initParameterTypes, null, gameObjects);
			initializerGUI.OnAddInitializerButtonPressedOverride = OnAddButtonPressed;
			initializerGUI.Changed += OnInitializerGUIChanged;
			initializerGUIs.Add(initializerGUI);
			return initializerGUI;
		}

		private void OnInitializerGUIChanged(InitializerGUI initializerGUI)
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.OnInitializerGUIChanged(): setupDone = false with Event.current:{Event.current?.type.ToString() ?? "None"}");
			#endif

			setupDone = false;
			Repaint();
		}

		public override void OnBeforeInspectorGUI()
		{
			#if DEV_MODE
			Profiler.BeginSample("MultiInitializableEditorDecorator.OnBeforeInspectorGUI");
			#endif

			OnBeginGUI();
			DrawInitializerGUIs();

			#if DEV_MODE
			Profiler.EndSample();
			#endif
		}

		public override void OnAfterInspectorGUI() => LayoutUtility.EndGUI(DecoratedEditor);

		private void DrawInitializerGUIs()
		{
			if(!drawInitializerGUI)
			{
				return;
			}
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(14f);

			GUILayout.Space(1f);
			
			EditorGUILayout.BeginVertical();

			for(int i = 0, lastIndex = initializerGUIs.Count - 1; i <= lastIndex; i++)
			{
				var initializerGUI = initializerGUIs[i];
				initializerGUI.OnInspectorGUI();
				if(!initializerGUI.IsValid())
				{
					SetupDuringNextOnGUI();
				}

				if(i < lastIndex)
				{ 
					GUILayout.Space(5f);
				}
			}
			
			EditorGUILayout.EndVertical();
			
			GUILayout.Space(7f);
			
			EditorGUILayout.EndHorizontal();

			GUILayout.Label(GUIContent.none, GUILayout.Height(0f));
			var rect = GUILayoutUtility.GetLastRect();
			rect.y += rect.height;
			rect.height = EditorGUIUtility.singleLineHeight;

			bool hasAtLeastOneInitializer = initializerGUIs.Count > 0 && initializerGUIs[0].Initializers.Length > 0 && initializerGUIs[0].Initializers[0] != null;
			bool drawAddButtonBelow = hasAtLeastOneInitializer && (HasAnyAddableInitializerTypes() || HasAnyInitializablesWithoutAnInitializerClass());
			if(drawAddButtonBelow)
			{
				DrawAddButton(rect);
			}

			GUILayout.Space(1f);
		}

		protected virtual bool ShouldRunSetupAgain()
		{
			if(!setupDone)
			{
				return true;
			}

			foreach(var initializerGUI in initializerGUIs)
			{
				if(!initializerGUI.IsValid())
				{
					return true;
				}
			}

			if(!target)
			{
				return true;
			}

			return UpdateInitializablesLastFrame();
		}

		private bool UpdateInitializablesLastFrame()
		{
			GetInitializablesFromTarget(Inspected, tempClientsList);
			if(tempClientsList.SequenceEqual(initializablesLastFrame))
			{
				tempClientsList.Clear();
				return false;
			}

			initializablesLastFrame.Clear();
			initializablesLastFrame.AddRange(tempClientsList);
			tempClientsList.Clear();
			return true;
		}

		public MultiInitializableEditorDecorator(Editor decoratedEditor) : base(decoratedEditor)
        {
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
		}

		protected override void Dispose(bool disposeManaged)
		{
			AnyPropertyDrawer.DisposeAllStaticState();
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

			ClearInitializerGUIs();
			// for(int i = 0, count = initializerGUIs.Count; i < count; i++)
			// {
			// 	initializerGUIs[i]?.Dispose();
			// }
			//
			// initializerGUIs.Clear();

			if(decoratedEditor)
			{
				Object.DestroyImmediate(decoratedEditor);
				decoratedEditor = null;
			}
		}

		private void DrawAddButton(Rect rect)
		{
			const float width = 30f;
			const float rightOffset = 8f;
			float x = rect.xMax - width - rightOffset;
			var backgroundRect = new Rect(x, rect.y - 2f, width, rect.height);

			if(Event.current.type == EventType.Repaint)
			{
				Styles.AddButtonBackground.Draw(backgroundRect, false, false, false, false);
			}

			GUILayout.Space(EditorGUIUtility.singleLineHeight + 3f);

			var addButtonRect = new Rect(x + 3f, rect.y - 2f, 25f, 16f);

			Styles.AddButtonIcon.tooltip = "Add State Machine Behaviour Initializer";
			if(GUI.Button(addButtonRect, Styles.AddButtonIcon, Styles.AddButtonStyle))
			{
				OnAddButtonPressed(addButtonRect);
			}

			Styles.AddButtonIcon.tooltip = "";
		}

		private void OnAddButtonPressed(Rect addRect)
		{
			var menu = new GenericMenu();
			bool menuIsEmpty = true;

			GetInitializablesFromTarget(Inspected, tempClientsList);

			foreach(TClient initializable in tempClientsList)
			{
				Type concreteType = initializable.GetType();
				var initializerTypes = InitializerEditorUtility.GetInitializerTypes(concreteType);
				if(!initializerTypes.Any())
				{
					menu.AddItem(new GUIContent("Generate Initializer For " + concreteType.Name), false, () => InitializerEditorUtility.GenerateAndAttachInitializer(targets, initializable));

					menuIsEmpty = false;
					continue;
				}

				foreach(var initializerType in initializerTypes)
				{
					if(gameObjects[0].TryGetComponent(initializerType, out _))
					{
						continue;
					}

					menu.AddItem(new GUIContent("Add " + initializerType.Name), false, () =>
					{
						for(int i = 0, count = gameObjects.Length; i < count; i++)
						{
							gameObjects[i].AddComponent(initializerType);
						}

						SetupDuringNextOnGUI();
					});

					menuIsEmpty = false;
				}
			}

			tempClientsList.Clear();

			if(!menuIsEmpty)
			{
				menu.DropDown(addRect);
			}
		}

		protected abstract void GetInitializablesFromTarget([DisallowNull] TTarget target, List<TClient> addToList);

		private static Type[] GetInitArgumentTypes(object initializable)
			=> TryGetIInitializableType(initializable.GetType(), out Type iinitializableType)
			? iinitializableType.GetGenericArguments()
			: Array.Empty<Type>();

		private static bool TryGetIInitializableType(Type type, out Type iinitializableType)
		{
			foreach(var interfaceType in type.GetInterfaces())
			{
				if(InitializerEditorUtility.IsGenericIInitializableType(interfaceType))
				{
					iinitializableType = interfaceType;
					return true;
				}
			}

			iinitializableType = null;
			return false;
		}

		private bool HasAnyAddableInitializerTypes()
		{
			GetInitializablesFromTarget(Inspected, tempClientsList);

			foreach(TClient initializable in tempClientsList)
			{
				foreach(var initializerType in InitializerEditorUtility.GetInitializerTypes(initializable.GetType()))
				{
					if(!Array.Exists(gameObjects, x => x .TryGetComponent(initializerType, out _)))
					{
						tempClientsList.Clear();
						return true;
					}
				}
			}

			tempClientsList.Clear();
			return false;
		}

		private bool HasAnyInitializablesWithoutAnInitializerClass()
		{
			GetInitializablesFromTarget(Inspected, tempClientsList);

			foreach(TClient initializable in tempClientsList)
			{
				if(!InitializerEditorUtility.HasAnyInitializerTypes(initializable.GetType()))
				{
					tempClientsList.Clear();
					return true;
				}
			}

			tempClientsList.Clear();
			return false;
		}

		private void OnBeginGUI()
		{
			LayoutUtility.BeginGUI(DecoratedEditor);

			if (setupDone)
			{
				foreach(var initializerGUI in initializerGUIs)
				{
					if(!initializerGUI.IsValid())
					{
						setupDone = false;
						break;
					}
				}

				if(setupDone && !ShouldRunSetupAgain())
				{
					return;
				}
			}

			if(Event.current.type != EventType.Layout)
			{
				SetupDuringNextOnGUI();
				return;
			}

			Setup();
		}

		private void SetupDuringNextOnGUI()
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.SetupDuringNextOnGUI(): setupDone = false");
			#endif

			setupDone = false;
			Repaint();

			if(Event.current != null)
			{
				LayoutUtility.ExitGUI();
			}
		}

		private void Repaint() => LayoutUtility.Repaint(DecoratedEditor);
	}
}