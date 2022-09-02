namespace Pancake.InitArgs
{
	/// <summary>
	/// Represents an object that can provide a <see cref="Value"/> of type <typeparamref name="TValue"/> on demand.
	/// <para>
	/// If a class derives from <see cref="UnityEngine.Object"/> and implements <see cref="IValueProvider{T}"/> then
	/// <see cref="Any{T}"/> can wrap an instance of this class and return its <see cref="IValueProvider{T}.Value"/>
	/// when <see cref="Any{T}.Value"/> is called.
	/// </para>
	/// </summary>
	/// <typeparam name="TValue"> Type of the provided value. </typeparam>
	public interface IValueProvider<TValue> : IValueProvider
	{
		/// <summary>
		/// Gets the value of the current <see cref="IValueProvider{TValue}"/> object.
		/// </summary>
		new TValue Value { get; }
	}

	/// <summary>
	/// Represents an object that can provide a <see cref="Value"/> of type <typeparamref name="TValue"/> on demand.
	/// <para>
	/// Base interface of <see cref="IValueProvider{TValue}"/>.
	/// </para>
	/// </summary>
	public interface IValueProvider
	{
		/// <summary>
		/// Gets the value of the current <see cref="IValueProvider"/> object.
		/// </summary>
		object Value { get; }
	}
}
