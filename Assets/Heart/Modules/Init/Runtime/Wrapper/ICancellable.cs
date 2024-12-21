using Sisus.Init.Internal;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object that can be requested to cancel any asynchronous operations that may be running on it.
	/// <para>
	/// When this interface is implemented by an object that is wrapped by a <see cref="IWrapper"/> component,
	/// <see cref="IsCancellationRequested"/> will become <see langword="true"/> when the wrapper is destroyed.
	/// </para>
	/// </summary>
	public interface ICancellable
	{
		#if UNITY_EDITOR
		/// <summary>
		/// Gets or sets whether the default implementation of <see cref="IsCancellationRequested"/> should return
		/// <see langword="true"/> during Edit Mode in the editor.
		/// <para> This property only exists in the editor. </para>
		/// </summary>
		static bool CancelAllByDefaultInEditMode { get; set; } = true;
		#endif

		/// <summary>
		/// Gets whether cancellation has been requested for asynchronous operations running on this object.
		/// </summary>
		bool IsCancellationRequested
		{
			get
			{
				#if UNITY_EDITOR
				// If we have exited play mode, then request cancellation of all running tasks.
				if(!EditorOnly.ThreadSafe.Application.IsPlaying)
				{
					return CancelAllByDefaultInEditMode;
				}
				#endif

				// if wrapped instance is not found, it could be because the wrapper has been destroyed.
				if(!Find.wrappedInstances.TryGetValue(this, out IWrapper wrapper))
				{
					return true;
				}

				#if UNITY_6000_0_OR_NEWER
				// Only MonoBehaviour type wrappers have the destroyCancellationToken property.
				// In other cases we rely on the global level CancellationToken.
				if(wrapper.AsMonoBehaviour is MonoBehaviour monoBehaviour)
				{
					return !monoBehaviour || monoBehaviour.destroyCancellationToken.IsCancellationRequested;
				}
				#endif

				return Updater.CancellationToken.IsCancellationRequested;
			}

			set
			{
				// Ignored in the default implementation.

				// This can be implemented if there is a need to control
				// the value of the property during tests, for example.
			} 
		}
	}
}