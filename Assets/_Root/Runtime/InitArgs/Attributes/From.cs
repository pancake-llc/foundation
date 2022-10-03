using System;
using static Pancake.Init.FlagsValues;

namespace Pancake.Init
{
	/// <summary>
	/// Defines where to search when trying to automatically locate an argument for
	/// a class that implements an <see cref="IArgs{}"/> interface.
	/// <seealso cref="InitOnResetAttribute"/>
	/// </summary>
	[Flags]
	public enum From
	{
		Default = _0,

		GameObject = _1,
		Children = _2,
		Parent = _3,
		GetOrAddComponent = _4,
		Scene = _5,
		Assets = _6,
		CreateInstance = _7,

		ChildrenOrParent = Children | Parent,
		Anywhere = Children | Parent | Scene | Assets
	}
}