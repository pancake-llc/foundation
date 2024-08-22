using System;
using UnityEngine;
using static Sisus.Init.FlagsValues;

namespace Sisus.Init
{
	/// <summary>
	/// Defines which additional GameObjects to include in search when
	/// finding objects attached to a GameObject or related GameObjects.
	/// </summary>
	/// <seealso cref="Find.In{T}(GameObject, Including)"/>
	/// <seealso cref="Find.AllIn{T}(GameObject, Including)"/>
	[Flags]
	public enum Including
	{
		/// <summary>
		/// Search the GameObject, its children and its grandchildren.
		/// </summary>
		Children = _1,

		/// <summary>
		/// Search the GameObject, its parent or and its grandparents.
		/// </summary>
		Parents = _2,

		/// <summary>
		/// Search all GameObjects in the scene that the GameObject is part of.
		/// </summary>
		Scene = _3,

		/// <summary>
		/// Search <see cref="GameObject.activeInHierarchy">inactive</see>
		/// <see cref="GameObjects"/> in addition to active ones.
		/// </summary>
		Inactive = _4,

		/// <summary>
		/// Search the GameObject, its children, its grandchildren, its parent and its grandparents.
		/// <para>
		/// Only <see cref="GameObject.activeInHierarchy">active</see> children and parents will be included in the search.
		/// </para>
		/// </summary>
		ChildrenAndParents = Children | Parents,

		/// <summary>
		/// Search the GameObject, its children, its grandchildren, its parent and its grandparents.
		/// <para>
		/// All children and parents will be included in the search regardless
		/// of their <see cref="GameObject.activeInHierarchy">active</see> state.
		/// </para>
		/// </summary>
		ChildrenAndParentsIncludingInactive = Children | Parents | Inactive,

		/// <summary>
		/// Search the GameObject, its parent and its grandparents.
		/// <para>
		/// All parents will be included in the search regardless
		/// of their <see cref="GameObject.activeInHierarchy">active</see> state.
		/// </para>
		/// </summary>
		ParentsIncludingInactive = Children | Inactive,

		/// <summary>
		/// Search the GameObject, its children and its grandchildren.
		/// <para>
		/// All children will be included in the search regardless
		/// of their <see cref="GameObject.activeInHierarchy">active</see> state.
		/// </para>
		/// </summary>
		ChildrenIncludingInactive = Children | Inactive,

		/// <summary>
		/// Search all GameObjects in the scene that the GameObject is part of.
		/// <para>
		/// All GameObjects that are part of the scene will be included in the search
		/// regardless of their <see cref="GameObject.activeInHierarchy">active</see> state.
		/// </para>
		/// </summary>
		SceneIncludingInactive = Scene | Inactive
	}
}