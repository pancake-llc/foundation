using System;
using UnityEngine;
using static Sisus.Init.FlagsValues;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// Defines where to search when trying to automatically locate an argument for
	/// a class that implements an <see cref="IArgs{}"/> interface.
	/// </summary>
	/// <seealso cref="InitOnResetAttribute"/>
	[Flags]
	public enum From
	{
		/// <summary>
		/// Locates the argument using the default search method for the argument type in question:
		/// <list type="bullet">
		/// <item>
		/// <term> <see cref="GameObject"/> </term>
		/// <description> <see cref="From.GameObject"/>. </description>
		/// </item>
		/// <item>
		/// <term> <see cref="Transform"/> </term>
		/// <description> <see cref="From.GameObject"/>. </description>
		/// </item>
		/// <item>
		/// <term> <see cref="Component"/> </term>
		/// <description> <see cref="From.Children"/> | <see cref="From.Parent"/> | <see cref="From.SameScene"/>. </description>
		/// </item>
		/// <item>
		/// <term> <see cref="interface"/> </term>
		/// <description> <see cref="From.Children"/> | <see cref="From.Parent"/> | <see cref="From.SameScene"/>. </description>
		/// </item>
		/// <item>
		/// <term> Other <see cref="Object"/> </term>
		/// <description> <see cref="From.Assets"/>. </description>
		/// </item>
		/// <item>
		/// <term> <see cref="GameObject"/> or <see cref="Component"/> Collection </term>
		/// <description> <see cref="From.Children"/>. </description>
		/// </item>
		/// <item>
		/// <term> Other </term>
		/// <description> <see cref="From.CreateInstance"/>. </description>
		/// </item>
		/// </list>
		/// </summary>
		Default = _0,

		GameObject = _1,

		/// <summary>
		/// <see cref="Find.InChildren"/>
		/// </summary>
		Children = _2,

		/// <summary>
		/// <see cref="Find.InParents"/>
		/// </summary>
		Parent = _3,
		GetOrAddComponent = _4,

		[Obsolete("Use " + nameof(SameScene) + " or " + nameof(AnyScene) + " instead.")]
		Scene = _5,

		SameScene = _5,
		Assets = _6,
		CreateInstance = _7,
		AnyScene = _8,

		ChildrenOrParent = Children | Parent,
		Anywhere = Children | Parent | SameScene | AnyScene | Assets,

		/// <summary>
		/// Retrieves the argument from the Initializer of the client.
		/// </summary>
		Initializer = -1,
	}
}