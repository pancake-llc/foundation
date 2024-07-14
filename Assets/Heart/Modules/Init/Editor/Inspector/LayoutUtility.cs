//#define DEBUG_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	public static class LayoutUtility
	{
		public static ExitGUIInfo ExitingGUI { get; private set; }
		public static Editor NowDrawing { get; private set; }

		private static readonly List<Action> deferredActions = new();

		static LayoutUtility()
		{
			Editor.finishedDefaultHeaderGUI -= BeginGUI;
			Editor.finishedDefaultHeaderGUI += BeginGUI;
		}

		public static void BeginGUI([DisallowNull] Editor editor)
		{
			ExitingGUI = ExitGUIInfo.None;
			NowDrawing = editor;

			if(deferredActions.Count > 0)
			{
				if(Event.current.type is not EventType.Layout)
				{
					Repaint(editor);
					return;
				}

				ApplyImmediate(deferredActions, out bool exitGUI);

				if(exitGUI)
				{
					ExitGUI(editor);
				}
			}
		}

		public static void EndGUI([DisallowNull] Editor editor)
		{
			if(deferredActions.Count <= 0)
			{
				return;
			}

			if(Event.current.type is not EventType.Repaint)
			{
				Repaint(editor);
				return;
			}

			ApplyImmediate(deferredActions, out bool exitGUI);

			if(exitGUI)
			{
				ExitGUI(editor);
			}
		}

		public static void ExitGUI([AllowNull] Editor editor)
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{nameof(LayoutUtility)}.{nameof(ExitGUI)} with Event.current:{Event.current?.type.ToString() ?? "None"}.");
			#endif

			ExitingGUI = new(editor ?? NowDrawing);
			NowDrawing = null;
			GUIUtility.ExitGUI();
		}

		public static void HideScriptReferenceField() => GUILayout.Space(-EditorGUIUtility.singleLineHeight - 1f);

		public static void OnNestedInspectorGUI([DisallowNull] this Editor editor)
		{
			bool hideScriptReference = CustomEditorUtility.genericInspectorType.IsInstanceOfType(editor);
			editor.OnNestedInspectorGUI(hideScriptReference);
		}

		public static void OnNestedInspectorGUI([DisallowNull] this Editor editor, bool hideScriptReference)
		{
			Editor parentEditor = NowDrawing;
			
			editor.OnBeforeNestedInspectorGUI(hideScriptReference);

			try
			{
				editor.OnInspectorGUI();
			}
			catch(Exception exception)
			{
				if(IsExitGUIException(exception))
				{
					ExitingGUI = new(editor);
				}
				else if(ShouldHideExceptionFromUsers(exception))
				{
					#if DEV_MODE
					Debug.LogException(exception);
					#endif
				}
				else
				{
					Debug.LogException(exception);
				}
			}

			editor.OnAfterNestedInspectorGUI(parentEditor);
		}

		public static void OnBeforeNestedInspectorGUI([DisallowNull] this Editor editor, bool hideScriptReference)
		{
			BeginGUI(editor);

			editor.serializedObject.Update();

			if(hideScriptReference)
			{
				HideScriptReferenceField();
			}
		}

		public static void OnAfterNestedInspectorGUI([DisallowNull] this Editor editor, Editor parentEditor)
		{
			editor.serializedObject.ApplyModifiedProperties();

			EndGUI(editor);

			NowDrawing = parentEditor;

			if(ExitingGUI)
			{
				ExitGUI(parentEditor);
			}
		}

		public static void ApplyWhenSafe([DisallowNull] Action action, Editor editor = null, ExecutionOptions executionOptions = ExecutionOptions.Default)
		{
			EventType eventType = Event.current?.type ?? EventType.Ignore;
			if(eventType == EventType.Layout)
			{
				if(executionOptions.HasFlag(ExecutionOptions.ExecuteImmediateIfLayoutEvent))
				{
					action();
					return;
				}
			}
			else if(executionOptions.HasFlag(ExecutionOptions.ExitGUIIfNotLayoutEvent))
			{
				Repaint(editor);
				ExitGUI(editor);
				return;
			}

			if(!executionOptions.HasFlag(ExecutionOptions.AllowDuplicates))
			{
				deferredActions.Remove(action);
			}

			deferredActions.Add(action);
			Repaint(editor);
		}

		private static void ApplyImmediate(List<Action> deferredActions, out bool exitGUI)
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{nameof(LayoutUtility)}.{nameof(ApplyImmediate)}({deferredActions[0].Method?.Name ?? deferredActions[0].ToString()}) with Event.current:{Event.current.type}.");
			#endif

			Action[] actions = deferredActions.ToArray();
			deferredActions.Clear();
			exitGUI = false;

			foreach(var action in actions)
			{
				try
				{
					action();
				}
				catch(Exception exception)
				{
					if(IsExitGUIException(exception))
					{
						exitGUI = true;
					}
					else if(ShouldHideExceptionFromUsers(exception))
					{
						#if DEV_MODE
						Debug.LogException(exception);
						#endif
					}
					else
					{
						Debug.LogException(exception);
					}
				}
			}
		}

		public static void Repaint([AllowNull] Editor editor = null)
		{
			GUI.changed = true;

			if(editor != null)
			{
				editor.Repaint();

				if(NowDrawing != editor && NowDrawing != null)
				{
					NowDrawing.Repaint();
				}
			}
			else if(NowDrawing != null)
			{
				NowDrawing.Repaint();
			}
			else
			{
				InspectorContents.Repaint();
			}
		}

		private static bool IsExitGUIException([DisallowNull] Exception exception)
		{
			while(exception is TargetInvocationException && exception.InnerException != null)
			{
				exception = exception.InnerException;
			}

			return exception is ExitGUIException;
		}

		private static bool ShouldHideExceptionFromUsers([DisallowNull] Exception exception)
			=> exception is ArgumentException && exception.Message.EndsWith("controls when doing repaint");
	}
}