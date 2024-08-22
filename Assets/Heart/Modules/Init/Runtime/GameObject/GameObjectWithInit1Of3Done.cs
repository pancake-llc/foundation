#define DEBUG_ENABLED

using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Builder for creating a new <see cref="GameObject"/> with three <see cref="Component">components</see>.
	/// <para>
	/// The second and third components still need to be initialized by calling
	/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}"/> and
	/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, Argument}"/>
	/// functions with arguments matching the argument lists defined by the <see cref="IArgs{TArgument}">IArgs</see>
	/// interfaces that <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/> implement.
	/// </para>
	/// <para>
	/// If <typeparamref name="TSecondComponent"/> or <typeparamref name="TThirdComponent"/> does not implement any IArgs
	/// interface then the parameterless Init2 or Init3 function should be used to initialize the component in question.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstComponent"> Type of the first added component; already initialized. </typeparam>
	/// <typeparam name="TSecondComponent"> Type of the second added component; not yet initialized. </typeparam>
	/// <typeparam name="TThirdComponent"> Type of the third added component; not yet initialized. </typeparam>
	public struct GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> where TFirstComponent : Component where TSecondComponent : Component where TThirdComponent : Component
	{
		internal GameObject gameObject;
		internal bool setActive;
		internal Components<TFirstComponent, TSecondComponent, TThirdComponent> components;

		internal GameObjectWithInit1Of3Done(GameObject gameObject, bool setActive, Components<TFirstComponent, TSecondComponent, TThirdComponent> components)
		{
			this.gameObject = gameObject;
			this.setActive = setActive;
			this.components = components;
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns the first added component.
		/// </summary>
		/// <param name="this"> GameObject being created with three components. </param>
		public static implicit operator TFirstComponent(GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.Init2().Init3().first;
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns the second added component.
		/// </summary>
		/// <param name="this"> GameObject being created with three components. </param>
		public static implicit operator TSecondComponent(GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.Init2().Init3().second;
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns the third added component.
		/// </summary>
		/// <param name="this"> GameObject being created with three components. </param>
		public static implicit operator TThirdComponent(GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.Init2().Init3().third;
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns both added components.
		/// </summary>
		/// <param name="this"> GameObject being created with three components. </param>
		public static implicit operator (TFirstComponent, TSecondComponent, TThirdComponent)(GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.Init2().Init3();
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns the created GameObject.
		/// </summary>
		/// <param name="this"> GameObject being created with three components. </param>
		public static implicit operator GameObject(GameObjectWithInit1Of3Done<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.Init2().Init3();
		}
	}
}