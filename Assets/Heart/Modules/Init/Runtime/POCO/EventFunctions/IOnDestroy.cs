namespace Sisus.Init
{
	/// <summary>
	/// Defines a class that wants to receive a callback during the OnDestroy event function of the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see> that <see cref="Wrapper{TWrapped}">wraps</see> it.
	/// </summary>
	public interface IOnDestroy
	{
		/// <summary>
		/// <see cref="OnDestroy"/> is called when the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see> is destroyed.
		/// </summary>
		void OnDestroy();
	}
}