namespace Sisus.Init.Internal
{
	/// <summary>
	/// Specifies the different states of initialization
	/// that a client that depends on some services can have.
	/// </summary>
	public enum InitState
	{
		/// <summary>
		/// Client's Init function has not been executed yet.
		/// </summary>
		Uninitialized = 0,

		/// <summary>
		/// Client's Init function has been executed but has not finished yet.
		/// </summary>
		Initializing = 1,

		/// <summary>
		/// Client's Init function has been executed and it has finished.
		/// </summary>
		Initialized = 2,

		/// <summary>
		/// Initialization of the client has failed.
		/// </summary>
		Failed = 3
	}	
}