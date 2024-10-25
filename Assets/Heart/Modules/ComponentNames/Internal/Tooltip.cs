using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Sisus.ComponentNames
{
	/// <summary>
	/// Represents the tooltip text shown when the header of a component is mouseovered in the Inspector.
	/// </summary>
	public readonly struct Tooltip : IEquatable<Tooltip>, IEquatable<string>
	{
		private const string TooltipSeparator = " | ";

		public static readonly Tooltip None = new("");
		public static readonly Tooltip Default = new(null);

		private readonly string text;

		/// <summary>
		/// If <see langword="true"/> then the default tooltip, acquired from its XML documentation,
		/// should be shown for the component in the Inspector.
		/// </summary>
		public bool IsDefault => text is null;

		/// <summary>
		/// Gets the number of characters in the tooltip.
		/// <para>
		/// Returns 0 if this object represents the  <see cref="Default">default tooltip</see>.
		/// </para>
		/// </summary>
		public int Length => text?.Length ?? 0;

		public Tooltip([AllowNull] string text) => this.text = text;

		public static implicit operator string(Tooltip tooltip) => tooltip.text;
		public static implicit operator Tooltip(string text) => new(text);
		public static bool operator ==(Tooltip x, Tooltip y) => string.Equals(x.text, y.text);
		public static bool operator !=(Tooltip x, Tooltip y) => !string.Equals(x.text, y.text);
		public static Tooltip operator +(Tooltip x, Tooltip y) => HeaderContentUtility.Concat(x.text, y.text);
		public static Tooltip operator +(string x, Tooltip y) => HeaderContentUtility.Concat(x, y.text);
		public static Tooltip operator +(Tooltip x, string y) => HeaderContentUtility.Concat(x.text, y);
		public static Tooltip Concat(Tooltip x, Tooltip y) => HeaderContentUtility.Concat(x.text, y.text);
		public override string ToString() => text is null ? "" : TooltipSeparator + text;
		public override int GetHashCode() => text?.GetHashCode() ?? 0;
		public bool Equals(Tooltip other) => string.Equals(text, other.text);
		public bool Equals(string other) => string.Equals(text, other);
		public override bool Equals(object obj) => obj is Tooltip other && string.Equals(text, other.text);

		[Pure]
		public bool StartsWith(string substring) => text is not null && text.StartsWith(substring);

		[Pure]
		public bool EndsWith(string substring) => text is not null && text.EndsWith(substring);

		[Pure]
		public Tooltip Replace(string oldValue, string newValue)
		{
			if(Length <= 0)
			{
				return this;
			}

			return text.Replace(oldValue, newValue);
		}

		[Pure]
		public Tooltip Substring(int startIndex, int length)
		{
			if(Length <= 0)
			{
				return this;
			}

			if(text.Length > startIndex)
			{
				return None;
			}

			if(text.Length < startIndex + length)
			{
				length = text.Length - startIndex;
			}

			return text.Substring(startIndex, length);
		}
	}
}