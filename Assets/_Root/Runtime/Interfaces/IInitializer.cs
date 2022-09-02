using System;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Pancake
{
	/// <summary>
	/// Represents an object that can can specify the arguments used to
	/// <see cref="IInitializable{}.Init">initialize</see> objects
	/// that implement one of the <see cref="IInitializable{}"/> interfaces.
	/// <para>
	/// Implemented by all the <see cref="Initializer{,}">Initializer</see> base classes.
	/// </para>
	/// </summary>
	public interface IInitializer
	{
		/// <summary>
		/// Existing target instance to initialize, if any.
		/// <para>
		/// If value is <see langword="null"/> then the argument is injected to a new instance.
		/// </para>
		/// <para>
		/// If value is a component on another GameObject then the argument is injected to a new instance
		/// created by cloning the GameObject with the component.
		/// </para>
		/// </summary>
		[CanBeNull]
		Object Target { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not <see cref="Target"/> can be assigned to a field of type <paramref name="type"/>
		/// or can be converted to it via the <see cref="IValueProvider{T}"/> interface.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool TargetIsAssignableOrConvertibleToType(Type type);

		/// <summary>
		/// Initializes the target with the init arguments specified on this initializer.
		/// </summary>
		/// <returns> The existing <see cref="Target"/> or new instance of type <see cref="TargetType"/>. </returns>
		[NotNull]
		object InitTarget();
	}
}