namespace Pancake
{
    /// <summary>
    /// Defines a class that wants to receive a callback during the Awake event function of the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see> that <see cref="Wrapper{TWrapped}">wraps</see> it.
    /// </summary>
    public interface IAwake
    {
        /// <summary>
        /// <see cref="Awake"/> is called when the script instance is being loaded.
        /// <para>
        /// <see cref="Awake"/> is called either when an <see cref="UnityEngine.GameObject.activeInHierarchy">active</see> <see cref="UnityEngine.GameObject">GameObject</see>
        /// that contains the script is initialized when a Scene loads, or when a previously inactive <see cref="UnityEngine.GameObject">GameObject</see> is set to active,
        /// or after a <see cref="UnityEngine.GameObject">GameObject</see> created with <see cref="UnityEngine.Object.Instantiate"/> is initialized.
        /// Use <see cref="Awake"/> to initialize variables or states before the application starts.
        /// </para>
        /// </summary>
        void Awake();
    }
}