namespace Pancake
{
	public interface IPoolable
	{
		/// <summary>
		/// This method will be called on 2nd Request call.
		/// Use Unity's Awake method for first initialization and this method for others
		/// </summary>
		void OnRequest();
		void OnReturn();
	}
}
