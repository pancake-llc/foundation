using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// This attribute can be added to a class that derives from one of the <see cref="MonoBehaviour{T}"/>
	/// base classes to automatically initialize it with dependencies when the component is first attached
	/// to a <see cref="GameObject"/> in the editor or when the user selects 'Reset' in the Inspector's context menu.
	/// <para>
	/// The dependencies are automatically retrieved from the scene hierarchy, relative to the <see cref="GameObject"/>
	/// that contains the <see cref="MonoBehaviour{T}"/>. You can specify <see cref="From"/> where <see cref="Object"/>
	/// arguments should be searched, or use the default search mode, which tries to pick a good search mode to use
	/// based on the type of the argument:
	/// <list>
	/// <item> <see cref="Transform"/> or <see cref="GameObject"/>: <see cref="From.GameObject"/>, </item>
	/// <item> <see cref="Component"/> or interface: <see cref="From.Children"/> or <see cref="From.Parent"/> or <see cref="From.SameScene"/>, </item>
	/// <item> other <see cref="Object"/>: <see cref="From.Assets"/>, </item>
	/// <item>
	/// Collection of <see cref="Component"/>, interface, <see cref="Transform"/> or <see cref="GameObject"/>: <see cref="From.Children"/>.
	/// For example <see cref="Collider"/>[] or <see cref="IEnumerable{Transform}"/>.
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// The found arguments are passed to the component's <see cref="IInitializable{T}.Init"/> function,
	/// where they can be assigned to serialized fields.
	/// </para>
	/// <para>
	/// This behaviour only occurs in edit mode and is meant to make it more convenient to add components without
	/// needing to assign all <see cref="Object"/> references manually using the inspector.
	/// </para>
	/// <para>
	/// This attribute can also be added to classes that derive from one of the <see cref="InitializerBase{T}"/> base classes.
	/// In this case the end result is the same, except the arguments are routed into the initializer instead.
	/// </para>
	/// </summary>
	/// <seealso cref="InitInEditModeAttribute"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true), Conditional("UNITY_EDITOR")]
	public class InitOnResetAttribute : Attribute
	{
		protected const From defaultFrom = From.Default;

		/// <summary>
		/// Defines where to search when trying to retrieve the first dependency of the attribute holder.
		/// </summary>
		public readonly From first;

		/// <summary>
		/// Defines where to search when trying to retrieve the second dependency of the attribute holder.
		/// </summary>
		public readonly From second;

		/// <summary>
		/// Defines where to search when trying to retrieve the third dependency of the attribute holder.
		/// </summary>
		public readonly From third;

		/// <summary>
		/// Defines where to search when trying to retrieve the fourth dependency of the attribute holder.
		/// </summary>
		public readonly From fourth;

		/// <summary>
		/// Defines where to search when trying to retrieve the fifth dependency of the attribute holder.
		/// </summary>
		public readonly From fifth;

		/// <summary>
		/// Defines where to search when trying to retrieve the sixth dependency of the attribute holder.
		/// </summary>
		public readonly From sixth;

		/// <summary>
		/// When a component that derives from <see cref="MonoBehaviour{T}"/> and has the
		/// <see cref="InitOnResetAttribute"/> is first added to a GameObject in the editor,
		/// or when the user hits the Reset button in the Inspector's context menu,
		/// the arguments accepted by the component are automatically gathered
		/// and passed to its <see cref="IInitializable{T}.Init"/> function.
		/// <para>
		/// This auto-initialization behaviour only occurs in edit mode during the Reset event function
		/// and is meant to make it more convenient to add components without needing to assign all
		/// <see cref="Object"/> references manually through the inspector.
		/// </para>
		/// </summary>
		public InitOnResetAttribute() : this(defaultFrom) { }

		/// <summary>
		/// When a component that derives from <see cref="MonoBehaviour{T}"/> and has the
		/// <see cref="InitOnResetAttribute"/> is first added to a GameObject in the editor,
		/// or when the user hits the Reset button in the Inspector's context menu,
		/// the arguments accepted by the component are automatically gathered
		/// and passed to its <see cref="IInitializable{T}.Init"/> function.
		/// <para>
		/// This auto-initialization behaviour only occurs in edit mode during the Reset event function
		/// and is meant to make it more convenient to add components without needing to assign all
		/// <see cref="Object"/> references manually through the inspector.
		/// </para>
		/// </summary>
		/// <param name="search"> Defines where to search when trying to retrieve the arguments accepted by the attribute holder. </param>
		public InitOnResetAttribute(From search)
		{
			first = search;
			second = search;
			third = search;
			fourth = search;
			fifth = search;
			sixth = search;
		}

		/// <summary>
		/// When a component that derives from <see cref="MonoBehaviour{T}"/> and has the
		/// <see cref="InitOnResetAttribute"/> is first added to a GameObject in the editor,
		/// or when the user hits the Reset button in the Inspector's context menu,
		/// the arguments accepted by the component are automatically gathered
		/// and passed to its <see cref="IInitializable{T}.Init"/> function.
		/// <para>
		/// This auto-initialization behaviour only occurs in edit mode during the Reset event function
		/// and is meant to make it more convenient to add components without needing to assign all
		/// <see cref="Object"/> references manually through the inspector.
		/// </para>
		/// </summary>
		/// <param name="first"> Defines where to search when trying to retrieve the first argument accepted by the attribute holder. </param>
		/// <param name="second"> Defines where to search when trying to retrieve the second argument accepted by the attribute holder. </param>
		/// <param name="third"> Defines where to search when trying to retrieve the third argument accepted by the attribute holder. </param>
		/// <param name="fourth"> Defines where to search when trying to retrieve the fourth argument accepted by the attribute holder. </param>
		/// <param name="fifth"> Defines where to search when trying to retrieve the fifth argument accepted by the attribute holder. </param>
		/// <param name="sixth"> Defines where to search when trying to retrieve the sixth argument accepted by the attribute holder. </param>
		public InitOnResetAttribute(From first, From second, From third = defaultFrom, From fourth = defaultFrom, From fifth = defaultFrom, From sixth = defaultFrom)
		{
			this.first = first;
			this.second = second;
			this.third = third;
			this.fourth = fourth;
			this.fifth = fifth;
			this.sixth = sixth;
		}
	}
}