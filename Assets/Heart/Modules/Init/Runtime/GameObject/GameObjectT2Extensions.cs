using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Extensions methods for initializing <see cref="GameObject{TFirstComponent, TSecondComponent}"/> objects.
	/// </summary>
	public static class GameObjectT2Extensions
	{
		#region InitBoth

		/// <summary>
		/// Initializes both added components without any additional arguments.
		/// <para>
		/// If the first added component implements <see cref="IArgs{TSecondComponent}"/> then its Init function
		/// will get called with the second added component passed as an argument.
		/// </para>
		/// <para>
		/// If the second added component implements <see cref="IArgs{TFirstComponent}"/> then its Init function
		/// will get called with the first added component passed as an argument.
		/// </para>
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		public static Components<TFirstComponent, TSecondComponent> Init<TFirstComponent, TSecondComponent>(this GameObject<TFirstComponent, TSecondComponent> @this)
				where TFirstComponent : Component
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();

			Components<TFirstComponent, TSecondComponent> result = new Components<TFirstComponent, TSecondComponent>();

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
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent>(this GameObject<TFirstComponent, TSecondComponent> @this)
				where TFirstComponent : Component
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();

			var components = new Components<TFirstComponent, TSecondComponent>();

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
		/// <typeparam name="TArgument"> Type of the argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"> The argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TArgument argument)
				where TFirstComponent : Component, IArgs<TArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when calling the first added component's Init function.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, Second secondArgument)
				where TFirstComponent : Component, ITwoArguments, IFirstArgument<TFirstArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
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
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when calling the first added component's Init function.
		/// </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TSecondArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			Second firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component, ITwoArguments, ISecondArgument<TSecondArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component, IArgs<TFirstArgument, TSecondArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>
			{
				first = AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, !@this.leaveInactive)
			};
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the four provided arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TFirstArgument, TSecondArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the first added component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TFirstComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>
			{
				first = AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, !@this.leaveInactive)
			};
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using four arguments with the second added component being the fourth one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the fourth argument when calling the first added component's Init function.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TSecondComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, Second fourthArgument)
				where TFirstComponent : Component, IFourArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument, TSecondComponent>(@this.gameObject, firstArgument, secondArgument, thirdArgument, components.second, !@this.leaveInactive);
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using five arguments with the second added component being the fourth one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the fourth argument when calling the first added component's Init function.
		/// </param>
		/// <param name="fifthArgument"> The fifth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TSecondComponent, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, Second fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFifthArgument<TFifthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument, TSecondComponent, TFifthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, components.second, fifthArgument, !@this.leaveInactive);
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using five arguments with the second added component being the fifth one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <param name="fifthArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the fifth argument when calling the first added component's Init function.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TSecondComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, Second fifthArgument)
				where TFirstComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			AddAndInit.Component<TFirstComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TSecondComponent>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, components.second, !@this.leaveInactive);
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using three arguments with the second added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the third argument when calling the first added component's Init function.
		/// </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, Second thirdArgument)
				where TFirstComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the third argument when calling the first added component's Init function.
		/// </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, Second thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component, IFourArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IFourthArgument<TFourthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the third argument when calling the first added component's Init function.
		/// </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TSecondComponent, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TFourthArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, Second thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when calling the first added component's Init function.
		/// </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, Second secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when calling the first added component's Init function.
		/// </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TThirdArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, Second secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component, IFourArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
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
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the first added component's Init function. </param>
		/// <param name="secondArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the second argument when calling the first added component's Init function.
		/// </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondComponent, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TFirstArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, Second secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
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
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when calling the first added component's Init function.
		/// </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			Second firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument>(@this.gameObject, components.second, secondArgument, thirdArgument, !@this.leaveInactive);
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using four arguments with the second added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when calling the first added component's Init function.
		/// </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			Second firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component, IFourArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, components.second, secondArgument, thirdArgument, fourthArgument, !@this.leaveInactive);
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using five arguments with the second added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument">
		/// The <see cref="Second.Component"/> token informing that the second added component should be used as the first argument when calling the first added component's Init function.
		/// </param>
		/// <param name="secondArgument"> The second argument passed to the first added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the first added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the first added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the first added component's Init function. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this GameObject<TFirstComponent, TSecondComponent> @this,
			Second firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component, IFiveArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(@this.gameObject, components.second, secondArgument, thirdArgument, fourthArgument, fifthArgument, !@this.leaveInactive);
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TFirstComponent"/> using the second added component as the argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"></param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TFirstComponent"/> class does not implement <see cref="IInitializable{TSecondComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent>(this GameObject<TFirstComponent, TSecondComponent> @this,
			Second argument)
				where TFirstComponent : Component, IOneArgument
				where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent>(@this.gameObject, components.second, !@this.leaveInactive);
			return new(@this.gameObject, !@this.leaveInactive, components);
		}
		
		/// <summary>
		/// Initialize the first added component of type <typeparamref name="TThirdComponent"/> using a delegate.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="init"> Delegate pointing to a function that accepts the second added component and handles initializing it. </param>
		/// <returns> Partially initialized <see cref="GameObject"/> still requiring <see cref="Init2"/> to be called until it is ready for usage. </returns>
		public static UninitializedGameObject<TFirstComponent, TSecondComponent> Init1<TFirstComponent, TSecondComponent>(this GameObject<TFirstComponent, TSecondComponent> @this, [DisallowNull] Action<TFirstComponent> init)
			where TFirstComponent : Component
			where TSecondComponent : Component
		{
			@this.OnBeforeFirstInit();
			var components = new Components<TFirstComponent, TSecondComponent>();
			if(!@this.gameObject.TryGetComponent(out components.second))
			{
				components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			components.first = AddAndInit.Component<TFirstComponent, TSecondComponent>(@this.gameObject, components.second, !@this.leaveInactive);
			return new(@this.gameObject, !@this.leaveInactive, components);
		}

		#endregion

		#region Init2

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> without any arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this)
				where TFirstComponent : Component
				where TSecondComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.second))
			{
				@this.components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}

			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using one argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TArgument"></typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"></param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TArgument argument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TArgument>(@this.gameObject, argument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TFirstArgument, TSecondArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument>(@this.gameObject, firstArgument, secondArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using four arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using five arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using six arguments.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the second added component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TFirstComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, First secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, ITwoArguments, IFirstArgument<TFirstArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TFirstComponent>(@this.gameObject, firstArgument, @this.components.first, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using two arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TSecondArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			First firstArgument, TSecondArgument secondArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, ITwoArguments, ISecondArgument<TSecondArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TSecondArgument>(@this.gameObject, @this.components.first, secondArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			First firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TSecondArgument, TThirdArgument>(@this.gameObject, @this.components.first, secondArgument, thirdArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using four arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			First firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFourArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument>(@this.gameObject, @this.components.first, secondArgument, thirdArgument, fourthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using five arguments with the first added component being the first one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			First firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFiveArguments, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(@this.gameObject, @this.components.first, secondArgument, thirdArgument, fourthArgument, fifthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using the first added component as the argument.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"></param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			First argument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IOneArgument
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstComponent>(@this.gameObject, @this.components.first, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using thre arguments with the first added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TFirstComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, First thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TFirstComponent>(@this.gameObject, firstArgument, secondArgument, @this.components.first, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using four arguments with the first added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TFirstComponent, TFourthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TFourthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, First thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFourArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IFourthArgument<TFourthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TFirstComponent, TFourthArgument>(@this.gameObject, firstArgument, secondArgument, @this.components.first, fourthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using five arguments with the first added component being the third one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TFirstComponent, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TFourthArgument, TFifthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, First thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TFirstComponent, TFourthArgument, TFifthArgument>(@this.gameObject, firstArgument, secondArgument, @this.components.first, fourthArgument, fifthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using four arguments with the first added component being the fourth one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFirstComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, First fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFourArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFirstComponent>(@this.gameObject, firstArgument, secondArgument, thirdArgument, @this.components.first, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using five arguments with the first added component being the fourth one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFirstComponent, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFifthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, First fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFifthArgument<TFifthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFirstComponent, TFifthArgument>(@this.gameObject, firstArgument, secondArgument, thirdArgument, @this.components.first, fifthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using five arguments with the first added component being the fifth one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFirstComponent}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, First fifthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFirstComponent>(@this.gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, @this.components.first, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using three arguments with the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TFirstComponent, TThirdArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TThirdArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, First secondArgument, TThirdArgument thirdArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IThreeArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TFirstComponent, TThirdArgument>(@this.gameObject, firstArgument, @this.components.first, thirdArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using four arguments with the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TFirstComponent, TThirdArgument, TFourthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TThirdArgument, TFourthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, First secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFourArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TFirstComponent, TThirdArgument, TFourthArgument>(@this.gameObject, firstArgument, @this.components.first, thirdArgument, fourthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}

		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using five arguments with the first added component being the second one.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the second added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the second added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the second added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the second added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the second added component's Init function. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TSecondComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TFirstComponent, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent, TFirstArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this,
			TFirstArgument firstArgument, First secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TFirstComponent : Component
				where TSecondComponent : Component, IFiveArguments, IFirstArgument<TFirstArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
		{
			@this.components.second = AddAndInit.Component<TSecondComponent, TFirstArgument, TFirstComponent, TThirdArgument, TFourthArgument, TFifthArgument>(@this.gameObject, firstArgument, @this.components.first, thirdArgument, fourthArgument, fifthArgument, @this.setActive);
			@this.OnAfterInitialized();
			return @this.components;
		}
		
		/// <summary>
		/// Initialize the second added component of type <typeparamref name="TSecondComponent"/> using a delegate.
		/// </summary>
		/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
		/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="init"> Delegate pointing to a function that accepts the second added component and handles initializing it. </param>
		/// <returns>
		/// Created object which can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>,
		/// <typeparamref name="TSecondComponent"/>
		/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
		/// </returns>
		public static Components<TFirstComponent, TSecondComponent> Init2<TFirstComponent, TSecondComponent>(this UninitializedGameObject<TFirstComponent, TSecondComponent> @this, [DisallowNull] Action<TSecondComponent> init)
			where TFirstComponent : Component
			where TSecondComponent : Component
		{
			if(!@this.gameObject.TryGetComponent(out @this.components.second))
			{
				@this.components.second = @this.gameObject.AddComponent<TSecondComponent>();
			}
			
			init(@this.components.second);

			@this.OnAfterInitialized();
			return @this.components;
		}

		#endregion
	}
}