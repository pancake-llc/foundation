using Sisus.Init;
using Pancake.Game.Interfaces;

namespace Pancake.Game
{
	/// <summary>
	/// Initializer for the <see cref="Player"/> component.
	/// </summary>
	[EditorIcon("icon_initializer")]
	internal sealed class PlayerInitializer : Initializer<Player, IPlayerStat>
	{
		#if UNITY_EDITOR
		/// <summary>
		/// This section can be used to customize how the Init arguments will be drawn in the Inspector.
		/// <para>
		/// The Init argument names shown in the Inspector will match the names of members defined inside this section.
		/// </para>
		/// <para>
		/// Any PropertyAttributes attached to these members will also affect the Init arguments in the Inspector.
		/// </para>
		/// </summary>
		private sealed class Init
		{
			public IPlayerStat Stat = default;
		}
		#endif
	}
}
