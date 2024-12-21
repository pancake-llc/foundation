//#define DEBUG_REPAINT
//#define DEBUG_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEngine;

#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
using Unity.Profiling;
#endif

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	internal static class LayoutUtility
	{
		#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
		private static readonly ProfilerMarker repaintMarker = new(ProfilerCategory.Gui, "Sisus.Repaint");
		#endif

		public static Editor NowDrawing { get; internal set; }

		private static readonly List<Action> onNextLayoutEvent = new();
		private static readonly List<Action> onNextRepaintEvent = new();

		static LayoutUtility()
		{
			Editor.finishedDefaultHeaderGUI -= BeginGUI;
			Editor.finishedDefaultHeaderGUI += BeginGUI;
		}

		public static void BeginGUI([DisallowNull] Editor editor)
		{
			NowDrawing = editor;

			if(onNextLayoutEvent.Count > 0)
			{
				if(Event.current.type is EventType.Layout)
				{
					ApplyImmediate(onNextLayoutEvent);
				}
				else
				{
					Repaint(editor);
				}
			}

			if(onNextRepaintEvent.Count > 0)
			{
				if(Event.current.type is EventType.Repaint)
				{
					ApplyImmediate(onNextRepaintEvent);
				}
				else
				{
					Repaint(editor);
				}
			}
		}

		public static void EndGUI([DisallowNull] Editor editor)
		{
			if(onNextLayoutEvent.Count <= 0)
			{
				return;
			}

			if(Event.current.type is not EventType.Repaint)
			{
				Repaint(editor);
				return;
			}

			ApplyImmediate(onNextLayoutEvent);
		}

		public static void ExitGUI()
		{
			if(Event.current is null)
			{
				#if DEV_MODE
				Debug.LogWarning($"{nameof(LayoutUtility)}.{nameof(ExitGUI)} with Event.current:None.");
				#endif

				return;
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{nameof(LayoutUtility)}.{nameof(ExitGUI)} with Event.current:{Event.current.type}.");
			#endif

			NowDrawing = null;
			GUIUtility.ExitGUI();
		}

		public static void OnNestedInspectorGUI([DisallowNull] this Editor editor)
		{
			var parentEditor = NowDrawing;

			editor.OnBeforeNestedInspectorGUI();

			try
			{
				if(CustomEditorUtility.GenericInspectorType.IsInstanceOfType(editor))
				{
					editor.serializedObject.DrawPropertiesWithoutScriptField();
				}
				else
				{
					editor.OnInspectorGUI();
				}
			}
			catch(ArgumentException e) when (ShouldHideExceptionFromUsers(e))
			{
				#if DEV_MODE
				Debug.LogWarning(e);
				#endif
			}
			finally
			{
				editor.OnAfterNestedInspectorGUI(parentEditor);
			}
		}

		public static void OnBeforeNestedInspectorGUI([DisallowNull] this Editor editor)
		{
			BeginGUI(editor);
			editor.serializedObject.Update();
		}

		public static void OnAfterNestedInspectorGUI([DisallowNull] this Editor editor, Editor parentEditor)
		{
			editor.serializedObject.ApplyModifiedProperties();

			EndGUI(editor);

			NowDrawing = parentEditor;
		}

		public static void OnLayoutEvent([DisallowNull] Action action, Editor editor = null, ExecutionOptions executionOptions = ExecutionOptions.Default)
		{
			EventType eventType = Event.current?.type ?? EventType.Ignore;
			if(eventType == EventType.Layout)
			{
				if(executionOptions.HasFlag(ExecutionOptions.CanBeExecutedImmediately))
				{
					action();
					return;
				}
			}
			else if(executionOptions.HasFlag(ExecutionOptions.ExitGUIIfWrongEvent))
			{
				Repaint(editor);
				ExitGUI();
				return;
			}

			if(!executionOptions.HasFlag(ExecutionOptions.AllowDuplicates))
			{
				onNextLayoutEvent.Remove(action);
			}

			onNextLayoutEvent.Add(action);
			Repaint(editor);
		}

		public static void OnRepaintEvent([DisallowNull] Action action, Editor editor = null, ExecutionOptions executionOptions = ExecutionOptions.Default)
		{
			EventType eventType = Event.current?.type ?? EventType.Ignore;
			if(eventType == EventType.Layout)
			{
				if(executionOptions.HasFlag(ExecutionOptions.CanBeExecutedImmediately))
				{
					action();
					return;
				}
			}
			else if(executionOptions.HasFlag(ExecutionOptions.ExitGUIIfWrongEvent))
			{
				Repaint(editor);
				ExitGUI();
				return;
			}

			if(!executionOptions.HasFlag(ExecutionOptions.AllowDuplicates))
			{
				onNextLayoutEvent.Remove(action);
			}

			onNextLayoutEvent.Add(action);
			Repaint(editor);
		}

		private static void ApplyImmediate(List<Action> deferredActions) //, out bool exitGUI)
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{nameof(LayoutUtility)}.{nameof(ApplyImmediate)}({deferredActions[0].Method?.Name ?? deferredActions[0].ToString()}) with Event.current:{Event.current.type}.");
			#endif

			Action[] actions = deferredActions.ToArray();
			deferredActions.Clear();
			Exception deferredException = null;

			for(int index = 0, count = actions.Length; index < count; index++)
			{
				Action action = actions[index];
				try
				{
					action();
				}
				catch(ArgumentException e) when(ShouldHideExceptionFromUsers(e))
				{
					#if DEV_MODE
					Debug.LogException(e);
					#endif
				}
				catch(ExitGUIException e) when(index < count - 1)
				{
					deferredException = e;
				}
				catch(Exception exception)
				{
					Debug.LogException(exception);
				}
			}

			if(deferredException is not null)
			{
				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"{deferredException.GetType().Name} with Event.current:{Event.current.type}.");
				#endif
				throw deferredException;
			}
		}

		public static void Repaint([AllowNull] Editor editor = null)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = repaintMarker.Auto();
			#endif

			GUI.changed = true;

			if(editor)
			{
				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(editor.GetType().Name + ".Repaint");
				#endif

				editor.Repaint();

				if(NowDrawing != editor && NowDrawing)
				{
					#if DEV_MODE && DEBUG_REPAINT
					Debug.Log(NowDrawing.GetType().Name + ".Repaint");
					#endif

					NowDrawing.Repaint();
				}
			}
			else if(NowDrawing)
			{
				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(NowDrawing.GetType().Name + ".Repaint");
				#endif

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

		private static bool ShouldHideExceptionFromUsers([DisallowNull] ArgumentException exception)
			=> exception.Message.EndsWith("controls when doing repaint");
	}
}