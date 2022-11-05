using HarmonyLib;

namespace Pancake.Console
{
	public interface IPatch
	{
		void Apply(Harmony harmony);
		void Remove(Harmony harmony);
	}
}