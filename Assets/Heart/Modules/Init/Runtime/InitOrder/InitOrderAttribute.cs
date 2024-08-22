using System;

namespace Sisus.Init
{
	/// <summary>
	/// Specifies the execution order for the script during initialization and other event functions.
	/// </summary>
	/// <seealso cref="InitAfterAttribute"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class InitOrderAttribute : UnityEngine.DefaultExecutionOrder
	{
		/// <summary>
		/// Specifies the execution order for the script during initialization and other event functions.
		/// </summary>
		/// <param name="category"> The category of the component. </param>
		/// <param name="order"> The execution of the component order within the category. </param>
		public InitOrderAttribute(Category category, int order) : base((int)category + order) { }

		/// <summary>
		/// Specifies the execution order for the script during initialization and other event functions.
		/// </summary>
		/// <param name="category"> The category of the component. </param>
		/// <param name="order"> The execution of the component order within the category. </param>
		public InitOrderAttribute(Category category, Order order) : base((int)category + (int)order) { }
	}
}