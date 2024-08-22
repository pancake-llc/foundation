#define DEBUG_ENABLED

using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents a pair of <see cref="Component">components</see> that exist on the same <see cref="GameObject"/>.
	/// <para>
	/// Can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>
	/// or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}"/>.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstComponent"> Type of the first component. </typeparam>
	/// <typeparam name="TSecondComponent"> Type of the second component. </typeparam>
	public struct Components<TFirstComponent, TSecondComponent> where TFirstComponent : Component where TSecondComponent : Component
	{
		public TFirstComponent first;
		public TSecondComponent second;

		/// <summary>
		/// Gets the <see cref="GameObject"/> that holds the pair of components.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator GameObject(Components<TFirstComponent, TSecondComponent> @this)
		{
			return @this.first.gameObject;
		}

		/// <summary>
		/// Gets the <see cref="Transform"/> component of the <see cref="GameObject"/> that holds the pair of components.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator Transform(Components<TFirstComponent, TSecondComponent> @this)
		{
			return @this.first.transform;
		}

		/// <summary>
		/// Gets the <see cref="first"/> component of the component pair.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator TFirstComponent(Components<TFirstComponent, TSecondComponent> @this)
		{
			return @this.first;
		}

		/// <summary>
		/// Gets the <see cref="second"/> component of the component pair.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator TSecondComponent(Components<TFirstComponent, TSecondComponent> @this)
		{
			return @this.second;
		}

		/// <summary>
		/// Gets a <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}">tuple</see> containing the component pair.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator (TFirstComponent, TSecondComponent)(Components<TFirstComponent, TSecondComponent> @this)
		{
			return (@this.first, @this.second);
		}

		/// <summary>
		/// Gets a <see cref="System.ValueTuple{TSecondComponent, TFirstComponent}">tuple</see> containing the component pair.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator (TSecondComponent, TFirstComponent)(Components<TFirstComponent, TSecondComponent> @this)
		{
			return (@this.second, @this.first);
		}
	}

	/// <summary>
	/// Represents three of <see cref="Component">components</see> that exist on the same <see cref="GameObject"/>.
	/// <para>
	/// Can be cast to <see cref="GameObject"/>, <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/>,
	/// <typeparamref name="TThirdComponent"/> or <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/>.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstComponent"> Type of the first component. </typeparam>
	/// <typeparam name="TSecondComponent"> Type of the second component. </typeparam>
	/// <typeparam name="TThirdComponent"> Type of the second component. </typeparam>
	public struct Components<TFirstComponent, TSecondComponent, TThirdComponent> where TFirstComponent : Component where TSecondComponent : Component where TThirdComponent : Component
	{
		public TFirstComponent first;
		public TSecondComponent second;
		public TThirdComponent third;

		/// <summary>
		/// Gets the <see cref="GameObject"/> that holds the pair of components.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator GameObject(Components<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.first.gameObject;
		}

		/// <summary>
		/// Gets the <see cref="Transform"/> component of the <see cref="GameObject"/> that holds the pair of components.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator Transform(Components<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.first.transform;
		}

		/// <summary>
		/// Gets the <see cref="first"/> one of the three components.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator TFirstComponent(Components<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.first;
		}

		/// <summary>
		/// Gets the <see cref="second"/> one of the three components.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator TSecondComponent(Components<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.second;
		}

		/// <summary>
		/// Gets the <see cref="third"/> one of the three components.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator TThirdComponent(Components<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.third;
		}

		/// <summary>
		/// Gets a <see cref="System.ValueTuple{TFirstComponent, TSecondComponent, TThirdComponent}"/> containing all three components.
		/// </summary>
		/// <param name="this"> Pair of component. </param>
		public static implicit operator (TFirstComponent, TSecondComponent, TThirdComponent)(Components<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return (@this.first, @this.second, @this.third);
		}
	}
}