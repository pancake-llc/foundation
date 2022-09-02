using JetBrains.Annotations;

namespace Pancake.Init
{
	/// <summary>
	/// Utility class containing methods related to retrieving arguments for objects that implement the
	/// <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface such as
	/// <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> and
	/// <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	public static class InitArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
	{
		/// <summary>
		/// Calls the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see> function on the <paramref name="client"/>
		/// with the arguments previously stored using the <see cref="InitArgs.Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}">Set</see> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GameObject.GetComponent">GetComponent</see>
		/// and <see cref="Object.FindObjectOfType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </typeparam>
		/// <param name="client"> The object to receive the dependencies. </typeparam>
		/// <exception cref="ArgumentNullException" > Thrown if the <paramref name="client"/> argument is <see langword="null"/>. </exception>
		public static bool TryGet<TClient>(Context context, [NotNull] TClient client) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			if(InitArgs.TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument))
			{
				client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return true;
			}
			return false;
		}
	}
}