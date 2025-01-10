//#define DEBUG_ENABLED

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using static Sisus.Init.Internal.TypeUtility;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using TypeCollection = System.Nullable<UnityEditor.TypeCache.TypeCollection>;
#else
using TypeCollection = System.Collections.Generic.IEnumerable<System.Type>;
#endif

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Editor-only utility class to help with automatically initializing <see cref="IArgs{}"/> objects during the Reset event function.
	/// </summary>
	/// <seealso cref="InitOnResetAttribute"/>
	/// <seealso cref="RequireComponent"/>
	internal static class AutoInitUtility
	{
		private static readonly ConcurrentDictionary<Type, bool> shouldAutoInit = new();
		private static readonly ConcurrentDictionary<Type, From[]> autoInitFrom = new();

		/// <summary>
		/// Examines the <paramref name="initializer"/> class and its <see cref="IInitializer.Target">client</see> class for the
		/// <see cref="InitOnResetAttribute"/> and if found uses them to cache data about where <see cref="From">from</see>
		/// the arguments of the client should be retrieved during auto-initialization.
		/// </summary>
		/// <param name="initializer"> Initializer responsible for initializing the client. </param>
		/// <param name="argumentCount"> Number of Init arguments that the client will receive. </param>
		/// <typeparam name="TClient"> Type of the object that will receive the Init arguments. </typeparam>
		/// <returns> <see keyword="true"/> if should auto init the client; otherwise, <see keyword="false"/>.</returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient>([DisallowNull] IInitializer initializer, int argumentCount)
		{
			var target = initializer.Target;
			var initializerType = initializer.GetType();

			if(shouldAutoInit.TryGetValue(initializerType, out bool result))
			{
				return result;
			}
			
			// 1. Initializer's own InitOnReset attribute is prioritized over any that might be found on the client.
			var clientType = target is TClient && target ? target.GetType() : typeof(TClient);
			if(TryGetInitOnResetAttribute(initializerType, out var attribute))
			{
				shouldAutoInit[initializerType] = true;
				autoInitFrom[initializerType] = GetArgumentSearchMethods(attribute, initializerType, clientType, argumentCount);
				return true;
			}

			// 2. If initializer does not have an InitOnResetAttribute, but its client does,
			// then copy over autoInitFrom values from the client to the initializer wherever possible.

			bool clientIsAutoInitialized;
			From[] clientIsAutoInitializedFrom;
			if(shouldAutoInit.TryGetValue(clientType, out result))
			{
				if(result)
				{
					clientIsAutoInitialized = true;
					clientIsAutoInitializedFrom = autoInitFrom[clientType];
				}
				else
				{
					clientIsAutoInitialized = false;
					clientIsAutoInitializedFrom = null;
				}
			}
			else if(TryGetInitOnResetAttribute(clientType, out attribute))
			{
				clientIsAutoInitializedFrom = GetArgumentSearchMethods(attribute, initializerType, clientType, argumentCount);
				clientIsAutoInitialized = true;
			}
			else
			{
				clientIsAutoInitializedFrom = null;
				clientIsAutoInitialized = false;
			}

			if(!clientIsAutoInitialized)
			{
				From[] initInitializerFrom = new From[argumentCount];
				for(int i = 0; i < argumentCount; i++)
				{
					initInitializerFrom[i] = From.GameObject;
				}

				autoInitFrom[initializerType] = initInitializerFrom;
				shouldAutoInit[initializerType] = false;
				return false;
			}

			// Don't try to get or add components to an initializer's game object, if it is not a component
			if(initializer is not Component initializerComponent)
			{
				autoInitFrom[initializerType] = clientIsAutoInitializedFrom.Select(from => from switch
				{
					From.GetOrAddComponent => From.Default,
					From.GameObject => From.Default,
					From.Children => From.Default,
					From.Parent => From.Default,
					From.ChildrenOrParent => From.Default,
					_ => from
				}).ToArray();
			}
			// Don't automatically attach new components to the game object with the initializer
			// based on attributes on the client component, unless the client and the initializer
			// exist on the same game object; it is more likely that this would be confusing than useful.
			else if(target
					&& target is Component clientComponent
					&& clientComponent.gameObject != initializerComponent.gameObject)
			{
				autoInitFrom[initializerType] = clientIsAutoInitializedFrom.Select(from => from != From.GetOrAddComponent ? from : From.GameObject).ToArray();
			}
			else
			{
				autoInitFrom[initializerType] = clientIsAutoInitializedFrom;
			}

			shouldAutoInit[initializerType] = true;
			return true;
		}

		/// <summary>
		/// Determines whether or not the dependency of <paramref name="client"/> is a component
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TArgument"> Type of the dependency of the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires <typeparamref name="TArgument"/>, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentForAutoInit<TClient, TArgument>([DisallowNull] TClient client, Context context)
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 1);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 2);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType)
			&& ArgumentIsRequiredComponentFor<TSecondArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 3);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType) && ArgumentIsRequiredComponentFor<TThirdArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 4);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 5);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TFifthArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 6);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TFifthArgument>(clientType) && ArgumentIsRequiredComponentFor<TSixthArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 7);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TFifthArgument>(clientType) && ArgumentIsRequiredComponentFor<TSixthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TSeventhArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 8);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TFifthArgument>(clientType) && ArgumentIsRequiredComponentFor<TSixthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TSeventhArgument>(clientType) && ArgumentIsRequiredComponentFor<TEighthArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 9);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TFifthArgument>(clientType) && ArgumentIsRequiredComponentFor<TSixthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TSeventhArgument>(clientType) && ArgumentIsRequiredComponentFor<TEighthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TNinthArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 10);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TFifthArgument>(clientType) && ArgumentIsRequiredComponentFor<TSixthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TSeventhArgument>(clientType) && ArgumentIsRequiredComponentFor<TEighthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TNinthArgument>(clientType) && ArgumentIsRequiredComponentFor<TTenthArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TEleventhArgument"> Type of the eleventh argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 11);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TFifthArgument>(clientType) && ArgumentIsRequiredComponentFor<TSixthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TSeventhArgument>(clientType) && ArgumentIsRequiredComponentFor<TEighthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TNinthArgument>(clientType) && ArgumentIsRequiredComponentFor<TTenthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TEleventhArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Determines whether or not all dependencies of <paramref name="client"/> are components
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TEleventhArgument"> Type of the eleventh argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TTwelfthArgument"> Type of the twelfth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>([DisallowNull] TClient client, Context context)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				// If the client has an initializer attached to it, never have it auto-initialize itself,
				// even if it has the InitOnReset or InitInEditMode attributes. The initializer will take over that job.
				if(result && context.IsUnitySafeContext() && HasInitializer(client))
				{
					#if DEV_MODE
					Debug.Log($"Ignoring cached result, because client has an initializer attached to it.");
					#endif

					return false;
				}

				return result;
			}

			if(TryGetInitOnResetAttribute(clientType, out var attribute))
			{
				#if DEV_MODE
				Debug.Log($"TryGetInitOnResetAttribute({clientType.Name}): true");
				#endif

				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = GetArgumentSearchMethods(attribute, null, clientType, 12);
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TThirdArgument>(clientType) && ArgumentIsRequiredComponentFor<TFourthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TFifthArgument>(clientType) && ArgumentIsRequiredComponentFor<TSixthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TSeventhArgument>(clientType) && ArgumentIsRequiredComponentFor<TEighthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TNinthArgument>(clientType) && ArgumentIsRequiredComponentFor<TTenthArgument>(clientType)
				&& ArgumentIsRequiredComponentFor<TEleventhArgument>(clientType) && ArgumentIsRequiredComponentFor<TTwelfthArgument>(clientType))
			{
				shouldAutoInit[clientType] = true;
				autoInitFrom[clientType] = new From[] { From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent,
														From.GetOrAddComponent, From.GetOrAddComponent, From.GetOrAddComponent };
				return true;
			}

			shouldAutoInit[clientType] = false;
			return false;
		}

		/// <summary>
		/// Finds or creates an instance of the <typeparamref name="TArgument"/> which is a dependency of the <paramref name="client"/>
		/// which can be retrieved automatically.
		/// <para>
		/// This function is called for each required dependency of the <paramref name="client"/> during its initialization phase
		/// when the <typeparamref name="TClient"/> class has the <see cref="InitOnResetAttribute"/> or
		/// <see cref="RequireComponent">RequireComponent attribute</see> targeting all dependencies.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient">
		/// Type of the <paramref name="client"/> which requires an argument of type <typeparamref name="TArgument"/>.
		/// </typeparam>
		/// <typeparam name="TArgument"> Type of the required argument to get or create. </typeparam>
		/// <param name="client"> The object which requires the argument. </param>
		/// <param name="argumentIndex"> Index of the argument among the client's <see cref="IArgs{}"/> arguments. </param>
		/// <returns> Instance of type <typeparamref name="TArgument"/> or <see langword="null"/>. </returns>
		[return: MaybeNull]
		internal static TArgument GetAutoInitArgument<TClient, TArgument>([DisallowNull] TClient client, int argumentIndex)
		{
			#if DEV_MODE
			Debug.Assert(autoInitFrom.ContainsKey(client.GetType()), client.GetType().Name);
			Debug.Assert(shouldAutoInit.TryGetValue(client.GetType(), out var shouldAutoInitClient), client.GetType().Name);
			Debug.Assert(shouldAutoInitClient, client.GetType().Name);
			#endif

			var from = autoInitFrom[client.GetType()][argumentIndex];
			return GetAutoInitArgument<TClient, TArgument>(client, argumentIndex, from);
		}

		/// <summary>
		/// Finds or creates an instance of the <typeparamref name="TArgument"/> which is a dependency of the <paramref name="client"/>
		/// which can be retrieved automatically.
		/// <para>
		/// This function is called for each required dependency of the <paramref name="client"/> during its initialization phase
		/// when the <typeparamref name="TClient"/> class has the <see cref="InitOnResetAttribute">AutoInit attribute</see> or
		/// <see cref="RequireComponent">RequireComponent attribute</see> targeting all dependencies.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient">
		/// Type of the <paramref name="client"/> which requires an argument of type <typeparamref name="TArgument"/>.
		/// </typeparam>
		/// <typeparam name="TArgument"> Type of the required argument to get or create. </typeparam>
		/// <param name="client"> The object which requires the argument. </param>
		/// <param name="argumentIndex"> Index of the argument among the client's <see cref="IArgs{}"/> arguments. </param>
		/// <param name="from"> Where to search when trying to locate an argument. </param>
		/// <returns> Instance of type <typeparamref name="TArgument"/> or <see langword="null"/>. </returns>
		[return: MaybeNull]
		internal static TArgument GetAutoInitArgument<TClient, TArgument>([DisallowNull] TClient client, int argumentIndex, From from)
		{
			if(from == From.Initializer)
			{
				return default;
			}

			var clientComponent = client as Component;
			bool clientIsComponent = clientComponent;

			bool fromAnyScene = HasFlag(from, From.AnyScene);
			bool fromSameScene = fromAnyScene || HasFlag(from, From.SameScene);
			bool fromParent = fromSameScene || HasFlag(from, From.Parent);
			bool fromChildren = fromSameScene || HasFlag(from, From.Children);

			ObjectType argumentObjectType = GetObjectType(typeof(TArgument));
			bool argumentIsInterface = argumentObjectType == ObjectType.Interface;
			bool argumentIsGameObjectOrTransform = argumentObjectType == ObjectType.GameObject || argumentObjectType == ObjectType.Transform;
			bool argumentIsComponentOrInterface = argumentIsInterface || argumentObjectType == ObjectType.Component;

			Type argumentElementType =  GetCollectionElementType(typeof(TArgument));
			ObjectType argumentElementObjectType =  argumentElementType is null ? ObjectType.None : GetObjectType(argumentElementType);
			bool argumentIsInterfaceCollection = argumentElementObjectType == ObjectType.Interface;
			bool argumentIsGameObjectOrTransformCollection = argumentElementObjectType is ObjectType.GameObject or ObjectType.Transform;
			bool argumentIsComponentOrInterfaceCollection = argumentIsInterfaceCollection || argumentElementObjectType == ObjectType.Component;
			bool argumentIsObjectOrInterfaceCollection = argumentIsInterfaceCollection || argumentElementObjectType == ObjectType.Object;

			bool argumentIsFindable = Find.typesToFindableTypes.ContainsKey(argumentElementType ?? typeof(TArgument)) || argumentIsGameObjectOrTransform || argumentIsGameObjectOrTransformCollection;

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"GetAutoInitArgument<{Internal.TypeUtility.ToString(typeof(TArgument))}>() with from:{from}, argumentIsFindable:{argumentIsFindable}, fromChildren:{fromChildren}, fromParent:{fromParent}, fromScene:{fromSameScene}, argumentObjectType:{argumentObjectType}, argumentElementType:{argumentElementType}, argumentElementObjectType:{argumentElementObjectType}, argumentIsComponentOrInterfaceCollection:{argumentIsComponentOrInterfaceCollection}, clientIsComponent:{clientIsComponent}, HasFlag(from, From.GameObject):{HasFlag(from, From.GameObject)}");
			#endif

			if(clientIsComponent && argumentIsFindable)
			{
				if(HasFlag(from, From.GameObject))
				{
					if(argumentIsGameObjectOrTransform)
					{
						if(argumentObjectType == ObjectType.GameObject)
						{
							return (TArgument)(object)clientComponent.gameObject;
						}

						return (TArgument)(object)clientComponent.transform;
					}
					else if(argumentIsGameObjectOrTransformCollection)
					{
						if(argumentElementObjectType == ObjectType.GameObject)
						{
							return ConvertToCollection<TArgument, GameObject>(new GameObject[] { clientComponent.gameObject });
						}

						return ConvertToCollection<TArgument, Transform>(new Transform[] { clientComponent.transform });
					}
					else if(argumentIsComponentOrInterfaceCollection)
					{
						var components = Find.AllIn(clientComponent.gameObject, argumentElementType);
						if(components.Length > 0 || from == From.GameObject)
						{
							return ConvertToCollection<TArgument, object>(components);
						}
					}
					else if(typeof(TArgument) == typeof(GameObject))
					{
						return (TArgument)(object)clientComponent.gameObject;
					}
					else if(Find.In<TArgument>(clientComponent.gameObject, out var argument))
					{
						return argument;
					}
				}

				if(fromChildren)
				{
					if(argumentIsGameObjectOrTransform)
					{
						if(argumentObjectType == ObjectType.GameObject)
						{
							return (TArgument)(object)clientComponent.gameObject;
						}

						return (TArgument)(object)clientComponent.transform;
					}

					if(argumentIsGameObjectOrTransformCollection)
					{
						int count = clientComponent.transform.childCount;
						if(argumentObjectType == ObjectType.GameObject)
						{
							var gameObjects = new GameObject[count];
							for(int i = count - 1; i >= 0; i--)
							{
								gameObjects[i] = clientComponent.transform.GetChild(i).gameObject;
							}

							return ConvertToCollection<TArgument, GameObject>(gameObjects);
						}

						var transforms = new Transform[count];
						for(int i = count - 1; i >= 0; i--)
						{
							transforms[i] = clientComponent.transform.GetChild(i);
						}

						return ConvertToCollection<TArgument, Transform>(transforms);
					}

					if(argumentIsComponentOrInterfaceCollection)
					{
						var components = clientComponent.GetComponentsInChildren(GetCollectionElementType(typeof(TArgument)), true);
						if(components.Length > 0 || (!fromParent && !fromSameScene))
						{
							return ConvertToCollection<TArgument, Component>(components);
						}
					}
					else if(argumentIsComponentOrInterface && Find.InChildren<TArgument>(clientComponent.gameObject, out var argument, true))
					{
						return argument;
					}
				}

				if(fromParent)
				{
					if(argumentIsGameObjectOrTransform)
					{
						if(argumentObjectType == ObjectType.GameObject)
						{
							return (TArgument)(object)clientComponent.gameObject;
						}

						return (TArgument)(object)clientComponent.transform;
					}

					if(argumentIsGameObjectOrTransformCollection)
					{
						if(argumentObjectType == ObjectType.GameObject)
						{
							var gameObjects = new List<GameObject>();
							for(var parent = clientComponent.transform.parent; parent; parent = parent.parent)
							{
								gameObjects.Add(parent.gameObject);
							}

							return ConvertToCollection<TArgument, GameObject>(gameObjects.ToArray());
						}

						var transforms = new List<Transform>();
						for(var parent = clientComponent.transform.parent; parent; parent = parent.parent)
						{
							transforms.Add(parent);
						}

						return ConvertToCollection<TArgument, Transform>(transforms.ToArray());
					}
					
					if(argumentIsComponentOrInterfaceCollection)
					{
						var components = clientComponent.GetComponentsInParent(GetCollectionElementType(typeof(TArgument)), true);
						if(components.Length > 0 || !fromSameScene)
						{
							return ConvertToCollection<TArgument, Component>(components);
						}
					}
					else if(argumentIsComponentOrInterface && Find.InParents<TArgument>(clientComponent.gameObject, out var argument, true))
					{
						return argument;
					}
				}

				if(fromSameScene)
				{
					if(argumentIsGameObjectOrTransform)
					{
						if(argumentObjectType == ObjectType.GameObject)
						{
							return (TArgument)(object)clientComponent.gameObject;
						}

						return (TArgument)(object)clientComponent.transform;
					}

					if(argumentIsGameObjectOrTransformCollection)
					{
						Transform[] transforms = Find.AllIn<Transform>(clientComponent.gameObject, Including.Scene | Including.Inactive);
						if(argumentObjectType == ObjectType.Transform)
						{
							return ConvertToCollection<TArgument, Transform>(transforms);
						}

						var gameObjects = transforms.Select(t => t.gameObject).ToArray();
						return ConvertToCollection<TArgument, GameObject>(gameObjects);
					}

					if(argumentIsComponentOrInterfaceCollection)
					{
						var componentOrInterfaceType = GetCollectionElementType(typeof(TArgument));
						var allInSameScene = Find.AllIn(clientComponent.gameObject, componentOrInterfaceType, Including.Scene | Including.Inactive);
						return ConvertToCollection<TArgument, object>(allInSameScene);
					}
				
					if(argumentIsComponentOrInterface)
					{
						// First try to find an active instance
						if(clientComponent.gameObject.activeInHierarchy && Find.In<TArgument>(clientComponent.gameObject, Including.Scene) is TArgument activeInstance)
						{
							return activeInstance;
						}
						
						if(Find.In<TArgument>(clientComponent.gameObject, Including.Scene | Including.Inactive) is TArgument activeOrInactiveInstance)
						{
							return activeOrInactiveInstance;
						}
					}
				}
			}

			if(fromAnyScene && argumentIsFindable)
			{
				if(argumentIsComponentOrInterfaceCollection)
				{
					var componentOrInterfaceType = GetCollectionElementType(typeof(TArgument));
					var allInAnyScene = Find.All(componentOrInterfaceType);
					return ConvertToCollection<TArgument, object>(allInAnyScene);
				}
				
				// First try to find an active instance
				if(Find.Any<TArgument>(false) is TArgument activeInstance)
				{
					return activeInstance;
				}
						
				if(Find.Any<TArgument>(true) is TArgument activeOrInactiveInstance)
				{
					return activeOrInactiveInstance;
				}
			}

			#if UNITY_EDITOR
			if(HasFlag(from, From.Assets) && argumentIsFindable)
			{
				if(argumentIsObjectOrInterfaceCollection)
				{
					var list = new List<Object>();

					var elementType = GetCollectionElementType(typeof(TArgument));
					if(typeof(Object).IsAssignableFrom(elementType))
					{
						foreach(var guid in UnityEditor.AssetDatabase.FindAssets("t:" + typeof(TArgument)))
						{
							string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
							var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TArgument));
							if(asset && typeof(TArgument).IsAssignableFrom(asset.GetType()))
							{
								list.Add(asset);
							}
						}
					}
					else
					{
						foreach(var derivedType in GetDerivedTypes(elementType))
						{
							if(!typeof(Object).IsAssignableFrom(derivedType))
							{
								continue;
							}

							foreach(var guid in UnityEditor.AssetDatabase.FindAssets("t:" + derivedType))
							{
								string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
								var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, derivedType);
								if(asset && elementType.IsAssignableFrom(asset.GetType()))
								{
									list.Add(asset);
								}
							}
						}
					}

					return ConvertToCollection<TArgument, Object>(list.ToArray());
				}

				return Find.Asset<TArgument>();
			}
			#endif

			if(clientIsComponent && HasFlag(from, From.GetOrAddComponent))
			{
				if(Find.In(clientComponent.gameObject, out TArgument component))
				{
					return component;
				}

				var addType = GetAddableType(typeof(TArgument), true);
				if(addType != null)
				{
					return (TArgument)(object)clientComponent.gameObject.AddComponent(addType);
				}
			}

			if(HasFlag(from, From.CreateInstance) && !typeof(TArgument).IsAbstract)
			{
				#if DEV_MODE
				Debug.Log($"CreateInstance: {typeof(TArgument).Name}");
				#endif

				var addType = GetAddableType(typeof(TArgument), false);
				if(addType != null)
				{
					var gameObject = new GameObject(typeof(TArgument).Name);
					return (TArgument)(object)gameObject.AddComponent(addType);
				}

				if(typeof(ScriptableObject).IsAssignableFrom(typeof(TArgument)))
				{
					if(typeof(TArgument) == typeof(ScriptableObject))
					{
						return default;
					}

					return (TArgument)(object)ScriptableObject.CreateInstance(typeof(TArgument));
				}

				if(typeof(TArgument) == typeof(Object))
				{
					return default;
				}

				if(argumentIsObjectOrInterfaceCollection)
				{
					return ConvertToCollection<TArgument, Object>(new Object[0]);
				}

				if(typeof(Type).IsAssignableFrom(typeof(TArgument)))
				{
					return default;
				}

				if(typeof(TArgument) == typeof(string))
				{
					return (TArgument)(object)"";
				}

				try
				{
					return Activator.CreateInstance<TArgument>();
				}
				#if DEV_MODE
				catch(Exception e)
				{
					Debug.LogWarning(e, client as Object);
				#else
				catch
				{
				#endif
					return (TArgument)FormatterServices.GetUninitializedObject(typeof(TArgument));
				}
			}

			return default;
		}

		[return: MaybeNull]
		private static Type GetAddableType(Type type, bool returnDerivedTypeIfAbstract)
		{
			if(!typeof(Component).IsAssignableFrom(type))
			{
				return null;
			}

			if(!type.IsAbstract)
			{
				if(type == typeof(Component) || type == typeof(Behaviour) || type == typeof(MonoBehaviour))
				{
					return null;
				}

				if(type == typeof(Collider))
				{
					return typeof(BoxCollider);
				}

				if(type == typeof(Renderer))
				{
					return typeof(MeshRenderer);
				}

				return type;
			}

			if(!returnDerivedTypeIfAbstract)
			{
				return null;
			}

			foreach(var derivedType in GetDerivedTypes(type))
			{
				if(derivedType.IsAbstract || !typeof(Component).IsAssignableFrom(derivedType))
				{
					continue;
				}

				return derivedType;
			}

			return null;
		}

		private static TypeCollection GetDerivedTypes([DisallowNull] Type inheritedType)
		{
			#if UNITY_EDITOR
			return UnityEditor.TypeCache.GetTypesDerivedFrom(inheritedType);
			#else
			return TypeUtility.GetDerivedTypes(inheritedType, inheritedType.Assembly, typeof(InitArgs).Assembly);
			#endif
		}

		private static ObjectType GetCollectionElementObjectType(Type type)
		{
			if(!typeof(IEnumerable).IsAssignableFrom(type))
			{
				return ObjectType.None;
			}

			if(type.IsInterface)
			{
				if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					var genericArgument = type.GetGenericArguments()[0];
					return GetObjectType(genericArgument);
				}
			}

			foreach(var interfaceType in type.GetInterfaces())
			{
				if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					var genericArgument = interfaceType.GetGenericArguments()[0];
					return GetObjectType(genericArgument);
				}
			}

			return ObjectType.None;
		}

		private static ObjectType GetObjectType(Type type)
		{
			if(!typeof(Object).IsAssignableFrom(type))
			{
				return type.IsInterface ? ObjectType.Interface : ObjectType.None;
			}

			if(typeof(Component).IsAssignableFrom(type))
			{
				return typeof(Transform).IsAssignableFrom(type) ? ObjectType.Transform : ObjectType.Component;
			}

			return type == typeof(GameObject) ? ObjectType.GameObject : ObjectType.Object;
		}
		
		/// <summary>
		/// Gets a value indicating whether or not <paramref name="clientType"/> has the <see cref="RequireComponent"/> attribute
		/// specifying that the client requires the <paramref name="argumentType"/> to be attached to the same <see cref="GameObject"/>.
		/// </summary>
		/// <typeparam name="TArgument"> The type of the argument to check. </typeparam>
		/// <param name="clientType"> The client type to check for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if <paramref name="clientType"/> requires a component of type <typeparamref name="TArgument"/> to be found on the same game object. </returns>
		private static bool ArgumentIsRequiredComponentFor<TArgument>([DisallowNull] Type clientType)
			=> ArgumentIsRequiredComponentFor(typeof(TArgument), clientType);

		/// <summary>
		/// Gets a value indicating whether or not <paramref name="clientType"/> has the <see cref="RequireComponent"/> attribute
		/// specifying that the client requires the <paramref name="argumentType"/> to be attached to the same <see cref="GameObject"/>.
		/// </summary>
		/// <param name="argumentType"> The type of the argument to check. </param>
		/// <param name="clientType"> The client type to check for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if <paramref name="clientType"/> requires a component of type <paramref name="argumentType"/> to be found on the same game object. </returns>
		private static bool ArgumentIsRequiredComponentFor([DisallowNull] Type argumentType, [DisallowNull] Type clientType)
		{
			if(!typeof(Component).IsAssignableFrom(argumentType) && !argumentType.IsInterface)
			{
				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"ArgumentIsRequiredComponentFor<{typeof(TArgument).Name}, {classType.Name}>() : false - argument was not a component nor interface.");
				#endif

				return false;
			}

			foreach(var attribute in clientType.GetCustomAttributes(true))
			{
				RequireComponent requireComponent = attribute as RequireComponent;
				if(requireComponent == null)
				{
					continue;
				}

				if(requireComponent.m_Type0 != null && (requireComponent.m_Type0.IsAssignableFrom(argumentType) || argumentType.IsAssignableFrom(requireComponent.m_Type0)))
				{
					return true;
				}

				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"RequireComponent(typeof({requireComponent.m_Type0.Name})) != RequireComponent(typeof({argumentType.Name}))");
				#endif

				if(requireComponent.m_Type1 != null && (requireComponent.m_Type1.IsAssignableFrom(argumentType) || argumentType.IsAssignableFrom(requireComponent.m_Type1)))
				{
					return true;
				}

				if(requireComponent.m_Type2 != null && (requireComponent.m_Type2.IsAssignableFrom(argumentType) || argumentType.IsAssignableFrom(requireComponent.m_Type2)))
				{
					return true;
				}
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"ArgumentIsRequiredComponentFor<{argumentType.Name}, {clientType.Name}>() : false - no RequireComponent(typeof({argumentType.Name})) found on the class {clientType.Name}.\nClass contained attributes: {string.Join("\n", clientType.GetCustomAttributes(true))}.");
			#endif

			return false;
		}

		/// <summary>
		/// Gets an array containing all the search methods that should be used for locating the client's initialization arguments during
		/// automatic edit mode initialization
		/// </summary>
		/// <param name="fromAttribute"> Attribute that can contain user-specified <see cref="From"/> values for each argument. </param>
		/// <param name="initializerType"> (Optional) Type of the initializer providing the arguments to the client. </param>
		/// <param name="clientType"> Type of the client that will receive the arguments. </param>
		/// <param name="argumentCount"> Number of arguments that the client accepts during initialization. </param>
		/// <returns> An array of size <paramref name="argumentCount"/>. </returns>
		private static From[] GetArgumentSearchMethods([DisallowNull] InitOnResetAttribute fromAttribute, [AllowNull] Type initializerType, [DisallowNull] Type clientType, int argumentCount)
		{
			bool isInitializer = initializerType is not null;
			if(!TryGetClientArgumentTypes(clientType, argumentCount, out Type[] argumentTypes) &&
				(isInitializer || !TryGetInitializerArgumentTypes(initializerType, argumentCount, out argumentTypes)))
			{
				return argumentCount switch
				{
					1 => new[] { GetFinalFromValueForUnknownArgumentType(fromAttribute.first, isInitializer)
							   },
					2 => new[] { GetFinalFromValueForUnknownArgumentType(fromAttribute.first, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.second, isInitializer)
							   },
					3 => new[] { GetFinalFromValueForUnknownArgumentType(fromAttribute.first, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.second, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.third, isInitializer)
							   },
					4 => new[] { GetFinalFromValueForUnknownArgumentType(fromAttribute.first, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.second, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.third, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.fourth, isInitializer)
							   },
					5 => new[] { GetFinalFromValueForUnknownArgumentType(fromAttribute.first, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.second, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.third, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.fourth, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.fifth, isInitializer)
							   },
					_ => new[] { GetFinalFromValueForUnknownArgumentType(fromAttribute.first, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.second, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.third, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.fourth, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.fifth, isInitializer),
								 GetFinalFromValueForUnknownArgumentType(fromAttribute.sixth, isInitializer) },
				};
			}

			return argumentCount switch
			{
				1 => new[] { GetFinalFromValue(fromAttribute.first, argumentTypes[0], isInitializer)
						   },
				2 => new[] { GetFinalFromValue(fromAttribute.first, argumentTypes[0], isInitializer),
							 GetFinalFromValue(fromAttribute.second, argumentTypes[1], isInitializer)
						   },
				3 => new[] { GetFinalFromValue(fromAttribute.first, argumentTypes[0], isInitializer),
							 GetFinalFromValue(fromAttribute.second, argumentTypes[1], isInitializer),
							 GetFinalFromValue(fromAttribute.third, argumentTypes[2], isInitializer)
						   },
				4 => new[] { GetFinalFromValue(fromAttribute.first, argumentTypes[0], isInitializer),
							 GetFinalFromValue(fromAttribute.second, argumentTypes[1], isInitializer),
							 GetFinalFromValue(fromAttribute.third, argumentTypes[2], isInitializer),
							 GetFinalFromValue(fromAttribute.fourth, argumentTypes[3], isInitializer)
						   },
				5 => new[] { GetFinalFromValue(fromAttribute.first, argumentTypes[0], isInitializer),
							 GetFinalFromValue(fromAttribute.second, argumentTypes[1], isInitializer),
							 GetFinalFromValue(fromAttribute.third, argumentTypes[2], isInitializer),
							 GetFinalFromValue(fromAttribute.fourth, argumentTypes[3], isInitializer),
							 GetFinalFromValue(fromAttribute.fifth, argumentTypes[4], isInitializer)
						   },
				_ => new[] { GetFinalFromValue(fromAttribute.first, argumentTypes[0], isInitializer),
							 GetFinalFromValue(fromAttribute.second, argumentTypes[1], isInitializer),
							 GetFinalFromValue(fromAttribute.third, argumentTypes[2], isInitializer),
							 GetFinalFromValue(fromAttribute.fourth, argumentTypes[3], isInitializer),
							 GetFinalFromValue(fromAttribute.fifth, argumentTypes[4], isInitializer),
							 GetFinalFromValue(fromAttribute.sixth, argumentTypes[5], isInitializer) },
			};
		}

		private static bool TryGetInitOnResetAttribute([DisallowNull] Type clientType, out InitOnResetAttribute attribute)
			=> (attribute = clientType.GetCustomAttribute<InitOnResetAttribute>(true)) is not null;

		private static bool TryGetClientArgumentTypes(Type clientType, int argumentCount, out Type[] argumentTypes)
		{
			var interfaceTypes = clientType.GetInterfaces();
			foreach(var interfaceType in interfaceTypes)
			{
				if(!interfaceType.IsGenericType)
				{
					continue;
				}

				switch(argumentCount)
				{
					case 1:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<>))
						{
							argumentTypes = interfaceType.GetGenericArguments();
							return true;
						}
						break;
					case 2:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,>))
						{
							argumentTypes = interfaceType.GetGenericArguments();
							return true;
						}
						break;
					case 3:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments();
							return true;
						}
						break;
					case 4:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments();
							return true;
						}
						break;
					case 5:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments();
							return true;
						}
						break;
					case 6:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,,,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments();
							return true;
						}
						break;
				}
			}

			argumentTypes = null;
			return false;
		}

		private static bool TryGetInitializerArgumentTypes(Type initializer, int argumentCount, out Type[] argumentTypes)
		{
			var interfaceTypes = initializer.GetInterfaces();
			foreach(var interfaceType in interfaceTypes)
			{
				if(!interfaceType.IsGenericType)
				{
					continue;
				}

				switch(argumentCount)
				{
					case 1:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IInitializer<,>))
						{
							argumentTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
							return true;
						}
						break;
					case 2:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IInitializer<,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
							return true;
						}
						break;
					case 3:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IInitializer<,,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
							return true;
						}
						break;
					case 4:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IInitializer<,,,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
							return true;
						}
						break;
					case 5:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IInitializer<,,,,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
							return true;
						}
						break;
					case 6:
						if(interfaceType.GetGenericTypeDefinition() == typeof(IInitializer<,,,,,,>))
						{
							argumentTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
							return true;
						}
						break;
				}
			}

			argumentTypes = null;
			return false;
		}


		private static From GetFinalFromValueForUnknownArgumentType(From userProvidedValue, bool isInitializer)
			=> userProvidedValue != From.Default ? userProvidedValue : isInitializer ? From.Initializer : From.Anywhere;

		/// <summary>
		/// If <paramref name="userProvidedValue"/> <see cref="From"/>-value from <see cref="InitOnResetAttribute"/> <see cref="From.Default"/>,
		/// then returns a <see cref="From"/> value fitting for the argument type. Otherwise just returns <paramref name="userProvidedValue"/>.
		/// </summary>
		private static From GetFinalFromValue(From userProvidedValue, [DisallowNull] Type argumentType, bool isInitializer)
		{
			if(userProvidedValue != From.Default)
			{
				return userProvidedValue;
			}

			if(isInitializer)
			{
				return From.Initializer;
			}

			if(argumentType == typeof(GameObject))
			{
				return From.GameObject;
			}

			if(Find.typesToFindableTypes.ContainsKey(argumentType))
			{
				if(typeof(Transform).IsAssignableFrom(argumentType))
				{
					return From.GameObject;
				}
				
				if(argumentType.IsInterface || typeof(Component).IsAssignableFrom(argumentType))
				{
					return From.Children | From.Parent | From.SameScene | From.AnyScene;
				}
				
				return From.Assets;
			}

			if(TryGetCollectionItemType(argumentType, out Type itemType)
			&& (typeof(GameObject).IsAssignableFrom(itemType)
			|| Find.typesToFindableTypes.ContainsKey(itemType)))
			{
				return GetFinalFromValue(userProvidedValue, itemType, isInitializer);
			}

			return From.CreateInstance;
		}

		private static bool TryGetCollectionItemType(Type argumentType, out Type itemType)
		{
			if(argumentType.IsArray)
			{
				itemType = argumentType.GetElementType();
				return itemType != null;
			}
			
			if(!argumentType.IsGenericType)
			{
				itemType = null;
				return false;
			}

			Type typeDefinition = argumentType.GetGenericTypeDefinition();
			if(typeDefinition == typeof(IEnumerable<>)
			|| typeDefinition == typeof(IReadOnlyCollection<>)
			|| typeDefinition == typeof(ICollection)
			|| typeDefinition == typeof(IList<>)
			|| typeDefinition == typeof(IReadOnlyList<>)
			|| typeDefinition == typeof(List<>))
			{
				itemType = argumentType.GetGenericArguments()[0];
				return true;
			}

			itemType = null;
			return false;
		}

		private static bool HasFlag(From value, From flag)
		{
			return (value & flag) == flag;
		}

		private enum ObjectType
		{
			None = 0,
			Object = 1,
			GameObject = 2,
			Transform = 3,
			Component = 4,
			Interface = 5
		}
	}
}
#endif