//#define DEBUG_SETUP_DURATION

//#define DRAW_INIT_SECTION_WITHOUT_EDITOR
//#define DEBUG_ENABLED

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Sisus.Init.EditorOnly.Internal;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
#if DEV_MODE
using UnityEngine.Profiling;
#endif

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Base class for custom editors for initializable targets that visualize non-serialized fields of the target in play mode.
	/// </summary>
	public class InitializableEditor : Editor
	{
		private static readonly string[] scriptField = { "m_Script" };

		[NonSerialized]
		private bool setupDone;
		[NonSerialized]
		private bool preOnGUISetupDone;
		[NonSerialized]
		private RuntimeFieldsDrawer runtimeFieldsDrawer;
		[NonSerialized]
		private InitializerGUI initializerGUI;
		private bool drawInitSection;
		private Editor internalEditor;
		[SerializeField]
		private bool internalEditorIsGenericInspector;

		protected bool ShowRuntimeFields { get; set; }

		/// <summary>
		/// The generic type definition of the Initializable base class from which the Editor target derives from,
		/// or if that is not known, then an IInitializable{T...} interface the target implements.
		/// <para>
		/// This is used to determine what the <see cref="BaseType"/> of the target's class is.
		/// </para>
		/// </summary>
		protected virtual Type BaseTypeDefinition => typeof(Object);

		/// <summary>
		/// The type of the Initializable base class from which the Editor target derives from.
		/// <para>
		/// Types of the Init parameters are determined from the <see cref="Type.GetGenericArguments"/>
		/// results for <see cref="BaseType"/>.
		/// </para>
		/// <para>
		/// Also when drawing runtime fields fields in the base type and types it derives from will be skipped.
		/// </para>
		/// </summary>
		protected Type BaseType
		{
			get
			{
				var baseTypeDefinition = BaseTypeDefinition;

				if(baseTypeDefinition.IsInterface)
				{
					Type parentType = target.GetType();

					if(baseTypeDefinition.IsGenericTypeDefinition)
					{
						for(Type baseType = parentType.BaseType; baseType != null; baseType = baseType.BaseType)
						{
							bool implementsInterface = false;
							foreach(var @interface in baseType.GetInterfaces())
							{
								if(@interface.IsGenericType && @interface.GetGenericTypeDefinition() == baseTypeDefinition)
								{
									implementsInterface = true;
									break;
								}
							}

							if(!implementsInterface)
							{
								return parentType;
							}

							parentType = baseType;
						}
					}
					else
					{
						for(Type baseType = parentType.BaseType; baseType != null; baseType = baseType.BaseType)
						{
							bool implementsInterface = false;
							foreach(var @interface in baseType.GetInterfaces())
							{
								if(@interface == baseTypeDefinition)
								{
									implementsInterface = true;
									break;
								}
							}

							if(!implementsInterface)
							{
								return parentType;
							}

							parentType = baseType;
						}
					}

					return typeof(MonoBehaviour).IsAssignableFrom(parentType) ? typeof(MonoBehaviour) : 
							typeof(ScriptableObject).IsAssignableFrom(parentType) ? typeof(ScriptableObject) :
							typeof(Object);
				}
				else if(baseTypeDefinition.IsGenericTypeDefinition)
				{
					for(Type baseType = target.GetType().BaseType; baseType != null; baseType = baseType.BaseType)
					{
						if(baseType.IsGenericType && baseType.GetGenericTypeDefinition() == baseTypeDefinition)
						{
							return baseType;
						}
					}
				}

				return baseTypeDefinition;
			}
		}

		[DidReloadScripts]
		private static void OnScriptsReloaded()
		{
			if(!EditorPrefs.HasKey(InitializerGUI.SetInitializerTargetOnScriptsReloadedEditorPrefsKey))
			{
				return;
			}

			var value = EditorPrefs.GetString(InitializerGUI.SetInitializerTargetOnScriptsReloadedEditorPrefsKey);
			EditorPrefs.DeleteKey(InitializerGUI.SetInitializerTargetOnScriptsReloadedEditorPrefsKey);

			int i = value.IndexOf(':');
			if(i <= 0)
			{
				return;
			}

			string initializerAssetGuid = value.Substring(0, i);
			string initializerAssetPath = AssetDatabase.GUIDToAssetPath(initializerAssetGuid);
			if(string.IsNullOrEmpty(initializerAssetPath))
			{
				return;
			}

			var initializerScript = AssetDatabase.LoadAssetAtPath<MonoScript>(initializerAssetPath);
			if(!initializerScript)
			{
				return;
			}

			var initializerType = initializerScript.GetClass();
			if(initializerType == null)
			{
				return;
			}
            
			var targetIds = value.Substring(i + 1).Split(';');
			foreach(var idString in targetIds)
			{
				if(!int.TryParse(idString, out int id))
				{
					continue;
				}

				var target = EditorUtility.InstanceIDToObject(id);

				var component = target as Component;
				if(component)
				{
					var gameObject = component.gameObject;
					var initializerComponent = gameObject.GetComponent(initializerType);
					if(!initializerComponent || initializerComponent is not IInitializer initializer || !initializer.TargetIsAssignableOrConvertibleToType(component.GetType()) || initializer.Target != null)
					{
						continue;
					}

					initializer.Target = component;
					continue;
				}

				var scriptableObject = target as ScriptableObject;
				if(scriptableObject && typeof(IInitializer).IsAssignableFrom(initializerType) && typeof(ScriptableObject).IsAssignableFrom(initializerType))
				{
					IInitializer initializer = CreateInstance(initializerType) as IInitializer;
					if(initializer == null || !initializer.TargetIsAssignableOrConvertibleToType(scriptableObject.GetType()) || initializer.Target)
					{
						continue;
					}

					Undo.RegisterCreatedObjectUndo(initializer as Object, "Add Initializer");
					initializer.Target = target;

					if(target is IInitializableEditorOnly initializableEditorOnly)
					{
						Undo.RecordObject(target, "Add Initializer");
						initializableEditorOnly.Initializer = initializer;
					}
				}
			}
		}

		protected virtual void OnEnable()
		{
			#if DEV_MODE
			Profiler.enableAllocationCallstacks = true;
			Profiler.maxUsedMemory = 1024 * 1024 * 1024;
			#endif

			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;
			EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
			EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

			if(target)
			{
				MonoScript script = target is MonoBehaviour monoBehaviour ? MonoScript.FromMonoBehaviour(monoBehaviour) : null;
				Type type = target.GetType();

				Menu.SetChecked(ComponentMenuItems.ShowInitSection, !InitializerGUI.IsInitSectionHidden(script, type));
			}
		}

		private void OnPlaymodeStateChanged(PlayModeStateChange stateChange)
		{
			switch(stateChange)
			{
				case PlayModeStateChange.ExitingEditMode:
				case PlayModeStateChange.EnteredPlayMode:
					DestroyWrappedEditors();
					break;
			}
		}

		private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] compilerMessages)
		{
			string assemblyName = Path.GetFileName(assemblyPath);

			if(internalEditor && string.Equals(assemblyName, Path.GetFileName(internalEditor.GetType().Assembly.Location)))
			{
				DestroyImmediate(internalEditor);
			}

			InitializerGUI.OnAssemblyCompilationStarted(ref initializerGUI, assemblyName);
		}

		protected virtual void Setup()
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.Setup() with Event.current:{Event.current.type}");
			#endif

			#if DEV_MODE
			Profiler.BeginSample("InitializableEditor.Setup");
			#if DEBUG_SETUP_DURATION
			var timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			#endif
			#endif

			setupDone = true;

			var baseType = BaseType;
			var firstTarget = targets[0];
			var initParameterTypes = BaseType is null || !baseType.IsGenericType || baseType.IsGenericTypeDefinition
				? InitializerEditorUtility.GetInitParameterTypes(firstTarget)
				: baseType.GetGenericArguments();

			DisposeInitializerGUI();
			initializerGUI = new InitializerGUI(targets, targets.Select(GetInitializable).ToArray(), initParameterTypes);
			initializerGUI.Changed += OnOwnedInitializerGUIChanged;

			if(!preOnGUISetupDone)
			{
				PreOnGUISetup(baseType);
			}

			LayoutUtility.Repaint(this);

			#if DEV_MODE
			Profiler.EndSample();
			#if DEBUG_SETUP_DURATION
			timer.Stop();
			Debug.Log(GetType().Name + ".Setup took " + timer.Elapsed.TotalSeconds + "s.");
			#endif
			#endif
		}

		private void PreOnGUISetup(Type baseType)
		{
			preOnGUISetupDone = true;

			var firstTarget = targets[0];
			ShowRuntimeFields = firstTarget is MonoBehaviour and (IOneArgument or ITwoArguments or IThreeArguments or IFourArguments or IFiveArguments or ISixArguments or ISevenArguments or IEightArguments or INineArguments or ITenArguments or IElevenArguments or ITwelveArguments);

			if(ShowRuntimeFields)
			{
				runtimeFieldsDrawer = new RuntimeFieldsDrawer(target, baseType);
			}

			#if DRAW_INIT_SECTION_WITHOUT_EDITOR
			drawInitSection = false;
			#else
			// Always draw the Init section if the client has an initializer attached
			if(InitializerUtility.HasInitializer(firstTarget))
			{
				drawInitSection = true;
			}
			// Otherwise draw it the user has not disabled initializer visibility via the context menu for the client type
			else
			{
				drawInitSection = !InitializerGUI.IsInitSectionHidden(firstTarget);
			}
			#endif
		}

		protected virtual object GetInitializable(Object inspectedTarget) => inspectedTarget;

		[Pure]
		protected virtual RuntimeFieldsDrawer CreateRuntimeFieldsDrawer() => new(target, BaseType);

		public override VisualElement CreateInspectorGUI()
		{
			#if DEV_MODE
			Profiler.BeginSample("InitializableEditor.CreateInspectorGUI");
			#endif

			if(!preOnGUISetupDone)
			{
				PreOnGUISetup(BaseType);
			}

			if(!internalEditor)
			{
				CreateInternalEditor();
			}

			var internalGUI = internalEditor.CreateInspectorGUI();
			if(internalGUI is null)
			{
				#if DEV_MODE
				Profiler.EndSample();
				#endif
				return null;
			}

			var root = new VisualElement();
			root.Add(new IMGUIContainer(IMGUIContainerDrawInitializerGUI));
			root.Add(internalGUI);
			if(ShowRuntimeFields)
			{
				root.Add(new IMGUIContainer(DrawRuntimeFieldsGUI));
			}

			#if DEV_MODE
			Profiler.EndSample();
			#endif

			return root;
		}

		private void IMGUIContainerDrawInitializerGUI()
		{
			HandleBeginGUI();
			DrawInitializerGUI();
		}

		public override void OnInspectorGUI()
		{
			#if DEV_MODE
			Profiler.BeginSample("InitializableEditor.OnInspectorGUI");
			#endif

			HandleBeginGUI();

			bool hierarchyModeWas = EditorGUIUtility.hierarchyMode;
			EditorGUIUtility.hierarchyMode = true;

			DrawInitializerGUI();
			BaseGUI();
			DrawRuntimeFieldsGUI();

			EditorGUIUtility.hierarchyMode = hierarchyModeWas;

			#if DEV_MODE
			Profiler.EndSample();
			#endif
		}

		protected virtual void BaseGUI()
		{
			#if DEV_MODE
			Profiler.BeginSample("InitializableEditor.BaseGUI");
			#endif

			if(!internalEditor)
			{
				CreateInternalEditor();
			}

			if(internalEditorIsGenericInspector)
			{
				serializedObject.Update();

				try
				{
					DrawPropertiesExcluding(serializedObject, scriptField);
				}
				finally
				{
					serializedObject.ApplyModifiedProperties();
				}
			}
			else
			{
				serializedObject.ApplyModifiedProperties();
				internalEditor.serializedObject.Update();

				try
				{
					internalEditor.OnInspectorGUI();
				}
				finally
				{
					internalEditor.serializedObject.ApplyModifiedProperties();
					serializedObject.Update();
				}
			}

			#if DEV_MODE
			Profiler.EndSample();
			#endif
		}

		protected virtual void DrawInitializerGUI()
		{
			#if DEV_MODE
			Profiler.BeginSample("InitializableEditor.InitializerGUI");
			#endif

			if(initializerGUI is null)
			{
				Setup();
			}

			if(drawInitSection)
			{
				initializerGUI.OnInspectorGUI();
			}

			#if DEV_MODE
			Profiler.EndSample();
			#endif
		}

		protected void DrawRuntimeFieldsGUI()
		{
			#if DEV_MODE
			Profiler.BeginSample("InitializableEditor.RuntimeFieldsGUI");
			#endif

			if(!ShowRuntimeFields)
			{
				return;
			}

			if(runtimeFieldsDrawer is null)
			{
				runtimeFieldsDrawer = CreateRuntimeFieldsDrawer();
			}

			runtimeFieldsDrawer.Draw();

			#if DEV_MODE
			Profiler.EndSample();
			#endif
		}

		private void OnDisable()
		{
			AnyPropertyDrawer.DisposeAllStaticState();
			EditorApplication.update -= OnUpdate;
			EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

			DestroyWrappedEditors();
		}

		private void DestroyWrappedEditors()
		{
			if(initializerGUI != null)
			{
				initializerGUI.Dispose();
				initializerGUI = null;
			}

			DestroyIfNotNull(ref internalEditor);
		}

		private void DestroyIfNotNull(ref Editor editor)
		{
			if(editor != null)
			{
				DestroyImmediate(editor);
			}

			editor = null;
		}

		private void OnUpdate()
		{
			if(Application.isPlaying && runtimeFieldsDrawer != null)
			{
				Repaint();
			}
		}

		private void HandleBeginGUI()
		{
			LayoutUtility.BeginGUI(this);
			HandleSetup();
		}

		private void HandleSetup()
		{
			if(!setupDone || !initializerGUI.IsValid())
			{
				SetupOrExitGUI();
			}
		}

		private void SetupOrExitGUI()
		{
			if(Event.current.type == EventType.Layout)
			{
				DisposeInitializerGUI();
				Setup();
				return;
			}
			
			SetupDuringNextOnGUI();
		}

		private void SetupDuringNextOnGUI()
		{
			setupDone = false;
			Repaint();
			LayoutUtility.ExitGUI(this);
		}

		private void OnOwnedInitializerGUIChanged(InitializerGUI initializerGUI)
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.OnInitializerGUIChanged with Event.current:{Event.current?.type.ToString() ?? "None"}");
			#endif

			setupDone = false;
			Repaint();
		}

		private void CreateInternalEditor()
		{
			var editorType = CustomEditorType.Get(target.GetType(), targets.Length > 0);
			CreateCachedEditor(targets, editorType, ref internalEditor);
			internalEditorIsGenericInspector = string.Equals(internalEditor.GetType().FullName, "UnityEditor.GenericInspector");
		}

		private void DisposeInitializerGUI()
		{
			if(initializerGUI is null)
			{
				return;
			}

			initializerGUI.Changed -= OnOwnedInitializerGUIChanged;
			initializerGUI.Dispose();
			initializerGUI = null;
		}
	}
}