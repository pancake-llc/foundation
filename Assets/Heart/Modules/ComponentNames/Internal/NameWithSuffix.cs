#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
	/// <summary>
	/// Specifies the name and suffix to show for a component in the Inspector.
	/// </summary>
	internal readonly struct NameWithSuffix
	{
		/// <summary>
		/// Title to show in the Inspector.
		/// </summary>
		[NotNull] public readonly string name;

		/// <summary>
		/// Suffix to show inside brackets after the title in the Inspector.
		/// </summary>
		[NotNull] public readonly string suffix;

		[NotNull] private readonly string joinedAsPlainText;
		[NotNull] private readonly string joinedAsRichText;

		internal NameWithSuffix([DisallowNull] Component component, [AllowNull] string nameOverride = null, [AllowNull] string suffixOverride = null) : this(new(nameOverride, suffixOverride), component) { }

		internal NameWithSuffix(HeaderContent headerContent, [DisallowNull] Component component)
		{
			var (nameOverride, suffixOverride, _) = headerContent;

			var @default = ComponentName.GetDefault(component, withoutScriptSuffix:false);

			if(nameOverride.IsDefault || nameOverride.Equals(@default.name))
			{
				name = @default.name;
				suffix = !suffixOverride.IsDefault ? suffixOverride : ComponentName.RemoveDefaultScriptSuffix ? "" : "Script";
			}
			else
			{
				this.name = nameOverride;
				suffix = suffixOverride.IsDefault || suffixOverride.Equals(@default.suffix) ? ComponentName.GetDefaultBuiltIn(component, component.GetType(), withoutScriptSuffix:true).name : suffixOverride;
			}

			if(suffix.Length == 0)
			{
				joinedAsPlainText = this.name;
				joinedAsRichText = this.name;
			}
			else
			{
				joinedAsPlainText = this.name + " (" + this.suffix + ")";
				joinedAsRichText = this.name + string.Format(ComponentName.InspectorSuffixFormat, this.suffix);
			}
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
		public void Deconstruct(out Name name, out Suffix suffix)
		{
			name = this.name;
			suffix = this.suffix;
		}

		public NameWithSuffix([DisallowNull] string name, [DisallowNull] string suffix)
		{
			this.name = name;
			this.suffix = suffix;

			if(suffix.Length == 0)
			{
				joinedAsPlainText = name;
				joinedAsRichText = name;
			}
			else
			{
				joinedAsPlainText = name + " (" + suffix + ")";
				joinedAsRichText = name + string.Format(ComponentName.InspectorSuffixFormat, suffix);
			}
		}

		public override string ToString() => joinedAsPlainText;
		internal string ToString(bool richText) => richText ? joinedAsRichText : joinedAsPlainText;
		public static implicit operator string(NameWithSuffix nameWithSuffix) => nameWithSuffix.joinedAsPlainText;
	}
}
#endif