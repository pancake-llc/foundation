using System;

namespace Sisus.Init.Internal
{
	internal static class InitInEditModeUtility
	{
		public static event Action UpdateAllRequested;

		/// <summary>
		/// Requests InitInEditModeUpdater to re-initialize all objects
		/// in all scenes with the InitInEditModeAttribute.
		/// </summary>
		public static void UpdateAll() => UpdateAllRequested?.Invoke();
	}
}