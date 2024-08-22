namespace Sisus.Init
{
	/// <summary>
	/// <see cref="Third.Component">Third.Component</see> token can be used with a <see cref="GameObjectT3Extensions.Init1{Third}"/>
	/// or <see cref="GameObjectT3Extensions.Init2{Third}"/> function to inform that the third added component should be used as one
	/// of the arguments when calling the first or second added component's <see cref="IInitializable{Third}.Init">Init</see> function.
	/// </summary>
	public enum Third
	{
		/// <summary>
		/// <see cref="Third.Component">Third.Component</see> token can be used with a <see cref="GameObjectT3Extensions.Init1{Third}"/>
		/// or <see cref="GameObjectT3Extensions.Init2{Third}"/> function to inform that the third added component should be used as one
		/// of the arguments when calling the first or second added component's <see cref="IInitializable{Third}.Init">Init</see> function.
		/// </summary>
		Component
	}
}