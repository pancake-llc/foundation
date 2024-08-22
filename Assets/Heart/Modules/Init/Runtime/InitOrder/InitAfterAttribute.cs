using System;

namespace Sisus.Init
{
	/// <summary>
	/// Attribute that can be added to an <see cref="IInitializer"/> class to specify that it should
	/// initialize its client only after some other types have been initialized.
	/// <para>
	/// This attribute only has an effect in situations where the client of the Initializer that contains
	/// this attribute, as well as instances of the <see cref="types"/> specified in the constructor,
	/// exist in the same scene, or the same prefab, so that they are loaded as part of the same batch.
	/// </para>
	/// </summary>
	/// <seealso cref="InitOrderAttribute"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class InitAfterAttribute : Attribute
	{
		/// <summary>
		/// Types that should be initialized by their respective Initializers before
		/// the Initializer with this attribute initializes its client.
		/// </summary>
		public readonly Type[] types;

		/// <summary>
		/// Attribute that can be added to an <see cref="IInitializer"/> class to specify that it should
		/// initialize its client only after some other types have been initialized.
		/// </summary>
		/// <param name="types">
		/// Types that should be initialized by their respective Initializers before the Initializer
		/// with this attribute initializes its client.
		/// </param>
		public InitAfterAttribute(params Type[] types) => this.types = types;
	}
}