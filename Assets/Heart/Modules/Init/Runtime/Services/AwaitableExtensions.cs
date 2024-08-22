#if UNITY_2023_1_OR_NEWER
using System.Threading.Tasks;
using UnityEngine;

namespace Sisus.Init.Internal
{
	internal static class AwaitableExtensions
	{
		internal static async Task<object> AsTask(this Awaitable<object> awaitable) => await awaitable;
	}
}
#endif