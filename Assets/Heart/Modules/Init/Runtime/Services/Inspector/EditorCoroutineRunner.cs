#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Class that can be used for starting and stopping coroutines in the editor even in edit mode.
	/// <para>
	/// The class implements <see cref="ICoroutineRunner"/> and it can be used to substitute other
	/// <see cref="ICoroutineRunner">ICoroutineRunners</see> such as <see cref="Wrapper">Wrappers</see>
	/// in unit tests.
	/// </para>
	/// </summary>
	public sealed class EditorCoroutineRunner : ICoroutineRunner, IDisposable
	{
		private readonly List<EditorCoroutine> running = new(1);

		/// <summary>
		/// Starts the provided <paramref name="coroutine"/>.
		/// </summary>
		/// <param name="coroutine"> The coroutine to start. </param>
		/// <returns>
		/// Always returns <see langword="null"/>.
		/// </returns>
		public Coroutine StartCoroutine([DisallowNull] IEnumerator coroutine)
		{
			if(running.Count == 0)
			{
				EditorCoroutine.Stopped += OnSomeEditorCoroutineStopped;
			}

			running.Add(EditorCoroutine.Start(coroutine));
			return null;
		}

		/// <inheritdoc/>
		public void StopCoroutine([DisallowNull] IEnumerator coroutine)
		{
			for(int i = running.Count - 1; i >= 0; i--)
			{
				EditorCoroutine runningCoroutine = running[i];
				if(runningCoroutine.Equals(coroutine))
				{
					runningCoroutine.Stop();
					return;
				}
			}
		}

		/// <inheritdoc/>
		void ICoroutineRunner.StopCoroutine([DisallowNull] Coroutine coroutine) => Debug.LogWarning($"{nameof(EditorCoroutineRunner)}.{nameof(StopCoroutine)}({nameof(Coroutine)}) is not supported. Use {nameof(StopCoroutine)}({nameof(IEnumerator)}) instead.");

		/// <summary>
		/// Stops all coroutines that have been started using <see cref="StartCoroutine"/> that are currently running.
		/// </summary>
		public void StopAllCoroutines()
		{
			for(int i = running.Count - 1; i >= 0; i--)
			{
				running[i].Stop();
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
		public bool MoveAllNext(bool skipWaits = false)
		{
			for(int i = running.Count - 1; i >= 0; i--)
			{
				running[i].MoveNext(skipWaits);
			}

			return running.Count > 0;
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
		public void MoveAllToEnd()
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

		~EditorCoroutineRunner() => HandleDispose();

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			HandleDispose();
		}

		private void HandleDispose()
		{
			EditorCoroutine.Stopped -= OnSomeEditorCoroutineStopped;
			StopAllCoroutines();
			running.Clear();
		}

		private void OnSomeEditorCoroutineStopped(EditorCoroutine coroutine)
		{
			running.Remove(coroutine);
			if(running.Count == 0)
			{
				EditorCoroutine.Stopped -= OnSomeEditorCoroutineStopped;
			}
		}
	}
}
#endif