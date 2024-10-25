#if UNITY_2023_1_OR_NEWER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Contains a collection of utility methods related to <see cref="Awaitable{TResult}"/>.
	/// </summary>
	internal static class AwaitableUtility
	{
		/// <summary>
		/// Gets an <see cref="Awaitable{TResult}"/> that's completed with the specified result.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result returned by the awaitable. </typeparam>
		/// <param name="result"> The result to store into the completed awaitable. </param>
		/// <returns> A completed awaitable. </returns>
		public static Awaitable<TResult> FromResult<TResult>(TResult result) => Result<TResult>.From(result);

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static Awaitable<(TResult1 result1, TResult2 result2)>
			WhenAll<TResult1, TResult2>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2)
					=> new Awaitables<TResult1, TResult2>(awaitable1, awaitable2);

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <typeparam name="TResult3"> The type of the result produced by the third awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3)>
			WhenAll<TResult1, TResult2, TResult3>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3)
					=> new Awaitables<TResult1, TResult2, TResult3>(awaitable1, awaitable2, awaitable3);

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <typeparam name="TResult3"> The type of the result produced by the third awaitable. </typeparam>
		/// <typeparam name="TResult4"> The type of the result produced by the fourth awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <param name="awaitable4"> The fourth awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4)>
			WhenAll<TResult1, TResult2, TResult3, TResult4>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4)
					=> new Awaitables<TResult1, TResult2, TResult3, TResult4>(awaitable1, awaitable2, awaitable3, awaitable4);

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <typeparam name="TResult3"> The type of the result produced by the third awaitable. </typeparam>
		/// <typeparam name="TResult4"> The type of the result produced by the fourth awaitable. </typeparam>
		/// <typeparam name="TResult5"> The type of the result produced by the fifth awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <param name="awaitable4"> The fourth awaitable to wait on for completion. </param>
		/// <param name="awaitable5"> The fifth awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4, TResult5 result5)>
			WhenAll<TResult1, TResult2, TResult3, TResult4, TResult5>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4, Awaitable<TResult5> awaitable5)
					=> new Awaitables<TResult1, TResult2, TResult3, TResult4, TResult5>(awaitable1, awaitable2, awaitable3, awaitable4, awaitable5);

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <typeparam name="TResult3"> The type of the result produced by the third awaitable. </typeparam>
		/// <typeparam name="TResult4"> The type of the result produced by the fourth awaitable. </typeparam>
		/// <typeparam name="TResult5"> The type of the result produced by the fifth awaitable. </typeparam>
		/// <typeparam name="TResult6"> The type of the result produced by the sixth awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <param name="awaitable4"> The fourth awaitable to wait on for completion. </param>
		/// <param name="awaitable5"> The fifth awaitable to wait on for completion. </param>
		/// <param name="awaitable6"> The sixth awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4, TResult5 result5, TResult6 result6)>
			WhenAll<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4, Awaitable<TResult5> awaitable5, Awaitable<TResult6> awaitable6)
					=> new Awaitables<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(awaitable1, awaitable2, awaitable3, awaitable4, awaitable5, awaitable6);

		/// <summary>
		/// Awaits an <see cref="Task{T}"/> or <see cref="Awaitable{T}"/> with any generic type and returns its result.
		/// </summary>
		/// <param name="awaitable"> <see cref="Task{T}"/> or <see cref="Awaitable{T}"/> which to await. </param>
		/// <returns>
		/// Result of the <see cref="Task{T}"/> or <see cref="Awaitable{T}"/>, if <paramref name="awaitable"/> was
		/// of either type; otherwise, the <paramref name="awaitable"/> itself.
		/// </returns>
		[return: NotNull]
		public static async Awaitable<object> Await([DisallowNull] object awaitable)
		{
			if(awaitable is Task task)
			{
				return await task.GetResult();
			}

			if(awaitable.GetType().GetMethod(nameof(Task<object>.GetAwaiter)) is not { } getAwaiterMethod)
			{
				return awaitable;
			}

			var awaiter = (INotifyCompletion)getAwaiterMethod.Invoke(awaitable, null);
			var awaiterType = awaiter.GetType();
			var isCompletedProperty = awaiterType.GetProperty(nameof(Task<object>.IsCompleted));
			var getResultMethod = awaiterType.GetMethod(nameof(TaskAwaiter<object>.GetResult));
			if((bool)isCompletedProperty.GetValue(awaiter, null))
			{
				return getResultMethod.Invoke(awaiter, null);
			}

			var taskCompletionSource = new TaskCompletionSource<bool>();
			awaiter.OnCompleted(() => taskCompletionSource.SetResult(true));
			await taskCompletionSource.Task;
			return getResultMethod.Invoke(awaiter, null);
		}
		
		static class Result<TResult>
		{
			private static readonly AwaitableCompletionSource<TResult> completionSource = new();

			internal static Awaitable<TResult> From(TResult result)
			{
				completionSource.SetResult(result);
				var awaitable = completionSource.Awaitable;
				completionSource.Reset();
				return awaitable;
			}

			internal static Awaitable<TResult> Container()
			{
				var awaitable = completionSource.Awaitable;
				completionSource.Reset();
				return awaitable;
			}
		}

		struct Awaitables<TResult1, TResult2>
		{
			int unfinishedRemaining;
			TResult1 result1;
			TResult2 result2;
			AwaitableCompletionSource<(TResult1 result1, TResult2 result2)> results;

			public static implicit operator Awaitable<(TResult1 result1, TResult2 result2)>(Awaitables<TResult1, TResult2> awaitables) => awaitables.results.Awaitable;

			public Awaitables(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2)
			{
				unfinishedRemaining = 3;
				result1 = default;
				result2 = default;
				results = new();
				Await(awaitable1);
				Await(awaitable2);
			}

			async void Await(Awaitable<TResult1> awaitable)
			{
				result1 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2));
				}
			}

			async void Await(Awaitable<TResult2> awaitable)
			{
				result2 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2));
				}
			}
		}

		struct Awaitables<TResult1, TResult2, TResult3>
		{
			int unfinishedRemaining;
			TResult1 result1;
			TResult2 result2;
			TResult3 result3;
			AwaitableCompletionSource<(TResult1 result1, TResult2 result2, TResult3 result3)> results;

			public static implicit operator Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3)>(Awaitables<TResult1, TResult2, TResult3> awaitables) => awaitables.results.Awaitable;

			public Awaitables(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3)
			{
				unfinishedRemaining = 3;
				result1 = default;
				result2 = default;
				result3 = default;
				results = new();
				Await(awaitable1);
				Await(awaitable2);
				Await(awaitable3);
			}

			async void Await(Awaitable<TResult1> awaitable)
			{
				result1 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3));
				}
			}

			async void Await(Awaitable<TResult2> awaitable)
			{
				result2 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3));
				}
			}

			async void Await(Awaitable<TResult3> awaitable)
			{
				result3 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3));
				}
			}
		}

		struct Awaitables<TResult1, TResult2, TResult3, TResult4>
		{
			int unfinishedRemaining;
			TResult1 result1;
			TResult2 result2;
			TResult3 result3;
			TResult4 result4;
			AwaitableCompletionSource<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4)> results;

			public static implicit operator Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4)>(Awaitables<TResult1, TResult2, TResult3, TResult4> awaitables) => awaitables.results.Awaitable;

			public Awaitables(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4)
			{
				unfinishedRemaining = 4;
				result1 = default;
				result2 = default;
				result3 = default;
				result4 = default;
				results = new();
				Await(awaitable1);
				Await(awaitable2);
				Await(awaitable3);
				Await(awaitable4);
			}

			async void Await(Awaitable<TResult1> awaitable)
			{
				result1 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4));
				}
			}

			async void Await(Awaitable<TResult2> awaitable)
			{
				result2 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4));
				}
			}

			async void Await(Awaitable<TResult3> awaitable)
			{
				result3 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4));
				}
			}

			async void Await(Awaitable<TResult4> awaitable)
			{
				result4 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4));
				}
			}
		}

		struct Awaitables<TResult1, TResult2, TResult3, TResult4, TResult5>
		{
			int unfinishedRemaining;
			TResult1 result1;
			TResult2 result2;
			TResult3 result3;
			TResult4 result4;
			TResult5 result5;
			AwaitableCompletionSource<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4, TResult5 result5)> results;

			public static implicit operator Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4, TResult5 result5)>(Awaitables<TResult1, TResult2, TResult3, TResult4, TResult5> awaitables) => awaitables.results.Awaitable;

			public Awaitables(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4, Awaitable<TResult5> awaitable5)
			{
				unfinishedRemaining = 5;
				result1 = default;
				result2 = default;
				result3 = default;
				result4 = default;
				result5 = default;
				results = new();
				Await(awaitable1);
				Await(awaitable2);
				Await(awaitable3);
				Await(awaitable4);
				Await(awaitable5);
			}

			async void Await(Awaitable<TResult1> awaitable)
			{
				result1 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5));
				}
			}

			async void Await(Awaitable<TResult2> awaitable)
			{
				result2 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5));
				}
			}

			async void Await(Awaitable<TResult3> awaitable)
			{
				result3 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5));
				}
			}

			async void Await(Awaitable<TResult4> awaitable)
			{
				result4 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5));
				}
			}

			async void Await(Awaitable<TResult5> awaitable)
			{
				result5 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5));
				}
			}
		}

		struct Awaitables<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>
		{
			int unfinishedRemaining;
			TResult1 result1;
			TResult2 result2;
			TResult3 result3;
			TResult4 result4;
			TResult5 result5;
			TResult6 result6;
			AwaitableCompletionSource<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4, TResult5 result5, TResult6 result6)> results;

			public static implicit operator Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4, TResult5 result5, TResult6 result6)>(Awaitables<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> awaitables) => awaitables.results.Awaitable;

			public Awaitables(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4, Awaitable<TResult5> awaitable5, Awaitable<TResult6> awaitable6)
			{
				unfinishedRemaining = 6;
				result1 = default;
				result2 = default;
				result3 = default;
				result4 = default;
				result5 = default;
				result6 = default;
				results = new();
				Await(awaitable1);
				Await(awaitable2);
				Await(awaitable3);
				Await(awaitable4);
				Await(awaitable5);
				Await(awaitable6);
			}

			async void Await(Awaitable<TResult1> awaitable)
			{
				result1 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5, result6));
				}
			}

			async void Await(Awaitable<TResult2> awaitable)
			{
				result2 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5, result6));
				}
			}

			async void Await(Awaitable<TResult3> awaitable)
			{
				result3 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5, result6));
				}
			}

			async void Await(Awaitable<TResult4> awaitable)
			{
				result4 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5, result6));
				}
			}

			async void Await(Awaitable<TResult5> awaitable)
			{
				result5 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5, result6));
				}
			}

			async void Await(Awaitable<TResult6> awaitable)
			{
				result6 = await awaitable;
				unfinishedRemaining--;
				if(unfinishedRemaining <= 0)
				{
					results.SetResult((result1, result2, result3, result4, result5, result6));
				}
			}
		}
	}
}
#endif