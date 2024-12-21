using System;
using System.Collections;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Sisus.Init.Internal
{
	using static NullExtensions;

	/// <summary>
	/// Class responsible for broadcasting the <see cref="IUpdate.Update"/>, <see cref="ILateUpdate.LateUpdate"/>
	/// and <see cref="IFixedUpdate.FixedUpdate"/> events to subscribers during the corresponding Unity event functions.
	/// <para>
	/// Classes wrapped by <see cref="Wrapper{TWrapped}"/> can subscribe to these events simply by implementing
	/// the <see cref="IUpdate"/>, <see cref="ILateUpdate"/> or <see cref="IFixedUpdate"/> interface. They will
	/// receive these events for as long as the wrapper component remains <see cref="Behaviour.enabled">enabled</see>
	/// and <see cref="GameObject.activeInHierarchy">active</see> and is not <see cref="UnityEngine.Object.Destroy">destroyed</see>.
	/// </para>
	/// <para>
	/// Note that these events do not get sent in edit mode, not even if the wrapper component
	/// has the <see cref="ExecuteAlways">ExecuteAlways attribute</see>.
	/// </para>
	/// </summary>
	[AddComponentMenu(Hidden)]
	internal sealed class Updater : MonoBehaviour, ICoroutineRunner
	{
		private const string Hidden = "";

		public static ICoroutineRunner CoroutineRunner { get; private set; }

		private static IUpdate[] updating = new IUpdate[128];
		private static ILateUpdate[] lateUpdating = new ILateUpdate[32];
		private static IFixedUpdate[] fixedUpdating = new IFixedUpdate[32];

		private static int updatingCount;
		private static int lateUpdatingCount;
		private static int fixedUpdatingCount;

		private static readonly object threadLock = new();

		private static MonoBehaviour persistentInstance;

		/// <summary>
		/// Cancellation token raised when the application is quitting or exiting Play Mode.
		/// </summary>
		public static CancellationToken CancellationToken
		{
			get
			{
				#if UNITY_6000_0_OR_NEWER
				return Application.exitCancellationToken;
				#else
				#if UNITY_EDITOR
				// In we have exited play mode, then request cancellation of all running tasks.
				if(EditorOnly.ThreadSafe.Application.TryGetIsPlaying(Context.Default, out bool isPlaying) && !isPlaying)
				{
					return new CancellationToken(canceled: true);
				}
				#endif

				if(persistentInstance)
				{
					return CancellationToken.None;
				}

				// If no persistent instance has been created yet, don't request cancellation.
				return persistentInstance is null
					? CancellationToken.None
					: new CancellationToken(canceled: true);
				#endif
			}
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Reset state when entering play mode in the editor to support Enter Play Mode Settings.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void OnEnterPlayMode()
		{
			lock(threadLock)
			{
				for(int i = 0; i < updatingCount; i++)
				{
					updating[i] = NullUpdatable.Instance;
				}

				for(int i = 0; i < lateUpdatingCount; i++)
				{
					lateUpdating[i] = NullUpdatable.Instance;
				}

				for(int i = 0; i < fixedUpdatingCount; i++)
				{
					fixedUpdating[i] = NullUpdatable.Instance;
				}

				updatingCount = 0;
				lateUpdatingCount = 0;
				fixedUpdatingCount = 0;
			}

			persistentInstance = null;
			CoroutineRunner = null;
		}
		#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			if(Service<ICoroutineRunner>.Instance is Updater updater)
			{
				persistentInstance = updater;
				CoroutineRunner = updater;
			}
			else
			{
				Service.RemoveInstanceChangedListener<ICoroutineRunner>(OnInstanceChanged);
				Service.AddInstanceChangedListener<ICoroutineRunner>(OnInstanceChanged);
			}

			static void OnInstanceChanged(Clients clients, ICoroutineRunner oldInstance, ICoroutineRunner newInstance)
			{
				if(newInstance is Updater updater)
				{
					persistentInstance = updater;
					CoroutineRunner = updater;
				}
			}
		}

		/// <summary>
		/// Subscribes <paramref name="subscriber"/> to receive callbacks during the MonoBehaviour.Update,
		/// MonoBehaviour.LateUpdate and MonoBehaviour.FixedUpdate event functions, if it implements the
		/// <see cref="IUpdate"/>, <see cref="ILateUpdate"/> or <see cref="IFixedUpdate"/> interfaces, respectively.
		/// </summary>
		/// <param name="subscriber"> object to start receiving the callbacks. </param>
		public static void Subscribe([DisallowNull] IEnableable subscriber)
		{
			lock(threadLock)
			{
				if(subscriber is IUpdate updateable)
				{
					if(Array.IndexOf(updating, updateable) == -1)
					{
						Add(ref updating, updateable, ref updatingCount);
					}
				}

				if(subscriber is ILateUpdate lateUpdateable)
				{
					if(Array.IndexOf(lateUpdating, lateUpdateable) == -1)
					{
						Add(ref lateUpdating, lateUpdateable, ref lateUpdatingCount);
					}
				}

				if(subscriber is IFixedUpdate fixedUpdateable)
				{
					if(Array.IndexOf(fixedUpdating, fixedUpdateable) == -1)
					{
						Add(ref fixedUpdating, fixedUpdateable, ref fixedUpdatingCount);
					}
				}
			}
		}

		/// <summary>
		/// Subscribes <paramref name="subscriber"/> to receive a callback during the MonoBehaviour.Update event function.
		/// </summary>
		/// <param name="subscriber"> object to start receiving the callback. </param>
		public static void Subscribe([DisallowNull] IUpdate subscriber)
		{
			lock(threadLock)
			{
				Add(ref updating, subscriber, ref updatingCount);
			}
		}

		/// <summary>
		/// Subscribes <paramref name="subscriber"/> to receive a callback during the MonoBehaviour.LateUpdate event function.
		/// </summary>
		/// <param name="subscriber"> object to start receiving the callback. </param>
		public static void Subscribe([DisallowNull] ILateUpdate subscriber)
		{
			lock(threadLock)
			{
				Add(ref lateUpdating, subscriber, ref lateUpdatingCount);
			}
		}

		/// <summary>
		/// Subscribes <paramref name="subscriber"/> to receive a callback during the MonoBehaviour.FixedUpdate event function.
		/// </summary>
		/// <param name="subscriber"> object to start receiving the callback. </param>
		public static void Subscribe([DisallowNull] IFixedUpdate subscriber)
		{
			lock(threadLock)
			{
				Add(ref fixedUpdating, subscriber, ref fixedUpdatingCount);
			}
		}

		/// <summary>
		/// Unsubscribes <paramref name="subscriber"/> from receiving callbacks during the MonoBehaviour.Update,
		/// MonoBehaviour.LateUpdate and MonoBehaviour.FixedUpdate event functions, if it implements the
		/// <see cref="IUpdate"/>, <see cref="ILateUpdate"/> or <see cref="IFixedUpdate"/> interfaces, respectively.
		/// </summary>
		/// <param name="subscriber"> object to stop receiving the callbacks. </param>
		public static void Unsubscribe([DisallowNull] IEnableable subscriber)
		{
			lock(threadLock)
			{
				if(subscriber is IUpdate updateable)
				{
					if(Array.IndexOf(updating, updateable) != -1)
					{
						Remove(ref updating, updateable, ref updatingCount, NullUpdatable.Instance);
					}
				}

				if(subscriber is ILateUpdate lateUpdateable)
				{
					if(Array.IndexOf(lateUpdating, lateUpdateable) != -1)
					{
						Remove(ref lateUpdating, lateUpdateable, ref updatingCount, NullUpdatable.Instance);
					}
				}

				if(subscriber is IFixedUpdate fixedUpdateable)
				{
					if(Array.IndexOf(fixedUpdating, fixedUpdateable) != -1)
					{
						Remove(ref fixedUpdating, fixedUpdateable, ref fixedUpdatingCount, NullUpdatable.Instance);
					}
				}
			}
		}

		/// <summary>
		/// Unsubscribes <paramref name="subscriber"/> from receiving a callback during the MonoBehaviour.Update event function.
		/// </summary>
		/// <param name="subscriber"> object to stop receiving the callback. </param>
		public static void Unsubscribe([DisallowNull] IUpdate subscriber)
		{
			lock(threadLock)
			{
				Remove(ref updating, subscriber, ref updatingCount, NullUpdatable.Instance);
			}
		}

		/// <summary>
		/// Unsubscribes <paramref name="subscriber"/> from receiving a callback during the MonoBehaviour.LateUpdate event function.
		/// </summary>
		/// <param name="subscriber"> object to stop receiving the callback. </param>
		public static void Unsubscribe([DisallowNull] ILateUpdate subscriber)
		{
			lock(threadLock)
			{
				Remove(ref lateUpdating, subscriber, ref lateUpdatingCount, NullUpdatable.Instance);
			}
		}

		/// <summary>
		/// Unsubscribes <paramref name="subscriber"/> from receiving a callback during the MonoBehaviour.FixedUpdate event function.
		/// </summary>
		/// <param name="subscriber"> object to stop receiving the callback. </param>
		public static void Unsubscribe([DisallowNull] IFixedUpdate subscriber)
		{
			lock(threadLock)
			{
				Remove(ref fixedUpdating, subscriber, ref fixedUpdatingCount, NullUpdatable.Instance);
			}
		}

		public static bool IsSubscribed([DisallowNull] IUpdate updateable)
		{
			lock(threadLock)
			{
				return Array.IndexOf(updating, updateable) != -1;
			}
		}

		public static bool IsSubscribed([DisallowNull] ILateUpdate lateUpdateable)
		{
			lock(threadLock)
			{
				return Array.IndexOf(lateUpdating, lateUpdateable) != -1;
			}
		}

		public static bool IsSubscribed([DisallowNull] IFixedUpdate fixedUpdateable)
		{
			lock(threadLock)
			{
				return Array.IndexOf(fixedUpdating, fixedUpdateable) != -1;
			}
		}

		/// <summary>
		/// Starts the provided <paramref name="coroutine"/>.
		/// </summary>
		/// <param name="coroutine"> The coroutine to start. </param>
		/// <returns>
		/// A reference to the started <paramref name="coroutine"/>.
		/// <para>
		/// This reference can be passed to <see cref="StopCoroutine(Coroutine)"/> to stop
		/// the execution of the coroutine.
		/// </para>
		/// </returns>
		public static new Coroutine StartCoroutine([DisallowNull] IEnumerator coroutine)
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				Debug.LogError("Not supported in edit mode");
				return default;
			}
			#endif

			// This will only occur in the rare cases that Init has not yet executed before
			// this method is called or that the application is quitting and the
			// persistentInstance has already been unloaded.
			if(!persistentInstance)
			{
				InvokeAtEndOfFrame(() => StartCoroutine(coroutine));
				return null;
			}

			return persistentInstance.StartCoroutine(coroutine);
		}

		/// <summary>
		/// Stops the provided <paramref name="coroutine"/>.
		/// </summary>
		/// <param name="coroutine"> The <see cref="IEnumerator">coroutine</see> to stop. </param>
		public static new void StopCoroutine([DisallowNull] IEnumerator coroutine)
		{
			// This will only occur in the rare cases that Init has not yet executed before
			// this method is called or that the application is quitting and the
			// persistentInstance has already been unloaded.
			if(!persistentInstance)
			{
				InvokeAtEndOfFrame(() => StopCoroutine(coroutine));
				return;
			}

			persistentInstance.StopCoroutine(coroutine);
		}

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
		public static new void StopCoroutine([DisallowNull] Coroutine coroutine)
		{
			// This will only occur in the rare cases that Init has not yet executed before
			// this method is called or that the application is quitting and the
			// persistentInstance has already been unloaded.
			if(!persistentInstance)
			{
				InvokeAtEndOfFrame(() => StopCoroutine(coroutine));
				return;
			}

			persistentInstance.StopCoroutine(coroutine);
		}

		/// <summary>
		/// Invokes <paramref name="action"/> at the end of this frame.
		/// </summary>
		/// <param name="action"> The action to invoke. </param>
		public static void InvokeAtEndOfFrame(Action action)
		{
			Subscribe(new DelayedAction(action));
		}

		private static void Add<T>(ref T[] array, T item, ref int count) where T : class
		{
			if(count >= array.Length)
			{
				Array.Resize(ref array, count + count);
			}

			array[count] = item;
			count++;
		}

		private static void Remove<T>(ref T[] array, T item, ref int count, T nullValue) where T : class
		{
			count--;

			int index = Array.IndexOf(array, item);
			if(index == -1)
			{
				Debug.LogWarning($"{nameof(Updater)}.{nameof(Unsubscribe)} was called for subscriber {(item != Null ? item.GetType().Name : "null")} which had not been registered.");
				return;
			}

			if(index == count)
			{
				array[index] = nullValue;
				return;
			}

			for(int i = index; i <= count; i++)
			{
				array[i] = array[i + 1];
			}
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			for(int i = updatingCount - 1; i >= 0; i--)
			{
				updating[i].Update(deltaTime);
			}
		}

		private void LateUpdate()
		{
			float deltaTime = Time.deltaTime;
			for(int i = lateUpdatingCount - 1; i >= 0; i--)
			{
				lateUpdating[i].LateUpdate(deltaTime);
			}
		}

		private void FixedUpdate()
		{
			float deltaTime = Time.fixedDeltaTime;
			for(int i = fixedUpdatingCount - 1; i >= 0; i--)
			{
				fixedUpdating[i].FixedUpdate(deltaTime);
			}
		}

		private readonly struct DelayedAction : ILateUpdate
		{
			public readonly Action action;

			public DelayedAction(Action action) => this.action = action;

			public void LateUpdate(float deltaTime)
			{
				Remove(ref lateUpdating, this, ref lateUpdatingCount, NullUpdatable.Instance);
				action();
			}
		}

		private sealed class NullUpdatable : IUpdate, IFixedUpdate, ILateUpdate
		{
			public static readonly NullUpdatable Instance = new ();

			private NullUpdatable() { }

			public void Update(float deltaTime)	{ }

			public void FixedUpdate(float deltaTime) { }

			public void LateUpdate(float deltaTime) { }
		}

	}
}