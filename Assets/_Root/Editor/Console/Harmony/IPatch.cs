using HarmonyLib;

namespace Needle.Console
{
	public interface IPatch
	{
		void Apply(Harmony harmony);
		void Remove(Harmony harmony);
	}
}