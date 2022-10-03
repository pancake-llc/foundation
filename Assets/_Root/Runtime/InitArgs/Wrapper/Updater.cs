using System;
using System.Collections;
using UnityEngine;
using JetBrains.Annotations;

namespace Pancake.Init
{
	using static NullExtensions;

	/// <summary>
	/// Class responsible for broadcasting the <see cref="IUpdate.Update"/>, <see cref="ILateUpdate.LateUpdate"/>
	/// and <see cref="IFixedUpdate.FixedUpdate"/> events to subscribers during the corresponding Unity event functions.
	/// <para>
	/// Classes wrapped by <see cref="Wrapper{TWrapped}"/> can subscribe to these events simply by implementing
	/// the <see cref="IUpdate"/>, <see cref="ILateUpdate"/> or <see cref="IFixedUpdate"/> interface. They will
	/// receiving these events for as long as the wrapper component remains <see cref="Behaviour.enabled">enabled</see>
	/// and <see cref="GameObject.activeInHierarchy">active</see> and is not <see cref="Object.Destroy">destroyed</see>.
	/// </para>
	/// <para>
	/// Note that these events do not get sent in edit mode, not even if the wrapper component
	/// has the <see cref="ExecuteAlways">ExecuteAlways attribute</see>.
	/// </para>
	/// </summary>
	[AddComponentMenu(Hidden)]
	[Service(typeof(ICoroutineRunner))]
	public sealed class Updater : MonoBehaviour, ICoroutineRunner
	{
		private const string Hidden = "";

		private static IUpdate[] updating = new IUpdate[128];
		private static ILateUpdate[] lateUpdating = new ILateUpdate[32];
		private static IFixedUpdate[] fixedUpdating = new IFixedUpdate[32];

		private static int updatingCount;
		private static int lateUpdatingCount;
		private static int fixedUpdatingCount;

		private static readonly object threadLock = new object();

		private static MonoBehaviour persistentInstance;

		#if UNITY_EDITOR
		/// <summary>
		/// Reset state when entering play mode in the editor to support Enter Play Mode Settings.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void OnEnterPlayMode()
		{
			lock(threadLock)
			{
				updatingCount = 0;
				lateUpdatingCount = 0;
				fixedUpdatingCount = 0;
			}

			persistentInstance = null;
		}
		#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			if(Service<ICoroutineRunner>.Instance is Updater updater)
			{
				persistentInstance = updater;
			}
			else
			{
				Service.RemoveInstanceChangedListener<ICoroutineRunner>(OnInstanceChanged);
				Service.AddInstanceChangedListener<ICoroutineRunner>(OnInstanceChanged);
			}
		}

		private static void OnInstanceChanged(ICoroutineRunner oldInstance, ICoroutineRunner newInstance)
		{
			if(newInstance is Updater updater)
			{
				persistentInstance = updater;
			}
		}

		/// <summary>
		/// Subscribes <paramref name="subscriber"/> to receive a callback during the MonoBehaviour.Update event function.
		/// </summary>
		/// <param name="subscriber"> object to receive the callback. </param>
		public static void Subscribe([NotNull] IUpdate subscriber)
		{
			lock(threadLock)
			{
				Add(ref updating, subscriber, ref updatingCount);
			}
		}

		/// <summary>
		/// Subscribes <paramref name="subscriber"/> to receive a callback during the MonoBehaviour.LateUpdate event function.
		/// </summary>
		/// <param name="subscriber"> object to receive the callback. </param>
		public static void Subscribe([NotNull] ILateUpdate subscriber)
		{
			lock(threadLock)
			{
				Add(ref lateUpdating, subscriber, ref lateUpdatingCount);
			}
		}

		/// <summary>
		/// Subscribes <paramref name="subscriber"/> to receive a callback during the MonoBehaviour.FixedUpdate event function.
		/// </summary>
		/// <param name="subscriber"> object to receive the callback. </param>
		public static void Subscribe([NotNull] IFixedUpdate subscriber)
		{
			lock(threadLock)
			{
				Add(ref fixedUpdating, subscriber, ref fixedUpdatingCount);
			}
		}

		/// <summary>
		/// Unsubscribes <paramref name="subscriber"/> from receiving a callback during the MonoBehaviour.Update event function.
		/// </summary>
		/// <param name="subscriber"> object to stop receiving the callback. </param>
		public static void Unsubscribe([NotNull] IUpdate subscriber)
		{
			lock(threadLock)
			{
				Remove(ref updating, subscriber, ref updatingCount);
			}
		}

		/// <summary>
		/// Unsubscribes <paramref name="subscriber"/> from receiving a callback during the MonoBehaviour.LateUpdate event function.
		/// </summary>
		/// <param name="subscriber"> object to stop receiving the callback. </param>
		public static void Unsubscribe([NotNull] ILateUpdate subscriber)
		{
			lock(threadLock)
			{
				Remove(ref lateUpdating, subscriber, ref lateUpdatingCount);
			}
		}

		/// <summary>
		/// Unsubscribes <paramref name="subscriber"/> from receiving a callback during the MonoBehaviour.FixedUpdate event function.
		/// </summary>
		/// <param name="subscriber"> object to stop receiving the callback. </param>
		public static void Unsubscribe([NotNull] IFixedUpdate subscriber)
		{
			lock(threadLock)
			{
				Remove(ref fixedUpdating, subscriber, ref fixedUpdatingCount);
			}
		}

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
		public static new Coroutine StartCoroutine([NotNull] IEnumerator coroutine)
		{
			// This will only occur in the rare cases that Init has not yet executed before
			// this method is called or that the application is quitting and the
			// persistentInstance has already been unloaded.
			if(persistentInstance == null)
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
		public static new void StopCoroutine([NotNull] IEnumerator coroutine)
		{
			// This will only occur in the rare cases that Init has not yet executed before
			// this method is called or that the application is quitting and the
			// persistentInstance has already been unloaded.
			if(persistentInstance == null)
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
		public static new void StopCoroutine([NotNull] Coroutine coroutine)
		{
			// This will only occur in the rare cases that Init has not yet executed before
			// this method is called or that the application is quitting and the
			// persistentInstance has already been unloaded.
			if(persistentInstance == null)
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

		private static void Remove<T>(ref T[] array, T item, ref int count) where T : class
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
				array[index] = null;
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
			for(int i = 0; i < updatingCount; i++)
			{
				updating[i].Update(deltaTime);
			}
		}

		private void LateUpdate()
		{
			float deltaTime = Time.deltaTime;
			for(int i = 0; i < lateUpdatingCount; i++)
			{
				lateUpdating[i].LateUpdate(deltaTime);
			}
		}

		private void FixedUpdate()
		{
			float deltaTime = Time.deltaTime;
			for(int i = 0; i < fixedUpdatingCount; i++)
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
				Remove(ref lateUpdating, this, ref lateUpdatingCount);
				action();
			}
		}
	}
}