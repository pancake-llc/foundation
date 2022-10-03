using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Init.EditorOnly
{
	/// <summary>
	/// Base class for custom editors for initializable targets that visualize non-serialized fields of the target in play mode.
	/// </summary>
	public abstract class InitializableEditor : UnityEditor.Editor
    {
		private static readonly string[] scriptField = new string[] { "m_Script" };

        [NonSerialized]
        private RuntimeFieldsDrawer runtimeFieldsDrawer;
		[NonSerialized]
		private InitializerDrawer initializerDrawer;
		private bool drawInitializerGUI;

		private UnityEditor.Editor internalEditor;
		[SerializeField]
		private bool internalEditorIsGenericInspector;

		protected bool ShowRuntimeFields => true;

		/// <summary>
		/// The generic type definition of the Initializable base class from which the Editor target derives from,
		/// or if that is not known, then an IInitializable<T...> interface the target implements.
		/// <para>
		/// This is used to determine what the <see cref="BaseType"/> for the target component.
		/// </para>
		/// </summary>
		protected abstract Type BaseTypeDefinition { get; }

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

                    return typeof(MonoBehaviour);
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
			if(!EditorPrefs.HasKey(InitializerDrawer.SetInitializerTargetOnScriptsReloadedKey))
			{
				return;
			}

			var value = EditorPrefs.GetString(InitializerDrawer.SetInitializerTargetOnScriptsReloadedKey);
			EditorPrefs.DeleteKey(InitializerDrawer.SetInitializerTargetOnScriptsReloadedKey);

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
            if(initializerScript == null)
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

                var target = EditorUtility.InstanceIDToObject(id) as Component;
                if(target == null)
				{
                    continue;
				}

                var gameObject = target.gameObject;
                var initializerComponent = gameObject.GetComponent(initializerType);
                if(initializerComponent == null || !(initializerComponent is IInitializer initializer) || !initializer.TargetIsAssignableOrConvertibleToType(target.GetType()) || initializer.Target != null)
				{
                    continue;
				}

                initializer.Target = target;
            }
		}

		protected virtual void OnEnable()
        {
			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;
			EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
			EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
		}

		private void OnPlaymodeStateChanged(PlayModeStateChange stateChange)
		{
			switch(stateChange)
			{
				case PlayModeStateChange.ExitingEditMode:
				case PlayModeStateChange.EnteredPlayMode:
					if(initializerDrawer != null)
					{
						initializerDrawer.Dispose();
						initializerDrawer = null;
					}
					break;
			}
		}

		private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] compilerMessages)
		{
			string assemblyName = Path.GetFileName(assemblyPath);

			if(internalEditor != null && string.Equals(assemblyName, Path.GetFileName(internalEditor.GetType().Assembly.Location)))
			{
				DestroyImmediate(internalEditor);
			}

			InitializerDrawer.OnAssemblyCompilationStarted(ref initializerDrawer, assemblyName);
		}

		protected virtual void Setup()
		{
			var baseType = BaseType;
			var initParameterTypes = baseType is null || !baseType.IsGenericType || baseType.IsGenericTypeDefinition ? Type.EmptyTypes : baseType.GetGenericArguments();
			var targetType = targets[0].GetType();
			initializerDrawer = new InitializerDrawer(targetType, targets, initParameterTypes);

			if(ShowRuntimeFields)
            {
                runtimeFieldsDrawer = new RuntimeFieldsDrawer(target, baseType);
            }

			#if DRAW_INIT_SECTION_WITHOUT_EDITOR
			drawInitializerGUI = false;
			#else
			// Only draw the Init bar if an Initializer class exists for the target type
			// or if the Initializable has no visible serialized fields in the Inspector.
			if(InitializerEditorUtility.GetInitializerTypes(targetType).Any())
			{
				drawInitializerGUI = true;
			}
			else
			{
				var iterator = serializedObject.GetIterator();
				drawInitializerGUI = !iterator.NextVisible(true) || !iterator.NextVisible(false);
			}
			#endif
		}

		[Pure]
		protected virtual RuntimeFieldsDrawer CreateRuntimeFieldsDrawer() => new RuntimeFieldsDrawer(target, BaseType);

		public override VisualElement CreateInspectorGUI()
		{
			if(internalEditor == null)
			{
				var editorType = InitializableEditorInjector.GetCustomEditorType(target.GetType(), targets.Length > 0);
				CreateCachedEditor(targets, editorType, ref internalEditor);
				internalEditorIsGenericInspector = string.Equals(internalEditor.GetType().FullName, "UnityEditor.GenericInspector");
			}

			var internalGUI = internalEditor.CreateInspectorGUI();
			if(internalGUI == null)
			{
				return null;
			}

			var root = new VisualElement();
			root.Add(new IMGUIContainer(InitializerGUI));
			root.Add(internalGUI);
			if(ShowRuntimeFields)
			{
				root.Add(new IMGUIContainer(RuntimeFieldsGUI));
			}
			return root;
		}

		public override void OnInspectorGUI()
		{
			InitializerGUI();
			BaseGUI();
			RuntimeFieldsGUI();
		}

		protected virtual void BaseGUI()
		{
			if(internalEditor == null)
			{
				var editorType = InitializableEditorInjector.GetCustomEditorType(target.GetType(), targets.Length > 0);
				CreateCachedEditor(targets, editorType, ref internalEditor);
				internalEditorIsGenericInspector = string.Equals(internalEditor.GetType().FullName, "UnityEditor.GenericInspector");
			}

			if(internalEditorIsGenericInspector)
			{
				serializedObject.Update();
				DrawPropertiesExcluding(serializedObject, scriptField);
				serializedObject.ApplyModifiedProperties();
			}
			else
			{
				serializedObject.ApplyModifiedProperties();
				internalEditor.serializedObject.Update();
				internalEditor.OnInspectorGUI();
				internalEditor.serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			}
		}

		protected virtual void InitializerGUI()
		{
			if(initializerDrawer is null)
			{
				Setup();
			}

			if(drawInitializerGUI)
			{
				initializerDrawer.OnInspectorGUI();
			}
		}

		protected void RuntimeFieldsGUI()
		{
			if(!ShowRuntimeFields)
			{
				return;
			}

			if(runtimeFieldsDrawer is null)
			{
				runtimeFieldsDrawer = CreateRuntimeFieldsDrawer();
			}

			runtimeFieldsDrawer.Draw();
		}

        private void OnDisable()
		{
			EditorApplication.update -= OnUpdate;
			EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

			DestroyWrappedEditors();
		}

		private void DestroyWrappedEditors()
		{
			DestroyIfNotNull(internalEditor);

			if(initializerDrawer != null)
			{
				initializerDrawer.Dispose();
				initializerDrawer = null;
			}
		}

		private void DestroyIfNotNull(UnityEditor.Editor editor)
		{
			if(editor != null)
			{
				DestroyImmediate(editor);
			}
		}

		private void OnUpdate()
		{
			if(Application.isPlaying && runtimeFieldsDrawer != null)
			{
                Repaint();
            }
		}
	}
}