using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.ExLibEditor
{
    /// <summary>
    /// Represents a coroutine that has been started running in the editor.
    /// <para>
    /// Also offers static methods for <see cref="Start">starting</see> and <see cref="Stop">stopping</see> coroutines.
    /// </para>
    /// </summary>
    public sealed class EditorCoroutine : YieldInstruction
    {
        private static readonly List<EditorCoroutine> Running = new List<EditorCoroutine>();
        private static readonly FieldInfo WaitForSecondsSeconds;

        private readonly Stack<object> _yielding = new Stack<object>();
        private double _waitUntil = 0d;

        public bool IsFinished => _yielding.Count == 0;

        static EditorCoroutine() { WaitForSecondsSeconds = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.NonPublic); }

        private EditorCoroutine(IEnumerator routine) => _yielding.Push(routine);

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
            if (Running.Count == 0)
            {
                EditorApplication.update += UpdateRunningCoroutines;
            }

            var editorCoroutine = new EditorCoroutine(coroutine);
            Running.Add(editorCoroutine);
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
        public static void Stop(EditorCoroutine coroutine)
        {
            Running.Remove(coroutine);

            if (Running.Count == 0)
            {
                EditorApplication.update -= UpdateRunningCoroutines;
            }
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
        /// Otherwise the <paramref name="coroutine"/> is started on an <see cref="Updater"/> Instance.
        /// </para>
        /// </summary>
        /// <param name="coroutine"> The <see cref="IEnumerator">coroutine</see> to stop. </param>
        public static void Stop(IEnumerator coroutine)
        {
            foreach (var editorCoroutine in Running)
            {
                int counter = editorCoroutine._yielding.Count;

                foreach (var item in editorCoroutine._yielding)
                {
                    if (counter == 1)
                    {
                        if (item == coroutine)
                        {
                            Stop(editorCoroutine);
                            return;
                        }
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
            Running.Clear();
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
        public static void FastForwardAll()
        {
            for (int i = Running.Count - 1; i >= 0; i--)
            {
                Running[i].FastForwardToEnd();
            }
        }

        /// <summary>
        /// Advances all currently running coroutine to their next phase.
        /// </summary>
        /// <param name="skipWaits">
        /// (Optional) If <see langword="true"/> then yield instructions
        /// <see cref="WaitForSeconds"/> and <see cref="WaitForSecondsRealtime"/> are skipped.
        /// </param>
        /// <returns> <see langword="true"/> if any coroutines are still running, <see langword="false"/> if all have finished. </returns>
        public static bool MoveAllNext(bool skipWaits = false)
        {
            for (int i = Running.Count - 1; i >= 0; i--)
            {
                Running[i].MoveNext(skipWaits);
            }

            return Running.Count > 0;
        }

        private static void UpdateRunningCoroutines()
        {
            for (int i = Running.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (!Running[i].MoveNext())
                    {
                        Running.RemoveAt(i);
                    }
                }
                catch
                {
                    Running.RemoveAt(i);
                    if (Running.Count == 0)
                    {
                        EditorApplication.update -= UpdateRunningCoroutines;
                    }

                    throw;
                }
            }

            if (Running.Count == 0)
            {
                EditorApplication.update -= UpdateRunningCoroutines;
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
        private bool MoveNext(bool skipWaits = false)
        {
            if (EditorApplication.timeSinceStartup < _waitUntil && !skipWaits)
            {
                return true;
            }

            if (_yielding.Count == 0)
            {
                return false;
            }

            var current = _yielding.Peek();

            if (current is IEnumerator enumerator)
            {
                if (!enumerator.MoveNext())
                {
                    _yielding.Pop();
                    return _yielding.Count > 0;
                }

                _yielding.Push(enumerator.Current);
                return true;
            }

            if (current is CustomYieldInstruction yieldInstruction)
            {
                if (yieldInstruction.keepWaiting)
                {
                    return true;
                }

                _yielding.Pop();
                return true;
            }

            if (current is WaitForSeconds waitForSeconds)
            {
                _waitUntil = EditorApplication.timeSinceStartup + (float) WaitForSecondsSeconds.GetValue(waitForSeconds);
                _yielding.Pop();
                return true;
            }

            if (current is WaitForSecondsRealtime waitForSecondsRealtime)
            {
                _waitUntil = EditorApplication.timeSinceStartup + waitForSecondsRealtime.waitTime;
                _yielding.Pop();
                return true;
            }

            if (current is WaitForEndOfFrame || current is WaitForFixedUpdate)
            {
                _yielding.Pop();
                return true;
            }

            if (current is AsyncOperation asyncOperation)
            {
                if (!asyncOperation.isDone)
                {
                    return false;
                }

                _yielding.Pop();
                return _yielding.Count > 0;
            }

            _yielding.Pop();
            return _yielding.Count > 0;
        }

        /// <summary>
        /// Continuously advances the coroutine to the next phase
        /// until it has reached the end.
        /// <para>
        /// Note that this locks the current thread until the coroutine has fully finished.
        /// If the coroutine contains <see cref="CustomYieldInstruction">CustomYieldInstructions</see>
        /// that take a long time to finish (or never finish in edit mode) this can cause the editor
        /// to freeze for the same duration.
        /// </para>
        /// </summary>
        public void FastForwardToEnd()
        {
            while (MoveNext(true)) ;
            Stop(this);
        }
    }
}