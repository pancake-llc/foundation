namespace Sisus.Init
{
	/// <summary>
	/// Defines the default execution orders of various components.
	/// <para>
	/// Certain event functions on scripts with lower values
	/// are executed before ones on scripts with larger values.
	/// </para>
	/// </summary>
	public static class ExecutionOrder
	{
		/// <summary>
		/// Default execution order for the <see cref="Referenceable"/> component.
		/// </summary>
		public const int Referenceable = -32000;

		/// <summary>
		/// Default execution order for the <see cref="ServiceTag"/> and <see cref="Init.ServiceTag"/> components.
		/// </summary>
		public const int ServiceTag = -31900;

		/// <summary>
		/// Default execution order for all <see cref="Initializer{,}">Initializer</see> components
		/// targeting a class that has the <see cref="ServiceAttribute"/>.
		/// </summary>
		public const int ServiceInitializer = -31300;

		/// <summary>
		/// Default execution order for all <see cref="Initializer{,}">Initializer</see> components
		/// targeting a <see cref="Wrapper{}"/> class that does not have the <see cref="ServiceAttribute"/>.
		/// </summary>
		public const int WrapperInitializer = -30000;

		/// <summary>
		/// Default execution order for all <see cref="Initializer{,}">Initializer</see> components
		/// targeting a class that does not have the <see cref="ServiceAttribute"/>.
		/// </summary>
		public const int Initializer = -20000;

		/// <summary>
		/// Largest possible script execution order value;
		/// </summary>
		public const int MaxValue = 32000;

		/// <summary>
		/// Smallest possible script execution order value;
		/// </summary>
		public const int MinValue = -32000;
		
	}
}