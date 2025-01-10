namespace Sisus.Init
{
	/// <summary>
	/// A base class for an object that depends on one service.
	/// <para>
	/// If the object depends on a class that has the <see cref="ServiceAttribute"/> then
	/// it will be able to receive it in its <see cref="Init"/> method automatically during initialization.
	/// </para>
	/// </summary>
	/// <typeparam name="TArgument"> Type of the argument received in the <see cref="Init"/> method. </typeparam>
	public abstract class Requires<TArgument> : IInitializable<TArgument>
	{
		public Requires()
		{
			if(InitArgs.TryGet(Context.Constructor, this, out TArgument argument))
			{
				Init(argument);
			}
		}

		public Requires(TArgument argument)
		{
			Init(argument);
		}

		void IInitializable<TArgument>.Init(TArgument argument) => Init(argument);

		protected abstract void Init(TArgument argument);
	}

	/// <summary>
	/// A base class for an object that depends on two services.
	/// <para>
	/// If the object depends exclusively on objects that have been registered as services using the <see cref="ServiceAttribute"/>,
	/// then it will be able to receive the services in its <see cref="Init"/> method automatically during its initialization.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument received in the <see cref="Init"/> method. </typeparam>
	public abstract class Requires<TFirstArgument, TSecondArgument>
		: IInitializable<TFirstArgument, TSecondArgument>
	{
		public Requires()
		{
			if(InitArgs.TryGet(Context.Constructor, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument))
			{
				Init(firstArgument, secondArgument);
			}
		}

		public Requires(TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			Init(firstArgument, secondArgument);
		}

		void IInitializable<TFirstArgument, TSecondArgument>
			.Init(TFirstArgument firstArgument, TSecondArgument secondArgument)
			=> Init(firstArgument, secondArgument);

		protected abstract void Init(TFirstArgument firstArgument, TSecondArgument secondArgument);
	}

	/// <summary>
	/// A base class for an object that depends on three services.
	/// <para>
	/// If the object depends exclusively on objects that have been registered as services using the <see cref="ServiceAttribute"/>,
	/// then it will be able to receive the services in its <see cref="Init"/> method automatically during its initialization.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument received in the <see cref="Init"/> method. </typeparam>
	public abstract class Requires<TFirstArgument, TSecondArgument, TThirdArgument>
		: IInitializable<TFirstArgument, TSecondArgument, TThirdArgument>
	{
		public Requires()
		{
			if(InitArgs.TryGet(Context.Constructor, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument))
			{
				Init(firstArgument, secondArgument, thirdArgument);
			}
		}

		public Requires(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
		{
			Init(firstArgument, secondArgument, thirdArgument);
		}

		void IInitializable<TFirstArgument, TSecondArgument, TThirdArgument>
			.Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
			=> Init(firstArgument, secondArgument, thirdArgument);

		protected abstract void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument);
	}

	/// <summary>
	/// A base class for an object that depends on four services.
	/// <para>
	/// If the object depends exclusively on objects that have been registered as services using the <see cref="ServiceAttribute"/>,
	/// then it will be able to receive the services in its <see cref="Init"/> method automatically during its initialization.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument received in the <see cref="Init"/> method. </typeparam>
	public abstract class Requires<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		: IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
	{
		public Requires()
		{
			if(InitArgs.TryGet(Context.Constructor, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument))
			{
				Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
			}
		}

		public Requires(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
		{
			Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
		}

		void IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			.Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
			=> Init(firstArgument, secondArgument, thirdArgument, fourthArgument);

		protected abstract void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument);
	}

	/// <summary>
	/// A base class for an object that depends on five services.
	/// <para>
	/// If the object depends exclusively on objects that have been registered as services using the <see cref="ServiceAttribute"/>,
	/// then it will be able to receive the services in its <see cref="Init"/> method automatically during its initialization.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument received in the <see cref="Init"/> method. </typeparam>
	public abstract class Requires<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		: IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
	{
		public Requires()
		{
			if(InitArgs.TryGet(Context.Constructor, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument))
			{
				Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			}
		}

		public Requires(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
		{
			Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
		}

		void IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			.Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
			=> Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);

		protected abstract void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument);
	}

	/// <summary>
	/// A base class for an object that depends on six services.
	/// <para>
	/// If the object depends exclusively on objects that have been registered as services using the <see cref="ServiceAttribute"/>,
	/// then it will be able to receive the services in its <see cref="Init"/> method automatically during its initialization.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument received in the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument received in the <see cref="Init"/> method. </typeparam>
	public abstract class Requires<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		: IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
	{
		public Requires()
		{
			if(InitArgs.TryGet(Context.Constructor, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument))
			{
				Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			}
		}

		public Requires(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
		{
			Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
		}

		void IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			.Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
			=> Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);

		protected abstract void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument);
	}
}