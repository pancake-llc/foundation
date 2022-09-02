using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Pancake.Init.EditorOnly
{
	[CustomEditor(typeof(Animator), true), CanEditMultipleObjects]
    public sealed class AnimatorEditor : UnityEditor.Editor
	{
		private static readonly Type internalEditorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AnimatorInspector");

		private static GUIContent iconToolbarPlus;
		private static GUIStyle preButton;
		private static GUIStyle footerBackground;

		private UnityEditor.Editor animatorEditor;
		private readonly List<InitializerDrawer> initializerDrawers = new List<InitializerDrawer>();
		private Type[] initializerTypes;
		private bool drawInitializerGUI;
		private RuntimeAnimatorController[] animatorControllers;
		private Animator[] animators;
		private GameObject[] gameObjects;

		private Animator FirstAnimator => animators[0];
		private RuntimeAnimatorController FirstAnimatorController => animatorControllers[0];

		private static Type GetInitializableType(Type type)
        {
			foreach(var interfaceType in type.GetInterfaces())
			{
				if(!interfaceType.IsGenericType)
				{
					continue;
				}

				var definition = interfaceType.GetGenericTypeDefinition();
				if(definition == typeof(IInitializable<>) || definition == typeof(IInitializable<,>) || definition == typeof(IInitializable<,,>)
				|| definition == typeof(IInitializable<,,,>) || definition == typeof(IInitializable<,,,,>) || definition == typeof(IInitializable<,,,,,>))
				{
					return interfaceType;
				}
			}

			return null;
		}

		private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] compilerMessages)
		{
			string assemblyName = Path.GetFileName(assemblyPath);

			if(animatorEditor != null && string.Equals(assemblyName, Path.GetFileName(animatorEditor.GetType().Assembly.Location)))
			{
				DestroyImmediate(animatorEditor);
			}

			for(int i = 0, count = initializerDrawers.Count; i < count; i++)
			{
				var initializerDrawer = initializerDrawers[i];
				InitializerDrawer.OnAssemblyCompilationStarted(ref initializerDrawer, assemblyName);
			}

			initializerDrawers.Clear();
		}

		private void Setup()
		{
			int targetCount = targets.Length;
			Array.Resize(ref animators, targetCount);
			Array.Resize(ref gameObjects, targetCount);
			Array.Resize(ref animatorControllers, targetCount);
			var animator = targets[0] as Animator;
			animators[0] = animator;
			gameObjects[0] = animator.gameObject;
			var firstAnimatorController = animator.runtimeAnimatorController;
			animatorControllers[0] = firstAnimatorController;
			bool canDrawInitializers = true;

			for(int i = 1; i < targetCount; i++)
			{
				animator = targets[i] as Animator;
				animators[i] = animator;
				gameObjects[i] = animator.gameObject;
				var animatorController = animator.runtimeAnimatorController;
				animatorControllers[i] = animatorController;
				if(animatorController == null || animatorController != firstAnimatorController)
				{
					canDrawInitializers = false;
				}
			}
			

			initializerDrawers.Clear();
			var initializerTypes = new List<Type>();
			var behaviours = FirstAnimator.GetBehaviours<StateMachineBehaviour>();

			foreach(var behaviourAndInitializableType in GetInitializableBehavioursInAnimator(FirstAnimator))
			{
				var behaviour = behaviourAndInitializableType.behaviour;
				Type behaviourType = behaviour.GetType();
				Type initializableType = behaviourAndInitializableType.initializableType;
				var initParameterTypes = initializableType.GetGenericArguments();

				foreach(var initializerType in InitializerEditorUtility.GetInitializerTypes(behaviourType))
				{
					if(initializerTypes.Contains(initializerType))
					{
						continue;
					}

					initializerTypes.Add(initializerType);

					for(int i = 0, initializerCount = gameObjects[0].GetComponents(initializerType).Length; i < initializerCount; i++)
					{
						var initializers = gameObjects.Select(gameObject => gameObject.GetComponents(initializerType).ElementAtOrDefault(i)).ToArray<Object>();
						if(initializers.Any(obj => obj == null))
						{
							canDrawInitializers = false;
							break;
						}

						if(initializers[0] is IInitializer initializer && Array.IndexOf(targets, initializer.Target) == -1)
						{
							continue;
						}

						var drawer = new InitializerDrawer(behaviourType, new Object[] { behaviour }, initParameterTypes, null, gameObjects);
						drawer.Initializers = initializers.Any(obj => obj == null) ? Array.Empty<Object>() : initializers;
						
						drawer.OnAddInitializerButtonPressedOverride = OnAddButtonPressed;
						initializerDrawers.Add(drawer);
					}
				}
			}

			if(initializerDrawers.Count == 0)
			{
				foreach(var behaviourAndInitializableType in GetInitializableBehavioursInAnimator(FirstAnimator))
				{
					var behaviour = behaviourAndInitializableType.behaviour;
					Type behaviourType = behaviour.GetType();
					Type initializableType = behaviourAndInitializableType.initializableType;
					var initParameterTypes = initializableType.GetGenericArguments();
					var drawer = new InitializerDrawer(behaviourType, new Object[] { behaviour }, initParameterTypes, null, gameObjects);
					drawer.OnAddInitializerButtonPressedOverride = OnAddButtonPressed;
					initializerDrawers.Add(drawer);
					break;
				}
			}

			this.initializerTypes = initializerTypes.ToArray();
			drawInitializerGUI = initializerDrawers.Count > 0 && canDrawInitializers && !Application.isPlaying;

			var editorType = InitializableEditorInjector.GetCustomEditorType(target.GetType(), targets.Length > 0);
			CreateCachedEditor(targets, editorType, ref animatorEditor);

			if(iconToolbarPlus == null)
			{
				iconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add State Machine Behaviour Initializer");
				preButton = "RL FooterButton";
				footerBackground = "RL Footer";
			}
		}

		public override VisualElement CreateInspectorGUI()
		{
			if(animatorEditor == null)
			{
				CreateCachedEditor(targets, internalEditorType, ref animatorEditor);
			}

			var internalGUI = animatorEditor.CreateInspectorGUI();
			if(internalGUI == null)
			{
				return null;
			}

			var root = new VisualElement();
			root.Add(new IMGUIContainer(InitializersGUI));
			root.Add(internalGUI);
			return root;
		}

		public override void OnInspectorGUI()
		{
			InitializersGUI();
			BaseGUI();
		}

		private void InitializersGUI()
		{
			if(initializerTypes is null)
			{
				Setup();
			}

			if(!drawInitializerGUI)
			{
				return;
			}

			for(int i = 0, count = initializerDrawers.Count; i < count; i++)
			{
				var drawer = initializerDrawers[i];
				if((drawer.Initializers.Length == 0 || drawer.Initializers[0] == null) && count > 1)
				{
					initializerDrawers.RemoveAt(i);
					count--;
					continue;
				}

				drawer.OnInspectorGUI();
			}

			GUILayout.Label(GUIContent.none, GUILayout.Height(0f));
			var rect = GUILayoutUtility.GetLastRect();
			rect.y += rect.height;
			rect.height = EditorGUIUtility.singleLineHeight;

			bool hasAtLeastOneInitializer = initializerDrawers.Count > 0 && initializerDrawers[0].Initializers.Length > 0 && initializerDrawers[0].Initializers[0] != null;
			bool drawAddButton = hasAtLeastOneInitializer && (GetAllAddableInitializerTypes(FirstAnimator).Any() || HasAnyInitializablesWithoutAnInitializerClass(FirstAnimator));
			if(drawAddButton)
			{
				DrawAddButton(rect);
			}
		}

		private void BaseGUI()
		{
			if(FirstAnimator == null || FirstAnimatorController != FirstAnimator.runtimeAnimatorController)
			{
				Setup();
			}

			serializedObject.ApplyModifiedProperties();
			animatorEditor.serializedObject.Update();
			animatorEditor.OnInspectorGUI();
			animatorEditor.serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
		}

		private void OnEnable()
        {
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
		}

		private void OnDisable()
		{
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

			for(int i = 0, count = initializerDrawers.Count; i < count; i++)
			{
				if(initializerDrawers[i] != null)
				{
					initializerDrawers[i].Dispose();
				}
			}

			initializerDrawers.Clear();

			if(animatorEditor != null)
			{
				DestroyImmediate(animatorEditor);
				animatorEditor = null;
			}
		}

		private void DrawAddButton(Rect rect)
		{
			float xMax = rect.xMax - 3f;
			float x = xMax - 33f;
			var backgroundRect = new Rect(x, rect.y, xMax - x, rect.height);

			if(Event.current.type == EventType.Repaint)
			{
				footerBackground.Draw(backgroundRect, false, false, false, false);
			}

			GUILayout.Space(EditorGUIUtility.singleLineHeight + 3f);

			var addButtonRect = new Rect(x + 4f, rect.y, 25f, 16f);

			if(GUI.Button(addButtonRect, iconToolbarPlus, preButton))
			{
				OnAddButtonPressed(addButtonRect);
			}
		}

		private void OnAddButtonPressed(Rect addRect)
		{
			var menu = new GenericMenu();
			bool menuIsEmpty = true;

			foreach(var behaviourAndInitializableType in GetInitializableBehavioursInAnimator(FirstAnimator))
			{
				var behaviour = behaviourAndInitializableType.behaviour;
				Type behaviourType = behaviour.GetType();
				Type initializableType = behaviourAndInitializableType.initializableType;

				var initializerTypes = InitializerEditorUtility.GetInitializerTypes(behaviourType);
				if(!initializerTypes.Any())
				{
					menu.AddItem(new GUIContent("Generate Initializer For " + behaviourType.Name), false, () =>
					{
						string initializerPath = ScriptGenerator.CreateInitializer(Find.Script(behaviourType));

						var addScriptMethod = typeof(InternalEditorUtility).GetMethod("AddScriptComponentUncheckedUndoable", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
						if(addScriptMethod is null)
						{
							#if DEV_MODE
							Debug.LogWarning("Method InternalEditorUtilityAddScriptComponentUncheckedUndoable not found.");
							#endif
							return;
						}

						var initializerScript = AssetDatabase.LoadAssetAtPath<MonoScript>(initializerPath);
						var initializerGuid = AssetDatabase.AssetPathToGUID(initializerPath);

						EditorPrefs.SetString(InitializerDrawer.SetInitializerTargetOnScriptsReloadedKey, initializerGuid + ":" + string.Join(";", targets.Select(t => t.GetInstanceID())));

						if(initializerScript != null)
						{
							Debug.Log($"Initializer class created at {initializerPath}.", initializerScript);

							var gameObject = gameObjects[0];
							addScriptMethod.Invoke(null, new Object[] { gameObject, initializerScript });
						}

						GUI.changed = true;
					});

					menuIsEmpty = false;
					continue;
				}

				foreach(var initializerType in InitializerEditorUtility.GetInitializerTypes(behaviourType))
				{
					if(gameObjects[0].TryGetComponent(initializerType, out _))
					{
						continue;
					}

					menu.AddItem(new GUIContent("Add " + initializerType.Name), false, () =>
					{
						var initParameterTypes = initializableType.GetGenericArguments();
						var initializers = new List<Object>();
						for(int i = 0, count = gameObjects.Length; i < count; i++)
						{
							var gameObject = gameObjects[i];
							var initializer = gameObject.AddComponent(initializerType);
							initializers.Add(initializer);
						}

						var drawer = new InitializerDrawer(behaviourType, new Object[] { behaviour }, initParameterTypes, null, gameObjects);
						drawer.Initializers = initializers.ToArray();
						drawer.OnAddInitializerButtonPressedOverride = OnAddButtonPressed;
						initializerDrawers.Add(drawer);
					});

					menuIsEmpty = false;
				}
			}

			if(!menuIsEmpty)
			{
				menu.DropDown(addRect);
			}
		}

		private IEnumerable<(StateMachineBehaviour behaviour, Type initializableType)> GetInitializableBehavioursInAnimator(Animator animator)
		{
			if(animator == null)
			{
				yield break;
			}

			foreach(var behaviour in animator.GetBehaviours<StateMachineBehaviour>())
			{
				var behaviourType = behaviour.GetType();
				if(!(GetInitializableType(behaviourType) is Type initializableType))
				{
					continue;
				}

				yield return (behaviour, initializableType);
			}
		}

		private IEnumerable<Type> GetAllAddableInitializerTypes(Animator animator)
		{
			foreach(var behaviourAndInitializableType in GetInitializableBehavioursInAnimator(animator))
			{
				var behaviour = behaviourAndInitializableType.behaviour;
				Type behaviourType = behaviour.GetType();
				Type initializableType = behaviourAndInitializableType.initializableType;
				foreach(var initializerType in InitializerEditorUtility.GetInitializerTypes(behaviourType))
				{
					if(animator.gameObject.TryGetComponent(initializerType, out _))
					{
						continue;
					}

					yield return initializerType;
				}
			}
		}

		private bool HasAnyInitializablesWithoutAnInitializerClass(Animator animator)
		{
			foreach(var behaviourAndInitializableType in GetInitializableBehavioursInAnimator(animator))
			{
				var behaviour = behaviourAndInitializableType.behaviour;
				Type behaviourType = behaviour.GetType();
				if(!InitializerEditorUtility.GetInitializerTypes(behaviourType).Any())
				{
					return true;
				}
			}

			return false;
		}
	}
}