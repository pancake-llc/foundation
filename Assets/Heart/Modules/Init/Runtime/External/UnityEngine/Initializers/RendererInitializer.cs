using UnityEngine;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Sets the <see cref="Renderer.material"/> property of a <see cref="Renderer"/> component
	/// to an Inspector-assigned value when the object is being loaded.
	/// </summary>
	internal sealed class RendererInitializer : CustomInitializer<Renderer, Material>
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
			public Material material = default;
		}
		#endif

		protected override void InitTarget(Renderer target, Material material) => target.sharedMaterial = material;
	}
}
