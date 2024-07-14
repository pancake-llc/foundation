using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object that is responsible for providing an initialization argument, and can be
	/// validated by an initializer to verify that it will be able to fulfill that resposibility at runtime.
	/// </summary>
	public interface INullGuardByType
	{
		/// <summary>
		/// Gets a value indicating whether or not null guard passes for this object or not, and if not,
		/// what was the cause of the failure.
		/// </summary>
		/// <typeparam name="TValue"> Type of the value whose availability should be checked. </typeparam>
		/// <param name="client">
		/// The component performing the evaluation, if being performed by a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// Value representing the result of the null guard.
		/// </returns>
		NullGuardResult EvaluateNullGuard<TValue>([AllowNull] Component client);
	}
}
