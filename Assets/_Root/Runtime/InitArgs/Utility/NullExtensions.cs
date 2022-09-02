using System.Runtime.CompilerServices;
using UnityEngine;
using Pancake.Init;
using Object = UnityEngine.Object;

namespace Pancake
{
	/// <summary>
	/// Helper class for checking whether the value of a variable that is of <see cref="object"/>, interface or generic
	/// type is <see langword="null"/>, <see cref="Object.Destroy">destroyed</see> or <see cref="GameObject.activeInHierarchy">inactive</see>.
	/// <para>
	/// By adding a using static directive it is possible to access the static members of the
	/// <see cref="NullExtensions"/> class without having to qualify the access with the type name, making it possible
	/// to perform safe and reliable null-checking conveniently by comparing objects against <see cref="Null"/>
	/// or <see cref="NullOrInactive"/>.
	/// </para>
	/// <example>
	/// <code>
	/// using Pancake.Init;
	/// using static Pancake.NullExtensions;
	/// 
	/// public class EventListener : MonoBehaviour{IEvent}, IEventListener
	/// {
	///		private IEvent trigger;
	/// 
	///		protected override void Init(IEventBroadcaster trigger)
	///		{
	///			this.trigger = trigger;
	///		}
	/// 
	///		private void OnEnable()
	///		{
	///			trigger.AddListener(this);
	///		}
	/// 
	///		private void OnEvent()
	///		{
	///			Debug.Log($"{name} heard event {trigger}.");
	///		}
	/// 
	///		private void OnDisable()
	///		{
	///			if(trigger != Null)
	///			{
	///				trigger.RemoveListener(this);
	///			}
	///		}
	/// }
	/// </code>
	/// </example>
	/// </summary>
	public static class NullExtensions
	{
		/// <summary>
		/// A value against which any <see cref="object"/> can be compared to determine whether or not it is
		/// <see langword="null"/> or an <see cref="Object"/> which has been <see cref="Object.Destroy">destroyed</see>.
		/// <example>
		/// <code>
		/// using static Pancake.NullExtensions;
		/// 
		/// public class EventListener : MonoBehaviour{IEvent}, IEventListener
		/// {
		///		private IEvent trigger;
		/// 
		///		protected override void Init(IEventBroadcaster trigger)
		///		{
		///			this.trigger = trigger;
		///		}
		/// 
		///		private void OnEnable()
		///		{
		///			trigger.AddListener(this);
		///		}
		/// 
		///		private void OnEvent()
		///		{
		///			Debug.Log($"{name} heard event {trigger}.");
		///		}
		/// 
		///		private void OnDisable()
		///		{
		///			if(trigger != Null)
		///			{
		///				trigger.RemoveListener(this);
		///			}
		///		}
		/// }
		/// </code>
		/// </example>
		/// </summary>
		public static readonly NullComparer Null = new NullComparer();

		/// <summary>
		/// A value against which any <see cref="object"/> can be compared to determine whether or not it is
		/// <see langword="null"/> or an <see cref="Object"/> which is <see cref="GameObject.activeInHierarchy">inactive</see>
		/// or has been <see cref="Object.Destroy">destroyed</see>.
		/// <example>
		/// <code>
		/// using static Pancake.NullExtensions;
		/// 
		/// public class LookAt : MonoBehaviour{ITrackable}
		/// {
		/// 	private ITrackable target;
		///
		/// 	protected override void Init(ITrackable target) => this.target = target;
		///
		/// 	private void Update()
		/// 	{
		/// 		if(target != NullOrInactive)
		/// 		{
		/// 			transform.LookAt(target.Position);
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </example>
		public static readonly NullOrInactiveComparer NullOrInactive = new NullOrInactiveComparer();

		/// <summary>
		/// Represents a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed</see>.
		/// </summary>
		public class NullComparer
		{
			internal NullComparer() { }

            public override bool Equals(object obj) => obj is Object unityObject ? unityObject == null : obj is null;
            public override int GetHashCode() => 0;
			public override string ToString() => "Null";

			/// <summary>
			/// Determines whether the specified <paramref name="unityObject"/> is a <see langword="null"/> reference or has been <see cref="Object.Destroy">destroyed</see>.
			/// </summary>
			/// <param name="@object"> The <see cref="Object"/> to check. </param>
			/// <returns> <see langword="true"/> if the specified <see cref="Object"/> is a <see langword="null"/> reference or has been <see cref="Object.Destroy">destroyed; otherwise, <see langword="false"/>. </returns>
			public static bool operator ==(Object unityObject, NullComparer @null) => unityObject == null;

			/// <summary>
			/// Determines whether the specified <paramref name="unityObject"/> is not a <see langword="null"/> reference and has not been <see cref="Object.Destroy">destroyed</see>.
			/// </summary>
			/// <param name="@object"> The <see cref="Object"/> to check. </param>
			/// <returns> <see langword="true"/> if the specified <see cref="Object"/> is not a <see langword="null"/> reference and has not been <see cref="Object.Destroy">destroyed; otherwise, <see langword="false"/>. </returns>
			public static bool operator !=(Object unityObject, NullComparer @null) => unityObject != null;

			/// <summary>
			/// Determines whether the specified <paramref name="object"/> is a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed</see>.
			/// </summary>
			/// <param name="@object"> The <see cref="object"/> to check. </param>
			/// <returns> <see langword="true"/> if the specified <see cref="object"/> is a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed; otherwise, <see langword="false"/>. </returns>
			public static bool operator ==(object @object, NullComparer @null) => @object is Object unityObject ? unityObject == null : @object is null;

			/// <summary>
			/// Determines whether the specified <paramref name="object"/> is not a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed</see>.
			/// </summary>
			/// <param name="@object"> The <see cref="object"/> to check. </param>
			/// <returns> <see langword="true"/> if the specified <see cref="object"/> is not a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed; otherwise, <see langword="false"/>. </returns>
			public static bool operator !=(object @object, NullComparer @null) => @object is Object unityObject ? unityObject != null : !(@object is null);
		}

		/// <summary>
		/// Represents a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed</see>
		/// or is <see cref="GameObject.activeInHierarchy">inactive</see>.
		/// </summary>
		public class NullOrInactiveComparer
		{
			internal NullOrInactiveComparer() { }

			public override bool Equals(object obj) => IsNullOrDestroyedOrInactive(obj);
			public override int GetHashCode() => 0;
			public override string ToString() => "NullOrInactive";

            public static bool operator ==(GameObject gameObject, NullOrInactiveComparer nullOrInactive) => gameObject == null || !gameObject.activeInHierarchy;
            public static bool operator !=(GameObject gameObject, NullOrInactiveComparer nullOrInactive) => gameObject != null && gameObject.activeInHierarchy;

			public static bool operator ==(Component component, NullOrInactiveComparer nullOrInactive) => component == null || !component.gameObject.activeInHierarchy;
			public static bool operator !=(Component component, NullOrInactiveComparer nullOrInactive) => component != null && component.gameObject.activeInHierarchy;

			public static bool operator ==(Object obj, NullOrInactiveComparer nullOrInactive) => IsNullOrDestroyedOrInactive(obj);
			public static bool operator !=(Object obj, NullOrInactiveComparer nullOrInactive) => !IsNullOrDestroyedOrInactive(obj);

			/// <summary>
			/// Determines whether the specified <paramref name="object"/> is a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed</see> or is <see cref="GameObject.activeInHierarchy">inactive</see>.
			/// </summary>
			/// <param name="@object"> The <see cref="object"/> to check. </param>
			/// <returns> <see langword="true"/> if the specified <see cref="object"/> is a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed or is <see cref="GameObject.activeInHierarchy">inactive</see>; otherwise, <see langword="false"/>. </returns>
			public static bool operator ==(object obj, NullOrInactiveComparer nullOrInactive) => IsNullOrDestroyedOrInactive(obj);

			/// <summary>
			/// Determines whether the specified <paramref name="object"/> is not a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed</see> or is <see cref="GameObject.activeInHierarchy">inactive</see>.
			/// </summary>
			/// <param name="@object"> The <see cref="object"/> to check. </param>
			/// <returns> <see langword="true"/> if the specified <see cref="object"/> is not a <see langword="null"/> reference or an <see cref="Object"/> that has been <see cref="Object.Destroy">destroyed or is <see cref="GameObject.activeInHierarchy">inactive</see>; otherwise, <see langword="false"/>. </returns>
			public static bool operator !=(object obj, NullOrInactiveComparer nullOrInactive) => !IsNullOrDestroyedOrInactive(obj);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static bool IsNullOrDestroyedOrInactive(object obj) => obj is null || IsDestroyedOrInactive(obj);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsDestroyedOrInactive(object obj) =>
					obj is Object unityObject
                    ? IsDestroyedOrInactive(unityObject)
                    : Find.WrapperOf(obj, out var wrapper) && !wrapper.gameObject.activeInHierarchy;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsDestroyedOrInactive(Object unityObject)
            {
				if(unityObject == null)
				{
					return true;
				}

				if(unityObject is Component component)
				{
					return !component.gameObject.activeInHierarchy;
				}

				if(unityObject is GameObject gameObject)
				{
					return !gameObject.activeInHierarchy;
				}

				return false;
            }
		}
	}
}