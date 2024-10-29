#pragma warning disable CS8524

using System;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static Sisus.Init.ValueProviders.ValueProviderUtility;

namespace Sisus.Init
{
	internal static class ServiceProvider<TService>
	{
		private enum ValueType
		{
			Null = 0,
			DirectReference,
			IValueProvider,
			IValueProviderT,
			IValueByTypeProvider,
			IValueByTypeProviderAsync,
			IValueProviderAsync,
			IValueProviderAsyncT,
		}

		private static ValueType valueType = ValueType.Null;
		private static object valueProvider = default;

		public static TService GetValue([AllowNull] Component client) => valueType switch
		{
			ValueType.Null => default,
			ValueType.DirectReference => (TService)(object)Object.Instantiate((Object)valueProvider),
			ValueType.IValueProvider => ((IValueProvider)valueProvider).TryGetFor(client, out object objectValue) ? Find.In<TService>(objectValue) : default,
			ValueType.IValueProviderT => ((IValueProvider<TService>)valueProvider).TryGetFor(client, out TService result) ? result : default,
			ValueType.IValueByTypeProvider => ((IValueByTypeProvider)valueProvider).TryGetFor(client, out TService result) ? result : default,
			ValueType.IValueByTypeProviderAsync => GetFromAwaitableIfCompleted<TService>(((IValueByTypeProviderAsync)valueProvider).GetForAsync<TService>(client)),
			ValueType.IValueProviderAsync => GetFromAwaitableIfCompleted<TService>(((IValueProviderAsync)valueProvider).GetForAsync(client)),
			ValueType.IValueProviderAsyncT => GetFromAwaitableIfCompleted<TService>(((IValueProviderAsync<TService>)valueProvider).GetForAsync(client)),
		};

		public static bool TryGetValue([AllowNull] Component client, out TService result) => valueType switch
		{
			ValueType.Null => None(out result),
			ValueType.DirectReference => TryInstantiate(out result),
			ValueType.IValueProvider => ((IValueProvider)valueProvider).TryGetFor(client, out object objectValue) ? Find.In(objectValue, out result) : None(out result),
			ValueType.IValueProviderT => ((IValueProvider<TService>)valueProvider).TryGetFor(client, out result),
			ValueType.IValueByTypeProvider => ((IValueByTypeProvider)valueProvider).TryGetFor(client, out result),
			ValueType.IValueByTypeProviderAsync => TryGetFromAwaitableIfCompleted<TService>(((IValueByTypeProviderAsync)valueProvider).GetForAsync<TService>(client), out result),
			ValueType.IValueProviderAsync => TryGetFromAwaitableIfCompleted<TService>(((IValueProviderAsync)valueProvider).GetForAsync(client), out result),
			ValueType.IValueProviderAsyncT => TryGetFromAwaitableIfCompleted<TService>(((IValueProviderAsync<TService>)valueProvider).GetForAsync(client), out result),
		};

		private static bool None(out TService result)
		{
			result = default;
			return false;
		}

		private static bool TryInstantiate(out TService instance)
		{
			Object prefab = (Object)valueProvider;
			if(prefab)
			{
				instance = (TService)(object)Object.Instantiate((Object)valueProvider);
				return true;
			}

			instance = default;
			return false;
		}

		public static object Unset()
		{
			var result = valueProvider;
			valueProvider = default;
			return result;
		}

		public static void Dispose()
		{
			var dispose = valueProvider;
			if(dispose is null)
			{
				return;
			}

			valueProvider = default;

			if(dispose is Object unityObject)
			{
				if(unityObject)
				{
					Object.Destroy(unityObject);
				}
			}
			else if(dispose is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		static ServiceProvider()
		{
			#if UNITY_EDITOR
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			#else
			UnityEngine.Application.quitting -= OnExitingApplicationOrPlayMode;
			UnityEngine.Application.quitting += OnExitingApplicationOrPlayMode;
			#endif

			#if !INIT_ARGS_DISABLE_SERVICE_INJECTION
			if(Service.nowSettingInstance != typeof(TService) && ServiceInjector.TryGetUninitializedServiceInfo(typeof(TService), out var serviceInfo))
			{
				#if UNITY_EDITOR
				if(!EditorOnly.ThreadSafe.Application.IsPlaying)
				{
					return;
				}
				#endif

				_ = ServiceInjector.LazyInit(serviceInfo, typeof(TService));
			}
			#endif
		}

		#if UNITY_EDITOR
		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if(state == PlayModeStateChange.ExitingPlayMode)
			{
				OnExitingApplicationOrPlayMode();
			}
		}
		#endif

		private static void OnExitingApplicationOrPlayMode()
		{
			if(valueProvider is null || valueProvider is Object || Find.typesToWrapperTypes.ContainsKey(valueProvider.GetType()))
			{
				return;
			}

			if(valueProvider is IOnDisable onDisable)
			{
				onDisable.OnDisable();
			}

			if(valueProvider is IOnDestroy onDestroy)
			{
				onDestroy.OnDestroy();
			}

			if(valueProvider is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		#if (ENABLE_BURST_AOT || ENABLE_IL2CPP) && !INIT_ARGS_DISABLE_AUTOMATIC_AOT_SUPPORT
		private static void EnsureAOTPlatformSupport() => ServiceUtility.EnsureAOTPlatformSupportForService<TService>();
		#endif
	}
}