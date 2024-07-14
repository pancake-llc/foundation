//#define DEBUG_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Base class for Editors that handle drawing an Init section at the top of a target's editor,
	/// that can have more than one initializer.
	/// </summary>
	/// <typeparam name="TTarget"> The component or scriptable object being inspected in the Inspector. </typeparam>
	/// <typeparam name="TClient"> Type of the object receiving Init arguments </typeparam>
	public abstract class MultiInitializableEditor<TTarget, TClient> : Editor where TTarget : Component where TClient : class
	{
		private static readonly List<TClient> tempClientsList = new();

		private Editor wrappedEditor;
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

			if(wrappedEditor != null && string.Equals(assemblyName, Path.GetFileName(wrappedEditor.GetType().Assembly.Location)))
			{
				DestroyImmediate(wrappedEditor);
				changed = true;
			}

			for(int i = initializerGUIs.Count - 1; i >= 0; i--)
			{
				var initializerDrawer = initializerGUIs[i];
				InitializerGUI.OnAssemblyCompilationStarted(ref initializerDrawer, assemblyName);
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

			var editorType = InitializableEditorInjector.GetCustomEditorType(target.GetType(), targets.Length > 0);
			CreateCachedEditor(targets, editorType, ref wrappedEditor);

			UpdateInitializablesLastFrame();

			LayoutUtility.Repaint(this);
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

						var initializerGUI = new InitializerGUI(targets, new object[] { initializable }, initParameterTypes, null, gameObjects);
						initializerGUI.Initializers = initializers.Any(obj => obj == null) ? Array.Empty<Object>() : initializers;
						initializerGUI.OnAddInitializerButtonPressedOverride = OnAddButtonPressed;
						initializerGUI.Changed += OnInitializerGUIChanged;
						initializerGUIs.Add(initializerGUI);
					}
				}
			}

			if(initializerGUIs.Count == 0)
			{
				foreach(TClient initializable in firstInitializables)
				{
					var initParameterTypes = GetInitArgumentTypes(initializable);
					var initializerGUI = new InitializerGUI(targets, new object[] { initializable }, initParameterTypes, null, gameObjects);
					initializerGUI.OnAddInitializerButtonPressedOverride = OnAddButtonPressed;
					initializerGUI.Changed += OnInitializerGUIChanged;
					initializerGUIs.Add(initializerGUI);
					break;
				}
			}

			drawInitializerGUI = initializerGUIs.Count > 0 && canDrawInitializers && !Application.isPlaying;
		}

		private void OnInitializerGUIChanged(InitializerGUI initializerGUI)
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.OnInitializerGUIChanged(): setupDone = false with Event.current:{Event.current?.type.ToString() ?? "None"}");
			#endif

			setupDone = false;
			Repaint();
		}

		public override VisualElement CreateInspectorGUI()
		{
			if(wrappedEditor == null)
			{
				bool multiEdit = targets.Length > 1;
				var wrappedEditorType = InitializableEditorInjector.GetCustomEditorType(typeof(TTarget), multiEdit);
				CreateCachedEditor(targets, wrappedEditorType, ref wrappedEditor);
			}

			var internalGUI = wrappedEditor.CreateInspectorGUI();
			if(internalGUI == null)
			{
				return null;
			}

			var root = new VisualElement();
			root.Add(new IMGUIContainer(IMGUIContainerOnGUIHandler));
			root.Add(internalGUI);
			return root;
		}

		private void IMGUIContainerOnGUIHandler()
		{
			HandleBeginGUI();
			InitializersGUI();
		}

		public override void OnInspectorGUI()
		{
			HandleBeginGUI();
			InitializersGUI();
			BaseGUI();
		}

		private void InitializersGUI()
		{
			if(!drawInitializerGUI)
			{
				return;
			}

			for(int i = 0, count = initializerGUIs.Count; i < count; i++)
			{
				var initializerGUI = initializerGUIs[i];
				initializerGUI.OnInspectorGUI();
				if(!initializerGUI.IsValid())
				{
					SetupDuringNextOnGUI();
				}
			}

			GUILayout.Label(GUIContent.none, GUILayout.Height(0f));
			var rect = GUILayoutUtility.GetLastRect();
			rect.y += rect.height;
			rect.height = EditorGUIUtility.singleLineHeight;

			bool hasAtLeastOneInitializer = initializerGUIs.Count > 0 && initializerGUIs[0].Initializers.Length > 0 && initializerGUIs[0].Initializers[0] != null;
			bool drawAddButton = hasAtLeastOneInitializer && (HasAnyAddableInitializerTypes() || HasAnyInitializablesWithoutAnInitializerClass(Inspected));
			if(drawAddButton)
			{
				DrawAddButton(rect);
			}
		}

		protected virtual bool ShouldRunSetupAgain() => UpdateInitializablesLastFrame();

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

		private void BaseGUI()
		{
			if(Inspected == null || ShouldRunSetupAgain())
			{
				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"{GetType().Name}.BaseGUI(): setupDone = false with Inspected={Inspected}, ShouldRunSetupAgain():{ShouldRunSetupAgain()}, setupDone:{setupDone}");
				#endif

				setupDone = false;
				Repaint();
				LayoutUtility.ExitGUI(this);
				return;
			}

			serializedObject.ApplyModifiedProperties();
			wrappedEditor.OnNestedInspectorGUI();
			serializedObject.Update();
		}

		private void OnEnable()
        {
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
		}

		private void OnDisable()
		{
			AnyPropertyDrawer.DisposeAllStaticState();

			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

			for(int i = 0, count = initializerGUIs.Count; i < count; i++)
			{
				initializerGUIs[i]?.Dispose();
			}

			initializerGUIs.Clear();

			if(wrappedEditor != null)
			{
				DestroyImmediate(wrappedEditor);
				wrappedEditor = null;
			}
		}

		private void DrawAddButton(Rect rect)
		{
			float xMax = rect.xMax - 3f;
			float x = xMax - 33f;
			var backgroundRect = new Rect(x, rect.y, xMax - x, rect.height);

			if(Event.current.type == EventType.Repaint)
			{
				Styles.AddButtonBackground.Draw(backgroundRect, false, false, false, false);
			}

			GUILayout.Space(EditorGUIUtility.singleLineHeight + 3f);

			var addButtonRect = new Rect(x + 4f, rect.y, 25f, 16f);

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
						var initParameterTypes = GetInitArgumentTypes(initializable);
						int count = gameObjects.Length;
						var initializers = new List<Object>(count);
						for(int i = 0; i < count; i++)
						{
							var gameObject = gameObjects[i];
							var initializer = gameObject.AddComponent(initializerType);
							initializers.Add(initializer);
						}

						var drawer = new InitializerGUI(targets, new object[] { initializable }, initParameterTypes, null, gameObjects);
						drawer.Initializers = initializers.ToArray();
						drawer.OnAddInitializerButtonPressedOverride = OnAddButtonPressed;
						initializerGUIs.Add(drawer);
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

		private static Type[] GetInitArgumentTypes(object initializable) => TryGetIInitializableType(initializable.GetType(), out Type iinitializableType) ? iinitializableType.GetGenericArguments() : Array.Empty<Type>();

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

		private bool HasAnyInitializablesWithoutAnInitializerClass(TTarget target)
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

		private void HandleBeginGUI()
		{
			LayoutUtility.BeginGUI(this);
			HandleSetup();
		}

		private void HandleSetup()
		{
			if(!setupDone)
			{
				SetupOrExitGUI();
				return;
			}

			for(int i = initializerGUIs.Count - 1; i >= 0; i--)
			{
				if(!initializerGUIs[i].IsValid())
				{
					SetupOrExitGUI();
					return;
				}
			}
		}

		private void SetupOrExitGUI()
		{
			if(Event.current.type == EventType.Layout)
			{
				Setup();
				return;
			}
			
			SetupDuringNextOnGUI();
		}

		private void SetupDuringNextOnGUI()
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.SetupDuringNextOnGUI(): setupDone = false");
			#endif

			setupDone = false;
			Repaint();
			LayoutUtility.ExitGUI(this);
		}
	}
}