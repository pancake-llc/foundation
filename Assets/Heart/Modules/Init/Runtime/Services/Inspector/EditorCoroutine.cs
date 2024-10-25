#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents a coroutine that has been started running in the editor.
	/// <para>
	/// Also offers static methods for <see cref="Start">starting</see> and <see cref="Stop">stopping</see> coroutines.
	/// </para>
	/// </summary>
	internal sealed class EditorCoroutine : YieldInstruction
	{
		public static event Action<EditorCoroutine> Stopped;

		private static readonly List<EditorCoroutine> running = new(1);
		private static readonly FieldInfo waitForSecondsSecondsField = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo invokeCompletionEventMethod = typeof(AsyncOperation).GetMethod("InvokeCompletionEvent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		private readonly Stack<object> yielding = new(1);
		private double waitUntil = 0d;

		public bool IsFinished => yielding.Count == 0;

		static EditorCoroutine()
		{
			waitForSecondsSecondsField = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if(waitForSecondsSecondsField is null)
			{
				Debug.LogWarning("Field WaitForSeconds.m_Seconds not found.");
			}

			invokeCompletionEventMethod = typeof(AsyncOperation).GetMethod("InvokeCompletionEvent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if(invokeCompletionEventMethod is null)
			{
				Debug.LogWarning("Method AsyncOperation.InvokeCompletionEvent not found.");
			}
		}

		private EditorCoroutine(IEnumerator routine) => yielding.Push(routine);

		/// <summary>
		/// Starts the provided <paramref name="coroutine"/>.
		/// </summary>
		/// <param name="coroutine"> The coroutine to start. </param>
		/// <returns>
		/// A reference to the started <paramref name="coroutine"/>.
		/// <para>
		/// This reference can be passed to <see cref="Stop"/> to stop
		/// the execution of the coroutine.
		/// </para>
		/// </returns>
		public static EditorCoroutine Start(IEnumerator coroutine)
		{
			if(running.Count == 0)
			{
				EditorApplication.update += UpdateRunningCoroutines;
			}

			var editorCoroutine = new EditorCoroutine(coroutine);
			running.Add(editorCoroutine);
			return editorCoroutine;
		}

		/// <summary>
		/// Stops the <paramref name="coroutine"/> that is running in edit mode.
		/// </summary>
		/// <param name="coroutine">
		/// Reference to the editor <see cref="IEnumerator">coroutine</see> to stop.
		/// <para>
		/// This is the reference that was returned by <see cref="Start"/>
		/// when the coroutine was started.
		/// </para>
		/// </param>
		public void Stop()
		{
			running.Remove(this);

			if(running.Count == 0)
			{
				EditorApplication.update -= UpdateRunningCoroutines;
			}

			Stopped?.Invoke(this);
		}

		/// <summary>
		/// Stops the <paramref name="coroutine"/> that is running.
		/// <para>
		/// If <see langword="this"/> is an object wrapped by a <see cref="Wrapper{TObject}"/> then
		/// the <paramref name="coroutine"/> is started on the wrapper behaviour.
		/// </para>
		/// <para>
		/// If <see langword="this"/> is a <see cref="MonoBehaviour"/> then the <paramref name="coroutine"/>
		/// is started on the object directly.
		/// </para>
		/// <para>
		/// Otherwise the <paramref name="coroutine"/> is started on an <see cref="Updater"/> instance.
		/// </para>
		/// </summary>
		/// <param name="coroutine"> The <see cref="IEnumerator">coroutine</see> to stop. </param>
		public static void Stop(IEnumerator coroutine)
		{
			foreach(var editorCoroutine in running)
			{
				int counter = editorCoroutine.yielding.Count;

				foreach(var item in editorCoroutine.yielding)
				{
					if(counter == 1 && item == coroutine)
					{
						editorCoroutine.Stop();
						return;
					}

					counter--;
				}
			}
		}

		/// <summary>
		/// Stops all coroutines that have been started using <see cref="Start"/> that are currently still running.
		/// </summary>
		public static void StopAll()
		{
			running.Clear();
			EditorApplication.update -= UpdateRunningCoroutines;
		}

		/// <summary>
		/// Continuously advances all currently running coroutines to their
		/// next phases until all of them have reached the end.
		/// <para>
		/// Note that this locks the current thread until all running coroutines have fully finished.
		/// If any coroutine contains <see cref="CustomYieldInstruction">CustomYieldInstructions</see>
		/// that take a long time to finish (or never finish in edit mode) this can cause the editor
		/// to freeze for the same duration.
		/// </para>
		/// </summary>
		public static void MoveAllToEnd()
		{
			int count = running.Count;
			while(count > 0)
			{
				for(int i = count - 1; i >= 0; i--)
				{
					running[i].MoveNext(true);
				}

				count = running.Count;
			}
		}

		/// <summary>
		/// Advances all currently running coroutines to their next phase.
		/// </summary>
		/// <param name="skipWaits">
		/// (Optional) If <see langword="true"/> then yield instructions
		/// <see cref="WaitForSeconds"/> and <see cref="WaitForSecondsRealtime"/> are skipped.
		/// </param>
		/// <returns> <see langword="true"/> if any coroutines are still running, <see langword="false"/> if all have finished. </returns>
		public static bool MoveAllNext(bool skipWaits = false)
		{
			for(int i = running.Count - 1; i >= 0; i--)
			{
				running[i].MoveNext(skipWaits);
			}

			return running.Count > 0;
		}

		private static void UpdateRunningCoroutines()
		{
			for(int i = running.Count - 1; i >= 0; i--)
			{
				running[i].MoveNext();
			}
		}

		/// <summary>
		/// Advances the coroutine to the next phase.
		/// </summary>
		/// <param name="skipWaits">
		/// (Optional) If <see langword="true"/> then yield instructions
		/// <see cref="WaitForSeconds"/> and <see cref="WaitForSecondsRealtime"/> are skipped.
		/// </param>
		/// <returns> <see langword="true"/> if coroutine is still running, <see langword="false"/> if it has finished. </returns>
		public bool MoveNext(bool skipWaits = false)
		{
			if(EditorApplication.timeSinceStartup < waitUntil && !skipWaits)
			{
				return true;
			}

			if(yielding.Count == 0)
			{
				Stop();
				return false;
			}

			var current = yielding.Peek();
			if(current is IEnumerator enumerator)
			{
				bool keepWaiting;
				try
				{
					keepWaiting = enumerator.MoveNext();
				}
				catch
				{
					keepWaiting = true;
				}

				if(!keepWaiting)
				{
					yielding.Pop();
					if(yielding.Count > 0)
					{
						return true;
					}

					Stop();
					return false;
				}

				yielding.Push(enumerator.Current);
				return true;
			}
			else if(current is CustomYieldInstruction yieldInstruction)
			{
				bool keepWaiting;
				try
				{
					keepWaiting = yieldInstruction.keepWaiting;
				}
				catch
				{
					keepWaiting = true;
				}

				if(!skipWaits)
				{
					if(!keepWaiting)
					{
						yielding.Pop();
					}

					return true;
				}
			}
			else if(current is WaitForSeconds waitForSeconds)
			{
				waitUntil = EditorApplication.timeSinceStartup + (float)waitForSecondsSecondsField.GetValue(waitForSeconds);

				if(!skipWaits)
				{
					yielding.Pop();
					return true;
				}
			}
			else if(current is WaitForSecondsRealtime waitForSecondsRealtime)
			{
				waitUntil = EditorApplication.timeSinceStartup + waitForSecondsRealtime.waitTime;
				if(!skipWaits)
				{
					yielding.Pop();
					return true;
				}
			}
			else if(current is WaitForEndOfFrame or WaitForFixedUpdate)
			{
				if(!skipWaits)
				{
					yielding.Pop();
					return true;
				}
			}
			else if(current is AsyncOperation asyncOperation && !asyncOperation.isDone)
			{
				if(!skipWaits)
				{
					return true;
				}

				invokeCompletionEventMethod?.Invoke(asyncOperation, null);
			}

			yielding.Pop();
			if(yielding.Count > 0)
			{
				return true;
			}

			Stop();
			return false;
		}

		/// <summary>
		/// Continuously advances the coroutine to the next phase until it has reached the end.
		/// <para>
		/// Note that this locks the current thread until the coroutine has fully finished.
		/// If the coroutine contains <see cref="CustomYieldInstruction">CustomYieldInstructions</see>
		/// that take a long time to finish (or never finish in edit mode) this can cause the editor
		/// to freeze for the same duration.
		/// </para>
		/// </summary>
		public void MoveToEnd()
		{
			while(MoveNext(true));
		}

		public bool Equals(IEnumerator coroutine)
		{
			int counter = yielding.Count;
			foreach(var item in yielding)
			{
				if(counter == 1 && item == coroutine)
				{
					return true;
				}

				counter--;
			}

			return false;
		}
	}
}
#endif