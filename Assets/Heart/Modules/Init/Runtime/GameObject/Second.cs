namespace Sisus.Init
{
	/// <summary>
	/// <see cref="Second.Component">Second.Component</see> token can be used with a <see cref="GameObjectT2Extensions.Init1{Second}"/> function to inform that
	/// the second added component should be used as one of the arguments when calling the first added component's <see cref="IInitializable{TArgument}.Init">Init</see> function.
	/// </summary>
	public enum Second
	{
		/// <summary>
		/// <see cref="Second.Component">Second.Component</see> token can be used with a <see cref="GameObjectT2Extensions.Init1{Second}"/> function to inform that
		/// the second added component should be used as one of the arguments when calling the first added component's <see cref="IInitializable{TArgument}.Init">Init</see> function.
		/// </summary>
		Component
	}
}