using UnityEngine;
using JetBrains.Annotations;
using System.ComponentModel;

namespace Pancake.Init
{
    /// <summary>
    /// A base class for <see cref="ScriptableObject">ScriptableObjects</see> that act as simple wrappers for plain old class objects.
    /// <para>
    /// A class wrapped by the <see cref="ScriptableWrapper{}"/> can be loaded from Resources using the
    /// <see cref="Find.Resource{TWrapped}"/> function.
    /// </para>
    /// <para>
    /// Optionally the wrapped class can receive callbacks during select unity event functions from the wrapper
    /// by implementing one or more of the following interfaces:
    /// <list type="bullet">
    /// <item>
    /// <term> <see cref="IAwake"/> </term>
    /// <description> Receive callback during the ScriptableObject.<see cref="Awake"/> event. </description>
    /// </item>
    /// <item>
    /// <term> <see cref="IOnEnable"/> </term>
    /// <description> Receive callback during the ScriptableObject.<see cref="OnEnable"/> event. </description>
    /// </item>
    /// <item>
    /// <term> <see cref="IUpdate"/> </term>
    /// <description> Receive callback during the Update event. </description>
    /// </item>
    /// <item>
    /// <term> <see cref="IFixedUpdate"/> </term>
    /// <description> Receive callback during the FixedUpdate event. </description>
    /// </item>
    /// <item>
    /// <term> <see cref="ILateUpdate"/> </term>
    /// <description> Receive callback during the LateUpdate event. </description>
    /// </item>
    /// <item>
    /// <term> <see cref="IOnDisable"/> </term>
    /// <description> Receive callback during the ScriptableObject.<see cref="OnDisable"/> event. </description>
    /// </item>
    /// <item>
    /// <term> <see cref="IOnDestroy"/> </term>
    /// <description> Receive callback during the ScriptableObject.<see cref="OnDestroy"/> event. </description>
    /// </item>
    /// </list>
    /// </para>
    /// <typeparam name="TWrapped"> Type of the plain old class object wrapped by this scriptable object. </typeparam>
    [TypeConverter(typeof(WrapperConverter))]
    public abstract class ScriptableWrapper<TWrapped> : ScriptableObject, IWrapper<TWrapped>, IValueProvider<TWrapped>
    {
        /// <summary>
        /// The plain old class object wrapped by this scriptable object.
        /// </summary>
        [SerializeField]
        private TWrapped wrapped = default;

        private bool awakeInvokedWithoutWrappedObject;

        /// <summary>
        /// The plain old class object wrapped by this scriptable object.
        /// </summary>
        public TWrapped WrappedObject => wrapped;

        /// <summary>
        /// The plain old class object wrapped by this scriptable object.
        /// </summary>
        object IWrapper.WrappedObject => wrapped;

        /// <summary>
        /// Scriptable objects are not attached to <see cref="GameObject">GameObjects</see>;
        /// this always returns <see langword="null"/>.
        /// </summary>
        [JetBrains.Annotations.NotNull]
        GameObject IWrapper.gameObject => null;

        /// <summary>
        /// Scriptable objects do not derive from <see cref="MonoBehaviour"/>;
        /// this always returns <see langword="null"/>.
        /// </summary>
        [JetBrains.Annotations.NotNull]
        MonoBehaviour IWrapper.AsMonoBehaviour => null;

        /// <summary>
        /// This wrapper as an <see cref="Object"/>.
        /// </summary>
        [JetBrains.Annotations.NotNull]
        Object IWrapper.AsObject => this;

        /// <summary>
        /// The plain old class object wrapped by this scriptable object.
        /// </summary>
        TWrapped IValueProvider<TWrapped>.Value => wrapped;

        /// <inheritdoc/>
        object IValueProvider.Value => wrapped;

        /// <summary>
        /// Provides the <see cref="ScriptableObject"/> with the object that it wraps.
        /// <para>
        /// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for the scriptable object.
        /// </para>
        /// <para>
        /// <see cref="Init"/> get called when the script is being loaded, during the Awake event when
        /// an instance is created using <see cref="ObjectExtensions.Instantiate{TWrapped}"/> or
        /// <see cref="Create.Instance{TScriptableWrapper, TWrapped}(TArgument)"/> or when a scene that contains a reference
        /// to the asset is loaded.
        /// </para>
        /// <para>
        /// In edit mode <see cref="Init"/> can also get called during the <see cref="Reset"/> event when an asset is being created from the scriptable object
        /// if the class has the <see cref="InitOnResetAttribute"/>.
        /// </para>
        /// </summary>
        /// <param name="wrapped"> <see cref="object"/> that this <see cref="ScriptableObject"/> wraps. </param>
        private void Init(TWrapped wrapped)
        {
            this.wrapped = wrapped;
            Find.wrappedInstances[wrapped] = this;
            
            if(awakeInvokedWithoutWrappedObject)
            {
                if(this.wrapped is IAwake awake)
                {
                    awake.Awake();
                }
            }
        }

        /// <summary>
		/// Reset state to default values.
		/// <para>
		/// <see cref="Reset"/> is called when the user selects Reset in the Inspector context menu or when creating an asset from the ScriptableObject the first time.
		/// </para>
		/// <para>
		/// This function is only called in edit mode.
		/// </para>
		/// </summary>
        protected virtual void Reset()
        {
            if(InitArgs.TryGet(Context.Reset, this, out TWrapped wrapped))
            {
                Init(wrapped);
            }
        }

        /// <summary>
		/// <see cref="Awake"/> is called when the script instance is being loaded and handles calling the <see cref="Init"/> function with the <see cref="TWrapped"/> argument.
		/// <para>
		/// <see cref="Awake"/> is called when a <see cref="UnityEngine.SceneManagement.Scene">Scene</see> containing a reference to the scriptable object asset loads,
		/// or when a new instance of the ScriptableObject is <see cref="Create.Instance{TScriptableWrapper, TWrapped}(TArgument)">created</see>.
		/// </para>
		/// <para>
		/// Unity calls <see cref="Awake"/> only once during the lifetime of the script instance. A script's lifetime lasts until the Scene that contains it is unloaded.
		/// If the Scene is loaded again, Unity loads the script instance again, so <see cref="Awake"/> will be called again.
		/// If the Scene is loaded multiple times additively, Unity loads several script instances, so <see cref="Awake"/> will be called several times (once on each instance).
		/// </para>
		/// <para>
		/// Note: Use <see cref="Awake"/> instead of the constructor for initialization, as the serialized state of the <see cref="ScriptableObject"/> is undefined at construction time.
		/// </para>
		/// </summary>
        protected virtual void Awake()
        {
            if(InitArgs.TryGet(Context.Awake, this, out TWrapped wrapped))
            {
                Init(wrapped);
            }
            else if(ReferenceEquals(this.wrapped, null))
            {
                awakeInvokedWithoutWrappedObject = true;
            }

            if(this.wrapped is IAwake awake)
            {
                awake.Awake();
            }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected virtual void OnEnable()
        {
            if(wrapped is IUpdate update)
            {
                Updater.Subscribe(update);
            }

            if(wrapped is ILateUpdate lateUpdate)
            {
                Updater.Subscribe(lateUpdate);
            }

            if(wrapped is IFixedUpdate fixedUpdate)
            {
                Updater.Subscribe(fixedUpdate);
            }

            if(wrapped is IOnEnable onEnable)
            {
                onEnable.OnEnable();
            }
        }

        /// <summary>
        /// This function is called when the scriptable object goes out of scope.
        /// <para>
        /// This is also called when the object is destroyed and can be used for any cleanup code.
        /// When scripts are reloaded after compilation has finished, OnDisable will be called,
        /// followed by an OnEnable after the script has been loaded.
        /// </para>
        /// </summary>
        protected virtual void OnDisable()
        {
            if(wrapped is IUpdate update)
            {
                Updater.Unsubscribe(update);
            }

            if(wrapped is ILateUpdate lateUpdate)
            {
                Updater.Unsubscribe(lateUpdate);
            }

            if(wrapped is IFixedUpdate fixedUpdate)
            {
                Updater.Unsubscribe(fixedUpdate);
            }

            if(wrapped is IOnDisable onDisable)
            {
                onDisable.OnDisable();
            }
        }

        /// <summary>
        /// The <see cref="OnDestroy"/> even function is called when this <see cref="ScriptableWrapper"/>
        /// is <see cref="Object.Destroy">destroyed</see>.
        /// <para>
        /// <see cref="OnDestroy"/> also gets called when the application <see cref="Application.Quit">shuts down</see>
        /// and when leaving Play mode inside the Editor.
        /// </para>
        /// </summary>
        protected virtual void OnDestroy()
        {
            if(ReferenceEquals(wrapped, null))
            {
                return;
            }

            if(wrapped is IOnDestroy onDestroy)
            {
                onDestroy.OnDestroy();
            }

            Find.wrappedInstances.Remove(wrapped);
        }

        /// <inheritdoc/>
        void IInitializable<TWrapped>.Init(TWrapped wrapped)
        {
            Init(wrapped);
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ScriptableWrapper{TWrapped}"/> to the <see cref="TWrapped"/> plain old class object that it wraps.
        /// </summary>
        /// <param name="wrapper"> The <see cref="ScriptableWrapper{TWrapped}"/> instance to convert. </param>
        public static implicit operator TWrapped(ScriptableWrapper<TWrapped> wrapper)
        {
            return wrapper.wrapped;
        }

        /// <summary>
        /// Defines an explicit conversion of a <see cref="TWrapped"/> to a <see cref="ScriptableWrapper{TWrapped}"/> that wraps it.
        /// </summary>
        /// <param name="wrapped"> The <see cref="TWrapped"/> instance to convert. </param>
        public static explicit operator ScriptableWrapper<TWrapped>(TWrapped wrapped)
        {
            return Find.WrapperOf(wrapped) as ScriptableWrapper<TWrapped>;
        }
    }
}