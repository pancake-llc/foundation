using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sisus.ComponentNames
{
	/// <summary>
	/// Specifies the name, suffix and tooltip to show for a component in the Inspector.
	/// </summary>
	public readonly struct HeaderContent
	{
		public static readonly HeaderContent Default = new();

		/// <summary>
		/// The main title to show in the header of the component in the Inspector.
		/// </summary>
		public readonly Name name;

		/// <summary>
		/// The suffix to show inside brackets after the main title in the Inspector.
		/// </summary>
		public readonly Suffix suffix;

		/// <summary>
		/// The tooltip to show when the header of the component is mouseovered in the Inspector.
		/// </summary>
		public readonly Tooltip tooltip;

		/// <summary>
		/// Specifies the name, suffix and tooltip to show for a component in the Inspector.
		/// </summary>
		/// <param name="name">
		/// The main title to show in the header of the component in the Inspector.
		/// </param>
		/// <param name="suffix">
		/// The suffix to show inside brackets after the main title in the Inspector.
		/// </param>
		/// <param name="tooltip">
		/// The tooltip to show when the header of the component is mouseovered in the Inspector.
		/// </param>
		public HeaderContent(in Name name = default, in Suffix suffix = default, in Tooltip tooltip = default)
		{
			this.name = name;
			this.suffix = suffix;
			this.tooltip = tooltip;
		}

		/// <summary>
		/// Converts a <see cref="HeaderContent"/> into a value tuple of <see cref="Name"/>, <see cref="Suffix"/> and <see cref="Tooltip"/>.
		/// </summary>
		/// <param name="name">
		/// The main title to show in the header of the component in the Inspector.
		/// </param>
		/// <param name="suffix">
		/// The suffix to show inside brackets after the main title in the Inspector.
		/// </param>
		/// <param name="tooltip">
		/// The tooltip to show when the header of the component is mouseovered in the Inspector.
		/// </param>
		public void Deconstruct(out Name name, out Suffix suffix, out Tooltip tooltip)
		{
			name = this.name;
			suffix = this.suffix;
			tooltip = this.tooltip;
		}

		internal void Apply(Component component) => component.SetName(name, suffix, tooltip);

		/// <summary>
		/// Converts the <paramref name="textInput"/> into a <see cref="HeaderContent"/>.
		/// </summary>
		/// <param name="textInput">
		/// Text input specifying the name - and optionally the suffix and tooltip - of a component.
		/// <para>
		/// Text should be in the format 'Name (Suffix) | Tooltip'.
		/// </para>
		/// </param>
		public static implicit operator HeaderContent(string textInput)
		{
			ParseNameSuffixAndTooltip(textInput, out var name, out var suffix, out var tooltip);
			return new(name, suffix, tooltip);
		}

		public static implicit operator HeaderContent(in Name name) => new(name, Suffix.Default, Tooltip.Default);
		public static implicit operator HeaderContent(in Suffix suffix) => new(Name.Default, suffix, Tooltip.Default);
		public static implicit operator HeaderContent(in Tooltip tooltip) => new(Name.Default, Suffix.Default, tooltip);

		public static implicit operator HeaderContent(GUIContent content)
		{
			ParseNameAndSuffix(content.text, out var name, out var suffix);
			return new(name, suffix, content.tooltip);
		}

		public static implicit operator HeaderContent(in (string name, string suffix, string tooltip) @override)
			=> new(@override.name, @override.suffix, @override.tooltip);

		public override string ToString() => name.ToString() + suffix.ToString() + tooltip.ToString();
		public override int GetHashCode() => HashCode.Combine(name, suffix, tooltip);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ParseNameSuffixAndTooltip(string textInput, out Name name, out Suffix suffix, out Tooltip tooltip)
		{
			int separator = textInput.IndexOf('|');
			if(separator != -1)
			{
				tooltip = textInput.Substring(separator + 1);
				textInput = textInput.Substring(0, separator);
			}
			else
			{
				tooltip = Tooltip.Default;
			}

			ParseNameAndSuffix(textInput, out name, out suffix);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ParseNameAndSuffix(string textInput, out Name name, out Suffix suffix)
		{
			int openingBracket = textInput.IndexOf('(');
			if(openingBracket is -1)
			{
				name = textInput;
				suffix = Suffix.Default;
				return;
			}

			int closingBracket = textInput.IndexOf(')', openingBracket + 1);
			if(closingBracket is -1)
			{
				name = textInput;
				suffix = Suffix.Default;
				return;
			}

			suffix = textInput.Substring(openingBracket + 1, closingBracket - openingBracket + 1);
			name = textInput.Substring(0, openingBracket);
		}
	}
}