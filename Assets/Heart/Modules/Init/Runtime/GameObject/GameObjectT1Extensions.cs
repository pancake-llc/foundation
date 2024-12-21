using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Extensions methods for initializing <see cref="GameObject{TComponent}"/> objects.
	/// </summary>
	public static class GameObjectT1Extensions
	{
		/// <summary>
		/// Initializes the added component without any arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <returns> The initialized component. </returns>
		public static TComponent Init<TComponent>(this GameObject<TComponent> @this) where TComponent : Component
		{
			@this.OnBeforeInit();

			TComponent component;
			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out component))
				{
					component = @this.gameObject.AddComponent<TComponent>();
				}
			}
			else
			{
				component = @this.gameObject.AddComponent<TComponent>();
			}

			@this.OnAfterInit();
			return component;
		}

		/// <summary>
		/// Initializes the added component with one argument.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the added component. </typeparam>
		/// <typeparam name="TArgument"> Type of the argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="argument"> The argument passed to the added component's Init function. </param>
		/// <returns> The initialized component. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static TComponent Init<TComponent, TArgument>(this GameObject<TComponent> @this,
			TArgument argument)
				where TComponent : Component, IArgs<TArgument>
		{
			@this.OnBeforeInit();

			TComponent component;
			if(!@this.isClone || !@this.gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), argument);

				component = @this.gameObject.AddComponent<TComponent>();

				if(!InitArgs.Clear<TArgument>(typeof(TComponent)))
				{
					@this.OnAfterInit();
					return component;
				}
			}

			if(component is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				@this.OnAfterInit();
				return component;
			}

			@this.OnBeforeException();
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		/// <summary>
		/// Initializes the added component with two arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the added component's Init function. </param>
		/// <returns> The initialized component. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static TComponent Init<TComponent, TFirstArgument, TSecondArgument>(this GameObject<TComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TComponent : Component, IArgs<TFirstArgument, TSecondArgument>
		{
			@this.OnBeforeInit();

			TComponent component;
			if(!@this.isClone || !@this.gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument);

				component = @this.gameObject.AddComponent<TComponent>();

				if(!InitArgs.Clear<TFirstArgument, TSecondArgument>(typeof(TComponent)))
				{
					@this.OnAfterInit();
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				@this.OnAfterInit();
				return component;
			}

			@this.OnBeforeException();
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		/// <summary>
		/// Initializes the added component with three arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the added component's Init function. </param>
		/// <returns> The initialized component. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static TComponent Init<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>(this GameObject<TComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			@this.OnBeforeInit();

			TComponent component;
			if(!@this.isClone || !@this.gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument);

				component = @this.gameObject.AddComponent<TComponent>();

				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument>(typeof(TComponent)))
				{
					@this.OnAfterInit();
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				@this.OnAfterInit();
				return component;
			}

			@this.OnBeforeException();
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		/// <summary>
		/// Initializes the added component with four arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the added component's Init function. </param>
		/// <returns> The initialized component. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static TComponent Init<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this GameObject<TComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			@this.OnBeforeInit();

			TComponent component;
			if(!@this.isClone || !@this.gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument);

				component = @this.gameObject.AddComponent<TComponent>();

				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(typeof(TComponent)))
				{
					@this.OnAfterInit();
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				@this.OnAfterInit();
				return component;
			}

			@this.OnBeforeException();
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		/// <summary>
		/// Initializes the added component with five arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the added component's Init function. </param>
		/// <returns> The initialized component. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static TComponent Init<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this GameObject<TComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			@this.OnBeforeInit();

			TComponent component;

			if(!@this.isClone || !@this.gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);

				component = @this.gameObject.AddComponent<TComponent>();

				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(typeof(TComponent)))
				{
					@this.OnAfterInit();
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				@this.OnAfterInit();
				return component;
			}

			@this.OnBeforeException();
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		/// <summary>
		/// Initializes the added component with six arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the added component. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="firstArgument"> The first argument passed to the added component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the added component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the added component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the added component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the added component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the added component's Init function. </param>
		/// <returns> The initialized component. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static TComponent Init<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(this GameObject<TComponent> @this,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TComponent : Component, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			@this.OnBeforeInit();

			TComponent component;

			if(!@this.isClone || !@this.gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);

				component = @this.gameObject.AddComponent<TComponent>();

				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(typeof(TComponent)))
				{
					@this.OnAfterInit();
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				@this.OnAfterInit();
				return component;
			}

			@this.OnBeforeException();
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		/// <summary>
		/// Initializes the added component using a delegate.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the added component. </typeparam>
		/// <param name="this"> new <see cref="GameObject"/> being initialized. </param>
		/// <param name="init"> Delegate pointing to a function that accepts the second added component and handles initializing it. </param>
		/// <returns> The initialized component. </returns>
		public static TComponent Init<TComponent>(this GameObject<TComponent> @this, [DisallowNull] Action<TComponent> init) where TComponent : Component
		{
			@this.OnBeforeInit();

			TComponent component;
			if(@this.isClone || typeof(Transform).IsAssignableFrom(typeof(TComponent)))
			{
				if(!@this.gameObject.TryGetComponent(out component))
				{
					component = @this.gameObject.AddComponent<TComponent>();
				}
			}
			else
			{
				component = @this.gameObject.AddComponent<TComponent>();
			}

			init(component);
			@this.OnAfterInit();
			return component;
		}
	}
}