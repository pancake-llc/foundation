using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
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
		public static bool IsValueProvider([AllowNull] Object obj) => obj is IValueProvider or IValueByTypeProvider or IValueByTypeProviderAsync or IValueProviderAsync;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAsyncValueProvider([AllowNull] Object obj) => obj is IValueProviderAsync or IValueByTypeProviderAsync;

		public static bool TryGetValueProviderValue([AllowNull] object potentialValueProvider, [DisallowNull] Type valueType, [NotNullWhen(true), MaybeNullWhen(false)] out object valueOrAwaitableToGetValue)
		{
			if(potentialValueProvider is IValueProviderAsync valueProviderAsync)
			{
				valueOrAwaitableToGetValue = valueProviderAsync.GetForAsync(potentialValueProvider as Component);
				return valueOrAwaitableToGetValue is not null;
			}

			if(potentialValueProvider is IValueProvider valueProvider)
			{
				return valueProvider.TryGetFor(potentialValueProvider as Component, out valueOrAwaitableToGetValue);
			}

			if(potentialValueProvider is IValueByTypeProviderAsync)
			{
				object[] args = new object[1] { potentialValueProvider as Component };
				if((bool)typeof(IValueByTypeProviderAsync).GetMethod(nameof(IValueByTypeProviderAsync.GetForAsync)).MakeGenericMethod(valueType).Invoke(potentialValueProvider, args))
				{
					valueOrAwaitableToGetValue = args[1];
					return valueOrAwaitableToGetValue is not null;
				}
			}

			if(potentialValueProvider is IValueByTypeProvider)
			{
				object[] args = new object[2] { potentialValueProvider as Component, null };
				if((bool)typeof(IValueByTypeProvider).GetMethod(nameof(IValueByTypeProvider.TryGetFor)).MakeGenericMethod(valueType).Invoke(potentialValueProvider, args))
				{
					valueOrAwaitableToGetValue = args[1];
					return valueOrAwaitableToGetValue is not null;
				}
			}
			
			valueOrAwaitableToGetValue = null;
			return false;
		}
	}
}