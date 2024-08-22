using System;

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Tells an <see cref="UnityEditor.Editor"/> class which <see cref="UnityEngine.Object"/>'s
	/// default <see cref="UnityEditor.Editor"/> it should wrap and extend.
	/// <para>
	/// An example use case is drawing an Init section first at the top and then
	/// the original GUI of the wrapped editor beneath it.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class CustomEditorDecoratorAttribute : Attribute
	{
		/// <summary>
		/// <see cref="UnityEngine.Object"/>-derived target class whose editor this editor should extend.
		/// </summary>
		public Type	TargetType { get; set; }

		/// <summary>
		/// Tells an <see cref="UnityEditor.Editor"/> class which <see cref="UnityEngine.Object"/>'s
		/// default <see cref="UnityEditor.Editor"/> it should wrap and extend.
		/// <para>
		/// An example use case is drawing an Init section first at the top and then
		/// the original GUI of the wrapped editor beneath it.
		/// </para>
		/// </summary>
		/// <param name="targetType">
		/// <see cref="UnityEngine.Object"/>-derived target class whose editor this editor should extend.
		/// </param>
		public CustomEditorDecoratorAttribute(Type targetType) => TargetType = targetType;
	}
}