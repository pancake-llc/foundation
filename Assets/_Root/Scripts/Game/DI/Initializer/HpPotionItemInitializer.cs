using Sisus.Init;
using Pancake.Game.Interfaces;

namespace Pancake.Game
{
	/// <summary>
	/// Initializer for the <see cref="HpPotionItem"/> component.
	/// </summary>
	[EditorIcon("icon_initializer")]
	internal sealed class HpPotionItemInitializer : Initializer<HpPotionItem, IPlayerStat, IEventTrigger>
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
			public IPlayerStat PlayerStat = default;
			public IEventTrigger OnCollected = default;
		}
		#endif
	}
}
