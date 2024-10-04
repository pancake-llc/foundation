#if !UNITY_2023_1_OR_NEWER
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Sisus.Init.Internal
{
	internal sealed class Until : IUpdate
	{
		private static readonly IUpdate instance = new Until();
		private static readonly Queue<TaskCompletionSource<bool>> waitingForNextFrame = new();

		/// <summary>
		/// Resumes execution on the Unity main thread, during an event where it's safe to use the Unity API.
		/// <para>
		/// An example use case is awaiting inside OnValidate until it's safe to access members like
		/// <see cref="Component.gameObject"/> and <see cref="Component.GetComponent{T}"/>.
		/// </para>
		/// </summary>
		/// <returns> Awaitable task that completes when it's safe to use the Unity API. </returns>
		public static Task<bool> UnitySafeContext()
		{
			lock(instance)
			{
				var completionSource = new TaskCompletionSource<bool>();
				waitingForNextFrame.Enqueue(completionSource);
				return completionSource.Task;
			}
		}

		#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		private static void InitInEditMode()
		{
			UnityEditor.EditorApplication.update -= OnUnitySafeContext;
			UnityEditor.EditorApplication.update += OnUnitySafeContext;
			EditorOnly.ThreadSafe.Application.IsPlaying = Application.isPlaying;
			OnUnitySafeContext();
		}
		#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitAtRuntime()
		{
			Updater.Subscribe(instance);
			OnUnitySafeContext();
		}

		void IUpdate.Update(float deltaTime) => OnUnitySafeContext();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnUnitySafeContext()
		{
			lock(instance)
			{
				while(waitingForNextFrame.TryDequeue(out var completionSource))
				{
					completionSource.SetResult(true);
				}
			}
		}
	}
}
#endif