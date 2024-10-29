using System.Collections;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// Extension methods for objects that implement <see cref="ICoroutines"/>.
	/// <para>
	/// Objects that are wrapped by a <see cref="Wrapper{}"/> component can start and stop
	/// <see cref="Coroutine">coroutines</see> on the wrapper component using these extension methods.
	/// </para>
	/// <see cref="IWrapper"/>-related extension methods for <see cref="GameObject"/> and <see cref="Component"/>.
	/// </summary>
	public static class ICoroutinesExtensions
	{
		/// <summary>
		/// Starts the provided <paramref name="coroutine"/> on <see langword="this"/> object.
		/// <para>
		/// If the object has a <see cref="ICoroutines.CoroutineRunner"/> then the
		/// <paramref name="coroutine"/> is started using it.
		/// </para>
		/// <para>
		/// If <see langword="this"/> is an object wrapped by a <see cref="Wrapper{TObject}"/> then
		/// the <paramref name="coroutine"/> is started on the wrapper behaviour.
		/// </para>
		/// <para>
		/// If <see langword="this"/> is a <see cref="MonoBehaviour"/> then the <paramref name="coroutine"/>
		/// is started on the object directly.
		/// </para>
		/// <para>
		/// Otherwise, the <paramref name="coroutine"/> is started on an <see cref="Updater"/> instance.
		/// </para>
		/// </summary>
		/// <typeparam name="T"> Type of <see langword="this"/> <see cref="object"/>. </typeparam>
		/// <param name="this">
		/// The <see cref="object"/> which requests the coroutine to start.
		/// <para>
		/// In cases where the object is attached to a <see cref="UnityEngine.Object.Destroy">destroyable</see>
		/// <see cref="GameObject"/>, the lifetime of the coroutine will be tied to the lifetime of the <see cref="Object"/>.
		/// </para>
		/// </param>
		/// <param name="coroutine"> The coroutine to start. </param>
		/// <returns>
		/// Reference to the <see cref="Coroutine"/> that was started.
		/// <para>
		/// This 
		/// </para>
		/// </returns>
		public static Coroutine StartCoroutine<T>([DisallowNull] this T @this, [DisallowNull] IEnumerator coroutine) where T : ICoroutines => @this.CoroutineRunner.StartCoroutine(coroutine);

		/// <summary>
		/// Stops the <paramref name="coroutine"/> that is running on <see langword="this"/> object.
		/// <para>
		/// If the object has a <see cref="ICoroutines.CoroutineRunner"/> then the
		/// <paramref name="coroutine"/> is stopped using it.
		/// </para>
		/// <para>
		/// If <see langword="this"/> is an object wrapped by a <see cref="Wrapper{TObject}"/> then
		/// the <paramref name="coroutine"/> is started on the wrapper behaviour.
		/// </para>
		/// <para>
		/// If <see langword="this"/> is a <see cref="MonoBehaviour"/> then the <paramref name="coroutine"/>
		/// is started on the object directly.
		/// </para>
		/// <para>
		/// Otherwise, the <paramref name="coroutine"/> is started on an <see cref="Updater"/> instance.
		/// </para>
		/// </summary>
		/// <param name="coroutine"> The <see cref="IEnumerator">coroutine</see> to stop. </param>
		public static void StopCoroutine<TObject>([DisallowNull]this TObject @this, [DisallowNull] IEnumerator coroutine) where TObject : ICoroutines => @this.CoroutineRunner?.StopCoroutine(coroutine);

		/// <summary>
		/// Stops the <paramref name="coroutine"/> that is running on <see langword="this"/> object.
		/// <para>
		/// If the object has a <see cref="ICoroutines.CoroutineRunner"/> then the
		/// <paramref name="coroutine"/> is stopped using it.
		/// </para>
		/// <para>
		/// If <see langword="this"/> is an object wrapped by a <see cref="Wrapper{TObject}"/> then
		/// the <paramref name="coroutine"/> is stopped on the wrapper behaviour.
		/// </para>
		/// <para>0
		/// If <see langword="this"/> is a <see cref="MonoBehaviour"/> then the <paramref name="coroutine"/>
		/// is stopped on the object directly.
		/// </para>
		/// <para>
		/// Otherwise, the <paramref name="coroutine"/> is stopped on an <see cref="Updater"/> instance.
		/// </para>
		/// </summary>
		/// <param name="coroutine">
		/// Reference to the <see cref="IEnumerator">coroutine</see> to stop.
		/// <para>
		/// This is the reference that was returned by <see cref="StartCoroutine"/>
		/// when the coroutine was started.
		/// </para>
		/// </param>
		public static void StopCoroutine<TObject>([DisallowNull]this TObject @this, [DisallowNull] Coroutine coroutine) where TObject : ICoroutines
		{
			if(@this.CoroutineRunner != null)
			{
				@this.CoroutineRunner.StopCoroutine(coroutine);
				return;
			}

			if(Find.wrappedInstances.TryGetValue(@this, out IWrapper wrapper))
			{
				if(wrapper.AsMonoBehaviour is MonoBehaviour monoBehaviour)
				{
					monoBehaviour.StopCoroutine(coroutine);
					return;
				}
			}
			else if(@this is MonoBehaviour monoBehaviour)
			{
				monoBehaviour.StopCoroutine(coroutine);
				return;
			}

			Updater.StopCoroutine(coroutine);
		}

		/// <summary>
		/// Stops all coroutines that are running on <see langword="this"/> object.
		/// <para>
		/// If the object has a <see cref="ICoroutines.CoroutineRunner"/> then all
		/// <paramref name="coroutine">coroutines</paramref> started using that are still running are stopped.
		/// </para>
		/// <para>
		/// If <see langword="this"/> is an object wrapped by a <see cref="Wrapper{TObject}"/> then
		/// the <paramref name="coroutine">coroutines</paramref> are stopped on the wrapper behaviour.
		/// </para>
		/// <para>
		/// If <see langword="this"/> is a <see cref="MonoBehaviour"/> then the <paramref name="coroutine">coroutines</paramref>
		/// are is stopped on the object directly.
		/// </para>
		/// <para>
		/// If this object is neither of those things, then this method can not be used to stop all its coroutines.
		/// You will need to use <see cref="StopCoroutine"/> instead to stop each running coroutine individually.
		/// </para>
		/// </summary>
		public static void StopAllCoroutines<TObject>([DisallowNull] this TObject @this) where TObject : ICoroutines
		{
			if(@this.CoroutineRunner != null)
			{
				@this.CoroutineRunner.StopAllCoroutines();
				return;
			}

			if(Find.wrappedInstances.TryGetValue(@this, out IWrapper wrapper))
			{
				if(wrapper.AsMonoBehaviour is MonoBehaviour monoBehaviour)
				{
					monoBehaviour.StopAllCoroutines();
				}
			}
			else if(@this is MonoBehaviour monoBehaviour)
			{
				monoBehaviour.StopAllCoroutines();
				return;
			}
		}
	}
}