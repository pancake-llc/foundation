//#define DEBUG_ENABLED

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using TypeCollection = System.Nullable<UnityEditor.TypeCache.TypeCollection>;
#else
using TypeCollection = System.Collections.Generic.IEnumerable<System.Type>;
#endif

namespace Pancake.Init.EditorOnly
{
	/// <summary>
	/// Editor-only utility class to help with automatically initializing <see cref="IArgs{}"/> objects during the Reset event function.
	/// <seealso cref="InitOnResetAttribute"/>
	/// <seealso cref="RequireComponent"/>
	/// </summary>
	internal static class AutoInitUtility
	{
		private enum ObjectType
        {
			None = 0,
			Object = 1,
			GameObject = 2,
			Transform = 3,
			Component = 4,
			Interface = 5
		}

		private static readonly ConcurrentDictionary<Type, bool> shouldAutoInit = new ConcurrentDictionary<Type, bool>();
		private static readonly ConcurrentDictionary<Type, From[]> autoInitFrom = new ConcurrentDictionary<Type, From[]>();

		/// <summary>
		/// Examines the <paramref name="initializer"/> class and its <see cref="IInitializer.Target">client</see> class for the
		/// <see cref="InitOnResetAttribute"/> and if found uses them to cache data about where <see cref="From">from</see>
		/// the arguments of the client should be retrieved during auto-initialization.
		/// </summary>
		/// <typeparam name="TClient"> Type of the component that the initializer initializes. </typeparam>
		/// <param name="initializer"> Initializer responsible for initializing client of type <typeparamref name="TClient"/>. </param>
		internal static void PrepareArgumentsForAutoInit<TClient>([NotNull] IInitializer initializer, int argumentCount) where TClient : Component
		{
			var client = initializer.Target;
			var initializerType = initializer.GetType();
			var clientType = client == null ? typeof(TClient) : client.GetType();

			if(shouldAutoInit.ContainsKey(initializerType))
			{
				return;
			}

			shouldAutoInit[initializerType] = true;

			if(HasInitOnResetAttribute(initializerType, argumentCount))
			{
				return;
			}

			From[] initFrom;
			if(HasInitOnResetAttribute(clientType, argumentCount))
			{
				initFrom = autoInitFrom[clientType];
				for(int i = 0; i < argumentCount; i++)
				{
					if(initFrom[i] == From.GetOrAddComponent)
					{
						initFrom[i] = From.GameObject;
					}
				}

				autoInitFrom[initializerType] = initFrom;
				return;
			}

			initFrom = new From[argumentCount];
			for(int i = 0; i < argumentCount; i++)
			{
				initFrom[i] = From.GameObject;
			}

			autoInitFrom[initializerType] = initFrom;
		}

		/// <summary>
		/// Determines whether or not the dependency of <paramref name="client"/> is a component
		/// which has been marked as being required by the client's class using the <see cref="RequireComponent">RequireComponent attribute</see>
		/// or if the client has the  <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TArgument"> Type of the dependency of the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires <typeparamref name="TArgument"/>, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentForAutoInit<TClient, TArgument>([NotNull] TClient client) where TClient : IArgs<TArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				return result;
			}

			if(HasInitOnResetAttribute(clientType, 1))
            {
				shouldAutoInit[clientType] = true;
				return true;
			}

			if(typeof(MonoBehaviour).IsAssignableFrom(typeof(TClient)) && ArgumentIsRequiredComponentFor<TArgument>(clientType))
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
		/// or if the client has the  <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument>([NotNull] TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				return result;
			}

			if(HasInitOnResetAttribute(clientType, 2))
            {
				shouldAutoInit[clientType] = true;
				return true;
			}

			if(ArgumentIsRequiredComponentFor<TFirstArgument>(clientType) && ArgumentIsRequiredComponentFor<TSecondArgument>(clientType))
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
		/// or if the client has the  <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument>([NotNull] TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				return result;
			}

			if(HasInitOnResetAttribute(clientType, 3))
			{
				shouldAutoInit[clientType] = true;
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
		/// or if the client has the  <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>([NotNull] TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
			{
				return result;
			}

			if(HasInitOnResetAttribute(clientType, 4))
			{
				shouldAutoInit[clientType] = true;
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
		/// or if the client has the  <see cref="InitOnResetAttribute">AutoInit attribute</see>.
		/// </summary>
		/// <typeparam name="TClient"> Type or base type of the <paramref name="client"/>. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument required by the <paramref name="client"/> to test for being auto-initializable. </typeparam>
		/// <param name="client"> Client whose class is checked for the <see cref="RequireComponent"/> attribute. </param>
		/// <returns> <see langword="true"/> if the <paramref name="client"/> class requires all its dependencies, otherwise, <see langword="false"/>. </returns>
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>([NotNull] TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
        {
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
            {
				return result;
            }

			if(HasInitOnResetAttribute(clientType, 5))
			{
				shouldAutoInit[clientType] = true;
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
		/// or if the client has the  <see cref="InitOnResetAttribute">AutoInit attribute</see>.
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
		internal static bool TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>([NotNull] TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
        {
			var clientType = client.GetType();

			if(shouldAutoInit.TryGetValue(clientType, out bool result))
            {
				return result;
            }

			if(HasInitOnResetAttribute(clientType, 6))
			{
				shouldAutoInit[clientType] = true;
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
		/// <returns> Instance of type <typeparamref name="TArgument"/> or <see langword="null"/>. </returns>
		[CanBeNull]
		internal static TArgument GetAutoInitArgument<TClient, TArgument>([NotNull] TClient client, int argumentIndex)
		{
			#if DEV_MODE
			Debug.Assert(autoInitFrom.ContainsKey(client.GetType()), client.GetType().Name);
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
		[CanBeNull]
		internal static TArgument GetAutoInitArgument<TClient, TArgument>([NotNull] TClient client, int argumentIndex, From from)
		{
			var clientComponent = client as object as Component;
			bool clientIsComponent = clientComponent != null;

			bool fromScene = HasFlag(from, From.Scene);
			bool fromParent = fromScene || HasFlag(from, From.Parent);
			bool fromChildren = fromScene || HasFlag(from, From.Children);

			ObjectType argumentObjectType = GetObjectType(typeof(TArgument));			
			bool argumentIsInterface = argumentObjectType == ObjectType.Interface;
			bool argumentIsGameObjectOrTransform = argumentObjectType == ObjectType.GameObject || argumentObjectType == ObjectType.Transform;
			bool argumentIsComponentOrInterface = argumentIsInterface || argumentObjectType == ObjectType.Component;
			bool argumentIsObjectOrInterface = argumentIsInterface || argumentObjectType == ObjectType.Object;

			ObjectType argumentCollectionObjectType = argumentObjectType != ObjectType.None ? ObjectType.None : GetCollectionObjectType(typeof(TArgument));
			bool argumentIsInterfaceCollection = argumentCollectionObjectType == ObjectType.Interface;
			bool argumentIsGameObjectOrTransformCollection = argumentCollectionObjectType == ObjectType.GameObject || argumentCollectionObjectType == ObjectType.Transform;
			bool argumentIsComponentOrInterfaceCollection = argumentIsInterface || argumentCollectionObjectType == ObjectType.Component;
			bool argumentIsObjectOrInterfaceCollection = argumentIsInterfaceCollection || argumentCollectionObjectType == ObjectType.Object;

			if(clientIsComponent)
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
					else if(argumentIsComponentOrInterface)
					{
						if(Find.In<TArgument>(clientComponent.gameObject, out var argument))
						{
							return argument;
						}
					}
					else if(argumentIsGameObjectOrTransformCollection)
					{
						if(argumentCollectionObjectType == ObjectType.GameObject)
						{
							return ConvertToCollectionOfType<TArgument, GameObject>(new GameObject[] { clientComponent.gameObject });
						}
						return ConvertToCollectionOfType<TArgument, Transform>(new Transform[] { clientComponent.transform });
					}
					else if(argumentIsComponentOrInterfaceCollection)
                    {
						var argument = Find.AllIn<TArgument>(clientComponent.gameObject);
						if(argument.Length > 0 || from == From.GameObject)
                        {
							return ConvertToCollectionOfType<TArgument, Component>(clientComponent.GetComponents(GetCollectionGenericArgumentType(typeof(TArgument))));
						}
					}
					else if(typeof(TArgument) == typeof(GameObject))
					{
						return (TArgument)(object)clientComponent.gameObject;
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
					else if(argumentIsComponentOrInterface)
					{
						if(Find.InChildren<TArgument>(clientComponent.gameObject, out var argument, true))
						{
							return argument;
						}
					}
					else if(argumentIsGameObjectOrTransformCollection)
					{
						int count = clientComponent.transform.childCount;
						if(argumentObjectType == ObjectType.GameObject)
						{
							var gameObjects = new GameObject[count];
                            for(int i = count - 1; i >= 0; i--)
                            {
								gameObjects[i] = clientComponent.transform.GetChild(i).gameObject;
							}
							return ConvertToCollectionOfType<TArgument, GameObject>(gameObjects);
						}
						var transforms = new Transform[count];
						for(int i = count - 1; i >= 0; i--)
						{
							transforms[i] = clientComponent.transform.GetChild(i);
						}
						return ConvertToCollectionOfType<TArgument, Transform>(transforms);
					}
					else if(argumentIsComponentOrInterfaceCollection)
                    {
						var components = clientComponent.GetComponentsInChildren(GetCollectionGenericArgumentType(typeof(TArgument)), true);
						if(components.Length > 0 || (!fromParent && !fromScene))
						{
							return ConvertToCollectionOfType<TArgument, Component>(components);
						}
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
					else if(argumentIsComponentOrInterface)
					{
						if(Find.InParents<TArgument>(clientComponent.gameObject, out var argument, true))
						{
							return argument;
						}
					}
					else if(argumentIsGameObjectOrTransformCollection)
					{
						if(argumentObjectType == ObjectType.GameObject)
						{
							var gameObjects = new List<GameObject>();
							for(var parent = clientComponent.transform.parent; parent != null; parent = parent.parent)
                            {
								gameObjects.Add(parent.gameObject);
							}
							return ConvertToCollectionOfType<TArgument, GameObject>(gameObjects.ToArray());
						}
						var transforms = new List<Transform>();
						for(var parent = clientComponent.transform.parent; parent != null; parent = parent.parent)
						{
							transforms.Add(parent);
						}
						return ConvertToCollectionOfType<TArgument, Transform>(transforms.ToArray());
					}
					else if(argumentIsComponentOrInterfaceCollection)
                    {
						var components = clientComponent.GetComponentsInParent(GetCollectionGenericArgumentType(typeof(TArgument)), true);
						if(components.Length > 0 || !fromScene)
						{
							return ConvertToCollectionOfType<TArgument, Component>(components);
						}
					}
				}
			}

			if(fromScene)
			{
				#if UNITY_EDITOR
				#if UNITY_2020_1_OR_NEWER
				var prefabStage = UnityEditor.SceneManagement.StageUtility.GetStage(clientComponent.gameObject);
				bool inPrefabStage = prefabStage != null;
				#else
				var prefabStage = UnityEditor.SceneManagement.StageUtility.GetStageHandle(clientComponent.gameObject);
				bool inPrefabStage = prefabStage.IsValid();
				#endif
				bool isPrefabAssetOrOpenInPrefabStage = inPrefabStage || !clientComponent.gameObject.scene.IsValid();
				var root = clientComponent.gameObject.transform.root.gameObject;
				#endif

				if(argumentIsGameObjectOrTransform)
                {
					if(argumentObjectType == ObjectType.GameObject)
					{
						return (TArgument)(object)clientComponent.gameObject;
					}
					return (TArgument)(object)clientComponent.transform;
				}

				if(argumentIsComponentOrInterface)
				{
					// First try to find an active instance
					TArgument argument;
					#if UNITY_EDITOR						
					if(isPrefabAssetOrOpenInPrefabStage)
					{
						argument = Find.InChildren<TArgument>(root, false);
					}
					else
					#endif
					{
						argument = Find.Any<TArgument>(false);
					}
					argument = Find.In<TArgument>(clientComponent.gameObject, Including.Scene);

					if(argument != null)
					{
						return argument;
					}

					#if UNITY_EDITOR						
					if(isPrefabAssetOrOpenInPrefabStage)
					{
						argument = Find.InChildren<TArgument>(root, true);
					}
					else
					#endif
					{
						argument = Find.Any<TArgument>(true);
					}

					if(argument != null)
					{
						return argument;
					}
				}
				else if(argumentIsGameObjectOrTransformCollection)
                {
					Transform[] transforms = Find.AllIn<Transform>(root, Including.Scene | Including.Inactive);
					if(argumentObjectType == ObjectType.Transform)
                    {
						return ConvertToCollectionOfType<TArgument, Transform>(transforms);
					}
					var gameObjects = transforms.Select(t => t.gameObject).ToArray();
					return ConvertToCollectionOfType<TArgument, GameObject>(gameObjects);
                }
				else if(argumentIsComponentOrInterfaceCollection)
				{
					var componentOrInterfaceType = GetCollectionGenericArgumentType(typeof(TArgument));

					#if UNITY_EDITOR						
					if(isPrefabAssetOrOpenInPrefabStage)
					{
						return ConvertToCollectionOfType<TArgument, object>(Find.AllInChildren(root, componentOrInterfaceType, true));
					}
					#endif

					var allInSameScene = Find.AllIn(clientComponent.gameObject, componentOrInterfaceType, Including.Scene | Including.Inactive);
					return ConvertToCollectionOfType<TArgument, object>(allInSameScene);
				}
			}

			#if UNITY_EDITOR
			if(HasFlag(from, From.Assets))
			{
				if(argumentIsObjectOrInterface)
				{
					if(!typeof(TArgument).IsAbstract && typeof(Object).IsAssignableFrom(typeof(TArgument)))
					{
						foreach(var guid in UnityEditor.AssetDatabase.FindAssets("t:" + typeof(TArgument)))
						{
							string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
							var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TArgument));
							if(asset != null && typeof(TArgument).IsAssignableFrom(asset.GetType()))
							{
								return (TArgument)(object)asset;
							}
						}
					}

					// Check derived types too because AssetDatabase.FindAssets only matches exact types.
					foreach(var derivedType in GetDerivedTypes(typeof(TArgument)))
					{
						foreach(var guid in UnityEditor.AssetDatabase.FindAssets("t:" + derivedType))
						{
							string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
							TArgument asset = (TArgument)(object)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TArgument));
							if(asset != null)
							{
								return asset;
							}
						}
					}
				}
				else if(argumentIsObjectOrInterfaceCollection)
				{
					var list = new List<Object>();

					var elementType = GetCollectionGenericArgumentType(typeof(TArgument));
					if(typeof(Object).IsAssignableFrom(elementType))
					{
						foreach(var guid in UnityEditor.AssetDatabase.FindAssets("t:" + typeof(TArgument)))
						{
							string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
							var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TArgument));
							if(asset != null && typeof(TArgument).IsAssignableFrom(asset.GetType()))
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
								if(asset != null && elementType.IsAssignableFrom(asset.GetType()))
								{
									list.Add(asset);
								}
							}
						}
					}

					return ConvertToCollectionOfType<TArgument, Object>(list.ToArray());
				}
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
					return ConvertToCollectionOfType<TArgument, Object>(new Object[0]);
                }

				if(typeof(Type).IsAssignableFrom(typeof(TArgument)))
                {
					return default;
                }

				try
				{
					return Activator.CreateInstance<TArgument>();
				}
				catch(Exception e)
                {
					Debug.LogError(e);
					return default;
                }
            }

			return default;
        }

		[CanBeNull]
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

		private static TypeCollection GetDerivedTypes([NotNull] Type inheritedType)
        {
            #if UNITY_EDITOR
			return UnityEditor.TypeCache.GetTypesDerivedFrom(inheritedType);
            #else
            return TypeUtility.GetDerivedTypes(inheritedType, inheritedType.Assembly, typeof(InitArgs).Assembly);
            #endif
        }

		private static ObjectType GetCollectionObjectType(Type type)
        {
			if(!typeof(IEnumerable).IsAssignableFrom(type))
			{
				return ObjectType.None;
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

		private static Type GetCollectionGenericArgumentType(Type type)
        {
			foreach(var interfaceType in type.GetInterfaces())
            {
				if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
					return interfaceType.GetGenericArguments()[0];
                }
            }
			return null;
		}

		private static TCollection ConvertToCollectionOfType<TCollection, TObject>(TObject[] source)
		{
			// Array
			if(typeof(TCollection).IsArray)
			{
				var elementType = typeof(TCollection).GetElementType();
				if(elementType == typeof(TObject))
				{
					return (TCollection)(object)source;
				}
				int count = source.Length;
				var array = Array.CreateInstance(elementType, count);
				Array.Copy(source, array, count);
				return (TCollection)(object)array;
			}

			// List or one of the IEnumerable interfaces implemented by list
			if(typeof(TCollection).IsGenericType)
            {
				var genericType = typeof(TCollection).GetGenericTypeDefinition();
				if(genericType == typeof(List<>) || genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>) || genericType == typeof(IEnumerable) || genericType == typeof(IList<>) || genericType == typeof(IReadOnlyCollection<>) || genericType == typeof(IReadOnlyList<>) || genericType == typeof(ICollection) || genericType == typeof(IList))
                {
					var argumentType = GetCollectionGenericArgumentType(typeof(TCollection));
					var parameterTypeGeneric = typeof(IEnumerable<>);
					var parameterType = parameterTypeGeneric.MakeGenericType(argumentType);
					var listType = typeof(List<>).MakeGenericType(argumentType);
					var listConstructor = listType.GetConstructor(new Type[] { parameterType });
					if(listConstructor != null)
					{
						return (TCollection)listConstructor.Invoke(new object[] { source });
					}
				}
			}

			return default;
		}

        /// <summary>
        /// Determines whether or not <typeparamref name="TArgument"/> is a <see cref="Component"/> type
        /// which has been marked as being required by the <typeparamref name="TClient"/> class
        /// through the usage of the <see cref="RequireComponent"/> attribute.
        /// </summary>
        /// <typeparam name="TArgument"> Type to test whether or not it is a component type required by the <typeparamref name="TClient"/> class. </typeparam>
        /// <param name="classType"> The <see cref="MonoBehaviour"/> type to check for the <see cref="RequireComponent"/> attribute. </param>
        /// <returns> <see langword="true"/> if <typeparamref name="TClient"/> requires <typeparamref name="TArgument"/>, otherwise false. </returns>
        private static bool ArgumentIsRequiredComponentFor<TArgument>([NotNull]Type classType)
		{
			if(!typeof(Component).IsAssignableFrom(typeof(TArgument)) && !typeof(TArgument).IsInterface)
            {
				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"ArgumentIsRequiredComponentFor<{typeof(TArgument).Name}, {classType.Name}>() : false - argument was not a component nor interface.");
				#endif

				return false;
            }

			foreach(var attribute in classType.GetCustomAttributes(true))
            {
				RequireComponent requireComponent = attribute as RequireComponent;
				if(requireComponent == null)
                {
					continue;
                }

				if(requireComponent.m_Type0 != null && (requireComponent.m_Type0.IsAssignableFrom(typeof(TArgument)) || typeof(TArgument).IsAssignableFrom(requireComponent.m_Type0)))
				{
					return true;
				}

				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"RequireComponent(typeof({requireComponent.m_Type0.Name})) != RequireComponent(typeof({typeof(TArgument).Name}))");
				#endif

				if(requireComponent.m_Type1 != null && (requireComponent.m_Type1.IsAssignableFrom(typeof(TArgument)) || typeof(TArgument).IsAssignableFrom(requireComponent.m_Type1)))
				{
					return true;
				}

				if(requireComponent.m_Type2 != null && (requireComponent.m_Type2.IsAssignableFrom(typeof(TArgument)) || typeof(TArgument).IsAssignableFrom(requireComponent.m_Type2)))
				{
					return true;
				}
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"ArgumentIsRequiredComponentFor<{typeof(TArgument).Name}, {classType.Name}>() : false - no RequireComponent(typeof({typeof(TArgument).Name})) found on the class {classType.Name}.\nClass contained attributes: {string.Join("\n", classType.GetCustomAttributes(true))}.");
			#endif

			return false;
        }

		/// <summary>
		/// Returns a value indicating whether or not the class of type <paramref name="clientType"/> contains the <see cref="InitOnResetAttribute"/>
		/// and caches <see cref="From"/> value of each argument in <see cref="autoInitFrom"/> with <paramref name="clientType"/> as the key.
		/// </summary>
		/// <param name="clientType"> Type of the class to check for the attribute. </param>
		/// <param name="argumentCount"> Number of arguments that the class accepts during initialization. </param>
		/// <param name="clientType"> Type of the client that will receive the arguments. </param>
		/// <returns> <see langword="true"/> if <paramref name="clientType"/> class contains the <see cref="InitOnResetAttribute"/>; otherwise, <see langword="false"/>. </returns>
		private static bool HasInitOnResetAttribute([NotNull] Type clientType, int argumentCount)
        {
			Type[] argumentTypes = null;

			foreach(var attribute in clientType.GetCustomAttributes(true))
            {
				InitOnResetAttribute autoInit = attribute as InitOnResetAttribute;
				if(autoInit == null)
                {
					continue;
                }

				switch(argumentCount)
                {
					case 1:
						autoInitFrom[clientType] = new From[]
						{
							GetFinalFromValue(autoInit.first, clientType, 0, argumentCount, ref argumentTypes)
						};
						return true;
					case 2:
						autoInitFrom[clientType] = new From[]
						{
							GetFinalFromValue(autoInit.first, clientType, 0, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.second, clientType, 1, argumentCount, ref argumentTypes)
						};
						return true;
					case 3:
						autoInitFrom[clientType] = new From[]
						{
							GetFinalFromValue(autoInit.first, clientType, 0, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.second, clientType, 1, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.third, clientType, 2, argumentCount, ref argumentTypes)
						};
						return true;
					case 4:
						autoInitFrom[clientType] = new From[]
						{
							GetFinalFromValue(autoInit.first, clientType, 0, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.second, clientType, 1, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.third, clientType, 2, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.fourth, clientType, 3, argumentCount, ref argumentTypes)
						};
						return true;
					case 5:
						autoInitFrom[clientType] = new From[]
						{
							GetFinalFromValue(autoInit.first, clientType, 0, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.second, clientType, 1, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.third, clientType, 2, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.fourth, clientType, 3, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.fifth, clientType, 4, argumentCount, ref argumentTypes)
						};
						return true;
					default:
						autoInitFrom[clientType] = new From[]
						{
							GetFinalFromValue(autoInit.first, clientType, 0, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.second, clientType, 1, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.third, clientType, 2, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.fourth, clientType, 3, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.fifth, clientType, 4, argumentCount, ref argumentTypes),
							GetFinalFromValue(autoInit.sixth, clientType, 5, argumentCount, ref argumentTypes)
						};
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// If <paramref name="userProvidedValue"/> <see cref="From"/>-value from <see cref="InitOnResetAttribute"/> <see cref="From.Default"/>,
		/// then returns a <see cref="From"/> value fitting for the argument type. Otherwise just returns <paramref name="userProvidedValue"/>.
		/// </summary>
		private static From GetFinalFromValue(From userProvidedValue, Type clientType, int argumentIndex, int argumentCount, ref Type[] argumentTypes)
        {
			if(userProvidedValue != From.Default)
            {
				return userProvidedValue;
            }

			if(argumentTypes == null)
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
							}
							break;
						case 2:
							if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,>))
							{
								argumentTypes = interfaceType.GetGenericArguments();
							}
							break;
						case 3:
							if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,>))
							{
								argumentTypes = interfaceType.GetGenericArguments();
							}
							break;
						case 4:
							if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,,>))
							{
								argumentTypes = interfaceType.GetGenericArguments();
							}
							break;
						case 5:
							if(interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,,,>))
							{
								argumentTypes = interfaceType.GetGenericArguments();
							}
							break;
					}
                }

			}

			if(argumentTypes == null || argumentTypes.Length <= argumentIndex)
            {
				return From.Anywhere;
            }

			var genericArgument = argumentTypes[argumentIndex];
			if(typeof(Component).IsAssignableFrom(genericArgument))
            {
				if(typeof(Transform).IsAssignableFrom(genericArgument))
				{
					return From.GameObject;
				}
				return From.Children | From.Parent | From.Scene;
            }

			if(genericArgument.IsInterface)
            {
				return From.Children | From.Parent | From.Scene;
			}

			if(typeof(Object).IsAssignableFrom(genericArgument))
            {
				if(genericArgument == typeof(GameObject))
                {
					return From.GameObject;
                }
				return From.Assets;
			}

			if(typeof(IEnumerable<GameObject>).IsAssignableFrom(genericArgument) || typeof(IEnumerable<Transform>).IsAssignableFrom(genericArgument) || typeof(IEnumerable<RectTransform>).IsAssignableFrom(genericArgument))
			{
				return From.Children;
			}			

			return From.CreateInstance;
		}

		private static bool HasFlag(From value, From flag)
		{
			return (value & flag) == flag;
		}
    }
}
#endif