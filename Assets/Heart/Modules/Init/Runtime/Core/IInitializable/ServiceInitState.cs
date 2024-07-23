using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static Sisus.Init.FlagsValues;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Specifies the different states of initialization
	/// that a client that depends on some services can have.
	/// </summary>
	[Flags]
	public enum ServiceInitState
	{
		Started = _1,

		InstanceAcquired = _2,

		/// <summary>
		/// The process of acquiring Init arguments for service has been started.
		/// </summary>
		AcquiringDependenciesStarted = _3,

		/// <summary>
		/// Initialization of the service has finished successfully.
		/// </summary>
		Finished = _4,

		/// <summary>
		/// Initialization of the service has failed.
		/// </summary>
		Failed = _5,

		EventFunctionsExecuted = _6
	}

	internal static class ServiceInitStateExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
		public static bool IsDone(this ServiceInitState @this, ServiceInitState flag) => (@this & flag) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
		public static ServiceInitState WithFlag(this ServiceInitState @this, ServiceInitState flag) => @this | flag;
	}
}