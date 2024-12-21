#if UNITY_2023_1_OR_NEWER
using UnityEngine;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Utility methods for <see cref="Awaitable"/> and <see cref="Awaitable{TResult}"/>.
	/// </summary>
	internal static class AwaitableUtility
	{
		/// <summary>
		/// Gets an <see cref="Awaitable{TResult}"/> that has already completed with the specified result.
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
		public static async Awaitable<(TResult1 result1, TResult2 result2)>
			WhenAll<TResult1, TResult2>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2)
					=> (await awaitable1, await awaitable2);

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
		public static async Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3)>
			WhenAll<TResult1, TResult2, TResult3>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3)
					=> (await awaitable1, await awaitable2, await awaitable3);

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
		public static async Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4)>
			WhenAll<TResult1, TResult2, TResult3, TResult4>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4)
					=> (await awaitable1, await awaitable2, await awaitable3, await awaitable4);

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
		public static async Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4, TResult5 result5)>
			WhenAll<TResult1, TResult2, TResult3, TResult4, TResult5>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4, Awaitable<TResult5> awaitable5)
					=> (await awaitable1, await awaitable2, await awaitable3, await awaitable4, await awaitable5);

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
		public static async Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4, TResult5 result5, TResult6 result6)>
			WhenAll<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>
				(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4, Awaitable<TResult5> awaitable5, Awaitable<TResult6> awaitable6)
					=> (await awaitable1, await awaitable2, await awaitable3, await awaitable4, await awaitable5, await awaitable6);

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
		}
	}
}
#endif