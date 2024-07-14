using System.Collections;
using UnityEngine;

namespace Sisus.Init
{
    /// <summary>
    /// Represents an object that can <see cref="StartCoroutine">start</see>
    /// and <see cref="StopCoroutine">stop</see> coroutines running.
    /// </summary>
    public interface ICoroutineRunner
    {
        /// <summary>
        /// Starts the provided <paramref name="coroutine"/>.
        /// </summary>
        /// <param name="coroutine"> The coroutine to start. </param>
        /// <returns>
        /// A reference to the started <paramref name="coroutine"/>.
        /// <para>
        /// This reference can be passed to <see cref="StopCoroutine"/> to stop
        /// the execution of the coroutine.
        /// </para>
        /// </returns>
        Coroutine StartCoroutine(IEnumerator coroutine);

        /// <summary>
        /// Stops the provided <paramref name="coroutine"/>.
        /// </summary>
        /// <param name="coroutine"> The <see cref="IEnumerator">coroutine</see> to stop. </param>
        void StopCoroutine(IEnumerator coroutine);

        /// <summary>
        /// Stops the provided <paramref name="coroutine"/>.
        /// </summary>
        /// <param name="coroutine">
        /// Reference to the <see cref="IEnumerator">coroutine</see> to stop.
        /// <para>
        /// This is the reference that was returned by <see cref="StartCoroutine"/>
        /// when the coroutine was started.
        /// </para>
        /// </param>
        void StopCoroutine(Coroutine coroutine);

        /// <summary>
        /// Stops all <paramref name="coroutine">coroutines</paramref> that are
        /// currently running on <see langword="this"/> object, if any.
        /// <para>
        void StopAllCoroutines();
    }
}