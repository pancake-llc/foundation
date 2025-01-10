using System;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using static Sisus.Init.Internal.InitializerUtility;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for <see cref="MonoBehaviour">MonoBehaviours</see> that act as simple wrappers for plain old class objects.
	/// <para>
	/// A class wrapped by the <see cref="Wrapper{}"/> component can be added to a <see cref="GameObject"/> using the
	/// <see cref="AddComponentExtensions.AddComponent{TWrapper, TWrapped}(GameObject, TWrapped)"/> method.
	/// </para>
	/// <para>
	/// Optionally the wrapped class can receive callbacks during select unity event functions from the wrapper
	/// by implementing one or more of the following interfaces:
	/// <list type="bullet">
	/// <item>
	/// <term> <see cref="IAwake"/> </term>
	/// <description> Receive callback during the MonoBehaviour.<see cref="Awake"/> event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IOnEnable"/> </term>
	/// <description> Receive callback during the MonoBehaviour.<see cref="OnEnable"/> event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IStart"/> </term>
	/// <description> Receive callback during the MonoBehaviour.<see cref="Start"/> event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IUpdate"/> </term>
	/// <description> Receive callback during the MonoBehaviour.Update event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IFixedUpdate"/> </term>
	/// <description> Receive callback during the MonoBehaviour.FixedUpdate event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="ILateUpdate"/> </term>
	/// <description> Receive callback during the MonoBehaviour.LateUpdate event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IOnDisable"/> </term>
	/// <description> Receive callback during the MonoBehaviour.<see cref="OnDisable"/> event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IOnDestroy"/> </term>
	/// <description> Receive callback during the MonoBehaviour.<see cref="OnDestroy"/> event. </description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// The wrapped object can start coroutines running on the wrapper component by implementing <see cref="ICoroutines"/>
	/// and then using <see cref="ICoroutineRunner.StartCoroutine"/>.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapped"> Type of the plain old class object wrapped by this component. </typeparam>
	[RequiredInterface(typeof(IWrapper)), RequiredInterface(typeof(IValueByTypeProvider)), RequiredInterface(typeof(IValueProvider))]
	public abstract class Wrapper<TWrapped> : Wrapper, IWrapper<TWrapped>, IValueProvider<TWrapped>, IValueByTypeProvider
	{
		/// <summary>
		/// The plain old class object wrapped by this component.
		/// </summary>
		[SerializeField]
		internal TWrapped wrapped = default;

		private bool awakeCalledWithoutWrappedObject;

		/// <summary>
		/// The plain old class object wrapped by this component.
		/// </summary>
		public new TWrapped WrappedObject => wrapped;

		bool IWrapper.enabled => enabled;

		/// <summary>
		/// The plain old class object wrapped by this component.
		/// </summary>
		object IWrapper.WrappedObject => wrapped;

		/// <summary>
		/// This wrapper as a <see cref="MonoBehaviour"/>.
		/// </summary>
		[NotNull]
		MonoBehaviour IWrapper.AsMonoBehaviour => this;

		/// <summary>
		/// This wrapper as an <see cref="Object"/>.
		/// </summary>
		[NotNull]
		Object IWrapper.AsObject => this;

		/// <summary>
		/// The plain old class object wrapped by this component.
		/// </summary>
		TWrapped IValueProvider<TWrapped>.Value => wrapped;

		public Wrapper() { }
		public Wrapper(TWrapped wrapped) => this.wrapped = wrapped;

		/// <inheritdoc/>
		bool IValueByTypeProvider.TryGetFor<TValue>(Component client, out TValue value)
		{
			if(wrapped is TValue result)
			{
				value = result;
				return true;
			}

			value = default;
			return false;
		}

		/// <inheritdoc/>
		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => typeof(TValue).IsAssignableFrom(typeof(TWrapped));

		/// <summary>
		/// Resets the <paramref name="wrapped"/> object to its default value.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the component to a GameObject the first time.
		/// </para>
		/// <para>
		/// This function is only called in the editor in edit mode.
		/// </para>
		/// </summary>
		/// <param name="wrapped"> The wrapped object to reset. </param>
		protected virtual void OnReset(ref TWrapped wrapped) { }

		/// <summary>
		/// Provides the <see cref="Component"/> with the object that it wraps.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for the component.
		/// </para>
		/// <para>
		/// <see cref="Init"/> get called when the script is being loaded, during the Awake event when
		/// the component is created using <see cref="InstantiateExtensions.Instantiate{TWrapped}(TWrapped)"/> or
		/// <see cref="AddComponentExtensions.AddComponent{TWrapper, TWrapped}(GameObject, TWrapped)"/>.
		/// </para>
		/// <para>
		/// In edit mode <see cref="Init"/> can also get called during the <see cref="Reset"/> event when the script is added to a <see cref="GameObject"/>,
		/// if the component has the <see cref="InitOnResetAttribute"/> or <see cref="RequireComponent"/> attribute for each argument.
		/// </para>
		/// <para>
		/// Note that <see cref="Init"/> gets called immediately following object creation even if the <see cref="GameObject"/> to which the component
		/// was added is <see cref="GameObject.activeInHierarchy">inactive</see> unlike some other initialization event functions such as Awake and OnEnable.
		/// </para>
		/// </summary>
		/// <param name="wrapped"> <see cref="object"/> that this <see cref="Component"/> wraps. </param>
		private void Init(TWrapped wrapped)
		{
			this.wrapped = wrapped;
			Find.wrappedInstances[wrapped] = this;

			if(wrapped is ICoroutines coroutineUser && coroutineUser.CoroutineRunner is null)
			{
				coroutineUser.CoroutineRunner = this;
			}

			if(awakeCalledWithoutWrappedObject && gameObject.activeInHierarchy)
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

				if(wrapped is IAwake awake)
				{
					awake.Awake();
				}

				if(wrapped is IOnEnable onEnable)
				{
					onEnable.OnEnable();
				}
			}
		}

		#if UNITY_EDITOR
		private void Reset()
		{
			if(InitArgs.TryGet(Context.Reset, this, out TWrapped wrapped))
			{
				Init(wrapped);
			}

			OnInitializableReset(this);
			OnReset(ref this.wrapped);
		}
		#endif

		/// <summary>
		/// <see cref="Awake"/> is called when the script instance is being loaded and handles calling the <see cref="Init"/> method with the <see cref="TWrapped"/> argument.
		/// <para>
		/// <see cref="Awake"/> is called either when an active <see cref="GameObject"/> that contains the script is initialized when a <see cref="UnityEngine.SceneManagement.Scene">Scene</see> loads,
		/// or when a previously <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject"/> is set active, or after a <see cref="GameObject"/> created with <see cref="Object.Instantiate"/>
		/// is initialized.
		/// </para>
		/// <para>
		/// Unity calls <see cref="Awake"/> only once during the lifetime of the script instance. A script's lifetime lasts until the Scene that contains it is unloaded.
		/// If the Scene is loaded again, Unity loads the script instance again, so <see cref="Awake"/> will be called again.
		/// If the Scene is loaded multiple times additively, Unity loads several script instances, so <see cref="Awake"/> will be called several times (once on each instance).
		/// </para>
		/// <para>
		/// For active <see cref="GameObject">GameObjects</see> placed in a Scene, Unity calls <see cref="Awake"/> after all active <see cref="GameObject">GameObjects</see>
		/// in the Scene are initialized, so you can safely use methods such as <see cref="GameObject.FindWithTag"/> to query other <see cref="GameObject">GameObjects</see>.
		/// </para>
		/// <para>
		/// The order that Unity calls each <see cref="GameObject"/>'s Awake (and by extension <see cref="Awake"/>) is not deterministic.
		/// Because of this, you should not rely on one <see cref="GameObject"/>'s Awake being called before or after another
		/// (for example, you should not assume that a reference assigned by one GameObject's <see cref="Awake"/> will be usable in another GameObject's <see cref="Awake"/>).
		/// Instead, you should use <see cref="Awake"/>/<see cref="Awake"/> to set up references between scripts, and use Start, which is called after all <see cref="Awake"/>
		/// and <see cref="Awake"/> calls are finished, to pass any information back and forth.
		/// </para>
		/// <para>
		/// <see cref="Awake"/> is always called before any Start functions. This allows you to order initialization of scripts.
		/// <see cref="Awake"/> is called even if the script is a disabled component of an active GameObject.
		/// <see cref="Awake"/> can not act as a coroutine.
		/// </para>
		/// <para>
		/// Note: Use <see cref="Awake"/> instead of the constructor for initialization, as the serialized state of the <see cref="Component"/> is undefined at construction time.
		/// </para>
		/// </summary>
		protected virtual void Awake()
		{
			if(InitArgs.TryGet(Context.Awake, this, out TWrapped injectedObject))
			{
				Init(injectedObject);
			}
			else if(wrapped is null)
			{
				awakeCalledWithoutWrappedObject = true;
				return;
			}

			if(wrapped is IAwake awake)
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

			if(wrapped is ICancellable cancellable)
			{
				cancellable.IsCancellationRequested = false;
			}
		}

		/// <summary>
		/// <see cref="Start"/> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
		/// <para>
		/// Like the <see cref="Awake"/> function, <see cref="Start"/> is called exactly once in the lifetime of the script.
		/// However, Awake is called when the script object is initialised, regardless of whether the script is enabled.
		/// Start may not be called on the same frame as Awake if the script is not enabled at initialisation time.
		/// </para>
		/// </summary>
		protected virtual void Start()
		{
			if(wrapped is IStart start)
			{
				start.Start();
			}
		}

		/// <summary>
		/// This function is called when the behaviour becomes disabled.
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

			if(wrapped is ICancellable cancellable)
			{
				cancellable.IsCancellationRequested = true;
			}
		}

		/// <summary>
		/// The <see cref="OnDestroy"/> even function is called when this <see cref="Wrapper"/>
		/// or the <see cref="GameObject"/> to which it is attached is <see cref="Object.Destroy">destroyed</see>,
		/// or the <see cref="GameObject.scene">scene</see> which the <see cref="GameObject"/> is part of is
		/// <see cref="SceneManager.UnloadSceneAsync">unloaded</see>.
		/// <para>
		/// <see cref="OnDestroy"/> also gets called when the application <see cref="Application.Quit">shuts down</see>
		/// and when leaving Play mode inside the Editor.
		/// </para>
		/// <para>
		/// Note: <see cref="OnDestroy"/> will only be called on <see cref="GameObject">GameObjects</see> that have
		/// previously been <see cref="GameObject.activeInHierarchy">active</see>.
		/// </para>
		/// </summary>
		protected async virtual void OnDestroy()
		{
			if (wrapped is null)
			{
				return;
			}

			Find.wrappedInstances.Remove(wrapped);

			if(wrapped is IOnDestroy onDestroy)
			{
				onDestroy.OnDestroy();
			}
			else if(wrapped is IDisposable disposable)
			{
				disposable.Dispose();
			}
			else if(wrapped is IAsyncDisposable asyncDisposable)
			{
				await asyncDisposable.DisposeAsync();
			}
		}

		/// <inheritdoc/>
		void IInitializable<TWrapped>.Init(TWrapped wrapped) => Init(wrapped);

		/// <summary>
		/// Defines an implicit conversion of a <see cref="Wrapper{TWrapped}"/> to the <see cref="TWrapped"/> plain old class object that it wraps.
		/// </summary>
		/// <param name="wrapper"> The <see cref="Wrapper{TWrapped}"/> instance to convert. </param>
		public static implicit operator TWrapped(Wrapper<TWrapped> wrapper) => wrapper.wrapped;

		/// <summary>
		/// Defines an explicit conversion of a <see cref="TWrapped"/> to a <see cref="Wrapper{TWrapped}"/> <see cref="Component">component</see> that wraps it.
		/// </summary>
		/// <param name="wrapped"> The <see cref="TWrapped"/> instance to convert. </param>
		public static explicit operator Wrapper<TWrapped>(TWrapped wrapped) => Find.WrapperOf(wrapped) as Wrapper<TWrapped>;

		private protected override object GetWrappedObject() => wrapped;
	}

	/// <summary>
	/// Base class for <see cref="MonoBehaviour">MonoBehaviours</see> that act as simple wrappers for plain old class objects.
	/// <para>
	/// A class wrapped by the <see cref="Wrapper"/> component can be added to a <see cref="GameObject"/> using the
	/// <see cref="AddComponentExtensions.AddComponent{TWrapper, TWrapped}(GameObject, TWrapped)"/> method.
	/// </para>
	/// <para>
	/// Optionally the wrapped class can receive callbacks during select unity event functions from the wrapper
	/// by implementing one or more of the following interfaces:
	/// <list type="bullet">
	/// <item>
	/// <term> <see cref="IAwake"/> </term>
	/// <description> Receive callback during the MonoBehaviour.Awake event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IOnEnable"/> </term>
	/// <description> Receive callback during the MonoBehaviour.OnEnable event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IStart"/> </term>
	/// <description> Receive callback during the MonoBehaviour.Start event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IUpdate"/> </term>
	/// <description> Receive callback during the MonoBehaviour.Update event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IFixedUpdate"/> </term>
	/// <description> Receive callback during the MonoBehaviour.FixedUpdate event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="ILateUpdate"/> </term>
	/// <description> Receive callback during the MonoBehaviour.LateUpdate event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IOnDisable"/> </term>
	/// <description> Receive callback during the MonoBehaviour.OnDisable event. </description>
	/// </item>
	/// <item>
	/// <term> <see cref="IOnDestroy"/> </term>
	/// <description> Receive callback during the MonoBehaviour.OnDestroy event. </description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// The wrapped object can start coroutines running on the wrapper component by implementing <see cref="ICoroutines"/>
	/// and then using <see cref="ICoroutineRunner.StartCoroutine"/>.
	/// </para>
	/// </summary>
	[RequiredInterface(typeof(IWrapper)), RequiredInterface(typeof(IValueByTypeProvider)), RequiredInterface(typeof(IValueProvider))]
	public abstract class Wrapper : MonoBehaviour, IWrapper, IValueProvider, IValueByTypeProvider
	{
		/// <summary>
		/// Gets a value indicating whether the behaviour is enabled.
		/// </summary>
		bool IWrapper.enabled => enabled;

		/// <summary>
		/// Gets the plain old class object wrapped by this component.
		/// </summary>
		object IValueProvider.Value => GetWrappedObject();

		/// <summary>
		/// Gets the plain old class object wrapped by this component.
		/// </summary>
		public object WrappedObject => GetWrappedObject();

		/// <summary>
		/// Gets the plain old class object wrapped by this component.
		/// </summary>
		private protected abstract object GetWrappedObject();

		/// <summary>
		/// This wrapper as a <see cref="MonoBehaviour"/>.
		/// </summary>
		[NotNull]
		MonoBehaviour IWrapper.AsMonoBehaviour => this;

		/// <summary>
		/// This wrapper as an <see cref="Object"/>.
		/// </summary>
		[NotNull]
		Object IWrapper.AsObject => this;

		/// <inheritdoc/>
		bool IValueByTypeProvider.TryGetFor<TValue>(Component client, out TValue value)
		{
			if(GetWrappedObject() is TValue result)
			{
				value = result;
				return true;
			}

			value = default;
			return false;
		}
	}
}