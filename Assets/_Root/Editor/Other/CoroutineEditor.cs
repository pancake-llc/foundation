using System.Collections;
using UnityEditor;

namespace Pancake.Editor
{
    /// <summary>
    /// A coroutine that can update based on editor application update.
    /// </summary>
    public class EditorCoroutine
    {
        private readonly IEnumerator _enumerator;

        private EditorCoroutine(IEnumerator enumerator) { _enumerator = enumerator; }

        /// <summary>
        /// Creates and starts a coroutine.
        /// </summary>
        /// <param name="enumerator">The coroutine to be started</param>
        /// <returns>The coroutine that has been started.</returns>
        public static EditorCoroutine StartCoroutine(IEnumerator enumerator)
        {
            var coroutine = new EditorCoroutine(enumerator);
            coroutine.Start();
            return coroutine;
        }

        private void Start() { EditorApplication.update += OnEditorUpdate; }

        /// <summary>
        /// Stops the coroutine.
        /// </summary>
        public void Stop()
        {
            if (EditorApplication.update == null) return;

            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            // Coroutine has ended, stop updating.
            if (!_enumerator.MoveNext())
            {
                Stop();
            }
        }
    }
}