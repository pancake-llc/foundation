using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Extensions methods for initializing <see cref="GameObject{TFirstComponent, TSecondComponent, TThirdComponent}"/> objects.
	/// </summary>
	public static class GameObjectT3Extensions
	{
		#region InitAll

		/// <summary>
		/// Initializes all added components without any additional arguments.
		/// <para>
		/// If the first added component implements <see cref="IArgs{TSecondComponent}"/>, <see cref="IArgs{TThirdComponent}"/>
		/// or <see cref="IArgs{TSecondComponent, TThirdComponent}"/> then its Init function will get called with the matching components as arguments.
		/// </para>
		/// <para>
		/// If the second added component implements <see cref="IArgs{TFirstComponent}"/>, <see cref="IArgs{TThirdComponent}"/>
		/// or <see cref="IArgs{TFirstComponent, TThirdComponent}"/> then its Init function will get called with the matching components as arguments.
		/// </para>
		/// <para>
		/// If the third added component implements <see cref="IArgs{TFirstComponent}"/>, <see cref="IArgs{TSecondComponent}"/>
		/// or <see cref="IArgs{TFirstComponent, TSecondComponent}"/> then its Init function will get called with the matching components as arguments.
		/// </para>
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>,
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();

			Components<TFirstComponent, TSecondComponent, TThirdComponent> result = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TFirstComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out result.first))
				{
					result.first = @this.gameObject.AddComponent<TFirstComponent>();
				}
			}
			else
			{
				result.first = @this.gameObject.AddComponent<TFirstComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out result.second))
			{
				result.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out result.third))
			{
				result.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.OnAfterLastInit();
			return result;
		}

		#endregion

		#region Init1

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> without any arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();

			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TFirstComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.first))
				{
					components.first = @this.gameObject.AddComponent<TFirstComponent>();
				}
			}
			else
			{
				components.first = @this.gameObject.AddComponent<TFirstComponent>();
			}

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> with one argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TArgument"> Type of the argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"> The argument used when <see cref="IInitializable{TArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TArgument argument)
				where TFirstComponent : Component, IArgs<TArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();

			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>()
			{
				first = AddAndInit.Component<TFirstComponent, TArgument>(@this.gameObject, argument, !@this.leaveInactive)
			};

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the provided argument and the second added component.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, Second secondArgument)
				where TFirstComponent : Component, ITwoArguments, IFirstArgument<TFirstArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondComponent>(@this.gameObject, firstArgument, components.second, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the second added component and the provided argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TSecondComponent, TSecondArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TSecondComponent, TSecondArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component, ITwoArguments, ISecondArgument<TSecondArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TSecondArgument>(@this.gameObject, components.second, secondArgument, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the two provided arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component, ITwoArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>
			{
				first = AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument>(@this.gameObject, firstArgument, secondArgument, !@this.leaveInactive)
			};
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the three provided arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();

			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>()
			{
				first = AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, !@this.leaveInactive)
			};

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using three arguments with the second added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the third argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, Second thirdArgument)
				where TFirstComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TSecondComponent>(@this.gameObject, firstArgument, secondArgument, components.second, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using four arguments with the second added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the third argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, Second thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component, IFourArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IFourthArgument<TFourthArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument>(@this.gameObject, firstArgument, secondArgument, components.second, fourthArgument, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using five arguments with the second added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the third argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fifthArgument"> The fifth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument, TFifthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TFourthArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, Second thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument, TFifthArgument>(@this.gameObject, firstArgument, secondArgument, components.second, fourthArgument, fifthArgument, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using three arguments with the second added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, Second secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondComponent, TThirdArgument>(@this.gameObject, firstArgument, components.second, thirdArgument, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using four arguments with the second added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TThirdArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, Second secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component, IFourArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument>(@this.gameObject, firstArgument, components.second, thirdArgument, fourthArgument, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using five arguments with the second added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fifthArgument"> The fifth argument used when <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, Second secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument>(@this.gameObject, firstArgument, components.second, thirdArgument, fourthArgument, fifthArgument, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using three arguments with the second added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument>(@this.gameObject, components.second, secondArgument, thirdArgument, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the second added component as the argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"> The argument used when <see cref="IInitializable{TSecondComponent}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second argument)
				where TFirstComponent : Component, IOneArgument
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent>(@this.gameObject, components.second, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using two arguments
		/// with the second added component being the first one and the third added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TSecondComponent, TThirdComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TSecondComponent, TThirdComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TThirdComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, Third secondArgument)
				where TFirstComponent : Component, ITwoArguments
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();

			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out components.third))
			{
				components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TThirdComponent>(@this.gameObject, components.second, components.third, !@this.leaveInactive);

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using two arguments
		/// with the third added component being the first one and the second added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TThirdComponent, TFirstComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TThirdComponent, TFirstComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TThirdComponent, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third firstArgument, Second secondArgument)
				where TFirstComponent : Component, ITwoArguments
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TThirdComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.third))
				{
					components.third = @this.gameObject.AddComponent<TThirdComponent>();
				}
			}
			else
			{
				components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TThirdComponent, TSecondComponent>(@this.gameObject, components.third, components.second, !@this.leaveInactive);
			
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using three arguments
		/// with the third added component being the first one and the second added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the first argument when
		/// <see cref="IInitializable{TThirdComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TThirdComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TThirdComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TThirdComponent, TFirstComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third firstArgument, Second secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component, IThreeArguments, IThirdArgument<TThirdArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TThirdComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.third))
				{
					components.third = @this.gameObject.AddComponent<TThirdComponent>();
				}
			}
			else
			{
				components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TThirdComponent, TSecondComponent, TThirdArgument>(@this.gameObject, components.third, components.second, thirdArgument, !@this.leaveInactive);
			
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using three arguments
		/// with the third added component being the first one and the second added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the first added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TThirdComponent, TSecondArgument, TFirstComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the third argument when
		/// <see cref="IInitializable{TThirdComponent, TSecondArgument, TFirstComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TThirdComponent, TSecondArgument, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third firstArgument, TSecondArgument secondArgument, Second thirdArgument)
				where TFirstComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TThirdComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.third))
				{
					components.third = @this.gameObject.AddComponent<TThirdComponent>();
				}
			}
			else
			{
				components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TThirdComponent, TSecondArgument, TSecondComponent>(@this.gameObject, components.third, secondArgument, components.second, !@this.leaveInactive);
			
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using three arguments
		/// with the second added component being the first one and the third added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the first added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, Third secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component, IThreeArguments, IThirdArgument<TThirdArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out components.third))
			{
				components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument>(@this.gameObject, components.second, components.third, thirdArgument, !@this.leaveInactive);
			
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using four arguments
		/// with the second added component being the first one and the third added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, Third secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component, IFourArguments, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out components.third))
			{
				components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument, TFourthArgument>(@this.gameObject, components.second, components.third, thirdArgument, fourthArgument, !@this.leaveInactive);
			
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using five arguments
		/// with the second added component being the first one and the third added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fift argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument, TFourthArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, Third secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component, IFiveArguments, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out components.third))
			{
				components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument, TFourthArgument, TFifthArgument>(@this.gameObject, components.second, components.third, thirdArgument, fourthArgument, fifthArgument, !@this.leaveInactive);
			
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using four arguments
		/// with the second added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TSecondArgument, TThirdComponent, TThirdArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component, IFourArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, components.second, secondArgument, thirdArgument, fourthArgument, !@this.leaveInactive);
			
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using three arguments
		/// with the second added component being the first one and the third added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdComponent}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdComponent}.Init">initializing</see> the first added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, TSecondArgument secondArgument, Third thirdArgument)
				where TFirstComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>();

			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TSecondComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out components.second))
				{
					components.second = @this.gameObject.AddComponent<TSecondComponent>();
				}
			}
			else
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			if(!@this.gameObject.TryGetComponent(out components.third))
			{
				components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TSecondArgument, TThirdComponent>(@this.gameObject, components.second, secondArgument, components.third, !@this.leaveInactive);
			
			return new(@this.gameObject, !@this.leaveInactive, components);
		}
		
		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the four provided arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
			where TFirstComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			where TSecondComponent : Component
			where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();

			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>
			{
				first = AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, !@this.leaveInactive)
			};

			return new(@this.gameObject, !@this.leaveInactive, components);
		}
		
		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the five provided arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fifthArgument"> The fifth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
			where TFirstComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			where TSecondComponent : Component
			where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();

			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>
			{
				first = AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, !@this.leaveInactive)
			};

			return new(@this.gameObject, !@this.leaveInactive, components);
		}
		
		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the six provided arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSixthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="fifthArgument"> The fifth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializing</see> the first added component. </param>
		/// <param name="sixthArgument"> The sixth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializing</see> the first added component. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
			where TFirstComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			where TSecondComponent : Component
			where TThirdComponent : Component
		{
			@this.OnBeforeFirstInit();

			var components = new Components<TFirstComponent, TSecondComponent, TThirdComponent>
			{
				first = AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, !@this.leaveInactive)
			};

			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using a delegate.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="init"> Delegate pointing to a function that accepts the first added component and handles initializing it. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> and <see cref="Init3"/> to be called until it is ready for usage. </returns>
		public static GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init1<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this, [DisallowNull] Action<TFirstComponent> init)
			where TFirstComponent : Component
			where TSecondComponent : Component
			where TThirdComponent : Component
		{
			var result = @this.Init1();
			init(result.components.first);
			return result;
		}
		#endregion

		#region Init2

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> without any arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.second))
			{
				@this.components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using one argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TArgument"></typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"></param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TArgument argument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TArgument>(@this.gameObject, argument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TFirstArgument, TSecondArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument>(@this.gameObject, firstArgument, secondArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the argument when
		/// <see cref="IInitializable{TFirstComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First argument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IOneArgument
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent>(@this.gameObject, @this.components.first, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the argument when
		/// <see cref="IInitializable{TFirstComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TThirdComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third argument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IOneArgument
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TThirdComponent>(@this.gameObject, @this.components.third, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstArgument, TFirstComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, First secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, ITwoArguments, IFirstArgument<TFirstArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TFirstComponent>(@this.gameObject, firstArgument, @this.components.first, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, ITwoArguments, ISecondArgument<TSecondArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TSecondArgument>(@this.gameObject, @this.components.first, secondArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument, TThirdArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TSecondArgument, TThirdArgument>(@this.gameObject, @this.components.first, secondArgument, thirdArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using thre arguments with the first added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, First thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TFirstComponent>(@this.gameObject, firstArgument, secondArgument, @this.components.first, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments with the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TFirstComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TThirdArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, First secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TFirstComponent, TThirdArgument>(@this.gameObject, firstArgument, @this.components.first, thirdArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments
		/// with the first added component being the first one and the third added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TThirdComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TThirdComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TThirdComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, Third secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, ITwoArguments
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TThirdComponent>(@this.gameObject, @this.components.first, @this.components.third, @this.setActive);

			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments
		/// with the third added component being the first one and the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TThirdComponent, TFirstComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TThirdComponent, TFirstComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TThirdComponent, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third firstArgument, First secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, ITwoArguments
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TThirdComponent, TFirstComponent>(@this.gameObject, @this.components.third, @this.components.first, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments
		/// with the third added component being the first one and the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the first argument when
		/// <see cref="IInitializable{TThirdComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the second argument when
		/// <see cref="IInitializable{TThirdComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TThirdComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the second added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TThirdComponent, TFirstComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third firstArgument, First secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, IThirdArgument<TThirdArgument>
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TThirdComponent, TFirstComponent, TThirdArgument>(@this.gameObject, @this.components.third, @this.components.first, thirdArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments
		/// with the third added component being the first one and the first added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TThirdComponent, TSecondArgument, TFirstComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="thirdArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the third argument when
		/// <see cref="IInitializable{TThirdComponent, TSecondArgument, TFirstComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TThirdComponent, TSecondArgument, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third firstArgument, TSecondArgument secondArgument, First thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TThirdComponent, TSecondArgument, TFirstComponent>(@this.gameObject, @this.components.third, secondArgument, @this.components.first, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments
		/// with the third added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the first argument when
		/// <see cref="IInitializable{TThirdComponent, TSecondComponent, TThirdComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TThirdComponent, TSecondComponent, TThirdComponent}.Init">initializing</see> the second added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TThirdComponent, TSecondComponent, TThirdComponent}.Init">initializing</see> the second added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TThirdComponent, TSecondArgument, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument, TThirdArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TThirdComponent, TSecondArgument, TThirdArgument>(@this.gameObject, @this.components.third, secondArgument, thirdArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments
		/// with the third added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TThirdComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TThirdComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the second added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TThirdComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the second added component. </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TThirdComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the second added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TThirdComponent, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Third firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFourArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TThirdComponent, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, @this.components.third, secondArgument, thirdArgument, fourthArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments
		/// with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="thirdArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFourArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, @this.components.first, secondArgument, thirdArgument, fourthArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments
		/// with the first added component being the first one and the third added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the second added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TThirdComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, Third secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, IThirdArgument<TThirdArgument>
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TThirdComponent, TThirdArgument>(@this.gameObject, @this.components.first, @this.components.third, thirdArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments
		/// with the first added component being the first one and the third added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdComponent}.Init">initializing</see> the second added component. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Third.Component"/> token informing that the third added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdComponent}.Init">initializing</see> the second added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, TSecondArgument secondArgument, Third thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TSecondArgument, TThirdComponent>(@this.gameObject, @this.components.first, secondArgument, @this.components.third, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}
		
		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using a delegate.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="init"> Delegate pointing to a function that accepts the second added component and handles initializing it. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still <see cref="Init3"/> to be called until it is ready for usage. </returns>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this, [DisallowNull] Action<TSecondComponent> init)
			where TFirstComponent : Component
			where TSecondComponent : Component
			where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.second))
			{
				@this.components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			
			init(@this.components.second);
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using the four provided arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init3"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> Init2<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
				where TThirdComponent : Component
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, @this.setActive);
			
			return new(@this.gameObject, @this.setActive, @this.components);
		}

		#endregion

		#region Init3

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TThirdComponent"/> without any arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using one argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TArgument"></typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The argument used when <see cref="IInitializable{TArgument}.Init">initializing</see> the third added component.
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TArgument argument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IArgs<TArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TArgument>(@this.gameObject, argument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using two arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializing</see> the third added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializing</see> the third added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IArgs<TFirstArgument, TSecondArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstArgument, TSecondArgument>(@this.gameObject, firstArgument, secondArgument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using three arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using two arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TFirstComponent}.Init">initializing</see> the third added component. </param>
		/// <param name="secondArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstArgument, TFirstComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, First secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, ITwoArguments, IFirstArgument<TFirstArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstArgument, TFirstComponent>(@this.gameObject, firstArgument, @this.components.first, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using two arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstComponent, TSecondArgument}.Init">initializing</see> the third added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, ITwoArguments, ISecondArgument<TSecondArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstComponent, TSecondArgument>(@this.gameObject, @this.components.first, secondArgument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using three arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument, TThirdArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstComponent, TSecondArgument, TThirdArgument>(@this.gameObject, @this.components.first, secondArgument, thirdArgument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using the first added component as the argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"></param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First argument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IOneArgument
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstComponent>(@this.gameObject, @this.components.first, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using three arguments with the first added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TFirstComponent}.Init">initializing</see> the third added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TFirstComponent}.Init">initializing</see> the third added component. </param>
		/// <param name="thirdArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the third argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TFirstComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, First thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstArgument, TSecondArgument, TFirstComponent>(@this.gameObject, firstArgument, secondArgument, @this.components.first, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using three arguments with the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TFirstComponent, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <param name="secondArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstArgument, TFirstComponent, TThirdArgument}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TFirstComponent, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TFirstComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TThirdArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, First secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstArgument, TFirstComponent, TThirdArgument>(@this.gameObject, firstArgument, @this.components.first, thirdArgument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TThirdComponent"/> using two arguments
		/// with the first added component being the first one and the second added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, Second secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, ITwoArguments
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstComponent, TSecondComponent>(@this.gameObject, @this.components.first, @this.components.second, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TThirdComponent"/> using two arguments
		/// with the second added component being the first one and the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TSecondComponent, TFirstComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TSecondComponent, TFirstComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, First secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, ITwoArguments
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TSecondComponent, TFirstComponent>(@this.gameObject, @this.components.second, @this.components.first, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TThirdComponent"/> using three arguments
		/// with the second added component being the first one and the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TSecondComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the second argument when
		/// <see cref="IInitializable{TSecondComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TSecondComponent, TFirstComponent, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TFirstComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, First secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IThreeArguments, IThirdArgument<TThirdArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TSecondComponent, TFirstComponent, TThirdArgument>(@this.gameObject, @this.components.second, @this.components.first, thirdArgument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TThirdComponent"/> using three arguments
		/// with the second added component being the first one and the first added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TSecondComponent, TSecondArgument, TFirstComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="thirdArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the third argument when
		/// <see cref="IInitializable{TSecondComponent, TSecondArgument, TFirstComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument, TFirstComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			Second firstArgument, TSecondArgument secondArgument, First thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TSecondComponent, TSecondArgument, TFirstComponent>(@this.gameObject, @this.components.second, secondArgument, @this.components.first, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TThirdComponent"/> using three arguments
		/// with the first added component being the first one and the second added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}.Init">initializing</see> the third added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondComponent, TThirdArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TThirdArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, Second secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IThreeArguments, IThirdArgument<TThirdArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstComponent, TSecondComponent, TThirdArgument>(@this.gameObject, @this.components.first, @this.components.second, thirdArgument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TThirdComponent"/> using three arguments
		/// with the first added component being the first one and the second added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="First.Component"/> token informing that the first added component should be used as the first argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TSecondComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstComponent, TSecondArgument, TSecondComponent}.Init">initializing</see> the third added component. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when
		/// <see cref="IInitializable{TFirstComponent, TSecondArgument, TSecondComponent}.Init">initializing</see> the third added component.
		/// </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument, TSecondComponent}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TSecondArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			First firstArgument, TSecondArgument secondArgument, Second thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstComponent, TSecondArgument, TSecondComponent>(@this.gameObject, @this.components.first, secondArgument, @this.components.second, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TSecondComponent"/> using four arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the third added component. </param>
		/// <param name="secondArgument"> The second argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the third added component. </param>
		/// <param name="thirdArgument"> The third argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the third added component. </param>
		/// <param name="fourthArgument"> The fourth argument used when <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializing</see> the third added component. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TThirdComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not receive the arguments during its initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component
				where TThirdComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			@this.components.third = AddAndInit.Component<TThirdComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, @this.setActive);

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the third added component of type <typeparamref name="TThirdComponent"/> using a delegate.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="init"> Delegate pointing to a function that accepts the second added component and handles initializing it. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>, <typeparamref name="TThirdComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
		/// </returns>
		public static Components<TFirstComponent, TSecondComponent, TThirdComponent> Init3<TFirstComponent, TSecondComponent, TThirdComponent>(this GameObjectWithInit2Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this, [DisallowNull] Action<TThirdComponent> init)
			where TFirstComponent : Component
			where TSecondComponent : Component
			where TThirdComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.third))
			{
				@this.components.third = @this.gameObject.AddComponent<TThirdComponent>();
			}
			
			init(@this.components.third);

			@this.OnAfterInitialized();
			return @this.components;
		}

		#endregion
		
	}
}