using Sisus.Init.Internal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sisus.Init
{
	/// <summary>
	/// Class that can act as a container for a globally shared instance of any class that has the <see cref="ServiceAttribute"/> on demand.
	/// </summary>
	/// <typeparam name="TService">
	/// The defining type of the service class, which is the type specified in its <see cref="ServiceAttribute"/>,
	/// or - if no other type has been explicitly specified - the exact type of the service class.
	/// <para>
	/// This type must be an interface that the service implements, a base type that the service derives from,
	/// or the exact type of the service.
	/// </para>
	/// </typeparam>
	/// <seealso cref="Service.Get{TService}"/>
	public static class Service<TService>
	{
		/// <summary>
		/// The shared instance of service of type <typeparamref name="TService"/>.
		/// <para>
		/// The returned object's class is of type <typeparamref name="TService"/>,
		/// derives from it, or implements an interface of that type.
		/// </para>
		/// <para>
		/// If no such service has been registered then <see langword="null"/> is returned.
		/// </para>
		/// </summary>
		#if DEBUG || INIT_ARGS_SAFE_MODE
		public static TService Instance { get; internal set; }
		#else
		public static TService Instance = default; // as a performance optimization, use a field instead of a property in release builds.
		#endif

		static Service()
		{
			#if DEV_MODE || (DEBUG && INIT_ARGS_SAFE_MODE)
			if(typeof(TService).IsValueType)
			{
				UnityEngine.Debug.LogWarning($"Service<{typeof(TService).Name}> was accessed but {typeof(TService).Name} is a value type. Only classes can be registered as services using the ServiceAttribute. Calling members of the Service class with a value type for the generic argument is not good practice because 'Service<TService>.Instance == null' will always be true with value types.");
			}
			#endif

			#if UNITY_EDITOR
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

			static void OnPlayModeStateChanged(PlayModeStateChange state)
			{
				if(state is PlayModeStateChange.ExitingPlayMode)
				{
					Service.Unset<TService>();
				}
			}
			#endif

			#if !INIT_ARGS_DISABLE_SERVICE_INJECTION
			if(Service.nowSettingInstance != typeof(TService))
			{
				if(ServiceInjector.TryGetUninitializedServiceInfo(typeof(TService), out var serviceInfo))
				{
					#if UNITY_EDITOR
					if(!EditorOnly.ThreadSafe.Application.IsPlaying)
					{
						return;
					}
					#endif

					_ = ServiceInjector.LazyInit(serviceInfo, typeof(TService));
				}
			}
			#endif
		}

		#if (ENABLE_BURST_AOT || ENABLE_IL2CPP) && !INIT_ARGS_DISABLE_AUTOMATIC_AOT_SUPPORT
		private static void EnsureAOTPlatformSupport() => ServiceUtility.EnsureAOTPlatformSupportForService<TService>();
		#endif
	}
}