using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Sisus.ComponentNames
{
	/// <summary>
	/// Represents the main title to show in the header of a component in the Inspector.
	/// </summary>
	public readonly struct Name : IEquatable<Name>, IEquatable<string>
	{
		public static readonly Name Default = new(null);
		private static readonly Name None = new("");

		private readonly string text;

		/// <summary>
		/// If <see langword="true"/> then the default name should be shown for the component in the Inspector.
		/// </summary>
		public bool IsDefault => text is null;

		/// <summary>
		/// Gets the number of characters in the name.
		/// <para>
		/// Returns 0 if this object represents the  <see cref="Default">default name</see>.
		/// </para>
		/// </summary>
		public int Length => text?.Length ?? 0;

		public Name([AllowNull] string text) => this.text = text;

		public static implicit operator string(Name name) => name.text;
		public static implicit operator Name(string text) => new(text);
		public static implicit operator Name(HeaderContent headerContent) => headerContent.name;
		public static bool operator ==(Name x, Name y) => string.Equals(x.text, y.text);
		public static bool operator !=(Name x, Name y) => !string.Equals(x.text, y.text);
		public static Name operator +(Name x, Name y) => HeaderContentUtility.Concat(x.text, y.text);
		public static Name operator +(string x, Name y) => HeaderContentUtility.Concat(x, y.text);
		public static Name operator +(Name x, string y) => HeaderContentUtility.Concat(x.text, y);
		public static Name Concat(Name x, Name y) => HeaderContentUtility.Concat(x.text, y.text);
		public override string ToString() => text ?? "";
		public override int GetHashCode() => text?.GetHashCode() ?? 0;
		public bool Equals(Name other) => string.Equals(text, other.text);
		public bool Equals(string other) => string.Equals(text, other);
		public override bool Equals(object obj) => obj is Name other && string.Equals(text, other.text);

		[Pure]
		public bool StartsWith(string substring) => text is not null && text.StartsWith(substring);

		[Pure]
		public bool EndsWith(string substring) => text is not null && text.EndsWith(substring);

		[Pure]
		public Name Replace(string oldValue, string newValue)
		{
			if(Length <= 0)
			{
				return this;
			}

			return text.Replace(oldValue, newValue);
		}

		[Pure]
		public Name Substring(int startIndex, int length)
		{
			if(Length <= 0)
			{
				return this;
			}

			if(text.Length <= startIndex)
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