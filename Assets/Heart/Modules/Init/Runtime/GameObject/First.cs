namespace Sisus.Init
{
	/// <summary>
	/// <see cref="First.Component">First.Component</see> token can be used with a <see cref="GameObjectT2Extensions.Init2{First}"/> function to inform that
	/// the first added component should be used as one of the arguments when calling the second added component's <see cref="IInitializable{TArgument}.Init">Init</see> function.
	/// </summary>
	public enum First
	{
		/// <summary>
		/// <see cref="First.Component">First.Component</see> token can be used with an <see cref="GameObjectT2Extensions.Init2{First}"/> function to inform that
		/// the first added component should be used as one of the arguments when calling the second added component's <see cref="IInitializable{TArgument}.Init">Init</see> function.
		/// </summary>
		Component
	}
}