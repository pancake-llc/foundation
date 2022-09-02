namespace Pancake.Init
{
    /// <summary>
    /// Defines the context from which a method is being called.
    /// <para>
    /// Used to identify the initialization phase during which a client is
    /// <see cref="InitArgs.TryGet">trying to get arguments that have been
    /// <see cref="InitArgs.Set">provided</see> for it.
    /// </para>
    /// </summary>
    public enum Context : byte
    {
        /// <summary>
        /// The method is getting called from a constructor.
        /// <para>
        /// Not a thread safe context.
        /// </para>
        /// </summary>
        Constructor = 0,

        /// <summary>
        /// The method is getting called from the <see cref="UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize"/> function.
        /// <para>
        /// Not a thread safe context.
        /// </para>
        /// </summary>
        OnAfterDeserialize = 0,

        /// <summary>
        /// The method is getting called from the OnValidate event function.
        /// <para>
        /// Not a thread safe context.
        /// </para>
        /// </summary>
        OnValidate = 0,

        /// <summary>
        /// The method is potentically getting called from a background thread.
        /// <para>
        /// Not a thread safe context.
        /// </para>
        /// </summary>
        Threaded = 0,

        /// <summary>
        /// The method is getting called from an Awake event function.
        /// <para>
        /// A thread safe context.
        /// </para>
        /// </summary>
        Awake = 1,

        /// <summary>
        /// The method is getting called from an OnEnable event function.
        /// <para>
        /// A thread safe context.
        /// </para>
        /// </summary>
        OnEnable = 1,

        /// <summary>
        /// The method is getting called from a Start event function.
        /// <para>
        /// A thread safe context.
        /// </para>
        /// </summary>
        Start = 1,

        /// <summary>
        /// The method is getting called from the main thread.
        /// <para>
        /// A thread safe context.
        /// </para>
        /// </summary>
        MainThread = 1,

        /// <summary>
        /// The method is getting called from the Reset event function.
        /// <para>
        /// A thread safe context.
        /// </para>
        /// </summary>
        Reset = 2
    }
}