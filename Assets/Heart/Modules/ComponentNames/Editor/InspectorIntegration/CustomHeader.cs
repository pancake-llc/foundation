using System;
using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
	/// <summary>
	/// Represents a class that is responsible for determining the header content to show in the Inspector
	/// by default for all components of type <see cref="TComponent"/>.
	/// <remarks>
	/// Header content consists of a <see cref="ComponentNames.Name"/>, <see cref="ComponentNames.Suffix"/> and <see cref="ComponentNames.Tooltip"/>.
	/// <para>
	/// The default header content specified by this class can still be overriden for individual components
	/// using the rename functionality in the Inspector, or by calling the
	/// <see cref="ComponentExtensions.SetName(Component, HeaderContent)"/> extension method in code.
	/// </para>
	/// </remarks>
	/// </summary>
	/// <typeparam name="TComponent">
	/// Type of the components whose default header content is determined by this class.
	/// </typeparam>
	public abstract class CustomHeader<TComponent> : ICustomHeader where TComponent : Component
	{
		/// <summary>
		/// Specifies the execution order of this custom header relative to other custom headers targeting the same component.
		/// <para>
		/// If multiple custom headers affect the same header content field (name, suffix or tooltip) on the same component,
		/// then the content returned by the custom header with the largest <see cref="ExecutionOrder"/> will override the
		/// ones returned by custom headers with smaller ones.
		/// </para>
		/// Custom headers can also modify components' pre-existing header content, instead of completely replacing them.
		/// The target component's pre-existing header content can be acquired using the <see cref="Name"/>,
		/// <see cref="Suffix"/> and <see cref="Tooltip"/> properties.
		/// <para>
		/// Default value is 0.
		/// </para>
		/// </summary>
		public virtual int ExecutionOrder => 0;

		protected Name Name { get; private set; }
		protected Suffix Suffix { get; private set; }
		protected Tooltip Tooltip { get; private set; }
		protected Name DefaultName { get; private set; }
		protected Suffix DefaultSuffix { get; private set; }
		protected Tooltip DefaultTooltip { get; private set; }

		/// <summary>
		/// Gets the default name to show for the <paramref name="target"/> in the Inspector.
		/// </summary>
		public virtual Name GetName(TComponent target) => null;

		/// <summary>
		/// Gets the default suffix to show after the name of the <paramref name="target"/> in the Inspector.
		/// <remarks>
		/// The suffix will be drawn in a grey color inside parentheses.
		/// </remarks>>
		/// </summary>
		/// <param name="target"> The component whose suffix to get. </param>
		/// <returns>
		/// A <see cref="ComponentNames.Suffix"/> object containing the text to draw as a suffix in the Inspector.
		/// <para>
		/// <see cref="Suffix.None"/> if no suffix should be drawn in the Inspector.
		/// </para>
		/// <para>
		/// <see cref="Suffix.Default"/> if the type name of the component should be drawn as a suffix in the Inspector.
		/// </para>
		/// </returns>
		public virtual Suffix GetSuffix(TComponent target) => null;

		/// <summary>
		/// Gets the default tooltip to show when the title of the <paramref name="target"/> is mouseovered in the Inspector.
		/// </summary>
		/// <param name="target"> The component whose tooltip to get. </param>
		/// <returns>
		/// A <see cref="ComponentNames.Tooltip"/> object containing the text to draw when the title is mouseovered.
		/// <para>
		/// <see cref="Tooltip.None"/> if no tooltip should be drawn when the title is mouseovered.
		/// </para>
		/// <para>
		/// <see cref="Tooltip.Default"/> if the summary of the component class should be drawn when the title is mouseovered.
		/// </para>
		/// </returns>
		public virtual Tooltip GetTooltip(TComponent target) => null;

		/// <summary>
		/// Gets the default header content to show for the <paramref name="target"/> in the Inspector.
		/// </summary>
		/// <param name="target"> The component whose header content to get. </param>
		/// <returns>
		/// A <see cref="HeaderContent"/> object containing the <see cref="ComponentNames.Name"/>, <see cref="ComponentNames.Suffix"/>
		/// and <see cref="ComponentNames.Tooltip"/> to show for the <paramref name="target"/> in the Inspector.
		/// </returns>
		private HeaderContent Get(TComponent target) => new(GetName(target), GetSuffix(target), GetTooltip(target));

		void ICustomHeader.Init(Name name, Suffix suffix, Tooltip tooltip, Name defaultName, Suffix defaultSuffix, Tooltip defaultTooltip)
		{
			Name = name;
			Suffix = suffix;
			Tooltip = tooltip;
			DefaultName = defaultName;
			DefaultSuffix = defaultSuffix;
			DefaultTooltip = defaultTooltip;
		}

		Name ICustomHeader.GetName(Component target) => GetName((TComponent)target);
		Suffix ICustomHeader.GetSuffix(Component target) => GetSuffix((TComponent)target);
		Tooltip ICustomHeader.GetTooltip(Component target) => GetTooltip((TComponent)target);
		int IComparable<ICustomHeader>.CompareTo(ICustomHeader other) => ExecutionOrder.CompareTo(other.ExecutionOrder);
	}
}