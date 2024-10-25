using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Sisus.ComponentNames
{
	/// <summary>
	/// Represents the suffix of a component shown inside brackets after the main title in the Inspector.
	/// </summary>
	public readonly struct Suffix : IEquatable<Suffix>, IEquatable<string>
	{
		internal const string Start = " (";
		internal const string End = ")";

		public static readonly Suffix None = new("");
		public static readonly Suffix Default = new(null);

		private readonly string text;

		/// <summary>
		/// If <see langword="true"/> then the default type name suffix should be shown for the component in the Inspector.
		/// </summary>
		public bool IsDefault => text is null;

		/// <summary>
		/// Gets the number of characters in the suffix (excluding brackets).
		/// <para>
		/// Returns 0 if this object represents the <see cref="Default">default suffix</see>.
		/// </para>
		/// </summary>
		public int Length => text?.Length ?? 0;

		public Suffix([AllowNull] string text) => this.text = text;

		public static implicit operator string(Suffix suffix) => suffix.text;
		public static implicit operator Suffix(string text) => new(text);
		public static bool operator ==(Suffix x, Suffix y) => string.Equals(x.text, y.text);
		public static bool operator !=(Suffix x, Suffix y) => !string.Equals(x.text, y.text);
		public static Suffix operator +(Suffix x, Suffix y) => HeaderContentUtility.Concat(x.text, y.text);
		public static Suffix operator +(string x, Suffix y) => HeaderContentUtility.Concat(x, y.text);
		public static Suffix operator +(Suffix x, string y) => HeaderContentUtility.Concat(x.text, y);
		public static Suffix Concat(Suffix x, Suffix y) => HeaderContentUtility.Concat(x.text, y.text);
		public override string ToString() => text is null ? "" : Start + text + End;
		public override int GetHashCode() => text?.GetHashCode() ?? 0;
		public bool Equals(Suffix other) => string.Equals(text, other.text);
		public bool Equals(string other) => string.Equals(text, other);
		public override bool Equals(object obj) => obj is Suffix other && string.Equals(text, other.text);

		[Pure]
		public bool StartsWith(string substring) => text is not null && text.StartsWith(substring);

		[Pure]
		public bool EndsWith(string substring) => text is not null && text.EndsWith(substring);

		[Pure]
		public Suffix Replace(string oldValue, string newValue)
		{
			if(Length <= 0)
			{
				return this;
			}

			return text.Replace(oldValue, newValue);
		}

		[Pure]
		public Suffix Substring(int startIndex, int length)
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