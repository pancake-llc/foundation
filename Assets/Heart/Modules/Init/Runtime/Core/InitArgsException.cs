using System;

namespace Sisus.Init
{
	/// <summary>
	/// Base class for all custom exceptions that can be thrown by Init(args)'s API.
	/// </summary>
	public abstract class InitArgsException : Exception
	{
		protected InitArgsException(string message) : base(message) { }
		protected InitArgsException(string message, Exception innerException) : base(message, innerException) { }
	}
}