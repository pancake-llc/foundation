using System;
using Object = UnityEngine.Object;

namespace Pancake.Init
{
    /// <summary>
    /// When a component that derives from <see cref="MonoBehaviour{}"/> and has the
    /// <see cref="InitOnResetAttribute"/> is first added to a GameObject in the editor,
    /// or when the user hits the Reset button in the Inspector's context menu,
    /// the arguments accepted by the component are automatically gathered
    /// and passed to its <see cref="IInitializable{}.Init"/> function.
    /// <para>
    /// This auto-initialization behaviour only occurs in edit mode during the Reset event function
    /// and is meant to make it more convenient to add components without needing to assign all
    /// <see cref="Object"/> references manually through the inspector.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class InitOnResetAttribute : Attribute
	{
		private const From defaultFrom = From.Default;

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
		/// When a component that derives from <see cref="MonoBehaviour{}"/> and has the
		/// <see cref="InitOnResetAttribute"/> is first added to a GameObject in the editor,
		/// or when the user hits the Reset button in the Inspector's context menu,
		/// the arguments accepted by the component are automatically gathered
		/// and passed to its <see cref="IInitializable{}.Init"/> function.
		/// <para>
		/// This auto-initialization behaviour only occurs in edit mode during the Reset event function
		/// and is meant to make it more convenient to add components without needing to assign all
		/// <see cref="Object"/> references manually through the inspector.
		/// </para>
		/// </summary>
		public InitOnResetAttribute() : this(defaultFrom) { }

		/// <summary>
		/// When a component that derives from <see cref="MonoBehaviour{}"/> and has the
		/// <see cref="InitOnResetAttribute"/> is first added to a GameObject in the editor,
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
		public InitOnResetAttribute(From search = defaultFrom)
		{
			first = search;
			second = search;
			third = search;
			fourth = search;
			fifth = search;
			sixth = search;
		}

		/// <summary>
		/// When a component that derives from <see cref="MonoBehaviour{}"/> and has the
		/// <see cref="InitOnResetAttribute"/> is first added to a GameObject in the editor,
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