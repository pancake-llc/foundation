using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Default implementation of the <see cref="Wrapper{}"/> base class for times when the wrapped object
	/// doesn't have a Wrapper component made specifically for it.
	/// </summary>
	[AddComponentMenu(Hidden)]
	internal sealed class DefaultWrapper : Wrapper<object>
	{
		private const string Hidden = "";
	}
}