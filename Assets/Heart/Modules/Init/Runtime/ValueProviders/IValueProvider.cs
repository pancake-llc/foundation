using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Scripting;
using static Sisus.NullExtensions;

namespace Sisus.Init
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
	/// <seealso cref="IValueProviderAsync{TValue}"/>
	/// <seealso cref="IValueByTypeProvider"/>
	/// <seealso cref="IValueByTypeProviderAsync"/>
	[RequireImplementors]
	public interface IValueProvider<TValue> : IValueProvider
	{
		/// <summary>
		/// Gets the value of type <typeparamref name="TValue"/> provided by this object.
		/// </summary>
		[MaybeNull]
		new TValue Value { get; }

		/// <summary>
		/// Gets the value of type <typeparamref name="TValue"/> provided by this object.
		/// </summary>
		[MaybeNull]
		object IValueProvider.Value => Value;

		/// <summary>
		/// Gets a value indicating whether this value provider can provide a value of type
		/// <typeparamref name="TValue"/> for the <paramref name="client"/> at this time.
		/// </summary>
		/// <param name="client">
		/// The component requesting the value, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if can provide a value for the client at this time; otherwise, <see langword="false"/>.
		/// </returns>
		new bool HasValueFor(Component client)
			=> this is INullGuard nullGuard
				? nullGuard.EvaluateNullGuard(client) == NullGuardResult.Passed
				: TryGetFor(client, out _);

		/// <summary>
		/// Gets the value of type <typeparamref name="TValue"/> for the <paramref name="client"/>.
		/// </summary>
		/// <param name="client">
		/// The component requesting the value, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <param name="value">
		/// When this method returns, contains the value of type <typeparamref name="TValue"/>, if available; otherwise, the default value of <typeparamref name="TValue"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if a value was provided; otherwise, <see langword="false"/>.
		/// </returns>
		bool TryGetFor([AllowNull] Component client, [NotNullWhen(true), MaybeNullWhen(false)] out TValue value)
		{
			value = Value;
			return value != Null;
		}
	}

	/// <summary>
	/// Represents an object that can provide a value on demand.
	/// <para>
	/// Base interface of <see cref="IValueProvider{TValue}"/>.
	/// </para>
	/// </summary>
	[RequireImplementors]
	public interface IValueProvider
	{
		/// <summary>
		/// Gets the value provided by this object.
		/// </summary>
		[MaybeNull]
		object Value { get; }

		bool TryGetFor([AllowNull] Component client, [NotNullWhen(true), MaybeNullWhen(false)] out object value)
		{
			value = Value;
			return value != Null;
		}

		/// <summary>
		/// Gets a value indicating whether this value provider can provide a value
		/// for the <paramref name="client"/> at this time.
		/// </summary>
		/// <param name="client">
		/// The component requesting the value, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if can provide a value for the client at this time; otherwise, <see langword="false"/>.
		/// </returns>
		bool HasValueFor(Component client)
			=> this is INullGuard nullGuard
				? nullGuard.EvaluateNullGuard(client) == NullGuardResult.Passed
				: TryGetFor(client, out _);
	}
}