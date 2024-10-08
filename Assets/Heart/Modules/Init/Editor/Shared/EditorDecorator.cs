//#define DEBUG_DISPOSE

using System;
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

		public bool DecoratingDefaultOrOdinEditor { get; private set; }

		protected EditorDecorator(Editor decoratedEditor)
		{
			this.decoratedEditor = decoratedEditor;
			this.isResponsibleForDecoratedEditorLifetime = false;
			DecoratingDefaultOrOdinEditor = CustomEditorUtility.IsDefaultOrOdinEditor(decoratedEditor.GetType());
			targets = decoratedEditor.targets;
			target = targets.FirstOrDefault();
		}

		~EditorDecorator() => Dispose(false);

		private static EditorDecorator Create(Type decoratorType, Editor decoratedEditor)
		{
			var constructor = decoratorType.GetConstructor(new[] { typeof(Editor) } );
			#if DEV_MODE
			if(constructor is null) Debug.LogError($"{decoratorType.Name} (for {decoratedEditor.GetType().Name}) is missing ctr(Editor)");
			#endif
			return (EditorDecorator)constructor.Invoke(new object[] { decoratedEditor });
		}

		public static bool CreateBeforeInspectorGUI(Type wrapperType, Editor wrappedEditor, out EditorDecorator beforeInspectorGUI)
		{
			if(!HasMethodOverride(wrapperType, nameof(OnBeforeInspectorGUI)))
			{
				beforeInspectorGUI = null;
				return false;
			}

			beforeInspectorGUI = Create(wrapperType, wrappedEditor);
			beforeInspectorGUI.onGUIHandler = beforeInspectorGUI.DrawBeforeInspectorGUI;
			return true;
		}

		public static bool CreateAfterInspectorGUI(Type wrapperType, Editor wrappedEditor, out EditorDecorator afterInspectorGUI)
		{
			if(!HasMethodOverride(wrapperType, nameof(OnAfterInspectorGUI)))
			{
				afterInspectorGUI = null;
				return false;
			}

			afterInspectorGUI = Create(wrapperType, wrappedEditor);
			afterInspectorGUI.onGUIHandler = afterInspectorGUI.DrawAfterInspectorGUI;
			return true;
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

		private static bool HasMethodOverride(Type wrapperType, string methodName) => wrapperType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public).DeclaringType != typeof(EditorDecorator);

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

			base.Dispose(disposeManaged);
		}

		private void DestroyIfNotNull(ref Editor editor)
		{
			if(editor)
			{
				#if DEV_MODE && DEBUG_DISPOSE
				Debug.Log(GetType().Name + "." + $"DestroyImmediate: {editor.GetType().Name} ({editor.GetInstanceID()})");
				#endif
				Object.DestroyImmediate(editor);
			}

			editor = null;
			isResponsibleForDecoratedEditorLifetime = false;
		}

		public static implicit operator bool(EditorDecorator editorDecorator) => editorDecorator is not null && editorDecorator.DecoratedEditor;
	}
}