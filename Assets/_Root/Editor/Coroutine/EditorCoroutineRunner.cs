using System.Collections;
using System.Reflection;
using UnityEngine;
using JetBrains.Annotations;

namespace Pancake.Editor
{
    /// <summary>
    /// Class that can be used for starting and stopping coroutines in the editor even in edit mode.
    /// <para>
    /// The class implements <see cref="ICoroutineRunner"/> and it can be used to substitute other
    /// <see cref="ICoroutineRunner">ICoroutineRunners</see> such as <see cref="Wrapper">Wrappers</see>
    /// in unit tests.
    /// </para>
    /// </summary>
    public class EditorCoroutineRunner : ICoroutineRunner
    {
        /// <summary>
        /// A single cached instance of EditorCoroutineRunner that can be shared across multiple client objects.
        /// <para>
        /// This same instance can also be accessed via Service<ICoroutineRunner>.Instance in edit mode.
        /// </para>
        /// </summary>
        public static readonly EditorCoroutineRunner sharedInstance = new EditorCoroutineRunner();

        private static readonly FieldInfo routineField;

        static EditorCoroutineRunner()
        {
            routineField = typeof(Coroutine).GetField("routine", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if(!Application.isPlaying)
            {
                Service.SetInstance<ICoroutineRunner>(sharedInstance);
                return;
            }
        }

        /// <summary>
        /// Starts the provided <paramref name="coroutine"/>.
        /// </summary>
        /// <param name="coroutine"> The coroutine to start. </param>
        /// <returns>
        /// Always returns <see langword="null"/>.
        /// </returns>
        public Coroutine StartCoroutine([NotNull] IEnumerator coroutine)
        {
            EditorCoroutine.Start(coroutine);
            return null;
        }

        /// <inheritdoc/>
        public void StopCoroutine([NotNull] Coroutine coroutine)
        {
            IEnumerator enumerator = (IEnumerator)routineField.GetValue(coroutine);
            EditorCoroutine.Stop(enumerator);
        }

        /// <inheritdoc/>
        public void StopCoroutine([NotNull] IEnumerator coroutine)
        {
            EditorCoroutine.Stop(coroutine);
        }

        /// <summary>
        /// Stops all coroutines that have been started using <see cref="StartCoroutine"/> that are currently running.
        /// </summary>
        public void StopAllCoroutines()
        {
            EditorCoroutine.StopAll();
        }
    }
}