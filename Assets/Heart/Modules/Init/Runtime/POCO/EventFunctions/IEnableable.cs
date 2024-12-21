using Sisus.Init.Internal;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object that can be <see cref="IEnableableExtensions.SetEnabled">enabled or disabled</see>.
	/// </summary>
	public interface IEnableable { }

	/// <summary>
	/// Extension methods for objects that implement the <see cref="IEnableable"/> interface.
	/// </summary>
	public static class IEnableableExtensions
    {
		/// <summary>
		/// Gets the enabled state of the object.
		/// <para>
		/// If the object has a <see cref="IWrapper{TWrapped}"/> Object, then this returns <see langword="true"/>
		/// if it has not been destroyed, and if it is not a <see cref="Behaviour.enabled">disabled</see>
		/// <see cref="Behaviour"/>.
		/// </para>
		/// <para>
		/// If this object does not have a wrapper, and it implements <see cref="IUpdate"/>, <see cref="ILateUpdate"/>
		/// or <see cref="IFixedUpdate"/>, then this returns <see langword="true"/> if the object is currently subscribed
		/// to receive callbacks during any of those events.
		/// </para>
		/// <para>
		/// If this object does not have a wrapper, and does not implement any of the above interfaces, then this always returns <see langword="true"/>.
		/// </para>
		/// </summary>
		/// <param name="enableable"> The object whose enabled state to check. </param>
    	public static bool IsEnabled(this IEnableable enableable)
    	{
    		if(Find.WrapperOf(enableable, out var wrapper))
    		{
    			return wrapper.AsObject && (wrapper.AsMonoBehaviour?.enabled ?? true);
    		}

    		if(enableable is IUpdate updatable && Updater.IsSubscribed(updatable))
    		{
    			return true;
    		}

    		if(enableable is ILateUpdate lateUpdateable && Updater.IsSubscribed(lateUpdateable))
    		{
    			return true;
    		}

    		if(enableable is IFixedUpdate fixedUpdateable && Updater.IsSubscribed(fixedUpdateable))
    		{
    			return true;
    		}

    		return true;
    	}

		/// <summary>
		/// Sets the enabled state of the object.
		/// <para>
		/// If the object has a <see cref="Wrapper{TWrapped}"/> component, then this sets its <see cref="Behaviour.enabled"/> state.
		/// </para>
		/// <para>
		/// If this object does not have a <see cref="Wrapper{TWrapped}"/> component, and it implements <see cref="IUpdate"/>,
		/// <see cref="ILateUpdate"/> or <see cref="IFixedUpdate"/>, then the enabled state controls whether the object
		/// is subscribed to receive callbacks during those events.
		/// </para>
		/// <para>
		/// If this object does not have a wrapper, and does not implement any of the above interfaces, then this always returns <see langword="true"/>.
		/// </para>
		/// <para>
		/// Disabled objects won't receive callbacks during the <see cref="IUpdate.Update"/>, <see cref="ILateUpdate.LateUpdate"/>,
		/// or <see cref="IFixedUpdate.FixedUpdate"/> events.
		/// </para>
		/// </summary>
		/// <param name="enableable"> The object whose enabled state to set. </param>
		/// <param name="value"> The enabled state to set. </param>
		public static void SetEnabled(this IEnableable enableable, bool value)
		{
			if(Find.WrapperOf(enableable, out var wrapper) && wrapper.AsMonoBehaviour)
			{
				wrapper.AsMonoBehaviour.enabled = value;
				return;
			}

			if(value)
			{
				if(enableable is IOnEnable onEnable)
				{
					onEnable.OnEnable();
				}
				
				Updater.Subscribe(enableable);
			}
			else
			{
				Updater.Unsubscribe(enableable);
				
				if(enableable is IOnDisable onDisable)
				{
					onDisable.OnDisable();
				}
			}
		}
    }
}