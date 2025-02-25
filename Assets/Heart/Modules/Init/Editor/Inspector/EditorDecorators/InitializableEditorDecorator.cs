//#define DEBUG_DISPOSE
//#define DEBUG_SETUP_DURATION
//#define DEBUG_ENABLED

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Sisus.Init.EditorOnly.Internal;
using Sisus.Init.Internal;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditor.Callbacks;
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
	public class InitializableEditorDecorator : EditorDecorator
	{
		[NonSerialized] private bool setupDone;
		[NonSerialized] private bool preOnGUISetupDone;
		[NonSerialized] private InitializerGUI initializerGUI;
		[NonSerialized] private RuntimeFieldsDrawer runtimeFieldsDrawer;
		private bool drawInitSection;
		private IVisualElementScheduledItem scheduledItem;

		protected bool ShowRuntimeFields { get; set; }

		public InitializableEditorDecorator(Editor decoratedEditor) : base(decoratedEditor)
		{
			#if DEV_MODE
			Profiler.enableAllocationCallstacks = true;
			Profiler.maxUsedMemory = 1024 * 1024 * 1024;
			#endif

			if(target)
			{
				MonoScript script = target is MonoBehaviour monoBehaviour ? MonoScript.FromMonoBehaviour(monoBehaviour) : null;
				Type type = target.GetType();

				Menu.SetChecked(ComponentMenuItems.ShowInitSection, !InitializerGUI.IsInitSectionHidden(script, type));
			}
		}

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
		/// Also, when drawing runtime fields in the base type and types it derives from will be skipped.
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

				if(baseTypeDefinition.IsGenericTypeDefinition)
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
				#if DEV_MODE
				Debug.LogWarning($"\"{value}\".IndexOf(':'): {i}");
				#endif
				return;
			}

			string initializerAssetGuid = value.Substring(0, i);
			string initializerAssetPath = AssetDatabase.GUIDToAssetPath(initializerAssetGuid);
			if(string.IsNullOrEmpty(initializerAssetPath))
			{
				#if DEV_MODE
				Debug.LogWarning($"GUIDToAssetPath({initializerAssetGuid}) returned null or empty");
				#endif
				return;
			}

			var initializerScript = AssetDatabase.LoadAssetAtPath<MonoScript>(initializerAssetPath);
			if(!initializerScript)
			{
				#if DEV_MODE
				Debug.LogWarning($"LoadAssetAtPath<MonoScript>({initializerAssetPath}) returned null");
				#endif
				return;
			}

			var initializerType = initializerScript.GetClass();
			if(initializerType is null)
			{
				#if DEV_MODE
				Debug.LogWarning($"{initializerScript.name}.GetClass() was null");
				#endif
				return;
			}

			var targetIds = value.Substring(i + 1).Split(';');
			foreach(var idString in targetIds)
			{
				if(!int.TryParse(idString, out int id))
				{
					#if DEV_MODE
					Debug.LogWarning($"!int.TryParse({idString})");
					#endif
					continue;
				}

				var target = EditorUtility.InstanceIDToObject(id);

				var component = target as Component;
				if(component)
				{
					var gameObject = component.gameObject;
					var initializerComponent = gameObject.GetComponent(initializerType);
					if(!initializerComponent || initializerComponent is not IInitializer initializer || !initializer.TargetIsAssignableOrConvertibleToType(component.GetType()) || initializer.Target)
					{
						continue;
					}

					initializer.Target = component;
					continue;
				}

				var scriptableObject = target as ScriptableObject;
				if(scriptableObject && typeof(IInitializer).IsAssignableFrom(initializerType) && typeof(ScriptableObject).IsAssignableFrom(initializerType))
				{
					var initializer = ScriptableObject.CreateInstance(initializerType) as IInitializer;
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
			var initParameterTypes = InitializerEditorUtility.GetInitParameterTypes(firstTarget);

			DisposeInitializerGUI();

			#if DEV_MODE
			Debug.Assert(SerializedObject is not null, "SerializedObject is null");
			Debug.Assert(targets is not null, "targets is null");
			Debug.Assert(initParameterTypes is not null, "initParameterTypes is null");
			#endif

			initializerGUI = new InitializerGUI(SerializedObject, targets.Select(GetInitializable).ToArray(), initParameterTypes);
			initializerGUI.Changed += OnOwnedInitializerGUIChanged;

			if(!preOnGUISetupDone)
			{
				PreOnGUISetup(baseType);
			}

			Repaint();

			#if DEV_MODE
			Profiler.EndSample();
			#if DEBUG_SETUP_DURATION
			timer.Stop();
			Debug.Log(GetType().Name + ".Setup took " + timer.Elapsed.TotalSeconds + "s.");
			#endif
			#endif
		}

		private void Repaint() => LayoutUtility.Repaint(DecoratedEditor);
		
		private void PreOnGUISetup(Type baseType)
		{
			preOnGUISetupDone = true;
			
			ShowRuntimeFields = ShouldShowRuntimeFields();

			if(ShowRuntimeFields)
			{
				runtimeFieldsDrawer = new(target, baseType);

				// In Play Mode we need to call Repaint occasionally so that the Runtime Fields GUI
				// reflects the current state of any values that might be changing.
				scheduledItem?.Pause();
				scheduledItem = schedule.Execute(RepaintIfDrawingRuntimeFields).Every(intervalMs: 500);
			}

			drawInitSection = ShouldDrawInitSection();
		}
		
		protected virtual bool ShouldShowRuntimeFields() => target is MonoBehaviour and (IOneArgument or ITwoArguments or IThreeArguments or IFourArguments or IFiveArguments or ISixArguments or ISevenArguments or IEightArguments or INineArguments or ITenArguments or IElevenArguments or ITwelveArguments);
		
		protected virtual bool ShouldDrawInitSection() => 
			// Always draw the Init section if the client has an initializer attached
			InitializerUtility.HasInitializer(target)
			// Otherwise draw it the user has not disabled initializer visibility via the context menu for the client type
			|| !InitializerGUI.IsInitSectionHidden(target);

		protected virtual object GetInitializable(Object inspectedTarget) => inspectedTarget;

		[Pure]
		protected virtual RuntimeFieldsDrawer CreateRuntimeFieldsDrawer() => new(target, BaseType);

		public override void OnBeforeInspectorGUI()
		{
			#if DEV_MODE
			Profiler.BeginSample("InitializableEditorDecorator.OnBeforeInspectorGUI");
			#endif

			if(OnBeginGUI())
			{
				DrawInitializerGUI();
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

			LayoutUtility.EndGUI(DecoratedEditor);

			#if DEV_MODE
			Profiler.EndSample();
			#endif
		}

		public override void OnAfterInspectorGUI()
		{
			if(!OnBeginGUI())
			{
				return;
			}

			if(ShowRuntimeFields)
			{
				#if DEV_MODE
				Profiler.BeginSample("InitializableEditor.RuntimeFieldsGUI");
				#endif

				if(runtimeFieldsDrawer is null)
				{
					runtimeFieldsDrawer = CreateRuntimeFieldsDrawer();
				}

				runtimeFieldsDrawer.Draw();

				#if DEV_MODE
				Profiler.EndSample();
				#endif
			}

			LayoutUtility.EndGUI(DecoratedEditor);
		}

		protected override void Dispose(bool disposeManaged)
		{
			AnyPropertyDrawer.DisposeAllStaticState();
			scheduledItem?.Pause();
			DisposeInitializerGUI();
			base.Dispose(disposeManaged);
		}

		private void RepaintIfDrawingRuntimeFields()
		{
			if(Application.isPlaying && runtimeFieldsDrawer != null)
			{
				Repaint();
			}
		}

		private bool OnBeginGUI()
		{
			if(!IsValid())
			{
				return false;
			}

			LayoutUtility.BeginGUI(DecoratedEditor);

			if (setupDone && initializerGUI.IsValid())
			{
				return true;
			}

			if(Event.current.type != EventType.Layout)
			{
				SetupDuringNextOnGUI();
				return false;
			}

			Setup();
			return true;

			void SetupDuringNextOnGUI()
			{
				setupDone = false;
				Repaint();
				LayoutUtility.ExitGUI();
			}
		}

		private void OnOwnedInitializerGUIChanged(InitializerGUI initializerGUI)
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.OnInitializerGUIChanged with Event.current:{Event.current?.type.ToString() ?? "None"}");
			#endif

			setupDone = false;
			Repaint();
		}

		private void DisposeInitializerGUI()
		{
			if(initializerGUI is null)
			{
				return;
			}

			#if DEV_MODE && DEBUG_DISPOSE
			Debug.Log($"DisposeInitializerGUI({initializerGUI.InitializerEditor?.GetType().Name})");
			#endif

			initializerGUI.Changed -= OnOwnedInitializerGUIChanged;
			initializerGUI.Dispose();
			initializerGUI = null;
		}

		protected override void OnDestroyingDecoratedEditor(Editor editor) => EditorDecoratorInjector.RemoveFrom(editor, ExecutionOptions.CanBeExecutedImmediately);
	}
}