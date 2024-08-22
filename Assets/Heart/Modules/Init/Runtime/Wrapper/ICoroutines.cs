using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object that can <see cref="ICoroutinesExtensions.StartCoroutine">start</see>
	/// and <see cref="ICoroutinesExtensions.StopCoroutine">stop</see> <see cref="Coroutine">coroutines</see>.
	/// </summary>
	public interface ICoroutines
	{
		/// <summary>
		/// Object that can be used to <see cref="ICoroutinesExtensions.StartCoroutine">start</see>
		/// and <see cref="ICoroutinesExtensions.StopCoroutine">stop</see> <see cref="Coroutine">coroutines</see>.
		/// </summary>
		[MaybeNull]
		ICoroutineRunner CoroutineRunner
		{
			get => Find.wrappedInstances.TryGetValue(this, out IWrapper wrapper) ? wrapper : Service.Get<ICoroutineRunner>();
			set => throw new NotImplementedException($"The default implementation of {nameof(ICoroutines)}.{nameof(CoroutineRunner)} does not support values being assigned to it. Implement the property in {GetType().Name} to add support for this.");
		}
	}
}