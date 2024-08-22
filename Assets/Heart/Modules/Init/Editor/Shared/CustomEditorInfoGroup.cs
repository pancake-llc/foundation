using System;

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Contains information about the custom editors targeting a particular <see cref="UnityEngine.Object"/> class.
	/// <para>
	/// Mirrors the internal CustomEditorAttributes.MonoEditorTypeStorage type.
	/// </para>
	/// </summary>
	public readonly struct CustomEditorInfoGroup
	{
		public readonly CustomEditorInfo[] customEditors;
		public readonly CustomEditorInfo[] customEditorsMultiEdition;

		/// <summary>
		/// Contains information about the custom editors targeting a particular <see cref="UnityEngine.Object"/> class.
		/// <para>
		/// Mirrors the internal CustomEditorAttributes.MonoEditorTypeStorage type.
		/// </para>
		/// </summary>
		public CustomEditorInfoGroup(CustomEditorInfo[] customEditors, CustomEditorInfo[] customEditorsMultiEdition)
		{
			this.customEditors = customEditors;
			this.customEditorsMultiEdition = customEditorsMultiEdition;
		}

		/// <summary>
		/// Contains information about the custom editors targeting a particular <see cref="UnityEngine.Object"/> class.
		/// <para>
		/// Mirrors the internal CustomEditorAttributes.MonoEditorTypeStorage type.
		/// </para>
		/// </summary>
		public CustomEditorInfoGroup(CustomEditorInfo customEditor, bool canEditMultipleObjects)
		{
			customEditors = new CustomEditorInfo[] { customEditor };
			customEditorsMultiEdition = canEditMultipleObjects ? customEditors : Array.Empty<CustomEditorInfo>();
		}
	}
}