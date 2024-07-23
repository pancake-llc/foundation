namespace Sisus.Init
{
	/// <summary>
	/// Represents the argument of an <see cref="IInitializable{TArgument}"/> class.
	/// </summary>
	/// <typeparam name="TArgument"> The type of the argument. </typeparam>
	public readonly struct Arg<TArgument>
	{
		/// <summary>
		/// An empty <see cref="Args{TArgument}"/> when no argument has been provided.
		/// </summary>
		public static readonly Arg<TArgument> None = new Arg<TArgument>(default, false);

		/// <summary>
		/// The provided argument or default value if no argument has been provided.
		/// </summary>
		public readonly TArgument argument;

		/// <summary>
		/// Indicates whether or not any arguments were provided for the client.
		/// </summary>
		public readonly bool provided;

		/// <summary>
		/// Initializes a new instance of the <see cref="Args{TArgument}"/> struct.
		/// </summary>
		/// <param name="argument"> The provided argument. </param>
		public Arg(TArgument argument)
		{
			this.argument = argument;
			provided = true;
		}

		private Arg(TArgument argument, bool provided)
		{
			this.argument = argument;
			this.provided = provided;
		}
	}

	/// <summary>
	/// Represents the arguments of an <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> class.
	/// </summary>
	/// <typeparam name="TFirstArgument"> The type of the argument. </typeparam>
	/// <typeparam name="TSecondArgument"> The type of the argument </typeparam>
	public readonly struct Args<TFirstArgument, TSecondArgument>
	{
		/// <summary>
		/// An empty <see cref="Args{TFirstArgument, TSecondArgument}"/> when no arguments have been provided.
		/// </summary>
		public static readonly Args<TFirstArgument, TSecondArgument> None = new Args<TFirstArgument, TSecondArgument>(default, default, false);

		/// <summary>
		/// The first provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFirstArgument firstArgument;

		/// <summary>
		/// The second provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TSecondArgument secondArgument;

		/// <summary>
		/// Indicates whether or not any arguments were provided for the client.
		/// </summary>
		public readonly bool provided;

		/// <summary>
		/// Initializes a new instance of the <see cref="Args{TFirstArgument, TSecondArgument}"/> struct.
		/// </summary>
		/// <param name="firstArgument"> The first provided argument. </param>
		/// <param name="secondArgument"> The second provided argument. </param>
		public Args(TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			provided = true;
		}

		private Args(TFirstArgument firstArgument, TSecondArgument secondArgument, bool provided)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.provided = provided;
		}
	}

	/// <summary>
	/// Represents the arguments of an <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> class.
	/// </summary>
	/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
	public readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument>
	{
		/// <summary>
		/// An empty <see cref="Args{TFirstArgument, TSecondArgument, TThirdArgument}"/> when no arguments have been provided.
		/// </summary>
		public static readonly Args<TFirstArgument, TSecondArgument, TThirdArgument> None = new Args<TFirstArgument, TSecondArgument, TThirdArgument>(default, default, default, false);

		/// <summary>
		/// The first provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFirstArgument firstArgument;

		/// <summary>
		/// The second provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TSecondArgument secondArgument;

		/// <summary>
		/// The third provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TThirdArgument thirdArgument;

		/// <summary>
		/// Indicates whether or not any arguments were provided for the client.
		/// </summary>
		public readonly bool provided;

		/// <summary>
		/// Initializes a new instance of the <see cref="Args{TFirstArgument, TSecondArgument, TThirdArgument}"/> struct.
		/// </summary>
		/// <param name="firstArgument"> The first provided argument. </param>
		/// <param name="secondArgument"> The second provided argument. </param>
		/// <param name="thirdArgument"> The third provided argument. </param>
		public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			provided = true;
		}

		private Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, bool provided)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.provided = provided;
		}
	}

	/// <summary>
	/// Represents the arguments of an <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> class.
	/// </summary>
	/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
	public readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
	{
		/// <summary>
		/// An empty <see cref="Args{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> when no arguments have been provided.
		/// </summary>
		public static readonly Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> None = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(default, default, default, default, false);

		/// <summary>
		/// The first provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFirstArgument firstArgument;

		/// <summary>
		/// The second provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TSecondArgument secondArgument;

		/// <summary>
		/// The third provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TThirdArgument thirdArgument;

		/// <summary>
		/// The fourth provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFourthArgument fourthArgument;

		/// <summary>
		/// Indicates whether or not any arguments were provided for the client.
		/// </summary>
		public readonly bool provided;

		/// <summary>
		/// Initializes a new instance of the <see cref="Args{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> struct.
		/// </summary>
		/// <param name="firstArgument"> The first provided argument. </param>
		/// <param name="secondArgument"> The second provided argument. </param>
		/// <param name="thirdArgument"> The third provided argument. </param>
		/// <param name="fourthArgument"> The fourth provided argument. </param>
		public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;
			provided = true;
		}

		private Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, bool provided)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;
			this.provided = provided;
		}
	}

	/// <summary>
	/// Represents the arguments of an <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> class.
	/// </summary>
	/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
	public readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
	{
		/// <summary>
		/// An empty <see cref="Args{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> when no arguments have been provided.
		/// </summary>
		public static readonly Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> None = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(default, default, default, default, default, false);

		/// <summary>
		/// Indicates whether or not any arguments were provided for the client.
		/// </summary>
		public readonly bool provided;

		/// <summary>
		/// The first provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFirstArgument firstArgument;

		/// <summary>
		/// The second provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TSecondArgument secondArgument;

		/// <summary>
		/// The third provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TThirdArgument thirdArgument;

		/// <summary>
		/// The fourth provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFourthArgument fourthArgument;

		/// <summary>
		/// The fifth provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFifthArgument fifthArgument;

		/// <summary>
		/// Initializes a new instance of the <see cref="Args{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> struct.
		/// </summary>
		/// <param name="firstArgument"> The first provided argument. </param>
		/// <param name="secondArgument"> The second provided argument. </param>
		/// <param name="thirdArgument"> The third provided argument. </param>
		/// <param name="fourthArgument"> The fourth provided argument. </param>
		/// <param name="fifthArgument"> fifth provided argument. </param>
		public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;
			this.fifthArgument = fifthArgument;
			provided = true;
		}

		private Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, bool provided)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;
			this.fifthArgument = fifthArgument;
			this.provided = provided;
		}
	}

	/// <summary>
	/// Represents the arguments of an <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> class.
	/// </summary>
	/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
	/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
	public readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
	{
		/// <summary>
		/// An empty <see cref="Args{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> when no arguments have been provided.
		/// </summary>
		public static readonly Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> None = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(default, default, default, default, default, default, false);

		/// <summary>
		/// Indicates whether or not any arguments were provided for the client.
		/// </summary>
		public readonly bool provided;

		/// <summary>
		/// The first provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFirstArgument firstArgument;

		/// <summary>
		/// The second provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TSecondArgument secondArgument;

		/// <summary>
		/// The third provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TThirdArgument thirdArgument;

		/// <summary>
		/// The fourth provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFourthArgument fourthArgument;

		/// <summary>
		/// The fifth provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TFifthArgument fifthArgument;

		/// <summary>
		/// The sixth provided argument or default value if no arguments have been provided.
		/// </summary>
		public readonly TSixthArgument sixthArgument;

		/// <summary>
		/// Initializes a new instance of the <see cref="Args{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> struct.
		/// </summary>
		/// <param name="firstArgument"> The first provided argument. </param>
		/// <param name="secondArgument"> The second provided argument. </param>
		/// <param name="thirdArgument"> The third provided argument. </param>
		/// <param name="fourthArgument"> The fourth provided argument. </param>
		/// <param name="fifthArgument"> fifth provided argument. </param>
		/// <param name="sixthArgument"> sixth provided argument. </param>
		public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;
			this.fifthArgument = fifthArgument;
			this.sixthArgument = sixthArgument;
			provided = true;
		}

		private Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, bool provided)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;
			this.fifthArgument = fifthArgument;
			this.sixthArgument = sixthArgument;
			this.provided = provided;
		}
	}
}