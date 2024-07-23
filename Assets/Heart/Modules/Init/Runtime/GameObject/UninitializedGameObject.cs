#define DEBUG_ENABLED

using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Builder for creating a new <see cref="GameObject"/> with two <see cref="Component">components</see>.
	/// <para>
	/// The second component still needs to be initialized by calling an
	/// <see cref="GameObjectT2Extensions.Init2{TFirstComponent, TSecondComponent, TArgument}"/>
	/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
	/// interface that <typeparamref name="TSecondComponent"/> implements.
	/// </para>
	/// <para>
	/// If <typeparamref name="TSecondComponent"/> does not implement any IArgs interface then
	/// <see cref="GameObjectT2Extensions.Init2{TFirstComponent, TSecondComponent}">the parameterless Init</see> function should be used.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
	/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
	public struct UninitializedGameObject<TFirstComponent, TSecondComponent> where TFirstComponent : Component where TSecondComponent : Component
	{
		internal GameObject gameObject;
		internal bool setActive;
		internal Components<TFirstComponent, TSecondComponent> components;

		internal UninitializedGameObject(GameObject gameObject, bool setActive, Components<TFirstComponent, TSecondComponent> components)
		{
			this.gameObject = gameObject;
			this.setActive = setActive;
			this.components = components;
		}

		internal void OnAfterInitialized()
		{
			if(setActive)
			{
				gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns the first added component.
		/// </summary>
		/// <param name="this"> GameObject being created with two components. </param>
		public static implicit operator TFirstComponent(UninitializedGameObject<TFirstComponent, TSecondComponent> @this)
		{
			return @this.Init2().first;
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns the second added component.
		/// </summary>
		/// <param name="this"> GameObject being created with two components. </param>
		public static implicit operator TSecondComponent(UninitializedGameObject<TFirstComponent, TSecondComponent> @this)
		{
			return @this.Init2().second;
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns both added components.
		/// </summary>
		/// <param name="this"> GameObject being created with two components. </param>
		public static implicit operator (TFirstComponent, TSecondComponent)(UninitializedGameObject<TFirstComponent, TSecondComponent> @this)
		{
			return @this.Init2();
		}

		/// <summary>
		/// Finalizes the <see cref="GameObject"/> creation and returns the created GameObject.
		/// </summary>
		/// <param name="this"> GameObject being created with two components. </param>
		public static implicit operator GameObject(UninitializedGameObject<TFirstComponent, TSecondComponent> @this)
		{
			return @this.Init2();
		}
	}
}