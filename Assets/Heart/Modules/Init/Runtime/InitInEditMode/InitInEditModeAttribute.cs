using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// Add this attribute to classes to have them be initialized in Edit Mode whenever any objects
	/// in the same scene or prefab are modified.
	/// <para>
	/// This attribute can be added to any components that implement <see cref="IInitializable"/>, which includes all
	/// classes that derive from <see cref="MonoBehaviour{}"/>. When this is done, dependencies for the component are automatically
	/// retrieved from the scene hierarchy, relative to the <see cref="GameObject"/> that contains the component with the attribute.
	/// You can specify <see cref="From"/> where <see cref="Object"/> arguments should be searched, or use the
	/// default search mode, which tries to pick a good search mode to use based on the type of the argument:
	/// <list>
	/// <item> <see cref="Transform"/> or <see cref="GameObject"/>: <see cref="From.GameObject"/>, </item>
	/// <item> <see cref="Component"/> or interface: <see cref="From.Children"/> or <see cref="From.Parent"/> or <see cref="From.SameScene"/>, </item>
	/// <item> other <see cref="Object"/>: <see cref="From.Assets"/>, </item>
	/// <item>
	/// Collection of <see cref="Component"/>, interface, <see cref="Transform"/> or <see cref="GameObject"/>: <see cref="From.Children"/>.
	/// For example Collider[] or IEnumerable<Transform>.
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// The found arguments are passed to the component's <see cref="IInitializable{}.Init"/> function,
	/// where they can be assigned to serialized fields.
	/// </para>
	/// <para>
	/// This behaviour only occurs in edit mode and is meant to make it more convenient to add components without
	/// needing to assign all <see cref="Object"/> references manually using the inspector.
	/// </para>
	/// <para>
	/// This attribute can also be added to classes that derive from one of the <see cref="Initializer{,}"/> base classes.
	/// In this case, by default, the Init arguments will be taken from the initializer and passed to the client's Init function.
	/// If you specify <see cref="From"/> where arguments should be retrieved from, using values other than <see cref="From.Initializer"/>
	/// or <see cref="From.Default"/>, then said arguments will be located from the scene and assigned to the initializer first,
	/// and then also passed to the client's Init function.
	/// </para>
	/// <para>
	/// If this attribute is present on both an initializer, as well as the client of the initializer, then the one found
	/// on the client class will be ignored, and the one found on the initializer will be in effect. This makes it possible
	/// to configure different options for the client when it has no initializers, and when it has different initializers
	/// attached to it.
	/// </para>
	/// </summary>
	/// <seealso cref="InitOnResetAttribute"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true), Conditional("UNITY_EDITOR")]
	public class InitInEditModeAttribute : InitOnResetAttribute
	{
		/// <summary>
		/// When this attribute is attached to a <see cref="MonoBehaviour{}"/> class, instances of the
		/// component are re-initialized whenever any objects in the same scene or prefab are modified.
		/// <para>
		/// When this attribute is attached to an <see cref="IInitializer"/> class, instance of the initializer
		/// will re-initialize their client whenever any objects in the same scene or prefab are modified.
		/// </para>
		/// <para>
		/// If the client component has an initializer attached to it, then the initialization arguments configured
		/// using the initializer are passed to the <see cref="IInitializable{}.Init"/> function of the client.
		/// </para>
		/// <para>
		/// If the client component does not have an initializer, then the initialization arguments are
		/// gathered according to the <see cref="From">search rules</see> specified via the arguments passed to
		/// the constructor of this attribute.
		/// </para>
		/// </summary>
		public InitInEditModeAttribute() : base() { }

		/// <summary>
		/// When a component that derives from <see cref="MonoBehaviour{}"/> and has the
		/// <see cref="InitInEditModeAttribute"/> is first added to a GameObject in the editor,
		/// or when the user hits the Reset button in the Inspector's context menu,
		/// the arguments accepted by the component are automatically gathered
		/// and passed to its <see cref="IInitializable{}.Init"/> function.
		/// <para>
		/// This auto-initialization behaviour only occurs in edit mode during the Reset event function
		/// and is meant to make it more convenient to add components without needing to assign all
		/// <see cref="Object"/> references manually through the inspector.
		/// </para>
		/// </summary>
		/// <param name="search"> Defines where to search when trying to retrieve the arguments accepted by the attribute holder. </param>
		public InitInEditModeAttribute(From search) : base(search) { }

		/// <summary>
		/// When a component that derives from <see cref="MonoBehaviour{}"/> and has the
		/// <see cref="InitInEditModeAttribute"/> is first added to a GameObject in the editor,
		/// or when the user hits the Reset button in the Inspector's context menu,
		/// the arguments accepted by the component are automatically gathered
		/// and passed to its <see cref="IInitializable{}.Init"/> function.
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
		public InitInEditModeAttribute(From first, From second, From third = defaultFrom, From fourth = defaultFrom, From fifth = defaultFrom, From sixth = defaultFrom)
			: base(first, second, third, fourth, fifth, sixth) { }
	}
}