//#define DEBUG_DISPOSE

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Sisus.Shared.EditorOnly
{
	/// <summary>
	/// Base class for an object that can draw GUI before and/or after an Editor in the Inspector.
	/// </summary>
	public abstract class EditorDecorator : IMGUIContainer
	{
		public readonly Object[] targets;
		public readonly Object target;

		public SerializedObject SerializedObject => decoratedEditor ? decoratedEditor.m_SerializedObject : null;
		public Editor DecoratedEditor => decoratedEditor;

		[NonSerialized] private Editor decoratedEditor;
		[NonSerialized] private bool isResponsibleForDecoratedEditorLifetime;
		[SerializeField] private bool managedDisposed;

		public bool DecoratingDefaultOrOdinEditor { get; private set; }

		protected EditorDecorator(Editor decoratedEditor)
		{
			#if DEV_MODE && DEBUG_DISPOSE
			Debug.Log($"{GetType().Name}.Ctr({decoratedEditor.target.GetType().Name})");
			#endif

			this.decoratedEditor = decoratedEditor;
			this.isResponsibleForDecoratedEditorLifetime = false;
			DecoratingDefaultOrOdinEditor = CustomEditorUtility.IsDefaultOrOdinEditor(decoratedEditor.GetType());
			targets = decoratedEditor.targets;
			target = targets.FirstOrDefault();
		}

		~EditorDecorator()
		{
			#if DEV_MODE
			Debug.LogWarning($"{GetType().Name} Finalizer was called. Did you forget to call Dispose()?");
			#endif

			Dispose(false);
		}

		private static EditorDecorator Create(Type decoratorType, Editor decoratedEditor)
		{
			var constructor = decoratorType.GetConstructor(new[] { typeof(Editor) } );
			#if DEV_MODE
			if(constructor is null) Debug.LogError($"{decoratorType.Name} (for {decoratedEditor.GetType().Name}) is missing ctr(Editor)");
			#endif
			return (EditorDecorator)constructor.Invoke(new object[] { decoratedEditor });
		}

		public static bool ShouldCreateBeforeInspectorGUI([DisallowNull] Type wrapperType) => HasMethodOverride(wrapperType, nameof(OnBeforeInspectorGUI));

		public static EditorDecorator CreateBeforeInspectorGUI([DisallowNull] Type wrapperType, Editor wrappedEditor)
		{
			var beforeInspectorGUI = Create(wrapperType, wrappedEditor);
			beforeInspectorGUI.onGUIHandler = beforeInspectorGUI.DrawBeforeInspectorGUI;
			return beforeInspectorGUI;
		}

		public static bool ShouldCreateAfterInspectorGUI([DisallowNull] Type wrapperType) => HasMethodOverride(wrapperType, nameof(OnAfterInspectorGUI));

		public static EditorDecorator CreateAfterInspectorGUI([DisallowNull] Type wrapperType, Editor wrappedEditor)
		{
			var afterInspectorGUI = Create(wrapperType, wrappedEditor);
			afterInspectorGUI.onGUIHandler = afterInspectorGUI.DrawAfterInspectorGUI;
			return afterInspectorGUI;
		}

		private void DrawBeforeInspectorGUI()
		{
			GUILayout.Space(2f);

			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
			{
				GUI.changed = false;

				try
				{
					OnBeforeInspectorGUI();
				}
				catch (Exception e)
				{
					if (e is ExitGUIException)
					{
						throw;
					}

					Debug.LogException(e);
				}
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawAfterInspectorGUI()
		{
			GUILayout.Space(2f);

			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
			{
				GUI.changed = false;

				try
				{
					OnAfterInspectorGUI();
				}
				catch (Exception e)
				{
					if (e is ExitGUIException)
					{
						throw;
					}

					Debug.LogException(e);
				}
			}
			EditorGUILayout.EndVertical();
		}

		private static bool HasMethodOverride(Type wrapperType, string methodName)
		{
			if(wrapperType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public) is { } method)
			{
				return method.DeclaringType != typeof(EditorDecorator);
			}

			#if DEV_MODE
			Debug.LogWarning($"Method '{methodName}' was not found on {wrapperType}.");
			#endif

			return false;
		}

		public virtual void OnBeforeInspectorGUI() { }

		public virtual void OnAfterInspectorGUI() { }

		public bool IsValid() => decoratedEditor && decoratedEditor.m_SerializedObject.IsValid();

		protected override void Dispose(bool disposeManaged)
		{
			#if DEV_MODE && DEBUG_DISPOSE
			Debug.Log($"{GetType().Name}.Dispose({disposeManaged})");
			#endif

			if(isResponsibleForDecoratedEditorLifetime)
			{
				DestroyIfNotNull(ref decoratedEditor);
			}

			if(managedDisposed)
			{
				return;
			}

			managedDisposed = disposeManaged;
			base.Dispose(disposeManaged);
			GC.SuppressFinalize(this);
		}

		protected virtual void OnDestroyingDecoratedEditor(Editor editor) { }

		private void DestroyIfNotNull(ref Editor editor)
		{
			if(editor)
			{
				#if DEV_MODE && DEBUG_DISPOSE
				Debug.Log(GetType().Name + "." + $"DestroyImmediate: {editor.GetType().Name} ({editor.GetInstanceID()})");
				#endif
				OnDestroyingDecoratedEditor(editor);
				Object.DestroyImmediate(editor);
			}

			editor = null;
			isResponsibleForDecoratedEditorLifetime = false;
		}

		public static implicit operator bool(EditorDecorator editorDecorator) => editorDecorator is not null && editorDecorator.DecoratedEditor;
	}
}