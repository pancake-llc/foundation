using System;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace Sisus.Init.ValueProviders
{
	internal static class ValueProviderUtility
	{
		internal const string CREATE_ASSET_MENU_GROUP = "Value Providers/";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValueProvider(Type type)
			=> typeof(IValueProvider).IsAssignableFrom(type)
			|| typeof(IValueByTypeProvider).IsAssignableFrom(type)
			|| typeof(IValueProviderAsync).IsAssignableFrom(type)
			|| typeof(IValueByTypeProviderAsync).IsAssignableFrom(type);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValueProvider(Object obj) => obj is IValueProvider or IValueByTypeProvider or IValueByTypeProviderAsync or IValueProviderAsync;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAsyncValueProvider(Object obj) => obj is IValueProviderAsync or IValueByTypeProviderAsync;
	}
}