using static Sisus.Init.FlagsValues;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Specifies the different states of initialization
	/// that a client that depends on some services can have.
	/// </summary>
	public enum InitState : byte
	{
		/// <summary>
		/// Client's Init function has not been executed yet.
		/// </summary>
		Uninitialized = _0,

		/// <summary>
		/// Client's Init function has been executed but has not finished yet.
		/// </summary>
		Initializing = _1,

		/// <summary>
		/// Client's Init function has been executed and it has finished.
		/// </summary>
		Initialized = _2
	}
}
