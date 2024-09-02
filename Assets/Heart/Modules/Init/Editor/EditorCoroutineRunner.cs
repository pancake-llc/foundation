using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Class that can be used for starting and stopping coroutines in the editor even in edit mode.
	/// <para>
	/// The class implements <see cref="ICoroutineRunner"/> and it can be used to substitute other
	/// <see cref="ICoroutineRunner">ICoroutineRunners</see> such as <see cref="Wrapper{TWrapped}">Wrappers</see>
	/// in unit tests.
	/// </para>
	/// </summary>
	[EditorService(typeof(ICoroutineRunner))]
	public sealed class EditorCoroutineRunner : ICoroutineRunner, IDisposable
	{
		/// <summary>
		/// A single cached instance of EditorCoroutineRunner that can be shared across multiple client objects.
		/// <para>
		/// This same instance can also be accessed via <see cref="Service{ICoroutineRunner}.Instance"/> in edit mode.
		/// </para>
		/// </summary>
		public static readonly EditorCoroutineRunner SharedInstance = new();

		private readonly List<EditorCoroutine> _running = new(1);

		[InitializeOnLoadMethod]
		private static void Init() => Service.SetInstance<ICoroutineRunner>(SharedInstance);

		/// <summary>
		/// Starts the provided <paramref name="coroutine"/>.
		/// </summary>
		/// <param name="coroutine"> The coroutine to start. </param>
		/// <returns>
		/// Always returns <see langword="null"/>.
		/// </returns>
		public Coroutine StartCoroutine([DisallowNull] IEnumerator coroutine)
		{
			if(_running.Count == 0)
			{
				EditorCoroutine.Stopped += OnSomeEditorCoroutineStopped;
			}

			_running.Add(EditorCoroutine.Start(coroutine));
			return null;
		}

		/// <inheritdoc/>
		public void StopCoroutine([DisallowNull] IEnumerator coroutine)
		{
			for(int i = _running.Count - 1; i >= 0; i--)
			{
				EditorCoroutine runningCoroutine = _running[i];
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
			for(int i = _running.Count - 1; i >= 0; i--)
			{
				_running[i].Stop();
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
			for(int i = _running.Count - 1; i >= 0; i--)
			{
				_running[i].MoveNext(skipWaits);
			}

			return _running.Count > 0;
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
			int count = _running.Count;
			while(count > 0)
			{
				for(int i = count - 1; i >= 0; i--)
				{
					_running[i].MoveNext(true);
				}

				count = _running.Count;
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
			_running.Clear();
		}

		private void OnSomeEditorCoroutineStopped(EditorCoroutine coroutine)
		{
			_running.Remove(coroutine);
			if(_running.Count == 0)
			{
				EditorCoroutine.Stopped -= OnSomeEditorCoroutineStopped;
			}
		}
	}
}