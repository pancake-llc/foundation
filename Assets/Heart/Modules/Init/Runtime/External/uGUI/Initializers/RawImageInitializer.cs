#if UNITY_UGUI
using UnityEngine;
using UnityEngine.UI;

namespace Sisus.Init.Internal.UI
{
	/// <summary>
	/// Sets the <see cref="RawImage.texture"/> property of an <see cref="RawImage"/>
	/// component to an Inspector-assigned value when the object is being loaded.
	/// </summary>
	internal sealed class RawImageInitializer : CustomInitializer<RawImage, Texture>
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
			public Texture texture = default;
		}
		#endif

		protected override void InitTarget(RawImage target, Texture texture)
		{
			target.texture = texture;
		}
	}
}
#endif