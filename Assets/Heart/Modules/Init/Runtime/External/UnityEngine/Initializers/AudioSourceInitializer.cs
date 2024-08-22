#if UNITY_2023_3_OR_NEWER
using UnityEngine.Audio;
using Sisus.Init;

namespace UnityEngine
{
	/// <summary>
	/// Initializer for the <see cref="AudioSource"/> component.
	/// </summary>
	internal sealed class AudioSourceInitializer : CustomInitializer<AudioSource, AudioResource>
	{
		#if UNITY_EDITOR
		/// <summary>
		/// This section can be used to customize how the Init arguments will be drawn in the Inspector.
		/// <para>
		/// The Init argument names shown in the Inspector will match the names of members defined inside this section.
		/// </para>
		/// <para>
		/// Any PropertyAttributes attached to these members will also affect the Init arguments in the Inspector.
		/// </para>
		/// </summary>
		private sealed class Init
		{
			public AudioResource AudioResource = default;
		}
		#endif

		protected override void InitTarget(AudioSource target, AudioResource audioResource)
		{
			target.resource = audioResource;
		}
	}
}
#endif